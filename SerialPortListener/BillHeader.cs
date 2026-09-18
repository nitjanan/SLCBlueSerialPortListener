using System;
using System.Collections.Generic;
using System.IO;

namespace SerialPortListener
{
    /// <summary>
    /// แบบหัวกระดาษบิล (ReportMain)
    /// ปกติชื่อบริษัท ที่อยู่ และเบอร์โทร ดึงมาจากตาราง Company ในฐานข้อมูล
    /// บางเครื่องต้องพิมพ์ในนามสาขาหรือนิติบุคคลอื่นโดยไม่แตะฐานข้อมูล จึงให้ตั้งชุดสำเร็จรูปไว้แล้วเลือกได้
    /// เก็บที่ config_billheader.txt รูปแบบเดียวกับ config_reportmain.txt
    /// </summary>
    public static class BillHeader
    {
        /// <summary>หัวกระดาษหนึ่งชุด</summary>
        public class HeaderInfo
        {
            public int Number;            // 0 = ใช้ค่าจากฐานข้อมูล
            public string Name;           // ชื่อที่แสดงในคอมโบบ็อกซ์
            public string CompanyName;    // -> PCompanyName
            public string Address;        // -> PAddress
            public string Telephone;      // -> PTelephone

            /// <summary>ชุดปริยาย ไม่แทนที่อะไรเลย ใช้ค่าจากฐานข้อมูลทุกฟิลด์</summary>
            public bool UsesDatabase
            {
                get { return Number == 0; }
            }

            /// <summary>ชุดที่ผู้ใช้พิมพ์ข้อความเอง</summary>
            public bool IsCustom
            {
                get { return Number == CustomNumber; }
            }
        }

        /// <summary>ชุดปริยาย ไม่แทนที่อะไร พฤติกรรมเหมือนก่อนมีฟีเจอร์นี้</summary>
        public static readonly HeaderInfo DatabaseHeader =
            new HeaderInfo { Number = 0, Name = "ใช้ค่าจากฐานข้อมูล (ค่าเริ่มต้น)" };

        /// <summary>หมายเลขของชุดที่ผู้ใช้พิมพ์ข้อความเอง เก็บในไฟล์เป็น Header=custom</summary>
        public const int CustomNumber = -1;

        private const string CustomPrefix = "Custom.";

        /// <summary>ชุดที่ผู้ใช้พิมพ์เอง อ่านจากคีย์ Custom.* ในไฟล์</summary>
        public static HeaderInfo GetCustomHeader()
        {
            Dictionary<string, string> d = ReadConfig();
            string comp, addr, tel;
            d.TryGetValue(CustomPrefix + "CompanyName", out comp);
            d.TryGetValue(CustomPrefix + "Address", out addr);
            d.TryGetValue(CustomPrefix + "Telephone", out tel);
            return new HeaderInfo
            {
                Number = CustomNumber,
                Name = "กรอกเอง",
                CompanyName = comp,
                Address = addr,
                Telephone = tel
            };
        }

        /// <summary>บันทึกข้อความที่ผู้ใช้พิมพ์เอง ช่องที่เว้นว่างจะใช้ค่าจากฐานข้อมูลเฉพาะช่องนั้น</summary>
        public static bool SaveCustomHeader(string companyName, string address, string telephone)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d[CustomPrefix + "CompanyName"] = (companyName ?? string.Empty).Trim();
            d[CustomPrefix + "Address"] = (address ?? string.Empty).Trim();
            d[CustomPrefix + "Telephone"] = (telephone ?? string.Empty).Trim();
            return WriteConfig(d);
        }

        private static readonly string ConfigPath =
            Path.Combine(Utils.AppDataDir, "config_billheader.txt");

        public static string ConfigFilePath
        {
            get { return ConfigPath; }
        }

        /// <summary>อ่านทั้งไฟล์เป็นคู่ key/value ไม่มีไฟล์หรืออ่านไม่ได้ก็คืนรายการว่าง</summary>
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

