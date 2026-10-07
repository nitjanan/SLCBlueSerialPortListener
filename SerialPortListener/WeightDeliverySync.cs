using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    // ดึง weight_delivery จาก web app (ตัวกลาง) ลง local DB
    // ตาชั่งแต่ละตัว (Blue = truck_m, Pink = truck_s) มี local DB แยกกัน แต่ส่งรายการไปที่ web app ตัวเดียวกัน
    // ถ้าไม่ดึงกลับมาตามวันที่ที่ต้องการดู ตาชั่งแต่ละตัวจะเห็นแค่ข้อมูลของวันที่เคยดึงไว้ล่าสุดเท่านั้น
    public static class WeightDeliverySync
    {
        // throw เมื่อเชื่อมต่อ API / login ไม่สำเร็จ ให้ผู้เรียกตัดสินใจเองว่าจะแจ้งเตือนอย่างไร
        public static async Task PullAsync(Datalayer dl, IEnumerable<DateTime> dates)
        {
            string baseUrl = "", username = "", password = "", compCode = "";

            OdbcCommand apiCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            apiCommand.CommandText = "SELECT url, username, password, comp_code FROM base_api WHERE id = 1";
            dl.connect();
            try
            {
                using (OdbcDataReader reader = apiCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        baseUrl = reader["url"].ToString();
                        username = reader["username"].ToString();
                        password = reader["password"].ToString();
                        compCode = reader["comp_code"].ToString();
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

                var noDateParse = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

                dl.connect();
                try
                {
                    foreach (DateTime date in dates.Select(d => d.Date).Distinct())
                    {
                        string dateStr = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        string apiUrl = $"{baseUrl}/weightdelivery/summary/api/by/comp/?comp_code={compCode}&date={dateStr}&page=1";

                        while (!string.IsNullOrEmpty(apiUrl))
                        {
                            HttpResponseMessage apiResponse = await client.GetAsync(apiUrl);
                            if (!apiResponse.IsSuccessStatusCode)
                                throw new Exception("API ERROR : " + await apiResponse.Content.ReadAsStringAsync());

                            string json = await apiResponse.Content.ReadAsStringAsync();
                            List<MainForm.WeightDelivery> orders;
                            apiUrl = null;

                            if (json.TrimStart().StartsWith("["))
                            {
                                orders = JsonConvert.DeserializeObject<List<MainForm.WeightDelivery>>(json, noDateParse);
                            }
                            else
                            {
                                var pageObj = JsonConvert.DeserializeObject<MainForm.DRFPaginationResponse<MainForm.WeightDelivery>>(json, noDateParse);
                                orders = pageObj?.results ?? pageObj?.data;
                                if (orders != null && orders.Count > 0)
                                    apiUrl = pageObj?.next;
                            }

                            if (orders == null)
                                break;

                            foreach (var item in orders)
                                Upsert(dl, item);
                        }
                    }
                }
                finally
                {
                    dl.close();
                }
            }
        }

        private static void Upsert(Datalayer dl, MainForm.WeightDelivery item)
        {
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = @"
                INSERT INTO weight_delivery
                (
                    weight_id,
                    weight_doc_id,
                    delivery_date,
                    bws,
                    comp_code,
                    do_doc_no,
                    carry_type_name,
                    weight_ton,
                    weight_q,
                    unit_name,
                    is_cancel
                )
                VALUES
                (
                    ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
                )
                ON CONFLICT (weight_id)
                DO UPDATE SET
                    weight_doc_id = EXCLUDED.weight_doc_id,
                    delivery_date = EXCLUDED.delivery_date,
                    bws = EXCLUDED.bws,
                    comp_code = EXCLUDED.comp_code,
                    do_doc_no = EXCLUDED.do_doc_no,
                    carry_type_name = EXCLUDED.carry_type_name,
                    weight_ton = EXCLUDED.weight_ton,
                    weight_q = EXCLUDED.weight_q,
                    unit_name = EXCLUDED.unit_name,
                    is_cancel = EXCLUDED.is_cancel
            ";

            pgCommand.Parameters.AddWithValue("", item.weight_id);
            pgCommand.Parameters.AddWithValue("", item.weight_doc_id);
            DateTime? wdDate = DbDate.Parse(item.delivery_date, item.do_doc_no);
            pgCommand.Parameters.Add("", OdbcType.Date).Value =
                wdDate.HasValue ? (object)wdDate.Value : DBNull.Value;
            pgCommand.Parameters.AddWithValue("", item.bws);
            pgCommand.Parameters.AddWithValue("", item.comp_code);
            pgCommand.Parameters.AddWithValue("", item.do_doc_no);
            pgCommand.Parameters.AddWithValue("", item.carry_type_name);
            pgCommand.Parameters.AddWithValue("", item.weight_ton);
            pgCommand.Parameters.AddWithValue("", item.weight_q);
            pgCommand.Parameters.AddWithValue("", item.unit_name);
            pgCommand.Parameters.AddWithValue("", item.is_cancel);

            pgCommand.ExecuteNonQuery();
        }
    }
}
