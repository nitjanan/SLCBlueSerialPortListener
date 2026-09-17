using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace SerialPortListener
{
    /// <summary>
    /// ศูนย์รวมวิธีอ่านค่าน้ำหนักจากพอร์ตอนุกรม
    /// แต่ละสาขาใช้ตาชั่งคนละรุ่น รูปแบบข้อมูลที่ส่งมาจึงไม่เหมือนกัน
    /// เดิมแยกกันอยู่คนละ branch ตอนนี้รวมมาไว้ที่เดียวแล้วให้เลือกจากหน้า ucHelp
    /// เก็บค่าที่เลือกไว้ใน config_serial.txt (รูปแบบเดียวกับ ReportMainTemplate)
    /// </summary>
    public static class SerialDataHandler
    {
        /// <summary>วิธีแยกค่าน้ำหนักออกจากข้อความที่รับมา</summary>
        public enum ParseMode
        {
            /// <summary>ตัดท้ายที่ Terminator แล้วตัดหน้าที่ Start จากนั้นจับตัวเลขด้วย Pattern</summary>
            Delimited,
            /// <summary>จับตัวเลขจากก้อนข้อมูลที่เพิ่งรับมาโดยตรง ไม่สนใจตัวคั่น</summary>
            RawChunk,
            /// <summary>หาจุดเริ่มจาก p หรือ q แล้วตรวจความสมเหตุสมผลของค่าก่อนรับ</summary>
            ValidatedPQ,
            /// <summary>อ่านบรรทัดสมบูรณ์บรรทัดสุดท้ายด้วย regex เต็มรูปแบบ ถอยไปบรรทัดก่อนหน้าถ้าเพี้ยน</summary>
            LineFrame,
            /// <summary>หา marker ในบรรทัด แล้วอ่านฟิลด์ตัวเลขความกว้างคงที่ที่มีทศนิยมแฝง</summary>
            LineMarker
        }

        /// <summary>นิยามของวิธีอ่านหนึ่งแบบ</summary>
        public class HandlerInfo
        {
            public string Key;            // คีย์ที่เก็บลงไฟล์ตั้งค่า
            public string DisplayName;    // ข้อความที่แสดงในคอมโบบ็อกซ์
            public string SourceBranch;   // branch ต้นทาง เอาไว้ไล่ย้อนกลับได้
            public ParseMode Mode;

            public string Terminator;     // ข้อความที่ใช้ตัดท้าย (null = ไม่ตัด)
            public string Start;          // ข้อความที่ใช้ตัดหน้า (null = ไม่ตัด)
            public int StartOffset;       // ข้ามกี่ตัวอักษรหลังจุดเริ่ม
            public string Pattern = @"\d+";

            public int MaxTextLength = 1000;  // ความยาวสูงสุดที่เก็บไว้ในช่องข้อมูลดิบ
            public bool TrimLeadingZeros = true;
            public bool ErrorOnNoMatch;   // อ่านไม่ได้ให้แสดง Error แทนการเงียบ
            public bool NoDataTimeout;    // ไม่มีข้อมูลเข้าภายใน 1.5 วินาที ให้แสดง Error
            public bool AutoWeight;       // ผู้ใช้ที่มีสิทธิ auto_weight ให้ตั้งน้ำหนักเป็น 100 โดยไม่อ่านพอร์ต
            public bool Buffered;         // รับข้อมูลลงบัฟเฟอร์ แล้วให้ timer อัปเดตหน้าจอแทน

            // ใช้เฉพาะ LineFrame / LineMarker
            public string FrameRegex;
            public string Marker;
            public int FieldLength;
            public int DecimalPlaces;
        }

        public const string DefaultKey = "KG_CR";

        /// <summary>รายการวิธีอ่านทั้งหมดที่รวบรวมมาจากทุก branch</summary>
        public static readonly HandlerInfo[] Handlers = new HandlerInfo[]
        {
            new HandlerInfo { Key = "KG_CR", DisplayName = "1. KG + CR (มาตรฐาน)",
                SourceBranch = "Blue_Master / Master_Blue_1", Mode = ParseMode.Delimited,
                Terminator = "KG", Start = "\r" },

            new HandlerInfo { Key = "RAW", DisplayName = "2. อ่านตรงจากข้อมูลดิบ (ไม่มีตัวคั่น)",
                SourceBranch = "39_Blue_new_11/03/25", Mode = ParseMode.RawChunk,
                TrimLeadingZeros = false },

            new HandlerInfo { Key = "KG_LOWER_CR", DisplayName = "3. kg + CR (ตัวพิมพ์เล็ก)",
                SourceBranch = "Blue_Uni_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "kg", Start = "\r" },

            new HandlerInfo { Key = "KG_LOWER_G3", DisplayName = "4. kg + G (เครื่องพี่รุ่ง)",
                SourceBranch = "Blue_DO_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "kg", Start = "G", StartOffset = 3 },

            new HandlerInfo { Key = "CR_PAREN3", DisplayName = "5. CR + วงเล็บเปิด",
                SourceBranch = "Blue_T4_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "\r", Start = "(", StartOffset = 3 },

            new HandlerInfo { Key = "CR_PAREN3_AUTO", DisplayName = "6. CR + วงเล็บเปิด + น้ำหนักอัตโนมัติ",
                SourceBranch = "Blue_T1_clean_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "\r", Start = "(", StartOffset = 3, AutoWeight = true },

            new HandlerInfo { Key = "CR_P", DisplayName = "7. CR + p",
                SourceBranch = "Blue_T4_15_11 / M3&Blue_match / JOB_Blue", Mode = ParseMode.Delimited,
                Terminator = "\r", Start = "p" },

            new HandlerInfo { Key = "CR_PQ_VALIDATED", DisplayName = "8. CR + p/q พร้อมตรวจค่าผิดปกติ",
                SourceBranch = "39_Blue_new_fix_invoid_11/03/25", Mode = ParseMode.ValidatedPQ,
                Terminator = "\r", TrimLeadingZeros = false },

            new HandlerInfo { Key = "KN_CR", DisplayName = "9. KN + CR",
                SourceBranch = "KT_Blue_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "KN", Start = "\r" },

            new HandlerInfo { Key = "NSM", DisplayName = "10. NSM (ST,GS, + ,Kg)",
                SourceBranch = "NSM_Blue_11/03/25", Mode = ParseMode.Delimited,
                Terminator = ",Kg", Start = "ST,GS,", MaxTextLength = 50, ErrorOnNoMatch = true },

            new HandlerInfo { Key = "NSM_AUTO", DisplayName = "11. NSM + น้ำหนักอัตโนมัติ + ตัดเมื่อไม่มีข้อมูล",
                SourceBranch = "NSM_Blue_Auto_update_01/08/2026", Mode = ParseMode.Delimited,
                Terminator = ",Kg", Start = "ST,GS,", MaxTextLength = 50, ErrorOnNoMatch = true,
                NoDataTimeout = true, AutoWeight = true },

            new HandlerInfo { Key = "ETB_P", DisplayName = "12. ETB + p",
                SourceBranch = "FT_ST_SURAT_STP_2025", Mode = ParseMode.Delimited,
                Terminator = "\x17", Start = "p" },

            new HandlerInfo { Key = "ETB_Q_AUTO", DisplayName = "13. ETB + q + น้ำหนักอัตโนมัติ",
                SourceBranch = "JOB_Blue_Auto_update_01/08/2026", Mode = ParseMode.Delimited,
                Terminator = "\x17", Start = "q", AutoWeight = true },

            new HandlerInfo { Key = "STX_SIGNED", DisplayName = "14. STX + CR (รองรับน้ำหนักติดลบ)",
                SourceBranch = "SRD_Blue_11/03/25", Mode = ParseMode.Delimited,
                Terminator = "\r", Start = "\x02", Pattern = @"-?\d+", TrimLeadingZeros = false },

            new HandlerInfo { Key = "KRABI_FRAME", DisplayName = "15. อ่านทีละบรรทัดแบบมีสถานะ (กระบี่)",
                SourceBranch = "KRABI_STP_2026", Mode = ParseMode.LineFrame, Buffered = true,
                FrameRegex = @"^\((?<status>[^\r\n])[ \t]*(?<weight>[-+]?\d+)[ \t]+(?<extra>[-+]?\d+)[ \t]*$" },

            new HandlerInfo { Key = "TYM_MARKER", DisplayName = "16. อ่านฟิลด์ความกว้างคงที่หลัง *0 (TYM)",
                SourceBranch = "TYM_Blue_Auto_update_01/08/2026", Mode = ParseMode.LineMarker,
                Buffered = true, Marker = "*0", FieldLength = 12, DecimalPlaces = 6 }
        };

        private static readonly string ConfigPath =
            Path.Combine(Utils.AppDataDir, "config_serial.txt");

        public static string ConfigFilePath
        {
            get { return ConfigPath; }
        }

        public static HandlerInfo Find(string key)
        {
            if (!string.IsNullOrEmpty(key))
                foreach (HandlerInfo h in Handlers)
                    if (string.Equals(h.Key, key, StringComparison.OrdinalIgnoreCase))
                        return h;
            return null;
        }

        // ---- อ่าน/เขียนไฟล์ตั้งค่า config_serial.txt ----
        // ไฟล์เดียวเก็บทั้งวิธีอ่านค่าและพารามิเตอร์ของสายสัญญาณ ในรูปแบบ key=value
        // บรรทัดที่ไม่รู้จักจะถูกคงไว้เสมอ เผื่อมีคนเพิ่มค่าอื่นไว้เอง

        /// <summary>อ่านทั้งไฟล์เป็นคู่ key/value ถ้าไม่มีไฟล์หรืออ่านไม่ได้จะคืนรายการว่าง</summary>
        private static Dictionary<string, string> ReadConfig()
        {
            Dictionary<string, string> d =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(ConfigPath))
                {
                    foreach (string line in File.ReadAllLines(ConfigPath))
                    {
                        string t = line.Trim();
                        if (t.Length == 0 || t.StartsWith("#"))
                            continue;
                        int eq = t.IndexOf('=');
                        if (eq <= 0)
                            continue;
                        d[t.Substring(0, eq).Trim()] = t.Substring(eq + 1).Trim();
                    }
                }
            }
            catch (Exception)
            {
                // อ่านไม่ได้ให้ถือว่ายังไม่เคยตั้งค่า ไม่ต้องทำให้โปรแกรมล้ม
            }
            return d;
        }

        /// <summary>เขียนค่าที่ส่งมาลงไฟล์ โดยคงบรรทัดอื่นและคอมเมนต์เดิมไว้</summary>
        private static bool WriteConfig(Dictionary<string, string> values)
        {
            try
            {
                List<string> lines = new List<string>();
                if (File.Exists(ConfigPath))
                    lines.AddRange(File.ReadAllLines(ConfigPath));

                foreach (KeyValuePair<string, string> kv in values)
                {
                    bool replaced = false;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string t = lines[i].Trim();
                        if (t.Length == 0 || t.StartsWith("#"))
                            continue;
                        int eq = t.IndexOf('=');
                        if (eq > 0 && t.Substring(0, eq).Trim().Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            lines[i] = kv.Key + "=" + kv.Value;
                            replaced = true;
                            break;
                        }
                    }
                    if (!replaced)
                        lines.Add(kv.Key + "=" + kv.Value);
                }

                if (!Directory.Exists(Utils.AppDataDir))
                    Directory.CreateDirectory(Utils.AppDataDir);
                File.WriteAllLines(ConfigPath, lines.ToArray());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>อ่านคีย์วิธีอ่านที่เลือกไว้ ถ้าไม่มีหรือไม่รู้จักให้ใช้ค่าเริ่มต้น</summary>
        public static string GetSelectedKey()
        {
            string v;
            if (ReadConfig().TryGetValue("Handler", out v))
            {
                HandlerInfo h = Find(v);
                if (h != null)
                    return h.Key;
            }
            return DefaultKey;
        }

        public static HandlerInfo GetSelectedHandler()
        {
            return Find(GetSelectedKey()) ?? Find(DefaultKey);
        }

        public static bool SaveSelectedHandler(string key)
        {
            if (Find(key) == null)
                return false;
            Dictionary<string, string> d = new Dictionary<string, string>();
            d["Handler"] = key;
            return WriteConfig(d);
        }

        // ---- พารามิเตอร์ของสายสัญญาณ ----
        // ค่าเริ่มต้นตรงกับที่โปรแกรมเคยใช้มาก่อนมีไฟล์ตั้งค่า จะได้ไม่เปลี่ยนพฤติกรรมเดิม

        /// <summary>พารามิเตอร์พอร์ตที่บันทึกไว้ (ไม่รวมชื่อพอร์ต ซึ่งอยู่ที่ config_port.txt)</summary>
        public class PortSettings
        {
            public int BaudRate = 2400;
            public Parity Parity = Parity.None;
            public int DataBits = 7;
            public StopBits StopBits = StopBits.One;
        }

        public static PortSettings GetPortSettings()
        {
            PortSettings ps = new PortSettings();
            Dictionary<string, string> d = ReadConfig();
            string v;

            int baud;
            if (d.TryGetValue("BaudRate", out v) && int.TryParse(v, out baud) && baud > 0)
                ps.BaudRate = baud;

            if (d.TryGetValue("Parity", out v))
            {
                try { ps.Parity = (Parity)Enum.Parse(typeof(Parity), v, true); }
                catch (Exception) { }
            }

            int bits;
            if (d.TryGetValue("DataBits", out v) && int.TryParse(v, out bits) && bits >= 5 && bits <= 8)
                ps.DataBits = bits;

            if (d.TryGetValue("StopBits", out v))
            {
                try { ps.StopBits = (StopBits)Enum.Parse(typeof(StopBits), v, true); }
                catch (Exception) { }
            }
            return ps;
        }

        public static bool SavePortSettings(PortSettings ps)
        {
            if (ps == null)
                return false;
            Dictionary<string, string> d = new Dictionary<string, string>();
            d["BaudRate"] = ps.BaudRate.ToString();
            d["Parity"] = ps.Parity.ToString();
            d["DataBits"] = ps.DataBits.ToString();
            d["StopBits"] = ps.StopBits.ToString();
            return WriteConfig(d);
        }

        /// <summary>ผลลัพธ์จากการพยายามแยกค่าน้ำหนัก</summary>
        public struct ParseResult
        {
            public bool HasValue;   // แยกค่าได้
            public bool IsError;    // แยกไม่ได้ และวิธีนี้กำหนดให้แสดง Error
            public string Text;     // ค่าที่จะเอาไปแสดง
            public bool IsNegative;
        }

        private static readonly char[] LineBreaks = new char[] { '\r', '\n' };

        /// <summary>
        /// แยกค่าน้ำหนัก เป็นฟังก์ชันล้วน ไม่แตะ UI จึงเอาไปทดสอบแยกได้
        /// accumulated = ข้อความสะสมทั้งหมดในช่องข้อมูลดิบ, chunk = ก้อนที่เพิ่งรับเข้ามา
        /// </summary>
        public static ParseResult Parse(HandlerInfo h, string accumulated, string chunk)
        {
            ParseResult r = new ParseResult();
            if (h == null)
                return r;
            try
            {
                switch (h.Mode)
                {
                    case ParseMode.RawChunk:
                        return MatchNumber(h, chunk ?? string.Empty);

                    case ParseMode.ValidatedPQ:
                        return ParseValidatedPQ(h, accumulated);

                    case ParseMode.LineFrame:
                        return ParseLineFrame(h, accumulated);

                    case ParseMode.LineMarker:
                        return ParseLineMarker(h, accumulated);

                    default:
                        return ParseDelimited(h, accumulated);
                }
            }
            catch (Exception)
            {
                // ข้อมูลยังมาไม่ครบเป็นเรื่องปกติ ให้ถือว่ายังอ่านไม่ได้
                r.IsError = h.ErrorOnNoMatch;
                return r;
            }
        }

        private static ParseResult ParseDelimited(HandlerInfo h, string text)
        {
            ParseResult r = new ParseResult();
            if (string.IsNullOrEmpty(text))
            {
                r.IsError = h.ErrorOnNoMatch;
                return r;
            }

            string window = text;
            if (!string.IsNullOrEmpty(h.Terminator))
            {
                int end = window.LastIndexOf(h.Terminator, StringComparison.Ordinal);
                if (end < 0)
                {
                    r.IsError = h.ErrorOnNoMatch;
                    return r;
                }
                window = window.Remove(end);
            }
            if (!string.IsNullOrEmpty(h.Start))
            {
                int start = window.LastIndexOf(h.Start, StringComparison.Ordinal);
                if (start < 0)
                {
                    r.IsError = h.ErrorOnNoMatch;
                    return r;
                }
                start += h.StartOffset;
                if (start > window.Length)
                {
                    r.IsError = h.ErrorOnNoMatch;
                    return r;
                }
                window = window.Substring(start);
            }
            return MatchNumber(h, window);
        }

        private static ParseResult MatchNumber(HandlerInfo h, string window)
        {
            ParseResult r = new ParseResult();
            MatchCollection mc = Regex.Matches(window, h.Pattern);
            if (mc.Count == 0)
            {
                r.IsError = h.ErrorOnNoMatch;
                return r;
            }
            string v = mc[0].Value;
            r.IsNegative = v.StartsWith("-");
            r.Text = h.TrimLeadingZeros ? v.TrimStart('0').PadLeft(1, '0') : v;
            r.HasValue = true;
            return r;
        }

        // 39_Blue_new_fix_invoid: หาจุดเริ่มจาก p หรือ q ตัวสุดท้าย แล้วกรองค่าที่เป็นไปไม่ได้ทิ้ง
        private static ParseResult ParseValidatedPQ(HandlerInfo h, string text)
        {
            ParseResult r = new ParseResult();
            if (string.IsNullOrEmpty(text))
                return r;

            int end = text.LastIndexOf('\r');
            if (end < 0)
                return r;
            string window = text.Remove(end);

            int op = window.LastIndexOfAny(new char[] { 'p', 'q' });
            if (op < 0)
                return r;
            window = window.Substring(op);

            MatchCollection mc = Regex.Matches(window, h.Pattern);
            if (mc.Count == 0)
                return r;

            int value;
            if (!int.TryParse(mc[0].Value, out value))
                return r;

            // ตาชั่งรุ่นนี้ส่งค่าเป็นขั้นละ 10 เสมอ ค่าที่ไม่ลงตัวหรือเกินพิสัยคือข้อมูลเพี้ยน ให้ทิ้ง
            if (value % 10 != 0 || value > 100000)
                return r;

            r.Text = (value < 10) ? "0" : mc[0].Value;
            r.HasValue = true;
            return r;
        }

        // KRABI: ใช้บรรทัดสมบูรณ์บรรทัดสุดท้าย ถ้าเพี้ยนให้ถอยไปบรรทัดก่อนหน้า
        private static ParseResult ParseLineFrame(HandlerInfo h, string raw)
        {
            ParseResult r = new ParseResult();
            if (string.IsNullOrEmpty(raw))
                return r;

            Regex re = new Regex(h.FrameRegex);
            int end = raw.LastIndexOfAny(LineBreaks);
            while (end > 0)
            {
                int start = raw.LastIndexOfAny(LineBreaks, end - 1) + 1;
                Match m = re.Match(raw.Substring(start, end - start));
                if (m.Success)
                {
                    string w = m.Groups["weight"].Value;
                    bool neg = w.StartsWith("-");
                    string digits = w.TrimStart('+', '-').TrimStart('0');
                    if (digits.Length == 0)
                        digits = "0";
                    r.IsNegative = neg && digits != "0";
                    r.Text = r.IsNegative ? "-" + digits : digits;
                    r.HasValue = true;
                    return r;
                }
                end = (start > 0) ? start - 1 : -1;
            }
            return r;
        }

        // TYM: หา marker แล้วอ่านตัวเลขความกว้างคงที่ที่มีทศนิยมแฝงอยู่ท้ายฟิลด์
        private static ParseResult ParseLineMarker(HandlerInfo h, string data)
        {
            ParseResult r = new ParseResult();
            if (string.IsNullOrEmpty(data))
                return r;

            string[] lines = data.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            int last = lines.Length - 1;
            // บรรทัดสุดท้ายที่ยังไม่ขึ้นบรรทัดใหม่ถือว่ารับมาไม่ครบ
            if (!(data.EndsWith("\n") || data.EndsWith("\r")))
                last--;

            for (int i = last; i >= 0; i--)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                int mp = line.LastIndexOf(h.Marker, StringComparison.Ordinal);
                if (mp < 0)
                    continue;

                int pos = mp + h.Marker.Length;
                while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                    pos++;

                int start = pos;
                while (pos < line.Length && char.IsDigit(line[pos]))
                    pos++;
                if (pos == start)
                    continue;

                string digits = line.Substring(start, pos - start);
                if (digits.Length != h.FieldLength)
                    continue;   // รับมาไม่ครบฟิลด์ ลองบรรทัดก่อนหน้า

                string intPart = digits.Substring(0, digits.Length - h.DecimalPlaces);
                string trimmed = intPart.TrimStart('0');
                r.Text = (trimmed.Length == 0) ? "0" : trimmed;
                r.HasValue = true;
                return r;
            }
            return r;
        }
    }
}
