using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    // ส่งข้อมูลชั่งจากคิว sync_outbox ขึ้นตาราง mirror บน web app ตลอดวัน
    // ไว้กู้ข้อมูลที่ยังไม่ได้อัพโหลดตอนเย็น เมื่อ local DB เสีย (ฟ้าผ่า/ไฟดับ)
    // ไม่ใช่การอัพโหลด weight จริง ขั้นตอนตอนเย็น (ucBackup.btULWeight) ยังเหมือนเดิม
    //
    // คิวถูกเติมโดย trigger ใน DB (sql/2026-11-23-weight-mirror-outbox.sql) จึงไม่ต้องแก้โค้ดบันทึกทุกจุด
    // ส่งไม่ได้ (เน็ตหลุด/server ล่ม) ข้อมูลค้างอยู่ในคิว รอบหน้าส่งใหม่เอง
    //
    // ลดโหลด server: ส่งทุก 3 นาทีเฉพาะตอนมีข้อมูล, รวมเป็นชุด, บีบอัด gzip,
    // เก็บ JWT ไว้ใช้ซ้ำ (login ใหม่เฉพาะตอนได้ 401), สุ่มเวลาเริ่ม 0–60 วินาทีไม่ให้ทุกสถานีส่งพร้อมกัน
    public static class WeightMirrorSync
    {
        // ปิดระบบสำเนา weight_mirror ไว้ก่อน : ไม่ส่งข้อมูล, ไม่สร้าง sync_outbox/trigger, ลบ trigger ที่มีอยู่ออก, ซ่อนปุ่มกู้ข้อมูล
        // เปิดกลับ = เปลี่ยนเป็น true แล้ว build ใหม่ (ตอนเปิดโปรแกรม MigrationRunner จะสร้าง trigger กลับให้เอง)
        public static readonly bool Enabled = false;

        private const string MirrorEndpoint = "/weightmirror/api/bulk/";
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(3);
        private const int MaxStartJitterMs = 60000;
        private const int BatchSize = 200;
        private const int MaxBatchesPerRun = 10;   // หลังเน็ตหลุดนาน ไล่ส่งค้างได้ ~2,000 แถวต่อรอบ
        private const int MaxErrorLength = 500;

        internal static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly object TimerLock = new object();
        private static Timer _timer;
        private static int _running;
        private static string _token;
        private static string _tokenBaseUrl;
        private static bool _useGzip = true;

        public static int PendingCount { get; private set; }
        public static DateTime? LastSuccessAt { get; private set; }
        public static string LastError { get; private set; }

        // เรียกจาก thread ของ timer ; ฝั่ง UI ต้อง BeginInvoke เอง
        public static event EventHandler StatusChanged;

        public static void Start()
        {
            if (!Enabled)
                return;
            lock (TimerLock)
            {
                if (_timer != null)
                    return;
                int jitter = new Random().Next(0, MaxStartJitterMs);
                _timer = new Timer(_ => RunSafe(), null, jitter, (int)Interval.TotalMilliseconds);
            }
        }

        public static void Stop()
        {
            lock (TimerLock)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private static void RunSafe()
        {
            // รอบก่อนยังไม่เสร็จ (เน็ตช้า) ไม่ซ้อนรอบ
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
                return;
            try
            {
                RunOnceAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LastError = Truncate(ex.Message);
                Trace.TraceWarning("WeightMirrorSync: " + ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static async Task RunOnceAsync()
        {
            Datalayer dl = new Datalayer();
            if (!dl.connect())
                throw new Exception("เชื่อมต่อฐานข้อมูลไม่ได้");
            try
            {
                OdbcConnection conn = dl.sqlConn();
                // ตาราง/trigger หายระหว่างใช้งาน (เช่นกู้ฐานจาก backup เก่า) สร้างใหม่ให้เลย ไม่ต้องรอเปิดโปรแกรมใหม่
                if (!MigrationRunner.IsWeightMirrorInstalled(conn))
                    MigrationRunner.RunWeightMirrorMigration(conn);

                ApiConfig cfg = ReadConfig(conn);
                if (string.IsNullOrWhiteSpace(cfg.BaseUrl))
                    return;

                for (int i = 0; i < MaxBatchesPerRun; i++)
                {
                    List<OutboxRow> rows = ReadPending(conn);
                    if (rows.Count == 0)
                        break;

                    try
                    {
                        await SendAsync(cfg, rows);
                    }
                    catch (Exception ex)
                    {
                        MarkFailed(conn, rows, ex.Message);
                        throw;
                    }

                    MarkSent(conn, rows);
                    LastSuccessAt = DateTime.Now;
                    LastError = null;

                    if (rows.Count < BatchSize)
                        break;
                }

                using (OdbcCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM sync_outbox WHERE sent_at < now() - interval '7 days'";
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                try { PendingCount = CountPending(dl.sqlConn()); } catch { }
                dl.close();
            }
        }

        internal class ApiConfig
        {
            public string BaseUrl, Username, Password, CompCode, MineCode, StationCode;
        }

        private class OutboxRow
        {
            public long Id;
            public int Rev;
            public string TableName, RowId, Action, Payload, QueuedAt;
        }

        // base_api id = 1 : web app กลาง (login + comp_code) ; id = 2 comp_code = เหมืองของตาชั่งนี้
        internal static ApiConfig ReadConfig(OdbcConnection conn)
        {
            var cfg = new ApiConfig();
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, url, username, password, comp_code FROM base_api WHERE id IN (1, 2)";
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        if (id == 1)
                        {
                            cfg.BaseUrl = reader["url"].ToString().TrimEnd('/');
                            cfg.Username = reader["username"].ToString();
                            cfg.Password = reader["password"].ToString();
                            cfg.CompCode = reader["comp_code"].ToString();
                        }
                        else
                        {
                            cfg.MineCode = reader["comp_code"].ToString();
                        }
                    }
                }
            }

            // รหัสตาชั่ง (แบบเดียวกับ MainForm.findBWS) ตาชั่งหลายตัวใช้ comp_code เดียวกัน ตอนกู้ต้องแยกตัวนี้
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT code FROM base_weight_station WHERE base_weight_station_id = 1";
                cfg.StationCode = (cmd.ExecuteScalar() ?? "").ToString().Trim();
            }
            return cfg;
        }

        private static List<OutboxRow> ReadPending(OdbcConnection conn)
        {
            var rows = new List<OutboxRow>();
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, rev, table_name, row_id, action, payload::text AS payload,
                           to_char(created_at, 'YYYY-MM-DD""T""HH24:MI:SSOF') AS queued_at
                    FROM sync_outbox
                    WHERE sent_at IS NULL
                    ORDER BY id
                    LIMIT " + BatchSize;
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new OutboxRow
                        {
                            Id = Convert.ToInt64(reader["id"]),
                            Rev = Convert.ToInt32(reader["rev"]),
                            TableName = reader["table_name"].ToString(),
                            RowId = reader["row_id"].ToString(),
                            Action = reader["action"].ToString().Trim(),
                            Payload = reader["payload"] == DBNull.Value ? null : reader["payload"].ToString(),
                            QueuedAt = reader["queued_at"].ToString(),
                        });
                    }
                }
            }
            return rows;
        }

        private static int CountPending(OdbcConnection conn)
        {
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM sync_outbox WHERE sent_at IS NULL";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // เทียบ rev ด้วย : ถ้าระหว่างส่งมีการแก้รายการเดิมอีก (rev เพิ่ม) แถวนั้นยังค้างในคิว รอบหน้าส่งเวอร์ชันใหม่
        private static void MarkSent(OdbcConnection conn, List<OutboxRow> rows)
        {
            string keys = string.Join(",", rows.Select(r =>
                "(" + r.Id.ToString(CultureInfo.InvariantCulture) + "," + r.Rev.ToString(CultureInfo.InvariantCulture) + ")"));
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE sync_outbox SET sent_at = now(), last_error = NULL " +
                                  "WHERE (id, rev) IN (VALUES " + keys + ")";
                cmd.ExecuteNonQuery();
            }
        }

        private static void MarkFailed(OdbcConnection conn, List<OutboxRow> rows, string error)
        {
            try
            {
                string ids = string.Join(",", rows.Select(r => r.Id.ToString(CultureInfo.InvariantCulture)));
                using (OdbcCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE sync_outbox SET attempts = attempts + 1, last_error = ? WHERE id IN (" + ids + ")";
                    cmd.Parameters.AddWithValue("", Truncate(error));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("WeightMirrorSync.MarkFailed: " + ex.Message);
            }
        }

        // ค่าในสำเนาต้องตรงกับในเครื่องทุกตัวอักษร : ทศนิยมอ่านเป็น decimal (0.000 ไม่กลายเป็น 0.0)
        // และไม่แปลง string วันที่เป็น DateTime (2026-11-03 ไม่กลายเป็น 2026-11-03T00:00:00)
        private static JToken ParsePayload(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json))
            {
                FloatParseHandling = FloatParseHandling.Decimal,
                DateParseHandling = DateParseHandling.None,
            })
            {
                return JToken.ReadFrom(reader);
            }
        }

        private static async Task SendAsync(ApiConfig cfg, List<OutboxRow> rows)
        {
            var items = new JArray();
            foreach (OutboxRow r in rows)
            {
                items.Add(new JObject
                {
                    ["table_name"] = r.TableName,
                    ["row_id"] = r.RowId,
                    ["action"] = r.Action,
                    ["rev"] = r.Rev,
                    ["queued_at"] = r.QueuedAt,
                    ["payload"] = r.Payload == null ? (JToken)JValue.CreateNull() : ParsePayload(r.Payload),
                });
            }
            var body = new JObject
            {
                ["comp_code"] = cfg.CompCode,
                ["mine_code"] = cfg.MineCode,
                ["station_code"] = cfg.StationCode,
                ["items"] = items,
            };
            byte[] json = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));

            HttpResponseMessage response = await PostAsync(cfg, json, retryOnUnauthorized: true);

            // server ยังไม่รองรับ gzip : ส่งแบบไม่บีบอัดไปก่อน
            if (_useGzip && response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                _useGzip = false;
                response = await PostAsync(cfg, json, retryOnUnauthorized: true);
            }

            if (!response.IsSuccessStatusCode)
            {
                string detail = await response.Content.ReadAsStringAsync();
                throw new Exception("MIRROR API " + (int)response.StatusCode + " : " + ShortErrorBody(detail));
            }
        }

        private static async Task<HttpResponseMessage> PostAsync(ApiConfig cfg, byte[] json, bool retryOnUnauthorized)
        {
            string token = await GetTokenAsync(cfg);

            var request = new HttpRequestMessage(HttpMethod.Post, cfg.BaseUrl + MirrorEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            ByteArrayContent content = new ByteArrayContent(_useGzip ? Gzip(json) : json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
            if (_useGzip)
                content.Headers.ContentEncoding.Add("gzip");
            request.Content = content;

            HttpResponseMessage response = await Client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized && retryOnUnauthorized)
            {
                // token หมดอายุ : login ใหม่ครั้งเดียว
                _token = null;
                return await PostAsync(cfg, json, retryOnUnauthorized: false);
            }
            return response;
        }

        // หน้า error ของ Django (DEBUG) เป็น HTML หลายร้อย KB ใส่ MessageBox ทั้งหน้าแล้วโปรแกรมค้าง
        // เอาแค่ exception_value / <title> หรือ 300 ตัวอักษรแรก
        internal static string ShortErrorBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "";
            if (body.TrimStart().StartsWith("<"))
            {
                Match m = Regex.Match(body, "class=\"exception_value\">(.*?)</", RegexOptions.Singleline);
                if (!m.Success)
                    m = Regex.Match(body, "<title>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                body = m.Success ? WebUtility.HtmlDecode(Regex.Replace(m.Groups[1].Value, "\\s+", " ")).Trim() : "(หน้า HTML)";
            }
            return body.Length <= 300 ? body : body.Substring(0, 300) + "...";
        }

        internal static void ClearToken()
        {
            _token = null;
        }

        internal static async Task<string> GetTokenAsync(ApiConfig cfg)
        {
            if (_token != null && _tokenBaseUrl == cfg.BaseUrl)
                return _token;

            string loginJson = JsonConvert.SerializeObject(new { username = cfg.Username, password = cfg.Password });
            HttpResponseMessage response = await Client.PostAsync(cfg.BaseUrl + "/jwt/create/",
                new StringContent(loginJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception("JWT ERROR : " + (int)response.StatusCode);

            string token = JObject.Parse(await response.Content.ReadAsStringAsync())["access"]?.ToString();
            if (string.IsNullOrEmpty(token))
                throw new Exception("JWT ERROR : ไม่มี access token");

            _token = token;
            _tokenBaseUrl = cfg.BaseUrl;
            return token;
        }

        private static byte[] Gzip(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                    gz.Write(data, 0, data.Length);
                return ms.ToArray();
            }
        }

        private static string Truncate(string s)
        {
            if (s == null)
                return null;
            return s.Length <= MaxErrorLength ? s : s.Substring(0, MaxErrorLength);
        }
    }
}
