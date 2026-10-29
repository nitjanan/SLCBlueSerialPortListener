using System;
using System.Data.Odbc;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    // ดึง UserScale (ผู้ชั่ง) จาก web app ลง table users ใน local
    // เดิมดึงได้เฉพาะหลัง login แล้ว (ปุ่ม Refresh / ดาวน์โหลดการตั้งค่า) ผู้ชั่งที่เพิ่ม/แก้รหัสผ่านบน web
    // จึง login ไม่ได้จนกว่าจะมีคนอื่น login เข้าไปกดดาวน์โหลดให้ หน้า Login จึงเรียก PullAsync ก่อนแจ้งว่ารหัสผิด
    public static class UserScaleSync
    {
        // throw เมื่อเชื่อมต่อ API / login ไม่สำเร็จ
        public static async Task<int> PullAsync(Datalayer dl)
        {
            string baseUrl = "", username = "", password = "", vStampDt = null;

            dl.connect();
            try
            {
                OdbcCommand apiCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                apiCommand.CommandText = "SELECT url, username, password FROM base_api WHERE id = 1";
                using (OdbcDataReader reader = apiCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        baseUrl = reader["url"].ToString();
                        username = reader["username"].ToString();
                        password = reader["password"].ToString();
                    }
                }

                OdbcCommand stampCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                stampCommand.CommandText = "SELECT v_stamp_dt FROM v_stamp WHERE table_name = 'users'";
                object raw = stampCommand.ExecuteScalar();
                if (raw is DateTime dt)
                    vStampDt = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                else if (raw != null && raw != DBNull.Value)
                    vStampDt = raw.ToString();
            }
            finally
            {
                dl.close();
            }

            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
            {
                string loginJson = JsonConvert.SerializeObject(new { username, password });
                HttpResponseMessage jwtResponse = await client.PostAsync(
                    $"{baseUrl}/jwt/create/", new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (!jwtResponse.IsSuccessStatusCode)
                    throw new Exception("JWT ERROR : " + (int)jwtResponse.StatusCode);

                string accessToken = JObject.Parse(await jwtResponse.Content.ReadAsStringAsync())["access"]?.ToString();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                string url = $"{baseUrl}/userScale/api/vStamp/{Uri.EscapeDataString(vStampDt ?? "2000-01-01 00:00:00")}/";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("API ERROR : " + (int)response.StatusCode);

                JArray items = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());
                if (items == null)
                    return 0;

                int count = 0;
                dl.connect();
                try
                {
                    foreach (JObject item in items)
                    {
                        Upsert(dl, item);
                        count++;
                    }
                }
                finally
                {
                    dl.close();
                }
                return count;
            }
        }

        // username ในระบบนี้ map มาจาก scale_id ของ API (ไม่ใช่ username ของ API) ; users_id เป็น auto ฝั่ง DB
        public static void Upsert(Datalayer dl, JObject d)
        {
            string scaleId = d["scale_id"]?.Type == JTokenType.Null ? null : d["scale_id"]?.ToString().Trim();

            if (!string.IsNullOrEmpty(scaleId))
            {
                OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
                // รหัสผ่าน/สิทธิ์ ว่างจาก web = ยังไม่ได้ตั้ง ให้คงค่าเดิมในเครื่องไว้ ไม่ทับเป็นค่าว่างจน login ไม่ได้
                cmd.CommandText = @"
                    INSERT INTO public.users(""firstname"", ""username"", ""password"", ""permission"")
                    VALUES (?, ?, NULLIF(?, ''), NULLIF(?, ''))
                    ON CONFLICT (""username"")
                    DO UPDATE SET ""firstname"" = EXCLUDED.""firstname"",
                    ""password"" = COALESCE(EXCLUDED.""password"", public.users.""password""),
                    ""permission"" = COALESCE(EXCLUDED.""permission"", public.users.""permission"");";
                cmd.Parameters.AddWithValue("", Val(d, "scale_name"));
                cmd.Parameters.AddWithValue("", scaleId);
                cmd.Parameters.AddWithValue("", Val(d, "password"));
                cmd.Parameters.AddWithValue("", Val(d, "permission"));
                cmd.ExecuteNonQuery();
            }

            OdbcCommand stamp = (OdbcCommand)dl.sqlConn().CreateCommand();
            stamp.CommandText = "UPDATE public.v_stamp SET v_stamp_dt = ? WHERE table_name = 'users'";
            stamp.Parameters.AddWithValue("", ParseVStamp(d));
            stamp.ExecuteNonQuery();
        }

        private static object Val(JObject d, string key)
        {
            JToken token = d[key];
            if (token == null || token.Type == JTokenType.Null)
                return DBNull.Value;
            return token.ToString();
        }

        private static readonly string[] VStampFormats =
        {
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss",
        };

        // API ส่ง v_stamp เป็น ISO 8601 ต้อง parse เป็น DateTime ก่อน ห้ามส่ง string ดิบ (Postgres ตีความตาม DateStyle)
        private static object ParseVStamp(JObject d)
        {
            string raw = d["v_stamp"]?.Type == JTokenType.Null ? null : (string)d["v_stamp"];
            if (string.IsNullOrEmpty(raw))
                return DBNull.Value;
            if (DateTime.TryParseExact(raw, VStampFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                return dt;
            return raw;
        }
    }
}
