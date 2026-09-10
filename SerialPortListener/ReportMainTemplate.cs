using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerialPortListener
{
    /// <summary>
    /// ตัวกลางเดียวที่ตัดสินว่า ReportMain จะใช้เทมเพลตไหน
    /// ทุกจุดที่พิมพ์ ReportMain ต้องเรียกผ่านคลาสนี้ ห้ามระบุชื่อไฟล์ .rdlc ตรง ๆ ที่อื่น
    ///
    /// เทมเพลตแต่ละแบบมาจาก branch จริงในที่เก็บโค้ด (ดูรายละเอียดใน Templates ด้านล่าง)
    /// และมีชุดพารามิเตอร์ไม่เท่ากัน จึงต้องกรองพารามิเตอร์ก่อนส่งเข้า ReportViewer
    /// เสมอ - ดู FilterParameters()
    /// </summary>
    static class ReportMainTemplate
    {
        /// <summary>ข้อมูลของเทมเพลตหนึ่งแบบ</summary>
        internal sealed class TemplateInfo
        {
            public int Number { get; private set; }
            public string DisplayName { get; private set; }
            public string ResourceName { get; private set; }

            // ขนาดกระดาษและขอบ (นิ้ว) - ต้องตรงกับ .rdlc มิฉะนั้นงานพิมพ์จะถูกตัดหรือเลื่อน
            public double PageWidthInches { get; private set; }
            public double PageHeightInches { get; private set; }
            public double MarginLeftInches { get; private set; }
            public double MarginRightInches { get; private set; }
            public double MarginTopInches { get; private set; }
            public double MarginBottomInches { get; private set; }

            public TemplateInfo(int number, string displayName, string resourceName,
                                double pageWidth, double pageHeight,
                                double marginLeft, double marginRight,
                                double marginTop, double marginBottom)
            {
                Number = number;
                DisplayName = displayName;
                ResourceName = resourceName;
                PageWidthInches = pageWidth;
                PageHeightInches = pageHeight;
                MarginLeftInches = marginLeft;
                MarginRightInches = marginRight;
                MarginTopInches = marginTop;
                MarginBottomInches = marginBottom;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        // เทมเพลตทั้งหมดที่มีหลักฐานจริงใน branch - ห้ามเพิ่มแบบที่ไม่มีที่มา
        // Template 1 = ของเดิมของ Master (A4 แนวตั้ง, มี PStoneDesc) จึงเป็นค่าเริ่มต้น
        // Template 2 = ใบสลิป 8x5.5 นิ้ว จาก KT_Blue_11/03/25_CCom (มี PScoopName)
        // Template 3 = A4 มาตรฐาน จาก Blue_T1_11/03/25 (39 พารามิเตอร์)
        // Template 4 = A4 พร้อมตรา ISO จาก CTM_Blue_11/03/25 (มี Tiso)
        // ขอบกระดาษของแบบ A4 ใช้ค่าเดิมที่โปรแกรมใช้อยู่ (0.46/0.46/0.60/0.30 นิ้ว)
        // ซึ่งเป็นค่าที่ปรับไว้กับเครื่องพิมพ์จริง ไม่ใช่ค่าใน .rdlc - คงไว้เพื่อไม่ให้งานพิมพ์เดิมเปลี่ยน
        // ส่วนใบสลิปใช้ขอบ 0.2 นิ้วตามที่ระบุใน .rdlc ของมันเอง
        private static readonly TemplateInfo[] Templates =
        {
            new TemplateInfo(1, "Template 1 - มาตรฐาน (A4)",        "SerialPortListener.ReportMain_Template1.rdlc",
                             8.27, 11.69, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(2, "Template 2 - ใบสลิป (8x5.5 นิ้ว)", "SerialPortListener.ReportMain_Template2.rdlc",
                             8.00,  5.50, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo(3, "Template 3 - A4 แบบเดิม",          "SerialPortListener.ReportMain_Template3.rdlc",
                             8.27, 11.69, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(4, "Template 4 - A4 พร้อมตรา ISO",     "SerialPortListener.ReportMain_Template4.rdlc",
                             8.27, 11.69, 0.46, 0.46, 0.60, 0.30),
        };

        public const int DefaultTemplate = 1;

        // เก็บ config ไว้ใน AppData เหมือน config_port.txt และ configs_backup.txt
        // เพราะโฟลเดอร์ที่ติดตั้งโปรแกรมเขียนไฟล์ไม่ได้ถ้าไม่ใช่ admin
        private static readonly string ConfigPath =
            Path.Combine(Utils.AppDataDir, "config_reportmain.txt");

        private const string TemplateKey = "Template";
        private const string MainCompKeyPrefix = "MainComp";

        // ชื่อบริษัทบนหัวกระดาษ (cbbMainComp) - ปรับได้ในไฟล์ config โดยไม่ต้องคอมไพล์ใหม่
        // ค่าเริ่มต้นคือชื่อที่ฝังอยู่ในไฟล์ .rdlc เดิม จึงพิมพ์ออกมาเหมือนก่อนมีตัวเลือกนี้
        private static readonly string[] DefaultMainCompanies =
        {
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด (1169)",
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด",
            "บริษัท โชคพนาไมนิ่ง จำกัด",
            "บริษัท โชคพนา (2512) จำกัด",
        };

        /// <summary>รายการเทมเพลตทั้งหมด ใช้ผูกกับ ComboBox ในหน้าตั้งค่า</summary>
        public static TemplateInfo[] All
        {
            get { return Templates; }
        }

        public static string ConfigFilePath
        {
            get { return ConfigPath; }
        }

        /// <summary>
        /// อ่านหมายเลขเทมเพลตที่เลือกไว้ ถ้าไฟล์หาย ค่าหาย ค่าเสีย หรืออยู่นอกช่วง
        /// จะคืนค่าเริ่มต้น (Template 1) เสมอ ไม่โยน exception
        /// </summary>
        public static int GetSelectedTemplateNumber()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return DefaultTemplate;

                foreach (string line in File.ReadAllLines(ConfigPath))
                {
                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                        continue;

                    string key = line.Substring(0, idx).Trim();
                    if (!string.Equals(key, TemplateKey, StringComparison.OrdinalIgnoreCase))
                        continue;

                    int value;
                    if (int.TryParse(line.Substring(idx + 1).Trim(), out value) && IsValid(value))
                        return value;

                    return DefaultTemplate;   // มีคีย์แต่ค่าใช้ไม่ได้
                }
            }
            catch (Exception)
            {
                // อ่านไฟล์ไม่ได้ (สิทธิ์ ไฟล์ถูกล็อก ฯลฯ) - ไม่ควรทำให้พิมพ์ไม่ได้
            }

            return DefaultTemplate;
        }

        /// <summary>ข้อมูลเทมเพลตที่เลือกอยู่</summary>
        public static TemplateInfo GetSelectedTemplate()
        {
            return Find(GetSelectedTemplateNumber()) ?? Templates[0];
        }

        /// <summary>
        /// ชื่อ embedded resource ของ .rdlc ที่ต้องใช้
        /// ถ้าไฟล์ของเทมเพลตที่เลือกไม่ได้ถูกฝังมาด้วย จะถอยไปใช้เทมเพลตเริ่มต้นแทน
        /// </summary>
        public static string GetReportMainResourceName()
        {
            TemplateInfo t = GetSelectedTemplate();

            if (ResourceExists(t.ResourceName))
                return t.ResourceName;

            TemplateInfo fallback = Find(DefaultTemplate);
            if (fallback != null && ResourceExists(fallback.ResourceName))
                return fallback.ResourceName;

            // ไฟล์เดิมก่อนมีระบบเทมเพลต - เป็นทางถอยสุดท้าย
            return "SerialPortListener.ReportMain.rdlc";
        }

        /// <summary>
        /// บันทึกเทมเพลตที่เลือก โดยรักษาบรรทัดอื่นในไฟล์ไว้ทั้งหมด
        /// คืนค่า true เมื่อบันทึกสำเร็จ
        /// </summary>
        public static bool SaveSelectedTemplate(int templateNumber)
        {
            if (!IsValid(templateNumber))
                templateNumber = DefaultTemplate;

            try
            {
                List<string> lines = new List<string>();
                if (File.Exists(ConfigPath))
                    lines.AddRange(File.ReadAllLines(ConfigPath));

                bool replaced = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    int idx = lines[i].IndexOf('=');
                    if (idx <= 0)
                        continue;

                    string key = lines[i].Substring(0, idx).Trim();
                    if (string.Equals(key, TemplateKey, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = TemplateKey + "=" + templateNumber;
                        replaced = true;
                        break;
                    }
                }

                if (!replaced)
                    lines.Add(TemplateKey + "=" + templateNumber);

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
        /// คัดเฉพาะพารามิเตอร์ที่รายงานนั้นประกาศไว้จริง
        /// เทมเพลตแต่ละแบบมีพารามิเตอร์ไม่เท่ากัน (39 หรือ 40 ตัว และคนละชื่อ)
        /// ถ้าส่งตัวที่รายงานไม่รู้จักเข้าไป ReportViewer จะโยน exception ทันที
        /// </summary>
        public static Microsoft.Reporting.WinForms.ReportParameter[] FilterParameters(
            Microsoft.Reporting.WinForms.LocalReport report,
            Microsoft.Reporting.WinForms.ReportParameter[] parameters)
        {
            if (report == null || parameters == null)
                return parameters ?? new Microsoft.Reporting.WinForms.ReportParameter[0];

            HashSet<string> known;
            try
            {
                known = new HashSet<string>(
                    report.GetParameters().Select(p => p.Name),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                // อ่านรายการพารามิเตอร์ไม่ได้ - ส่งของเดิมไปตามปกติ
                return parameters;
            }

            return parameters.Where(p => known.Contains(p.Name)).ToArray();
        }

        /// <summary>
        /// รายชื่อบริษัทสำหรับหัวกระดาษ อ่านจาก config (MainComp1, MainComp2, ...)
        /// ถ้าไม่ได้ตั้งไว้จะใช้รายการเริ่มต้น
        /// </summary>
        public static string[] GetMainCompanies()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    List<string> found = new List<string>();
                    foreach (string line in File.ReadAllLines(ConfigPath))
                    {
                        int idx = line.IndexOf('=');
                        if (idx <= 0)
                            continue;

                        string key = line.Substring(0, idx).Trim();
                        if (!key.StartsWith(MainCompKeyPrefix, StringComparison.OrdinalIgnoreCase))
                            continue;

                        int n;
                        if (!int.TryParse(key.Substring(MainCompKeyPrefix.Length), out n))
                            continue;

                        string value = line.Substring(idx + 1).Trim();
                        if (value.Length > 0)
                            found.Add(value);
                    }

                    if (found.Count > 0)
                        return found.ToArray();
                }
            }
            catch (Exception)
            {
                // อ่านไม่ได้ - ใช้ค่าเริ่มต้น
            }

            return DefaultMainCompanies;
        }

        private static bool IsValid(int templateNumber)
        {
            return Find(templateNumber) != null;
        }

        private static TemplateInfo Find(int templateNumber)
        {
            return Templates.FirstOrDefault(t => t.Number == templateNumber);
        }

        private static bool ResourceExists(string resourceName)
        {
            try
            {
                return System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .Any(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
