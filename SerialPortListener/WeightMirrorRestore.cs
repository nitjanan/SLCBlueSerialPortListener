using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    // กู้รายการชั่งของตาชั่งเครื่องนี้ลงตาราง weight ในเครื่อง ใช้หลัง local DB เสีย (ฟ้าผ่า/ไฟดับ)
    // ดึง 2 แหล่งจาก web app ตามลำดับ :
    //   1. weight จริงบน server (ที่อัพโหลดตอนเย็นแล้ว และอาจถูกแก้บนเว็บ) weight/api/between/bws/...
    //   2. สำเนา weight_mirror (WeightMirrorSync.cs ส่งขึ้นไปทุก 3 นาที) เติมรายการที่ยังไม่ได้อัพโหลดตอนเย็น
    // ดึงครบทั้งสองแหล่งก่อนแล้วค่อยเขียนลงเครื่องใน transaction เดียว ดึงพลาดกลางทาง = ไม่มีอะไรถูกเขียน
    //
    // - ยึดข้อมูลจาก weight บน server ก่อน (ใช้ทั้งแถวตามที่ server มี ไม่ผสมกับสำเนา)
    //   แล้วเติมจาก weight_mirror เฉพาะรายการที่ server ไม่มี (ยังไม่ได้อัพโหลดตอนเย็น)
    //   สำเนาที่ weight_id มีบน server แต่อยู่นอกช่วงวันที่ จะถามซ้ำด้วย weight_id แล้วใช้ของ server
    //   ผล : กด "ไม่ใช้สำเนา" ก่อนหรือกดสำเนาก่อน ได้ผลเหมือนกัน
    // - เติมเฉพาะรายการที่ไม่มีในเครื่อง ไม่ทับรายการที่มีอยู่แล้ว
    // - สำเนาที่ถูกลบไปแล้ว (is_deleted) ไม่เอากลับมา
    // - ALTER SEQUENCE weight_weight_id_seq RESTART WITH (weight_id ล่าสุด + 1) ไม่งั้นรายการที่ชั่งใหม่จะใช้เลขซ้ำกับรายการเดิม
    // - ไม่ส่งรายการที่เพิ่งกู้กลับขึ้น mirror ซ้ำ (ลบออกจากคิว sync_outbox)
    public static class WeightMirrorRestore
    {
        private const int MaxErrorSamples = 5;

        public class StationInfo
        {
            public string StationCode, CompCode, MineCode, LastRecordDate;
            public int Count;
            public bool IsThisStation;

            public override string ToString()
            {
                string text = $"{StationCode}  |  บริษัท {Dash(CompCode)}  |  เหมือง {Dash(MineCode)}  |  {Count:N0} รายการ  |  ล่าสุด {LastRecordDate ?? "-"}";
                return IsThisStation ? text + "  (เครื่องนี้)" : text;
            }

            private static string Dash(string s) => string.IsNullOrEmpty(s) ? "-" : s;
        }

        // รหัสตาชั่งของเครื่องนี้ + สรุปสำเนาบน server (Mirror ว่างได้ = ไม่มีสำเนา ยังดึง weight จริงได้)
        public class RestoreSource
        {
            public string StationCode;
            public List<StationInfo> Mirror = new List<StationInfo>();
            public DateTime? WeightFirstDate;   // วันแรกที่มีรายการชั่งที่อัพโหลดแล้วบน server (null = ไม่มี)
        }

        public class SourceCount
        {
            public int Fetched, Inserted, AlreadyExists, Failed;
        }

        public class RestoreResult
        {
            public SourceCount Server = new SourceCount();   // weight จริงบน server
            public SourceCount Mirror = new SourceCount();   // สำเนา weight_mirror
            public int MirrorDeleted;
            public long? SequenceSetTo;
            public string DocNumberSetTo, DocNumberYear;   // seq_doc_num.run_number ที่ตั้งใหม่ (null = ไม่ได้ตั้ง)
            public List<string> Errors = new List<string>();
            public int Failed => Server.Failed + Mirror.Failed;
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None,     // วันที่ต้องเป็น string ตามที่ส่งมา ไม่แปลงตาม culture
            FloatParseHandling = FloatParseHandling.Decimal, // numeric ไม่เพี้ยนจาก double
        };

        // ชื่อ field ของ WeightSerializer (server) -> column ของ weight ในเครื่อง
        // กลับด้านจาก ucBackup.GetWeightJsonDataForDate (ตัวอัพโหลดตอนเย็น) ให้ตรงกันทุกตัว
        private static readonly Dictionary<string, string> ServerToLocal = new Dictionary<string, string>
        {
            ["weight_id"] = "weight_id",
            ["date"] = "วันที่",
            ["date_in"] = "วันที่ชั่งเข้า",
            ["date_out"] = "วันที่ชั่งออก",
            ["time_in"] = "เวลาชั่งเข้า",
            ["time_out"] = "เวลาชั่งออก",
            ["doc_id"] = "เลขที่เอกสาร",
            ["car_registration"] = "รหัสทะเบียนรถ",
            ["car_registration_name"] = "ทะเบียนรถ",
            ["province"] = "จังหวัด",
            ["driver"] = "รหัสคนขับ",
            ["driver_name"] = "คนขับ",
            ["customer"] = "รหัสลูกค้า",
            ["customer_name"] = "ลูกค้า",
            ["site"] = "site_id",
            ["site_name"] = "หน้างาน",
            ["mill"] = "mill_id",
            ["mill_name"] = "โรงโม่",
            ["stone_type"] = "stone_type_id",
            ["stone_type_name"] = "ชนิดหิน",
            ["pay"] = "จ่ายเงิน",
            ["scale_id"] = "รหัสผู้ชั่ง",
            ["scale_name"] = "ชื่อผู้ชั่ง",
            ["scoop"] = "รหัสผู้ตัก",
            ["scoop_name"] = "ชื่อผู้ตัก",
            ["vat_type"] = "ชนิดvat",
            ["stone_color"] = "ประเภทหิน",
            ["car_team"] = "car_team_id",
            ["car_team_name"] = "ทีม",
            ["clean_type"] = "ล้าง",
            ["transport"] = "ขนส่ง",
            ["note"] = "หมายเหตุ",
            ["ship_cost"] = "ค่าขนส่ง",
            ["carry_type_name"] = "carry_type_name",
            ["line_type"] = "line_type",
            ["bag_type"] = "ลักษณะถุง",
            ["fertilizer_name"] = "ชนิดปุ๋ย",
            ["pack_weight"] = "น้ำหนักบรรจุ",
            ["price_per_ton"] = "ราคาตัน",
            ["vat"] = "vat",
            ["q"] = "คิว",
            ["amount"] = "จำนวณเงิน",
            ["amount_vat"] = "จำนวนเงินสุทธิ",
            ["weight_in"] = "น้ำหนักรถ",
            ["weight_out"] = "น้ำหนักรวม",
            ["weight_total"] = "น้ำหนักสินค้า",
            ["oil_content"] = "oil_content",
            ["origin_weight"] = "origin_weight",
            ["origin_q"] = "origin_q",
            ["freight_cost"] = "ค่าบรรทุก",
            ["ton"] = "จำนวนตัน",
            ["sack"] = "จำนวนกระสอบ",
            ["price_up"] = "ค่าขึ้น",
            ["price_down"] = "ค่าลง",
            ["price_up_total"] = "ค่าขึ้นรวม",
            ["price_down_total"] = "ค่าลงรวม",
            ["freight_cost_total"] = "ค่าบรรทุกรวม",
            ["base_weight_station_name"] = "base_weight_station_name",
            ["is_s"] = "is_s",
            ["is_cancel"] = "is_cancel",
            ["exp_bill"] = "exp_bill",
            ["exp_change"] = "exp_change",
            ["exp_remission"] = "exp_remission",
            ["exp_note"] = "exp_note",
            ["exp_type"] = "exp_type",
            ["bws"] = "bws",
            ["do_doc_no"] = "do_doc_no",
            ["stone_desc"] = "stone_desc",
        };

        // ค้นด้วยรหัสตาชั่งของเครื่องนี้ (base_weight_station.code id = 1) ไม่ใช่ comp_code
        // เพราะตาชั่งหลายตัว (เช่น Blue/Pink) ใช้ comp_code เดียวกัน ค้นด้วย comp_code จะได้รายการของเครื่องอื่นมาปน
        public static async Task<RestoreSource> GetSourceAsync()
        {
            WeightMirrorSync.ApiConfig cfg = ReadConfig();
            var source = new RestoreSource { StationCode = cfg.StationCode };

            JObject data = await GetJsonAsync(cfg,
                $"{cfg.BaseUrl}/weightmirror/api/stations/?station_code={Uri.EscapeDataString(cfg.StationCode)}&table_name=weight") as JObject ?? new JObject();

            // server รุ่นเก่าไม่มี field นี้ = null
            if (DateTime.TryParseExact(data["weight_first_date"]?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime first))
                source.WeightFirstDate = first;

            foreach (JObject r in (data["results"] as JArray ?? new JArray()).OfType<JObject>())
            {
                var s = new StationInfo
                {
                    StationCode = r["station_code"]?.ToString() ?? "",
                    CompCode = r["comp_code"]?.ToString() ?? "",
                    MineCode = r["mine_code"]?.ToString() ?? "",
                    LastRecordDate = r["last_record_date"]?.Type == JTokenType.Null ? null : r["last_record_date"]?.ToString(),
                    Count = r["count"]?.Value<int>() ?? 0,
                };
                s.IsThisStation = s.StationCode == cfg.StationCode;
                source.Mirror.Add(s);
            }
            return source;
        }

        // mirror = null : ไม่มีสำเนาบน server ดึงเฉพาะ weight จริง
        public static async Task<RestoreResult> RestoreAsync(StationInfo mirror, DateTime dateFrom, IProgress<string> progress = null)
        {
            WeightMirrorSync.ApiConfig cfg = ReadConfig();

            // ดึงให้ครบก่อนแตะฐานในเครื่อง
            List<JObject> serverRows = await FetchServerWeightAsync(cfg, dateFrom, progress);
            progress?.Report("ดึงสำเนา ...");
            List<JObject> mirrorItems = mirror == null ? new List<JObject>() : await FetchMirrorAsync(cfg, mirror, dateFrom);

            // สำเนาที่ weight_id ไม่อยู่ในช่วงวันที่ที่ดึงจาก server (เช่นวันที่ถูกแก้บนเว็บ) ถามซ้ำด้วย weight_id
            // มีบน server = ใช้ข้อมูล server แทนสำเนา ผลจึงเหมือนกันไม่ว่าจะกดกู้แบบไหนก่อน
            var serverIds = new HashSet<string>(serverRows.Select(r => r["weight_id"]?.ToString() ?? ""), StringComparer.Ordinal);
            List<string> unknownIds = mirrorItems
                .Select(i => (i["payload"] as JObject)?["weight_id"]?.ToString())
                .Where(id => !string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"^\d+$") && !serverIds.Contains(id))
                .Distinct().ToList();
            if (unknownIds.Count > 0)
            {
                progress?.Report("ตรวจสำเนากับเซิร์ฟเวอร์ ...");
                serverRows.AddRange(await FetchServerWeightByIdsAsync(cfg, unknownIds));
            }
            progress?.Report("กำลังบันทึกลงเครื่อง ...");

            var result = new RestoreResult();
            result.Server.Fetched = serverRows.Count;
            result.Mirror.Fetched = mirrorItems.Count;

            Datalayer dl = new Datalayer();
            if (!dl.connect())
                throw new Exception("เชื่อมต่อฐานข้อมูลไม่ได้");
            try
            {
                OdbcConnection conn = dl.sqlConn();
                Dictionary<string, string> columns = ReadWeightColumns(conn);
                // psqlODBC คืน boolean เป็นตัวอักษร (BoolsAsChar) จึงใช้ 0/1
                bool hasOutbox = Convert.ToInt32(Scalar(conn, null,
                    "SELECT CASE WHEN to_regclass('public.sync_outbox') IS NULL THEN 0 ELSE 1 END")) == 1;

                using (OdbcTransaction tx = conn.BeginTransaction())
                {
                    // 1. weight จริงบน server ก่อน ใช้ตามที่ server มีทั้งแถว ไม่ผสมกับสำเนา
                    foreach (JObject row in serverRows)
                        InsertRow(conn, tx, MapServerRow(row), columns, hasOutbox, result.Server, result.Errors, "weight");

                    // 2. สำเนา : เติมเฉพาะรายการที่ยังไม่มี (รายการที่อัพโหลดแล้วถูกเพิ่มในข้อ 1 ไปแล้ว)
                    foreach (JObject item in mirrorItems)
                    {
                        if (item["is_deleted"]?.Type == JTokenType.Boolean && item["is_deleted"].Value<bool>())
                        {
                            result.MirrorDeleted++;
                            continue;
                        }
                        InsertRow(conn, tx, item["payload"] as JObject, columns, hasOutbox, result.Mirror, result.Errors, "สำเนา");
                    }

                    result.SequenceSetTo = BumpSequence(conn, tx);
                    UpdateDocNumber(conn, tx, result);
                    tx.Commit();
                }
            }
            finally
            {
                dl.close();
            }
            return result;
        }

        // แถวที่เสียไม่ทำให้แถวอื่นกู้ไม่ได้ (SAVEPOINT ต่อแถว)
        private static void InsertRow(OdbcConnection conn, OdbcTransaction tx, JObject row, Dictionary<string, string> columns,
                                      bool hasOutbox, SourceCount count, List<string> errors, string sourceName)
        {
            string rowId = row?["weight_id"]?.ToString();
            try
            {
                Exec(conn, tx, "SAVEPOINT restore_row");
                if (InsertIfMissing(conn, tx, row, columns))
                {
                    count.Inserted++;
                    if (hasOutbox)
                        Exec(conn, tx, "DELETE FROM sync_outbox WHERE table_name = 'weight' AND row_id = ? AND sent_at IS NULL", rowId);
                }
                else
                {
                    count.AlreadyExists++;
                }
                Exec(conn, tx, "RELEASE SAVEPOINT restore_row");
            }
            catch (Exception ex)
            {
                Exec(conn, tx, "ROLLBACK TO SAVEPOINT restore_row");
                count.Failed++;
                if (errors.Count < MaxErrorSamples)
                    errors.Add($"[{sourceName}] weight_id {rowId} : {ex.Message}");
            }
        }

        // แปลง JSON ของ server เป็นชื่อ column ในเครื่อง ; เวลา "12:54:00" (TimeField) -> "12:54" ตามรูปแบบในเครื่อง
        // ใช้ค่าจาก server อย่างเดียว : column ที่ไม่ได้อัพโหลดเป็น null ; ค่าว่างที่ server เก็บเป็น null ก็เป็น null
        private static JObject MapServerRow(JObject serverRow)
        {
            var local = new JObject();
            foreach (KeyValuePair<string, string> map in ServerToLocal)
            {
                JToken value = serverRow[map.Key];
                if (value == null)
                    continue;
                if ((map.Key == "time_in" || map.Key == "time_out") && value.Type == JTokenType.String)
                {
                    string t = value.ToString();
                    if (Regex.IsMatch(t, @"^\d{2}:\d{2}:\d{2}"))
                        value = t.Substring(0, 5);
                }
                local[map.Value] = value;
            }
            return local;
        }

        // weight จริงของตาชั่งนี้ตามช่วงวันที่ (server กรองด้วย bws = รหัสตาชั่ง)
        // endpoint นี้ไม่แบ่งหน้า จึงดึงทีละเดือน กันคำตอบใหญ่จนหมดเวลา (timeout 30 วินาที) เมื่อดึงย้อนหลังหลายปี
        private static async Task<List<JObject>> FetchServerWeightAsync(WeightMirrorSync.ApiConfig cfg, DateTime dateFrom,
                                                                        IProgress<string> progress)
        {
            var rows = new List<JObject>();
            DateTime today = DateTime.Today;
            for (DateTime start = dateFrom.Date; start <= today; start = start.AddMonths(1))
            {
                DateTime end = start.AddMonths(1).AddDays(-1);
                if (end > today)
                    end = today;
                progress?.Report($"ดึงรายการชั่ง {start:MM/yyyy} ...");
                JToken data = await GetJsonAsync(cfg,
                    $"{cfg.BaseUrl}/weight/api/between/bws/{start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" +
                    $"/{end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/{Uri.EscapeDataString(cfg.StationCode)}/");
                rows.AddRange((data as JArray ?? new JArray()).OfType<JObject>());
            }
            return rows;
        }

        private static WeightMirrorSync.ApiConfig ReadConfig()
        {
            Datalayer dl = new Datalayer();
            if (!dl.connect())
                throw new Exception("เชื่อมต่อฐานข้อมูลไม่ได้");
            try
            {
                WeightMirrorSync.ApiConfig cfg = WeightMirrorSync.ReadConfig(dl.sqlConn());
                if (string.IsNullOrWhiteSpace(cfg.BaseUrl))
                    throw new Exception("ไม่พบ url ใน base_api id = 1");
                if (string.IsNullOrWhiteSpace(cfg.StationCode))
                    throw new Exception("ไม่พบรหัสตาชั่ง (base_weight_station.code ของ base_weight_station_id = 1)\r\n" +
                                        "ตั้งรหัสตาชั่งให้ถูกต้องก่อน (ดาว์นโหลดการตั้งค่า หรือแก้ในตาราง base_weight_station)");
                return cfg;
            }
            finally
            {
                dl.close();
            }
        }

        // ไล่ดึงทีละหน้าด้วยเลข page เอง ไม่ใช้ next ที่ server สร้าง (หลัง proxy อาจเป็น http แทน https)
        private static async Task<List<JObject>> FetchMirrorAsync(WeightMirrorSync.ApiConfig cfg, StationInfo station, DateTime dateFrom)
        {
            var items = new List<JObject>();
            // station_code คือตัวค้นหลัก ; comp_code/mine_code ของแถวที่เลือกใช้กรองเพิ่ม (กรณีรหัสตาชั่งเดียวมีหลายบริษัท)
            string baseQuery = $"{cfg.BaseUrl}/weightmirror/api/list/?station_code={Uri.EscapeDataString(station.StationCode ?? "")}" +
                               $"&comp_code={Uri.EscapeDataString(station.CompCode ?? "")}" +
                               $"&mine_code={Uri.EscapeDataString(station.MineCode ?? "")}" +
                               $"&table_name=weight&date_from={dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            for (int page = 1; page <= 1000; page++)
            {
                JObject data = await GetJsonAsync(cfg, baseQuery + "&page=" + page) as JObject ?? new JObject();
                JArray results = data["results"] as JArray;
                if (results == null || results.Count == 0)
                    break;
                items.AddRange(results.OfType<JObject>());
                if (data["next"] == null || data["next"].Type == JTokenType.Null)
                    break;
            }
            return items;
        }

        // weight ของตาชั่งนี้ตาม weight_id (server ตอบเฉพาะที่พบ) ทีละ 200 ตัว
        private static async Task<List<JObject>> FetchServerWeightByIdsAsync(WeightMirrorSync.ApiConfig cfg, List<string> ids)
        {
            var rows = new List<JObject>();
            for (int i = 0; i < ids.Count; i += 200)
            {
                string chunk = string.Join(",", ids.Skip(i).Take(200));
                JObject data = await GetJsonAsync(cfg,
                    $"{cfg.BaseUrl}/weightmirror/api/weight/?station_code={Uri.EscapeDataString(cfg.StationCode)}&ids={chunk}") as JObject ?? new JObject();
                rows.AddRange((data["results"] as JArray ?? new JArray()).OfType<JObject>());
            }
            return rows;
        }

        private static async Task<JToken> GetJsonAsync(WeightMirrorSync.ApiConfig cfg, string url)
        {
            HttpResponseMessage response = await SendGetAsync(cfg, url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                WeightMirrorSync.ClearToken();
                response = await SendGetAsync(cfg, url);
            }
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception("API " + (int)response.StatusCode + " : " + WeightMirrorSync.ShortErrorBody(body));
            return JsonConvert.DeserializeObject<JToken>(body, JsonSettings) ?? new JObject();
        }

        private static async Task<HttpResponseMessage> SendGetAsync(WeightMirrorSync.ApiConfig cfg, string url)
        {
            string token = await WeightMirrorSync.GetTokenAsync(cfg);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await WeightMirrorSync.Client.SendAsync(request);
        }

        // ชื่อ column -> ชนิดตาม catalog (ใช้ CAST ค่า string ที่ส่งผ่าน ODBC ให้ตรงชนิด)
        private static Dictionary<string, string> ReadWeightColumns(OdbcConnection conn)
        {
            var columns = new Dictionary<string, string>(StringComparer.Ordinal);
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT a.attname::text AS name, format_type(a.atttypid, a.atttypmod) AS type
                                    FROM pg_attribute a
                                    WHERE a.attrelid = 'public.weight'::regclass AND a.attnum > 0 AND NOT a.attisdropped";
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        columns[reader["name"].ToString()] = reader["type"].ToString();
                }
            }
            return columns;
        }

        // ใช้เฉพาะ column ที่มีทั้งในสำเนาและในตารางเครื่องนี้ ตารางสองฝั่งอาจต่างรุ่นกัน
        private static bool InsertIfMissing(OdbcConnection conn, OdbcTransaction tx, JObject payload, Dictionary<string, string> columns)
        {
            if (payload == null || payload["weight_id"] == null || payload["weight_id"].Type == JTokenType.Null)
                throw new Exception("สำเนาไม่มี weight_id");

            var names = new List<string>();
            var values = new List<object>();
            foreach (JProperty p in payload.Properties())
            {
                if (!columns.ContainsKey(p.Name))
                    continue;
                names.Add(p.Name);
                values.Add(ToDbValue(p.Value));
            }

            string sql = "INSERT INTO public.weight (" + string.Join(", ", names.Select(QuoteIdent)) + ") VALUES (" +
                         string.Join(", ", names.Select(n => "CAST(? AS " + columns[n] + ")")) +
                         ") ON CONFLICT (weight_id) DO NOTHING";

            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                foreach (object v in values)
                    cmd.Parameters.AddWithValue("", v);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        private static object ToDbValue(JToken t)
        {
            switch (t.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return DBNull.Value;
                case JTokenType.Boolean:
                    return t.Value<bool>() ? "true" : "false";
                case JTokenType.Integer:
                case JTokenType.Float:
                    return Convert.ToString(((JValue)t).Value, CultureInfo.InvariantCulture);
                case JTokenType.Object:
                case JTokenType.Array:
                    return t.ToString(Formatting.None);
                default:
                    return t.ToString();
            }
        }

        // หลังกู้ทุกครั้ง : ALTER SEQUENCE weight_weight_id_seq RESTART WITH (weight_id ล่าสุด + 1)
        // คืนเลขที่รายการถัดไปจะได้ ; ไม่มีรายการในตาราง = ไม่แตะ sequence
        private static long? BumpSequence(OdbcConnection conn, OdbcTransaction tx)
        {
            string seq = Scalar(conn, tx, "SELECT pg_get_serial_sequence('public.weight', 'weight_id')") as string;
            if (string.IsNullOrEmpty(seq) || !Regex.IsMatch(seq, "^[A-Za-z0-9_.\"]+$"))
                seq = "weight_weight_id_seq";   // weight_id ไม่ได้ผูกกับ sequence (ไม่ใช่ serial) ใช้ชื่อมาตรฐาน

            object maxObj = Scalar(conn, tx, "SELECT max(weight_id) FROM public.weight");
            if (maxObj == null || maxObj == DBNull.Value)
                return null;
            long next = Convert.ToInt64(maxObj) + 1;

            Exec(conn, tx, "ALTER SEQUENCE " + seq + " RESTART WITH " + next.ToString(CultureInfo.InvariantCulture));
            return next;
        }

        // หลังกู้ทุกครั้ง : seq_doc_num.run_number = เลขที่เอกสาร ของ weight_id ล่าสุด
        // MainForm อ่าน run_number ของปี DateTime.Now.ToString("yyyy") แล้ว +1 เป็นเลขเอกสารใบถัดไป
        // ปีใช้วันที่ของรายการนั้นจัดรูปแบบแบบเดียวกับ MainForm (ตาม culture ของเครื่อง) ไม่มีวันที่ = ปีปัจจุบัน
        // เลขที่เอกสารว่าง/ไม่ใช่ตัวเลข = ไม่แตะ (MainForm แปลงเป็น int)
        private static void UpdateDocNumber(OdbcConnection conn, OdbcTransaction tx, RestoreResult result)
        {
            string docNo = null, dateText = null;
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT \"เลขที่เอกสาร\"::text AS doc_no, to_char(\"วันที่\", 'YYYY-MM-DD') AS d " +
                                  "FROM public.weight ORDER BY weight_id DESC LIMIT 1";
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;
                    docNo = reader["doc_no"] == DBNull.Value ? null : reader["doc_no"].ToString().Trim();
                    dateText = reader["d"] == DBNull.Value ? null : reader["d"].ToString();
                }
            }
            if (string.IsNullOrEmpty(docNo) || !Regex.IsMatch(docNo, @"^\d{1,9}$"))
                return;

            DateTime docDate = DateTime.TryParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d)
                ? d : DateTime.Now;
            string year = docDate.ToString("yyyy");

            object exists = Scalar(conn, tx, "SELECT count(*) FROM public.seq_doc_num WHERE run_year = ?", year);
            if (Convert.ToInt64(exists) > 0)
                Exec(conn, tx, "UPDATE public.seq_doc_num SET run_number = ? WHERE run_year = ?", docNo, year);
            else
                Exec(conn, tx, "INSERT INTO public.seq_doc_num (run_number, run_year) VALUES (?, ?)", docNo, year);

            result.DocNumberSetTo = docNo;
            result.DocNumberYear = year;
        }

        private static string QuoteIdent(string name)
        {
            return "\"" + name.Replace("\"", "\"\"") + "\"";
        }

        private static void Exec(OdbcConnection conn, OdbcTransaction tx, string sql, params object[] args)
        {
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                foreach (object a in args)
                    cmd.Parameters.AddWithValue("", a ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private static object Scalar(OdbcConnection conn, OdbcTransaction tx, string sql, params object[] args)
        {
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                foreach (object a in args)
                    cmd.Parameters.AddWithValue("", a ?? DBNull.Value);
                return cmd.ExecuteScalar();
            }
        }
    }
}
