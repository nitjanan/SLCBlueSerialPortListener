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

        // เทมเพลตทั้งหมดมาจาก ReportMain.rdlc ที่มีอยู่จริงใน branch - ห้ามเพิ่มแบบที่ไม่มีที่มา
        //
        // สแกน ReportMain.rdlc ทั้ง 247 branch พบไฟล์ที่ต่างกัน 28 เวอร์ชัน
        // แต่เมื่อจัดกลุ่มตามสิ่งที่มีผลจริง (ขนาดกระดาษ + ชุดพารามิเตอร์ + รหัสแบบฟอร์ม + ตราสัญลักษณ์)
        // เหลือ 11 แบบ จากนั้นถอดออก 4 แบบตามที่ผู้ใช้กำหนด และเพิ่มของ Blue_Uni_Auto_update เข้ามา
        // ปัจจุบันเหลือ 8 แบบให้เลือก
        //
        // เลขเทมเพลตคงเดิมเสมอ ไม่เรียงใหม่แม้จะมีการเอาบางแบบออก
        // เพราะค่าที่บันทึกไว้ใน config ของแต่ละหน่วยงานอ้างอิงเลขนี้
        // เลข 1, 7, 10, 11 เคยถูกใช้แล้วและถูกถอดออก จะไม่นำกลับมาใช้ซ้ำ
        //
        //  #   ที่มา (branch ตัวแทน)              ต่างจากชุดพื้นฐานตรงไหน
        //  2   KT_Blue_11/03/25(_CCom)           +PScoopName, ตรา KT
        //  3   Blue_T1_11/03/25 / Blue_DO        ชุดพื้นฐาน 39 ตัว  (ค่าเริ่มต้น - ใช้มากที่สุด 129 branch)
        //  4   CTM_Blue_11/03/25                 +Tiso, ไม่มีรหัสแบบฟอร์ม
        //  5   39_Blue_new_11/03/25              39 ตัว, FM-PD-03 (Sandvik)
        //  6   NSM_Blue_11/03/25 / TYM           +PScoopName, FM-PD-03 (Sandvik)
        //  8   NSM_Blue_Auto_update_01/08/2026   +PScoopName +PStoneDesc
        //  9   FT_ST_SURAT_STP_2025 / KRABI      +PNote +PStoneDesc
        // 12   Blue_Uni_Auto_update_01/08/2026   +PStoneDesc
        //
        // ขอบกระดาษของแบบ A4 ใช้ค่าเดิมที่โปรแกรมใช้อยู่ (0.46/0.46/0.60/0.30 นิ้ว)
        // ซึ่งเป็นค่าที่ปรับไว้กับเครื่องพิมพ์จริง ไม่ใช่ค่าใน .rdlc - คงไว้เพื่อไม่ให้งานพิมพ์เดิมเปลี่ยน
        // ส่วนใบสลิปใช้ขอบ 0.2 นิ้วตามที่ระบุใน .rdlc ของมันเอง
        private const double A4W = 8.27, A4H = 11.69;
        private const double SlipW = 8.00, SlipH = 5.50;

        // ชื่อที่แสดงมีรหัสหน่วยงานกำกับ เพื่อให้เลือกได้ถูกโดยไม่ต้องเปิดดูไฟล์
        // (T1, T4, Uni, JOB ฯลฯ คือรหัสหน่วยงาน ไม่ใช่เลขเทมเพลต)
        private static readonly TemplateInfo[] Templates =
        {
            new TemplateInfo( 3, "Template 3 - T1 / T4 / Uni / JOB / DO (A4)",        "SerialPortListener.ReportMain_Template3.rdlc",  A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(12, "Template 12 - Uni / T1 / T4 / JOB อัปเดตอัตโนมัติ (A4)", "SerialPortListener.ReportMain_Template12.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 5, "Template 5 - 39 / SRD (A4 Sandvik)",                "SerialPortListener.ReportMain_Template5.rdlc",  A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 4, "Template 4 - CTM (A4 + ตรา ISO)",                   "SerialPortListener.ReportMain_Template4.rdlc",  A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 2, "Template 2 - KT (ใบสลิป)",                          "SerialPortListener.ReportMain_Template2.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 6, "Template 6 - NSM / TYM / KRD (ใบสลิป Sandvik)",     "SerialPortListener.ReportMain_Template6.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 8, "Template 8 - NSM / TYM อัปเดตอัตโนมัติ (ใบสลิป)",   "SerialPortListener.ReportMain_Template8.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 9, "Template 9 - SURAT / KRABI (ใบสลิป STP)",           "SerialPortListener.ReportMain_Template9.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
        };

        public const int DefaultTemplate = 3;   // แบบที่ใช้มากที่สุด (129 branch)

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
        /// จะคืนค่าเริ่มต้น (Template 3) เสมอ ไม่โยน exception
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
        /// ปรับรายการพารามิเตอร์ให้ตรงกับที่รายงานประกาศไว้จริง ทำสองอย่าง
        ///   1. ตัดตัวที่รายงานไม่รู้จักออก  - ถ้าส่งไป ReportViewer จะโยน exception
        ///   2. เติมตัวที่รายงานประกาศไว้แต่ผู้เรียกไม่ได้ส่งมา ด้วยค่าว่าง
        ///      - ถ้าไม่เติม จะได้ error "The 'X' parameter is missing a value"
        ///
        /// ข้อ 2 จำเป็นเพราะเทมเพลตมาจากคนละ branch และประกาศพารามิเตอร์ไม่เหมือนกัน
        /// เช่น Template 2 ต้องใช้ PScoopName ส่วน Template 4 ต้องใช้ Tiso
        /// ผู้เรียกควรส่งค่าจริงมาให้ครบ ส่วนการเติมค่าว่างนี้เป็นตาข่ายกันพลาด
        /// เพื่อให้การเพิ่มเทมเพลตใหม่ในอนาคตไม่ทำให้พิมพ์ไม่ได้
        /// </summary>
        public static Microsoft.Reporting.WinForms.ReportParameter[] FilterParameters(
            Microsoft.Reporting.WinForms.LocalReport report,
            Microsoft.Reporting.WinForms.ReportParameter[] parameters)
        {
            if (parameters == null)
                parameters = new Microsoft.Reporting.WinForms.ReportParameter[0];
            if (report == null)
                return parameters;

            List<string> declared;
            try
            {
                declared = report.GetParameters().Select(p => p.Name).ToList();
            }
            catch (Exception)
            {
                // อ่านรายการพารามิเตอร์ไม่ได้ - ส่งของเดิมไปตามปกติ
                return parameters;
            }

            HashSet<string> known = new HashSet<string>(declared, StringComparer.OrdinalIgnoreCase);
            HashSet<string> supplied = new HashSet<string>(
                parameters.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

            List<Microsoft.Reporting.WinForms.ReportParameter> result =
                new List<Microsoft.Reporting.WinForms.ReportParameter>();

            foreach (Microsoft.Reporting.WinForms.ReportParameter p in parameters)
            {
                if (!known.Contains(p.Name))
                    continue;   // รายงานนี้ไม่รู้จัก - ส่งไปจะเกิด exception

                result.Add(Normalize(p.Name, p.Values));
            }

            // เติมตัวที่รายงานประกาศไว้แต่ผู้เรียกไม่ได้ส่งมาเลย
            foreach (string name in declared)
            {
                if (!supplied.Contains(name))
                    result.Add(new Microsoft.Reporting.WinForms.ReportParameter(name, BlankValue));
            }

            return result.ToArray();
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

        // พารามิเตอร์ในใบชั่งทุกแบบตั้ง AllowBlank = False ไว้
        // ค่า null หรือสตริงว่างจะถูกนับว่า "ยังไม่ได้ระบุค่า" และ render ไม่ผ่าน
        // จึงต้องแทนด้วยช่องว่างหนึ่งตัว เหมือนที่ strNotEmty ในโค้ดเดิมทำ
        private const string BlankValue = " ";

        /// <summary>
        /// คืนพารามิเตอร์ที่ค่าใช้งานได้เสมอ ถ้าค่าเดิมเป็น null หรือว่าง จะแทนด้วยช่องว่าง
        /// จุดนี้สำคัญ เพราะค่าอย่าง Weight.ScoopName เป็น null เมื่อพิมพ์จากหน้าจอหลัก
        /// (มีการกำหนดค่าเฉพาะตอนพิมพ์จากหน้ารายงาน) และ Company.Tiso ว่างถ้าไม่ได้ตั้งไว้
        /// </summary>
        private static Microsoft.Reporting.WinForms.ReportParameter Normalize(
            string name, System.Collections.Specialized.StringCollection values)
        {
            if (values == null || values.Count == 0)
                return new Microsoft.Reporting.WinForms.ReportParameter(name, BlankValue);

            string[] cleaned = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                string v = values[i];
                cleaned[i] = string.IsNullOrEmpty(v) || v.Trim().Length == 0 ? BlankValue : v;
            }

            return new Microsoft.Reporting.WinForms.ReportParameter(name, cleaned);
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