        /// <summary>
        /// ชุดทั้งหมดที่ตั้งไว้ในไฟล์ ตัวแรกเป็นชุดปริยายเสมอ
        /// ไล่หา HeaderN.* ตั้งแต่ 1 ไปจนกว่าจะไม่เจอ ชุดที่ไม่มีข้อมูลเลยจะถูกข้าม
        /// </summary>
        public static HeaderInfo[] GetHeaders()
        {
            List<HeaderInfo> list = new List<HeaderInfo>();
            list.Add(DatabaseHeader);

            Dictionary<string, string> d = ReadConfig();
            for (int n = 1; n <= 99; n++)
            {
                string prefix = "Header" + n + ".";
                string name, comp, addr, tel;
                d.TryGetValue(prefix + "Name", out name);
                d.TryGetValue(prefix + "CompanyName", out comp);
                d.TryGetValue(prefix + "Address", out addr);
                d.TryGetValue(prefix + "Telephone", out tel);

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(comp) &&
                    string.IsNullOrEmpty(addr) && string.IsNullOrEmpty(tel))
                    continue;

                list.Add(new HeaderInfo
                {
                    Number = n,
                    Name = string.IsNullOrEmpty(name) ? ("แบบที่ " + n) : name,
                    CompanyName = comp,
                    Address = addr,
                    Telephone = tel
                });
            }
            list.Add(GetCustomHeader());
            return list.ToArray();
        }

        public static HeaderInfo Find(int number)
        {
            if (number == CustomNumber)
                return GetCustomHeader();
            if (number <= 0)
                return DatabaseHeader;
            foreach (HeaderInfo h in GetHeaders())
                if (h.Number == number)
                    return h;
            return null;
        }

        /// <summary>หมายเลขชุดที่เลือกไว้ ถ้าไม่มีหรือชี้ไปยังชุดที่ไม่มีอยู่จริงให้ใช้ค่าจากฐานข้อมูล</summary>
        public static int GetSelectedNumber()
        {
            string v;
            if (ReadConfig().TryGetValue("Header", out v))
            {
                if (string.Equals(v, "custom", StringComparison.OrdinalIgnoreCase))
                    return CustomNumber;
                int n;
                if (int.TryParse(v, out n) && Find(n) != null)
                    return n;
            }
            return 0;
        }

        public static HeaderInfo GetSelectedHeader()
        {
            return Find(GetSelectedNumber()) ?? DatabaseHeader;
        }

        /// <summary>บันทึกหมายเลขชุดที่เลือก โดยคงบรรทัดอื่นและคอมเมนต์ในไฟล์ไว้</summary>
        public static bool SaveSelectedNumber(int number)
        {
            if (Find(number) == null)
                return false;
            try
            {
                List<string> lines = new List<string>();
                if (File.Exists(ConfigPath))
                    lines.AddRange(File.ReadAllLines(ConfigPath));

                bool replaced = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    string t = lines[i].Trim();
                    if (t.Length == 0 || t.StartsWith("#"))
                        continue;
                    int eq = t.IndexOf('=');
                    if (eq > 0 && t.Substring(0, eq).Trim().Equals("Header", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = "Header=" + NumberToText(number);
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                    lines.Add("Header=" + NumberToText(number));

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

        private static string NumberToText(int number)
        {
            return (number == CustomNumber) ? "custom" : number.ToString();
        }

        /// <summary>
        /// ค่าที่จะใช้แทนพารามิเตอร์ตัวนี้ตามชุดที่เลือกไว้
        /// คืน null ถ้าไม่ใช่พารามิเตอร์หัวกระดาษ หรือชุดที่เลือกไม่ได้กำหนดค่าไว้
        /// (เว้นว่างในไฟล์ = ไม่แทนที่ ปล่อยให้ใช้ค่าจากฐานข้อมูลเฉพาะฟิลด์นั้น)
        /// </summary>
        public static string GetOverride(string parameterName)
        {
            HeaderInfo h = GetSelectedHeader();
            if (h == null || h.UsesDatabase)
                return null;

            string v = null;
            if (string.Equals(parameterName, "PCompanyName", StringComparison.OrdinalIgnoreCase))
                v = h.CompanyName;
            else if (string.Equals(parameterName, "PAddress", StringComparison.OrdinalIgnoreCase))
                v = h.Address;
            else if (string.Equals(parameterName, "PTelephone", StringComparison.OrdinalIgnoreCase))
                v = h.Telephone;

            return string.IsNullOrEmpty(v) ? null : v;
        }
    }
}
