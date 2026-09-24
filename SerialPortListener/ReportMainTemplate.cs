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
        // (2026, รอบรวมกลุ่ม): ก่อนหน้านี้มี 25 แบบ โดยแยกกันแม้ความต่างจะพิมพ์ออกมาเหมือนกันทุก
        // ประการ (เช่น Template 3 กับ 5 พารามิเตอร์/สิ่งที่แสดงเหมือนกันทุกตัว ต่างแค่ "เลขที่" เป็น
        // ข้อความคงที่ใน 3 หรือมาจาก Parameters!TDocName ใน 5 - แต่ทั้งคู่ตั้งค่า TDocName เป็นข้อความ
        // เดียวกันเสมอจากโค้ด (preparePrint) ผลพิมพ์จึงเหมือนกัน 100% ต่างกันแค่ "วิธีเขียนไฟล์ภายใน")
        // ผู้ใช้เห็นว่าตัวเลือกเยอะเกินจำเป็น จึงรวมกลุ่มใหม่ด้วยเกณฑ์ที่หยาบขึ้น:
        //
        //   รวมเป็นแบบเดียวกัน ถ้า "ขนาดกระดาษ + ชุดพารามิเตอร์ที่แสดงผลจริง (ไม่นับที่ Hidden)"
        //   ตรงกันทุกตัว แม้ label/ตำแหน่ง/ฟอนต์จะต่างกันเล็กน้อยก็ไม่ถือเป็นเหตุแยก
        //
        //   ยังคงแยกไว้ ถ้ามีสิ่งที่ "เห็นผลจริงตอนพิมพ์" ต่างกัน เช่น
        //   - ตรา/เครื่องหมายที่แสดงจริง (Template 2 พิมพ์คำว่า "KT" จริง ส่วน 21 มีข้อความ
        //     "(Sandvik)" แต่ Hidden=true ไม่เคยพิมพ์ออกมา - จึงยังแยกกันเพราะ 2 มีของที่ 21 ไม่มี)
        //   - ช่องข้อมูลที่แสดง/ไม่แสดงต่างกันจริง (เช่น Template 4 มีช่อง Tiso แสดงจริง,
        //     18/25 มีช่อง Plc, 19/20 ไม่แสดง TDocName, 22 ไม่แสดง POilContent,
        //     23/30 ไม่แสดง PDatePrintAndCopyNum, 30 แสดง TLogo เพิ่มด้วย)
        //   - ขนาดกระดาษต่างกัน (A4 21x29.7cm ขอบ 1.143cm ทุกด้าน กับใบสลิป 8x5.5in ขอบ 0.2in)
        //     ไม่รวมข้ามขนาดกระดาษเด็ดขาด แม้พารามิเตอร์ที่เหลือจะเหมือนกัน
        //
        // ตรวจด้วยการอ่าน XML จริงทุกไฟล์ (ไม่ใช่เดา) หาพารามิเตอร์ที่ประกาศ, Textbox ที่ผูกกับ
        // Parameters!X.Value พร้อมเช็ค Visibility/Hidden ของแต่ละกล่อง, และ diff ข้อความคงที่
        // (<Value> ที่ไม่ใช่ parameter) ทีละคู่ในกลุ่มที่ชุดพารามิเตอร์ตรงกัน - ไม่มีไฟล์ไหนใช้รูปภาพ
        // (Image element) เป็นตรา/โลโก้เลยสักไฟล์ ทุกอย่างเป็นข้อความล้วน
        //
        // ผลรวมกลุ่ม 4 ชุด (25 เหลือ 16 แบบ ครอบคลุม 193 จาก 193 branch เท่าเดิม 100%):
        //   3  ดูดกลืน  5, 12, 14, 15, 16, 17   (A4, พารามิเตอร์ที่แสดงจริงตรงกันทุกตัว 31 ตัว
        //                                        ส่วนต่างทั้งหมดเป็นข้อความ Hidden=true)
        //   6  ดูดกลืน  24                       (ใบสลิป NSM, ไฟล์ .rdlc ต่างกันแค่ ZIndex)
        //   9  ดูดกลืน  28                       (ใบสลิป SURAT, พิมพ์ออกมาเหมือนกันทุกตัวอักษร)
        //  19  ดูดกลืน  20                       (A4 JOB/T1/NSM "clean", พารามิเตอร์+ข้อความเหมือนกัน)
        //
        // (2026, เรียงเลขใหม่): เดิมเลขเทมเพลตคงตามเลขไฟล์ .rdlc ต้นทาง (ไม่ต่อเนื่อง เพราะบางแบบ
        // ถูกดูดกลืนออกไปแล้ว) ผู้ใช้ขอให้เรียงใหม่เป็น 1-16 ต่อเนื่องกันเพื่อความง่ายในการเลือก
        // ผลคือ config เดิมของแต่ละหน่วยงาน (ที่ชี้เลขเก่า) จะไม่ตรงกับแบบฟอร์มเดิมอีกต่อไป -
        // ต้องไปตั้งค่าเลข Template ใหม่ในเครื่องที่ติดตั้งไว้แล้วทุกเครื่องตามตารางนี้
        //
        //  เลขใหม่  เลขเก่า  ที่มา (branch ตัวแทน)                    branch  หมายเหตุ
        //    1        2     KT_Blue_11/03/25 (+_CCom)                    2    KT (ใบสลิป, มีตรา KT แสดงจริง)
        //    2        3     19/09new_request_before_match             148    T1/T4/Uni/DO/39/SRD/Uni-auto-update/
        //                                                                     39-Pink/add_lc รวมกัน (กลุ่มใหญ่สุด, A4)
        //    3        4     CTM_Blue                                     3    CTM สาขา Blue (A4 + ช่อง Tiso แสดงจริง)
        //    4        6     NSM_Blue_11/03/25 (+Auto_update/OLD_NSM)     4    NSM รวมกัน (ใบสลิป)
        //    5        9     FT_ST_SURAT_STP_2025 (+SURAT_STP_2025)       4    SURAT/KRABI รวมกัน (ใบสลิป มีช่องหมายเหตุ)
        //    6       13     Blue_Uni_03/03/25                            7    Uni รุ่นเก่า (A4 + ตรา Sandvik แสดงจริง)
        //    7       18     Blue_add_lc (+Pink_add_lc)                   5    ตระกูล "add_lc" มีช่อง Plc (A4)
        //    8       19     JOB_Blue (+Blue_T1_clean)                    9    JOB / T1 / NSM รุ่น clean รวมกัน (A4)
        //    9       21     KT_Blue_03/03/25 (+TYM_Blue_new_11/03/25)    2    KT/TYM รุ่นเก่า (ใบสลิป, ตรา (Sandvik) ซ่อนอยู่)
        //   10       22     39_Blue (=39_Pink เนื้อหาเหมือนกันทุกไบต์)   2    39 รุ่นก่อน "New" (A4)
        //   11       23     M3_version (+master)                         2    M3 (A4)
        //   12       25     Blue_add_lc_Uni_30/04/24                     1    add_lc + Uni รวมกัน มีช่อง Plc + Sandvik (A4)
        //   13       26     KRD_Blue_new_11/03/25                        1    KRD (ใบสลิป)
        //   14       27     KT_Blue                                      1    KT รุ่นแรกสุด (ใบสลิป, ไม่แสดง TDocName)
        //   15       29     TYM_Blue_Auto_update_01/08/2026              1    TYM อัปเดตอัตโนมัติ (ใบสลิป)
        //   16       30     UNI_version                                  1    Uni รุ่นแรกสุด (A4, ไม่แสดง PDatePrintAndCopyNum
        //                                                                      แต่แสดง TLogo)
        //
        // ชื่อไฟล์ .rdlc / ชื่อ embedded resource ยังใช้เลขเดิมตามไฟล์จริง (ResourceName ด้านล่าง)
        // มีแค่ Number/DisplayName ที่เปลี่ยนเป็นเลขใหม่ - ไม่ต้องเปลี่ยนชื่อไฟล์ในโปรเจกต์
        //
        // รวม 193 branch ครบทุกอัน (148+7+9+5+3+4+2+2+4+2+2+1+1+1+1+1 = 193)
        //
        // (2026, รอบก่อนหน้า): เพิ่มช่อง "รายละเอียดหิน" (PStoneDesc) ให้ครบทุกแบบที่ยังไม่มี -
        // เดิมมีแค่ 4 แบบที่พิมพ์ค่านี้จริง (9, 12 เดิม, 20 เดิม, 29) ที่เหลือประกาศพารามิเตอร์ไว้เฉย ๆ
        // หรือไม่มีเลย จึงพิมพ์ข้อมูลนี้ไม่ออกทั้งที่หน้าจอส่งค่ามาให้เสมอ (ดู FPrint.cs)
        // กล่องข้อความวางต่อจาก PStoneType เดิม ใบสลิปบางแบบพื้นที่แน่น อาจซ้อนทับกล่องข้อความที่ซ่อน
        // ไว้เล็กน้อย (ไม่กระทบการแสดงผล) ควรเปิดดูใน Report Designer เพื่อขยับให้สวยงามอีกทีถ้าต้องการ
        //
        // หมายเหตุสำคัญเรื่องคำว่า (Sandvik):
        // ข้อความนี้ไม่ได้อยู่ในไฟล์รายงาน แต่โค้ดส่งเข้าไปตอนรันผ่านพารามิเตอร์ TLogo
        // (preparePrint ตั้ง Company.TLogo = "(Sandvik)" สำหรับโหมด 1 และ 3 ส่วนโหมด 2 ตั้งเป็นช่องว่าง)
        // ไฟล์รายงานมีแค่ =Parameters!TLogo.Value จึงค้นด้วยการหาคำว่า Sandvik ในไฟล์ .rdlc ไม่เจอ
        // ต้องดูว่าช่อง TLogo ถูก Hidden ไว้หรือไม่ในแต่ละไฟล์แทน
        //
        // ส่วนรหัสแบบฟอร์ม FM-... ถูก Hidden = true ในทุกไฟล์ที่ตรวจ จึงไม่เคยพิมพ์ออกมา
        //
        // ขอบกระดาษของแบบ A4 ใช้ค่าเดิมที่โปรแกรมใช้อยู่ (0.46/0.46/0.60/0.30 นิ้ว)
        // ซึ่งเป็นค่าที่ปรับไว้กับเครื่องพิมพ์จริง ไม่ใช่ค่าใน .rdlc - คงไว้เพื่อไม่ให้งานพิมพ์เดิมเปลี่ยน
        // (ยืนยันแล้วว่า .rdlc ของทุกแบบ A4 ในตารางนี้ประกาศขอบในไฟล์เท่ากันหมดคือ 1.143cm ทุกด้าน
        //  ซึ่งใกล้เคียง 0.46in ที่ใช้อยู่ - ไม่ใช่เหตุผลที่ทำให้พิมพ์ต่างกัน)
        // ส่วนใบสลิปใช้ขอบ 0.2 นิ้วตามที่ระบุใน .rdlc ของมันเอง (ทุกแบบใบสลิปตรงกันหมด)
        private const double A4W = 8.27, A4H = 11.69;
        private const double SlipW = 8.00, SlipH = 5.50;

        // ชื่อที่แสดงมีรหัสหน่วยงานกำกับ เพื่อให้เลือกได้ถูกโดยไม่ต้องเปิดดูไฟล์
        // (T1, T4, Uni, NSM ฯลฯ คือรหัสหน่วยงาน ไม่ใช่เลขเทมเพลต)
        private static readonly TemplateInfo[] Templates =
        {
            new TemplateInfo( 1, "Template 1 - KT (ใบสลิป มีตรา KT)",                     "SerialPortListener.ReportMain_Template2.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 2, "Template 2 - T1 / T4 / Uni / DO / 39 / SRD (A4)",       "SerialPortListener.ReportMain_Template3.rdlc",  A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 3, "Template 3 - CTM สาขา Blue (A4 + ช่องรหัสเอกสาร)",      "SerialPortListener.ReportMain_Template4.rdlc",  A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 4, "Template 4 - NSM (ใบสลิป)",                             "SerialPortListener.ReportMain_Template6.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 5, "Template 5 - SURAT / KRABI (ใบสลิป มีช่องหมายเหตุ)",    "SerialPortListener.ReportMain_Template9.rdlc",  SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo( 6, "Template 6 - Uni รุ่นเก่า (A4 + ตรา Sandvik)",          "SerialPortListener.ReportMain_Template13.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 7, "Template 7 - แบบมีเลข LC (A4)",                         "SerialPortListener.ReportMain_Template18.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 8, "Template 8 - JOB / T1 / NSM รุ่น clean (A4)",           "SerialPortListener.ReportMain_Template19.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo( 9, "Template 9 - KT / TYM รุ่นเก่า (ใบสลิป)",               "SerialPortListener.ReportMain_Template21.rdlc", SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo(10, "Template 10 - 39 รุ่นก่อน New (A4)",                    "SerialPortListener.ReportMain_Template22.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(11, "Template 11 - M3 (A4)",                                 "SerialPortListener.ReportMain_Template23.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(12, "Template 12 - แบบมีเลข LC + Uni (A4)",                  "SerialPortListener.ReportMain_Template25.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
            new TemplateInfo(13, "Template 13 - KRD (ใบสลิป)",                            "SerialPortListener.ReportMain_Template26.rdlc", SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo(14, "Template 14 - KT รุ่นแรกสุด (ใบสลิป)",                  "SerialPortListener.ReportMain_Template27.rdlc", SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo(15, "Template 15 - TYM อัปเดตอัตโนมัติ (ใบสลิป)",            "SerialPortListener.ReportMain_Template29.rdlc", SlipW, SlipH, 0.20, 0.20, 0.20, 0.20),
            new TemplateInfo(16, "Template 16 - Uni รุ่นแรกสุด (A4)",                     "SerialPortListener.ReportMain_Template30.rdlc", A4W, A4H, 0.46, 0.46, 0.60, 0.30),
        };

        public const int DefaultTemplate = 2;   // แบบที่ใช้มากที่สุด (148 จาก 193 branch หลังรวมกลุ่ม) - เดิมคือเลข 3 ก่อนเรียงใหม่

        // เก็บ config ไว้ใน AppData เหมือน config_port.txt และ configs_backup.txt
        // เพราะโฟลเดอร์ที่ติดตั้งโปรแกรมเขียนไฟล์ไม่ได้ถ้าไม่ใช่ admin
        private static readonly string ConfigPath =
            Path.Combine(Utils.AppDataDir, "config_reportmain.txt");

        private const string TemplateKey = "Template";
        private const string MainCompKeyPrefix = "MainComp";

        // ชื่อบริษัทบนหัวกระดาษ (cbbMainComp) - ปรับได้ในไฟล์ config โดยไม่ต้องคอมไพล์ใหม่
        // รายการนี้รวบรวมจากหัวกระดาษของทุก branch ที่ลงท้ายด้วย 11/03/25 (20 commit ที่ไม่ซ้ำกัน)
        // บวกกับรายการใน cbbMainComp ของ KT_Blue_11/03/25_CCom ซึ่งเป็นต้นทางของตัวเลือกนี้
        // ตัวอักษรย่อท้ายบรรทัดคือหน่วยงานที่ใช้ชื่อนั้น
        private static readonly string[] DefaultMainCompanies =
        {
            "บริษัท 39 ศิลาทอง จำกัด",                        // 39
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด",                   // Blue, Pink, SRD
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด (1169)",            // Blue
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด(Sandvik) 1169",     // Blue
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด nonvat",            // Pink
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด(Sandvik) nonvat",   // Pink
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด สาขา 00003",        // SRD
            "บริษัท ศิลาชัยสุราษฎร์ จำกัด สาขา 00003 nonvat", // SRD
            "บริษัท ครีเอทีฟ มิเนอรัล จำกัด",                 // CTM
            "บริษัท ครีเอทีฟ มิเนอรัล จำกัด (non vat)",       // CTM
            "บริษัท โชคพนาไมนิ่ง จำกัด",                      // KRD, KT, NSM, TYM
            "บริษัท โชคพนาไมนิ่ง จำกัด (เหมืองแร่คุณธวัช)",   // NSM
            "บริษัท โชคพนาไมนิ่ง จำกัด สาขาที่ 00003",        // KRD
            "บริษัท โชคพนาไมนิ่ง จำกัด สาขาที่00007",         // TYM
            "บริษัท โชคพนา (2512) จำกัด",                     // KT_CCom
            "บริษัท เจ.โอ.บี.คอนสตรัคชั่น จำกัด",             // JOB, NSM
            "บริษัท วี ร็อค ซัพพลาย จำกัด",                   // JOB, KRD, KT, NSM, TYM
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
        /// จะคืนค่าเริ่มต้น (Template 2) เสมอ ไม่โยน exception
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
        /// เช่น Template 1 ต้องใช้ PScoopName ส่วน Template 3 ต้องใช้ Tiso
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

                // แบบหัวกระดาษบิลที่ผู้ใช้เลือกไว้ มีสิทธิ์แทนค่าชื่อบริษัท/ที่อยู่/โทรศัพท์
                // ทำที่นี่จุดเดียว ทั้งหน้าพรีวิวและการพิมพ์ตรงจึงได้หัวกระดาษเดียวกันเสมอ
                string billOverride = BillHeader.GetOverride(p.Name);
                if (billOverride != null)
                {
                    result.Add(new Microsoft.Reporting.WinForms.ReportParameter(p.Name, billOverride));
                    continue;
                }

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
