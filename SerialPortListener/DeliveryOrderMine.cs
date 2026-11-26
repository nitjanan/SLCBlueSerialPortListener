using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    // ==================================================================================================
    // ใบ DO แยกตามเหมือง (ตาม do_database.xlsx) : column ที่เกี่ยวข้องกันระหว่าง k2m / Django / Windows
    // --------------------------------------------------------------------------------------------------
    //  ความหมาย              | k2m API (quo01)           | Django (web app)                       | Windows (local DB นี้)
    //  ---------------------|---------------------------|----------------------------------------|------------------------------
    //  บริษัทที่ออกใบสั่งขาย    | company (query + field)   | BaseCompany.aff_so -> BaseAffiliatedCompany.name | ไม่เก็บ: FetchAffAsync() -> AffInfo.aff
    //  เหมือง                | mine_code                 | DeliveryOrder.comp_code (= BaseCompany.code) | delivery_order.comp_code
    //                       |                           |                                        | base_api id=2 comp_code = เหมืองของตาชั่งนี้
    //                       |                           |                                        | base_api id=1 comp_code = เหมืองเดียวกัน (sync กับ Django)
    //  เลขที่ DO              | docNo                     | DeliveryOrder.doc_no                   | delivery_order.doc_no
    //  วันที่ส่ง              | deliveryDate              | DeliveryOrder.delivery_date            | delivery_order.delivery_date
    //  หน้างาน               | deliveryCode (หลัง "___")  | DeliveryOrder.site_id                  | delivery_order.site_id
    //                       | deliveryLocation          | DeliveryOrder.site_name                | delivery_order.site_name
    //  จำนวนรถ               | carCompany / carCustomer  | DeliveryOrder.car_company / car_customer | delivery_order.car_company / car_customer
    //  ลูกค้า                 | customerCode/Name/Address | DeliveryOrder.customer_code/_name/_address | delivery_order.customer_code/_name/_address
    //  สินค้า                 | productCode / productName | DeliveryOrder.product_code / product_name | delivery_order.product_code / product_name
    //  ปริมาณ                | qty / unitName            | DeliveryOrder.qty / unit_name          | delivery_order.qty / unit_name
    //  อื่น ๆ                 | saleName / note / status  | DeliveryOrder.sale_name / note / status | delivery_order.sale_name / note / status
    //  ประเภทการส่ง           | deliveryType              | (ไม่เก็บ)                               | delivery_order.delivery_type
    //  การจ่าย                | creditType ("cash" = เงินสด, "credit-day" = เงินเชื่อ) | DeliveryOrder.credit_type | delivery_order.credit_type
    // --------------------------------------------------------------------------------------------------
    // k2m ส่ง DO ของทุกเหมืองที่ใช้บริษัทที่ออกใบสั่งขายเดียวกันมาพร้อมกัน ตาชั่งต้องเก็บ/แสดงเฉพาะเหมืองของตัวเอง
    // ฝั่ง Django ดูตารางเดียวกันเหนือ insertDeliveryFromApiK2M() ใน weightapp/views.py
    // ==================================================================================================
    public static class DeliveryOrderMine
    {
        // response ของ Django GET /company/aff/api/by/comp/?comp_code=<เหมือง>
        public class AffInfo
        {
            public string comp_code { get; set; }   // เหมือง = BaseCompany.code
            public string aff { get; set; }         // บริษัทที่ออกใบสั่งขาย = BaseCompany.aff_so.name -> ส่งเป็น k2m company
            public List<string> mines { get; set; } // เหมืองทั้งหมดที่ aff_so เดียวกัน (ใช้ใน ResolveMine ตอน mine_code ว่าง)

            // true = ถาม web app ไม่สำเร็จ (เช่น server ยังไม่มี endpoint นี้) ใช้ค่าแบบเวอร์ชันเดิมแทน
            [JsonIgnore]
            public bool IsFallback { get; private set; }

            // พฤติกรรมเดิมก่อนมี /company/aff/ : ส่ง k2m company = comp_code ของ base_api id = 2 และไม่กรองตามเหมือง
            public static AffInfo Fallback(string mineCode)
            {
                return new AffInfo
                {
                    comp_code = mineCode,
                    aff = mineCode,
                    mines = new List<string> { mineCode },
                    IsFallback = true
                };
            }
        }

        private static bool compCodeColumnReady;

        private static void EnsureColumn(Datalayer dl, string column, string type)
        {
            using (OdbcCommand check = (OdbcCommand)dl.sqlConn().CreateCommand())
            {
                check.CommandText = @"SELECT COUNT(*) FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'delivery_order' AND column_name = ?";
                check.Parameters.AddWithValue("", column);
                if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                {
                    using (OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                    {
                        cmd.CommandText = "ALTER TABLE public.delivery_order ADD COLUMN " + column + " " + type;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // local DB เดิมไม่มีคอลัมน์ comp_code / credit_type ใน delivery_order: เพิ่มให้เองครั้งแรกที่ใช้
        public static void EnsureCompCodeColumn(Datalayer dl)
        {
            if (compCodeColumnReady)
                return;

            dl.connect();
            try
            {
                // PostgreSQL 9.5 ยังไม่มี ADD COLUMN IF NOT EXISTS (มีตั้งแต่ 9.6) จึงเช็คจาก information_schema เอง
                EnsureColumn(dl, "comp_code", "varchar(20)");
                // credit_type = k2m creditType ("cash" = เงินสด, "credit-day" = เงินเชื่อ) ใช้ติ๊ก "จ่าย" ที่ MainForm
                EnsureColumn(dl, "credit_type", "varchar(20)");
                // mine_unknown = 1 : k2m ไม่ส่ง mine_code มา และสังกัดมีหลายเหมือง จึงเดาเหมืองไม่ได้
                // ยังเก็บไว้ให้เหมืองนี้เห็น แต่หน้า TableDeliveryOrder ไฮไลต์สีเหลือง + ถามยืนยันก่อนเลือก
                EnsureColumn(dl, "mine_unknown", "smallint NOT NULL DEFAULT 0");
                compCodeColumnReady = true;
            }
            finally
            {
                dl.close();
            }
        }

        // เหมืองของตาชั่งนี้ = base_api id = 2 comp_code
        public static string GetOwnMineCode(Datalayer dl)
        {
            dl.connect();
            try
            {
                using (OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                {
                    cmd.CommandText = "SELECT comp_code FROM base_api WHERE id = 2";
                    object val = cmd.ExecuteScalar();
                    return val == null || val == DBNull.Value ? "" : val.ToString().Trim();
                }
            }
            finally
            {
                dl.close();
            }
        }

        // ถาม web app ว่าเหมืองนี้อยู่สังกัดไหน (login ด้วย base_api id = 1)
        // throw เมื่อเชื่อมต่อ / login / หาสังกัดไม่สำเร็จ
        public static async Task<AffInfo> FetchAffAsync(Datalayer dl, string mineCode)
        {
            string baseUrl = "", username = "", password = "";

            dl.connect();
            try
            {
                using (OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                {
                    cmd.CommandText = "SELECT url, username, password FROM base_api WHERE id = 1";
                    using (OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            baseUrl = reader["url"].ToString();
                            username = reader["username"].ToString();
                            password = reader["password"].ToString();
                        }
                    }
                }
            }
            finally
            {
                dl.close();
            }

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                string loginJson = JsonConvert.SerializeObject(new { username = username, password = password });
                HttpResponseMessage jwtResponse = await client.PostAsync(
                    $"{baseUrl}/jwt/create/", new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (!jwtResponse.IsSuccessStatusCode)
                    throw new Exception("JWT ERROR : " + await jwtResponse.Content.ReadAsStringAsync());

                string accessToken = JObject.Parse(await jwtResponse.Content.ReadAsStringAsync())["access"]?.ToString();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(
                    $"{baseUrl}/company/aff/api/by/comp/?comp_code={Uri.EscapeDataString(mineCode)}");
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"ไม่พบสังกัดของเหมือง {mineCode} : {body}");

                AffInfo info = JsonConvert.DeserializeObject<AffInfo>(body);
                if (info == null || string.IsNullOrWhiteSpace(info.aff))
                    throw new Exception($"ไม่พบสังกัดของเหมือง {mineCode}");
                if (info.mines == null)
                    info.mines = new List<string>();
                return info;
            }
        }

        // ผลการคัด DO จาก k2m ว่าเป็นของเหมืองนี้ไหม
        public enum MineMatch
        {
            Own,        // เป็นของเหมืองนี้
            Unknown,    // k2m ไม่ระบุเหมือง และสังกัดมีหลายเหมือง : เก็บไว้ แต่ติดธง mine_unknown
            OtherMine   // เป็นของเหมืองอื่น : ไม่เก็บ
        }

        public static MineMatch Classify(string mineCode, AffInfo aff, string ownMine)
        {
            // fallback (ไม่รู้สังกัด) : เก็บทุกแถวเหมือนเวอร์ชันเดิม
            if (aff == null || aff.IsFallback)
                return MineMatch.Own;
            string itemMine = ResolveMine(mineCode, aff.mines);
            if (itemMine == null)
            {
                // ไม่ระบุเหมือง: เก็บเฉพาะที่เหมืองหลักของสังกัด (aff SLC -> ตาชั่ง SLC เท่านั้น ไม่ลง UNI / SRD)
                // สังกัดที่ไม่มีเหมืองหลัก เก็บทุกเหมืองเหมือนเดิม
                string home = UnknownMineHome(aff);
                if (home == null || string.Equals(home, ownMine, StringComparison.OrdinalIgnoreCase))
                    return MineMatch.Unknown;
                return MineMatch.OtherMine;
            }
            return string.Equals(itemMine, ownMine, StringComparison.OrdinalIgnoreCase)
                ? MineMatch.Own : MineMatch.OtherMine;
        }

        // เหมืองที่เก็บใบ DO ที่ k2m ไม่ระบุเหมือง = เหมืองที่ code ตรงกับชื่อสังกัด ไม่มีคืน null
        // (ให้ผลเหมือน unknown_mine_home() ฝั่ง web app)
        public static string UnknownMineHome(AffInfo aff)
        {
            if (aff == null || aff.mines == null || string.IsNullOrWhiteSpace(aff.aff))
                return null;
            foreach (string code in aff.mines)
            {
                if (!string.IsNullOrWhiteSpace(code)
                    && string.Equals(code.Trim(), aff.aff.Trim(), StringComparison.OrdinalIgnoreCase))
                    return code.Trim();
            }
            return null;
        }

        // เหมืองของแถว DO จาก k2m: ใช้ mine_code ถ้ามี ถ้าว่างและสังกัดมีเหมืองเดียวใช้เหมืองนั้น ไม่งั้นคืน null
        // (ให้ผลเหมือน resolve_k2m_mine_code() ฝั่ง web app)
        public static string ResolveMine(string mineCode, List<string> affMines)
        {
            if (!string.IsNullOrWhiteSpace(mineCode))
                return mineCode.Trim();
            if (affMines != null && affMines.Count == 1)
                return affMines[0];
            return null;
        }
    }
}
