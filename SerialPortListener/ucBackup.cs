using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerialPortListener
{
    public partial class ucBackup : UserControl
    {
        Datalayer dl;
        public ucBackup()
        {
            InitializeComponent();
            dl = new Datalayer();

            bool isConnected = dl.connect();
            if (isConnected)
                dl.close();
            btULWeight.Enabled = isConnected;

            LoadBackupConfig();
            InitAutoBackupTimer();
        }

        // ตอน constructor ทำงาน (สร้าง ucBackup เป็นลูกของ MainForm) ยังไม่ผ่าน Login
        // Globals.Permission จึงยังไม่ถูกตั้งค่า เช็คสิทธิ์ใหม่ทุกครั้งที่แสดงหน้านี้แทน
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                ApplyBackupConfigPermission();
        }

        // เฉพาะ user ที่มีสิทธิ์ add_setting เท่านั้นที่แก้ไข/บันทึกการตั้งค่า backup ได้ user อื่นดูได้อย่างเดียว
        private void ApplyBackupConfigPermission()
        {
            bool canEdit = Globals.isPermissionAddSetting();

            tbPgDumpPath.ReadOnly = !canEdit;
            tbBackupDir.ReadOnly = !canEdit;
            btnBrowsePgDump.Enabled = canEdit;
            btnBrowseBackupDir.Enabled = canEdit;
            btnSaveBackupConfig.Enabled = canEdit;
            chkAutoBackup.Enabled = canEdit;
        }

        // ปุ่ม "ตรวจสอบอัพเดท" ถูกย้ายมาไว้ที่ ucBackup แต่ logic การเช็ค/ติดตั้งอัพเดทยังอยู่ที่ MainForm
        // (ต้องใช้ dl, findBWS(), GetJwtToken() ของ MainForm) จึงส่งต่อผ่าน event ให้ MainForm จัดการ
        public event EventHandler CheckUpdateRequested;

        public bool CheckUpdateButtonEnabled
        {
            get { return btnCheckUpdate.Enabled; }
            set { btnCheckUpdate.Enabled = value; }
        }

        private void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            CheckUpdateRequested?.Invoke(this, EventArgs.Empty);
        }

        // ตารางที่ต้องดาวน์โหลด : (num, table_name, url_segment, pk_field ในแต่ละ JSON record สำหรับใช้ log)
        // เทียบกับ get_json_url() ใน auto_setting_sell.py ; URL รูปแบบ {baseUrl}/{segment}/api/vStamp/{time}/
        private static readonly (int Num, string TableName, string UrlSegment, string PkField)[] SettingTables =
        {
            (1, "base_job_type", "baseJobType", "base_job_type_id"),
            (2, "base_customer", "baseCustomer", "customer_id"),
            (3, "base_site", "baseSite", "base_site_id"),
            (4, "base_customer_site", "baseCustomerSite", "id"),
            (5, "base_stone_type", "baseStoneType", "รหัสหิน"),
            (6, "base_mill", "baseMill", "รหัสโรงโม่"),
            (7, "base_car_team", "baseCarTeam", "รหัสทีม"),
            (8, "base_car", "baseCar", "รหัสรถร่วม"),
            (9, "base_scoop", "baseScoop", "รหัสผู้ตัก"),
            (10, "base_driver", "baseDriver", "รหัสผู้ขับ"),
            (11, "base_car_registration", "baseCarRegistration", "รหัสทะเบียนรถ"),
            (12, "users", "userScale", "scale_id"),
        };

        private async void btDLSetting_Click(object sender, EventArgs e)
        {
            btDLSetting.Enabled = false;
            try
            {
                await DownloadSettingAsync(FindForm());
            }
            finally
            {
                btDLSetting.Enabled = true;
            }
        }

        // เทียบกับ btDLSetting_Click แต่แยกออกมาให้เรียกใช้จากที่อื่นได้ (เช่น btRefresh ใน MainForm)
        public async Task DownloadSettingAsync(Form owner)
        {
            frmDownloadProgress progress = new frmDownloadProgress();
            progress.Show(owner);

            try
            {
                string baseUrl = GetBaseApi(1, 1);
                string apiUsername = GetBaseApi(2, 1);
                string apiPassword = GetBaseApi(3, 1);

                progress.Log("Authentication...");

                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    string accessToken = await GetJwtTokenAsync(client, baseUrl, apiUsername, apiPassword);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        progress.Log("Authentication failed.");
                        MessageBox.Show("เข้าสู่ระบบ API ไม่สำเร็จ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    progress.Log("Authentication successful.");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    int totalUpdated = 0;
                    StringBuilder errors = new StringBuilder();

                    foreach (var table in SettingTables)
                    {
                        dl.connect();
                        try
                        {
                            string vStampDt = GetVStampDt(table.TableName);
                            string encodedStamp = Uri.EscapeDataString(vStampDt ?? "");
                            string url = $"{baseUrl}/{table.UrlSegment}/api/vStamp/{encodedStamp}/";

                            HttpResponseMessage response = await client.GetAsync(url);
                            if (!response.IsSuccessStatusCode)
                            {
                                progress.Log("Failed to fetch JSON data from the URL.");
                                errors.AppendLine($"{table.TableName} : โหลดข้อมูลไม่สำเร็จ ({(int)response.StatusCode})");
                                continue;
                            }

                            string json = await response.Content.ReadAsStringAsync();
                            JArray items = JsonConvert.DeserializeObject<JArray>(json);
                            if (items == null)
                                continue;

                            foreach (JObject item in items)
                            {
                                ProcessUpdate(table.Num, item);
                                totalUpdated++;
                                progress.Log($"Update or Create {table.TableName} {item[table.PkField]} : successful.");
                            }
                        }
                        catch (Exception exTable)
                        {
                            progress.Log($"Error updating row: {exTable.Message}");
                            errors.AppendLine($"{table.TableName} : {exTable.Message}");
                        }
                        finally
                        {
                            dl.close();
                        }
                    }

                    string summary = $"ดาวน์โหลดและอัพเดทข้อมูลสำเร็จ {totalUpdated} รายการ";
                    if (errors.Length > 0)
                        summary += "\r\n\r\nข้อผิดพลาด:\r\n" + errors.ToString();

                    progress.Log(summary);

                    MessageBox.Show(summary, "ดาวน์โหลดการตั้งค่า", MessageBoxButtons.OK,
                        errors.Length > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                progress.Log("Error: " + ex.Message);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress.AllowClose();
                progress.Close();
            }
        }

        // เทียบกับ getBaseApi() ใน MainForm.cs : mode 1=url, 2=username, 3=password, 4=comp_code, 5=token
        private string GetBaseApi(int mode, int baseApiId)
        {
            string url = "", username = "", password = "", compCode = "", token = "";

            try
            {
                dl.connect();
                OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
                cmd.CommandText = "SELECT url, username, password, comp_code, token FROM base_api WHERE id = " + baseApiId;
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        url = reader["url"].ToString();
                        username = reader["username"].ToString();
                        password = reader["password"].ToString();
                        compCode = reader["comp_code"].ToString();
                        token = reader["token"].ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }

            switch (mode)
            {
                case 1: return url;
                case 2: return username;
                case 3: return password;
                case 4: return compCode;
                case 5: return token;
                default: return "";
            }
        }

        // เทียบกับ GetJwtToken() ใน MainForm.cs
        private async Task<string> GetJwtTokenAsync(HttpClient client, string baseUrl, string username, string password)
        {
            try
            {
                string jwtUrl = $"{baseUrl}/jwt/create/";
                string loginJson = JsonConvert.SerializeObject(new { username, password });
                var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

                HttpResponseMessage jwtResponse = await client.PostAsync(jwtUrl, loginContent);
                if (!jwtResponse.IsSuccessStatusCode)
                    return null;

                string jwtResult = await jwtResponse.Content.ReadAsStringAsync();
                dynamic jwtObj = JsonConvert.DeserializeObject(jwtResult);
                return jwtObj?.access?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        // เทียบกับ get_v_stamp_dt() ใน auto_setting_sell.py
        private string GetVStampDt(string tableName)
        {
            string vStampDt = null;
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = "SELECT v_stamp_dt FROM v_stamp WHERE table_name = ?";
            cmd.Parameters.AddWithValue("", tableName);
            using (OdbcDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    object raw = reader["v_stamp_dt"];
                    if (raw is DateTime dt)
                        vStampDt = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        vStampDt = raw?.ToString();
                }
            }
            return vStampDt;
        }

        // ค่าจาก JObject เป็น object สำหรับ OdbcParameter (null -> DBNull.Value)
        private static object S(JObject d, string key)
        {
            JToken token = d[key];
            if (token == null || token.Type == JTokenType.Null)
                return DBNull.Value;
            return token.ToString();
        }

        // เทียบกับ processs_update() ใน auto_setting_sell.py
        private void ProcessUpdate(int num, JObject data)
        {
            switch (num)
            {
                case 1: ProcessBaseJobType(data); break;
                case 2: ProcessBaseCustomer(data); break;
                case 3: ProcessBaseSite(data); break;
                case 4: ProcessBaseCustomerSite(data); break;
                case 5: ProcessBaseStoneType(data); break;
                case 6: ProcessBaseMill(data); break;
                case 7: ProcessBaseCarTeam(data); break;
                case 8: ProcessBaseCar(data); break;
                case 9: ProcessBaseScoop(data); break;
                case 10: ProcessBaseDriver(data); break;
                case 11: ProcessBaseCarRegistration(data); break;
                case 12: ProcessBaseUser(data); break;
            }
        }

        private void UpdateVStamp(string tableName, JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = "UPDATE public.v_stamp SET v_stamp_dt = ? WHERE table_name = ?";
            cmd.Parameters.AddWithValue("", ParseVStamp(d));
            cmd.Parameters.AddWithValue("", tableName);
            cmd.ExecuteNonQuery();
        }

        // API ส่ง v_stamp มาเป็น ISO 8601 เช่น "2025-02-25T09:47:21.984276" (ไม่ใช่ dd/mm หรือ mm/dd)
        // ต้อง parse เป็น DateTime ในฝั่ง C# ก่อนส่งเป็น parameter เสมอ ห้ามส่ง string ดิบให้ Postgres
        // เพราะ Postgres จะตีความตาม DateStyle ของตัวเอง (ปกติ MDY) ทำให้วันที่ผิดเพี้ยน/error out of range
        private static readonly string[] VStampFormats =
        {
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss",
        };

        private static object ParseVStamp(JObject d)
        {
            string raw = (string)d["v_stamp"];
            if (string.IsNullOrEmpty(raw))
                return DBNull.Value;

            if (DateTime.TryParseExact(raw, VStampFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dt))
                return dt;

            // สำรอง: รูปแบบ ISO 8601 อื่น ๆ ที่ไม่ตรง pattern ด้านบนเป๊ะ ๆ (เช่น มี timezone offset)
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out dt))
                return dt;

            return raw;
        }

        private void ProcessBaseMill(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_mill(""รหัสโรงโม่"", ""ชื่อโรงโม่"", ""weight_type"")
                VALUES (?, ?, ?)
                ON CONFLICT (""รหัสโรงโม่"")
                DO UPDATE SET ""ชื่อโรงโม่"" = EXCLUDED.""ชื่อโรงโม่"", ""weight_type"" = EXCLUDED.""weight_type"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสโรงโม่"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อโรงโม่"));
            cmd.Parameters.AddWithValue("", S(d, "weight_type"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = "UPDATE public.weight SET โรงโม่ = ? WHERE mill_id = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อโรงโม่"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสโรงโม่"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_mill", d);
        }

        private void ProcessBaseCar(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_car(""รหัสรถร่วม"", ""ชื่อรถร่วม"", ""รหัสทีม"")
                VALUES (?, ?, ?)
                ON CONFLICT (""รหัสรถร่วม"")
                DO UPDATE SET ""ชื่อรถร่วม"" = EXCLUDED.""ชื่อรถร่วม"", ""รหัสทีม"" = EXCLUDED.""รหัสทีม"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสรถร่วม"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อรถร่วม"));
            cmd.Parameters.AddWithValue("", S(d, "รหัสทีม"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("base_car", d);
        }

        private void ProcessBaseCarRegistration(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_car_registration (""รหัสทะเบียนรถ"", ""ชื่อทะเบียนรถ"", ""ประเภทรถ"", ""company"")
                VALUES (?, ?, ?, ?)
                ON CONFLICT (""รหัสทะเบียนรถ"")
                DO UPDATE SET ""ชื่อทะเบียนรถ"" = EXCLUDED.""ชื่อทะเบียนรถ"", ""ประเภทรถ"" = EXCLUDED.""ประเภทรถ"", ""company"" = EXCLUDED.""company"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสทะเบียนรถ"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อทะเบียนรถ"));
            cmd.Parameters.AddWithValue("", S(d, "ประเภทรถ"));
            cmd.Parameters.AddWithValue("", S(d, "company"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = @"UPDATE public.weight SET ""ทะเบียนรถ"" = ? WHERE ""รหัสทะเบียนรถ"" = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อทะเบียนรถ"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสทะเบียนรถ"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_car_registration", d);
        }

        private void ProcessBaseCarTeam(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_car_team (""รหัสทีม"", ""ชื่อทีม"")
                VALUES (?, ?)
                ON CONFLICT (""รหัสทีม"")
                DO UPDATE SET ""ชื่อทีม"" = EXCLUDED.""ชื่อทีม"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสทีม"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อทีม"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = "UPDATE public.weight SET ทีม = ? WHERE car_team_id = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อทีม"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสทีม"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_car_team", d);
        }

        private void ProcessBaseCustomer(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_customer (""รหัสลูกค้า"", ""ชื่อลูกค้า"", ""ที่อยู่"", ""ส่งที่"", ""base_job_type_id"", ""base_vat_type_id"", ""weight_type"")
                VALUES (?, ?, ?, ?, ?, ?, ?)
                ON CONFLICT (""รหัสลูกค้า"")
                DO UPDATE SET ""ชื่อลูกค้า"" = EXCLUDED.""ชื่อลูกค้า"", ""ที่อยู่"" = EXCLUDED.""ที่อยู่"", ""ส่งที่"" = EXCLUDED.""ส่งที่"",
                ""base_job_type_id"" = EXCLUDED.""base_job_type_id"", ""base_vat_type_id"" = EXCLUDED.""base_vat_type_id"", ""weight_type"" = EXCLUDED.""weight_type"";";
            cmd.Parameters.AddWithValue("", S(d, "customer_id"));
            cmd.Parameters.AddWithValue("", S(d, "customer_name"));
            cmd.Parameters.AddWithValue("", S(d, "address"));
            cmd.Parameters.AddWithValue("", S(d, "send_to"));
            cmd.Parameters.AddWithValue("", S(d, "base_job_type"));
            cmd.Parameters.AddWithValue("", S(d, "base_vat_type"));
            cmd.Parameters.AddWithValue("", S(d, "weight_type"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = @"UPDATE public.weight SET ลูกค้า = ? WHERE รหัสลูกค้า = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "customer_name"));
            cmdWeight.Parameters.AddWithValue("", S(d, "customer_id"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_customer", d);
        }

        private void ProcessBaseDriver(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_driver(""รหัสผู้ขับ"", ""ชื่อผู้ขับ"", ""company"")
                VALUES (?, ?, ?)
                ON CONFLICT (""รหัสผู้ขับ"")
                DO UPDATE SET ""ชื่อผู้ขับ"" = EXCLUDED.""ชื่อผู้ขับ"", ""company"" = EXCLUDED.""company"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสผู้ขับ"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อผู้ขับ"));
            cmd.Parameters.AddWithValue("", S(d, "company"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = @"UPDATE public.weight SET คนขับ = ? WHERE รหัสคนขับ = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อผู้ขับ"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสผู้ขับ"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_driver", d);
        }

        private void ProcessBaseJobType(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_job_type (""base_job_type_id"", ""base_job_type_name"")
                VALUES (?, ?)
                ON CONFLICT (""base_job_type_id"")
                DO UPDATE SET ""base_job_type_name"" = EXCLUDED.""base_job_type_name"";";
            cmd.Parameters.AddWithValue("", S(d, "base_job_type_id"));
            cmd.Parameters.AddWithValue("", S(d, "base_job_type_name"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("base_job_type", d);
        }

        private void ProcessBaseScoop(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_scoop (""รหัสผู้ตัก"", ""ชื่อผู้ตัก"", ""company"")
                VALUES (?, ?, ?)
                ON CONFLICT (""รหัสผู้ตัก"")
                DO UPDATE SET ""ชื่อผู้ตัก"" = EXCLUDED.""ชื่อผู้ตัก"", ""company"" = EXCLUDED.""company"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสผู้ตัก"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อผู้ตัก"));
            cmd.Parameters.AddWithValue("", S(d, "company"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = "UPDATE public.weight SET ชื่อผู้ตัก = ? WHERE รหัสผู้ตัก = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อผู้ตัก"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสผู้ตัก"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_scoop", d);
        }

        private void ProcessBaseStoneType(JObject d)
        {
            JToken calQ = d["cal_q"];
            JToken inactive = d["inactive"];

            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_stone_type (""รหัสหิน"", ""ชื่อหิน"", ""ค่าคำนวณคิว"", ""inactive"")
                VALUES (?, ?, ?, ?)
                ON CONFLICT (""รหัสหิน"")
                DO UPDATE SET ""ชื่อหิน"" = EXCLUDED.""ชื่อหิน"", ""ค่าคำนวณคิว"" = EXCLUDED.""ค่าคำนวณคิว"", ""inactive"" = EXCLUDED.""inactive"";";
            cmd.Parameters.AddWithValue("", S(d, "รหัสหิน"));
            cmd.Parameters.AddWithValue("", S(d, "ชื่อหิน"));
            cmd.Parameters.AddWithValue("", calQ != null && calQ.Type != JTokenType.Null ? (object)calQ.ToObject<decimal>() : DBNull.Value);
            cmd.Parameters.AddWithValue("", inactive != null && inactive.Type != JTokenType.Null ? (object)inactive.ToObject<bool>() : false);
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = "UPDATE public.weight SET ชนิดหิน = ? WHERE stone_type_id = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "ชื่อหิน"));
            cmdWeight.Parameters.AddWithValue("", S(d, "รหัสหิน"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_stone_type", d);
        }

        private void ProcessBaseSite(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_site(""base_site_id"", ""base_site_name"", ""weight_type"")
                VALUES (?, ?, ?)
                ON CONFLICT (""base_site_id"")
                DO UPDATE SET ""base_site_name"" = EXCLUDED.""base_site_name"", ""weight_type"" = EXCLUDED.""weight_type"";";
            cmd.Parameters.AddWithValue("", S(d, "base_site_id"));
            cmd.Parameters.AddWithValue("", S(d, "base_site_name"));
            cmd.Parameters.AddWithValue("", S(d, "weight_type"));
            cmd.ExecuteNonQuery();

            OdbcCommand cmdWeight = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmdWeight.CommandText = "UPDATE public.weight SET หน้างาน = ? WHERE site_id = ?";
            cmdWeight.Parameters.AddWithValue("", S(d, "base_site_name"));
            cmdWeight.Parameters.AddWithValue("", S(d, "base_site_id"));
            cmdWeight.ExecuteNonQuery();

            UpdateVStamp("base_site", d);
        }

        private void ProcessBaseUser(JObject d)
        {
            // users_id เป็น auto (identity/serial) ทางฝั่ง DB ; username ในระบบนี้ map มาจาก scale_id ของ API (ไม่ใช่ username ของ API)
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.users(""firstname"", ""username"", ""password"", ""permission"")
                VALUES (?, ?, ?, ?)
                ON CONFLICT (""username"")
                DO UPDATE SET ""firstname"" = EXCLUDED.""firstname"",
                ""password"" = COALESCE(EXCLUDED.""password"", public.users.""password""),
                ""permission"" = COALESCE(EXCLUDED.""permission"", public.users.""permission"");";
            cmd.Parameters.AddWithValue("", S(d, "scale_name"));
            cmd.Parameters.AddWithValue("", S(d, "scale_id"));
            cmd.Parameters.AddWithValue("", S(d, "password"));
            cmd.Parameters.AddWithValue("", S(d, "permission"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("users", d);
        }

        private void ProcessBaseCustomerSite(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO public.base_customer_site(""id"", ""customer_id"", ""site_id"")
                VALUES (?, ?, ?)
                ON CONFLICT (""id"")
                DO UPDATE SET ""customer_id"" = EXCLUDED.""customer_id"", ""site_id"" = EXCLUDED.""site_id"";";
            cmd.Parameters.AddWithValue("", S(d, "id"));
            cmd.Parameters.AddWithValue("", S(d, "customer"));
            cmd.Parameters.AddWithValue("", S(d, "site"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("base_customer_site", d);
        }

        // เทียบกับ AU_weight_to_local.py : main() / get_json_url() / processs_update()
        private async void btDLWeight_Click(object sender, EventArgs e)
        {
            btDLWeight.Enabled = false;

            frmDownloadProgress progress = new frmDownloadProgress();
            progress.Show(FindForm());

            try
            {
                string baseUrl = GetBaseApi(1, 1);
                string apiUsername = GetBaseApi(2, 1);
                string apiPassword = GetBaseApi(3, 1);
                string compCode = GetBaseApi(4, 1);
                string lc = GetLc();

                progress.Log("Authentication...");

                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    string accessToken = await GetJwtTokenAsync(client, baseUrl, apiUsername, apiPassword);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        progress.Log("Authentication failed.");
                        MessageBox.Show("เข้าสู่ระบบ API ไม่สำเร็จ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    progress.Log("Authentication successful.");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    int totalUpdated = 0;
                    StringBuilder errors = new StringBuilder();

                    for (int num = 1; num <= 3; num++)
                    {
                        string tableName = num == 1 ? "weight" : num == 2 ? "weight_delivery" : "delivery_order";

                        dl.connect();
                        string vStampDt;
                        try
                        {
                            vStampDt = GetVStampDt(tableName);
                        }
                        finally
                        {
                            dl.close();
                        }

                        if (string.IsNullOrEmpty(vStampDt))
                        {
                            progress.Log($"{tableName} : ไม่พบ v_stamp_dt");
                            continue;
                        }

                        string encodedStamp = Uri.EscapeDataString(vStampDt);
                        string url;
                        if (num == 1)
                            url = $"{baseUrl}/weight/api/vStamp/{encodedStamp}/{lc}/";
                        else if (num == 2)
                            url = $"{baseUrl}/weightdelivery/api/vStamp/?comp_code={compCode}&v_stamp={encodedStamp}";
                        else
                            url = $"{baseUrl}/deliveryorder/api/vStamp/?comp_code={compCode}&v_stamp={encodedStamp}";

                        try
                        {
                            List<JObject> items;
                            if (num == 1)
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (!response.IsSuccessStatusCode)
                                {
                                    progress.Log("Failed to fetch JSON data from the URL.");
                                    errors.AppendLine($"{tableName} : โหลดข้อมูลไม่สำเร็จ ({(int)response.StatusCode})");
                                    continue;
                                }
                                string json = await response.Content.ReadAsStringAsync();
                                items = JsonConvert.DeserializeObject<JArray>(json)?.Cast<JObject>().ToList() ?? new List<JObject>();
                            }
                            else
                            {
                                items = await FetchAllPagesAsync(client, url);
                            }

                            foreach (JObject item in items)
                            {
                                dl.connect();
                                try
                                {
                                    switch (num)
                                    {
                                        case 1: ProcessWeightUpdate(item); break;
                                        case 2: ProcessWeightDeliveryUpdate(item); break;
                                        case 3: ProcessDeliveryOrderUpdate(item); break;
                                    }
                                    totalUpdated++;
                                    string pk = num == 3 ? item["doc_no"]?.ToString() : item["weight_id"]?.ToString();
                                    progress.Log($"Update {tableName} {pk} : successful.");
                                }
                                catch (Exception exRow)
                                {
                                    progress.Log($"Error updating row: {exRow.Message}");
                                    errors.AppendLine($"{tableName} : {exRow.Message}");
                                }
                                finally
                                {
                                    dl.close();
                                }
                            }
                        }
                        catch (Exception exTable)
                        {
                            progress.Log($"Error: {exTable.Message}");
                            errors.AppendLine($"{tableName} : {exTable.Message}");
                        }
                    }

                    string summary = $"ดาวน์โหลดและอัพเดทข้อมูลสำเร็จ {totalUpdated} รายการ";
                    if (errors.Length > 0)
                        summary += "\r\n\r\nข้อผิดพลาด:\r\n" + errors.ToString();

                    progress.Log(summary);

                    MessageBox.Show(summary, "ดาวน์โหลดรายการชั่งที่แก้ไข", MessageBoxButtons.OK,
                        errors.Length > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                progress.Log("Error: " + ex.Message);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress.AllowClose();
                progress.Close();
                btDLWeight.Enabled = true;
            }
        }

        // เทียบกับ lc = default_values.get('lc') ใน AU_weight_to_local.py
        // แต่แทนที่จะอ่านจาก configs.txt ใช้ code จาก public.base_weight_station (เทียบกับ findBWS() ใน MainForm.cs)
        private string GetLc()
        {
            string lc = "";
            dl.connect();
            try
            {
                OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
                cmd.CommandText = "SELECT code FROM base_weight_station WHERE base_weight_station_id = 1";
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lc = reader["code"].ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }
            return lc;
        }

        // เทียบกับ fetch_all_pages() ใน AU_weight_to_local.py
        private async Task<List<JObject>> FetchAllPagesAsync(HttpClient client, string baseUrl)
        {
            List<JObject> allData = new List<JObject>();
            int page = 1;

            while (true)
            {
                string paginatedUrl = $"{baseUrl}&page={page}";
                HttpResponseMessage response = await client.GetAsync(paginatedUrl);
                if (!response.IsSuccessStatusCode)
                    break;

                string json = await response.Content.ReadAsStringAsync();
                JToken data = JsonConvert.DeserializeObject<JToken>(json);

                if (data is JObject dObj && dObj["results"] != null)
                {
                    allData.AddRange(dObj["results"].Cast<JObject>());
                    if (dObj["next"] == null || dObj["next"].Type == JTokenType.Null)
                        break;
                }
                else if (data is JArray dArr)
                {
                    allData.AddRange(dArr.Cast<JObject>());
                    break;
                }
                else
                {
                    break;
                }

                page++;
            }

            return allData;
        }

        // ค่าจาก JObject เป็น bool สำหรับ OdbcParameter
        private static object SBool(JObject d, string key)
        {
            JToken token = d[key];
            if (token == null || token.Type == JTokenType.Null)
                return false;
            return token.ToObject<bool>();
        }

        // เทียบกับ processs_weight() ใน AU_weight_to_local.py
        private void ProcessWeightUpdate(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                UPDATE public.weight
                SET
                    ""รหัสทะเบียนรถ"" = ?,
                    ""ทะเบียนรถ"" = COALESCE(?, ''),
                    ""จังหวัด"" = ?,
                    ""รหัสคนขับ"" = ?,
                    ""คนขับ"" = ?,
                    ""รหัสลูกค้า"" = ?,
                    ""ลูกค้า"" = ?,
                    ""site_id"" = ?,
                    ""หน้างาน"" = COALESCE(?, ''),
                    ""mill_id"" = ?,
                    ""โรงโม่"" = COALESCE(?, ''),
                    ""stone_type_id"" = ?,
                    ""ชนิดหิน"" = COALESCE(?, ''),
                    ""จ่ายเงิน"" = COALESCE(?, ''),
                    ""รหัสผู้ชั่ง"" = ?,
                    ""ชื่อผู้ชั่ง"" = ?,
                    ""รหัสผู้ตัก"" = ?,
                    ""ชื่อผู้ตัก"" = COALESCE(?, ''),
                    ""ชนิดvat"" = ?,
                    ""ประเภทหิน"" = ?,
                    ""car_team_id"" = ?,
                    ""ทีม"" = COALESCE(?, ''),
                    ""ล้าง"" = ?,
                    ""ขนส่ง"" = ?,
                    ""หมายเหตุ"" = ?,
                    ""ค่าขนส่ง"" = ?,
                    ""carry_type_name"" = COALESCE(?, ''),
                    ""line_type"" = ?,
                    ""ลักษณะถุง"" = ?,
                    ""ชนิดปุ๋ย"" = ?,
                    ""น้ำหนักบรรจุ"" = ?,
                    ""ราคาตัน"" = ?,
                    ""vat"" = ?,
                    ""คิว"" = ?,
                    ""จำนวณเงิน"" = ?,
                    ""จำนวนเงินสุทธิ"" = ?,
                    ""น้ำหนักรถ"" = ?,
                    ""น้ำหนักรวม"" = ?,
                    ""น้ำหนักสินค้า"" = ?,
                    ""oil_content"" = ?,
                    ""origin_weight"" = ?,
                    ""origin_q"" = ?,
                    ""ค่าบรรทุก"" = ?,
                    ""จำนวนตัน"" = ?,
                    ""จำนวนกระสอบ"" = ?,
                    ""ค่าขึ้น"" = ?,
                    ""ค่าลง"" = ?,
                    ""ค่าขึ้นรวม"" = ?,
                    ""ค่าลงรวม"" = ?,
                    ""ค่าบรรทุกรวม"" = ?,
                    ""base_weight_station_name"" = ?,
                    ""is_s"" = ?,
                    ""is_cancel"" = ?,
                    ""exp_bill"" = ?,
                    ""exp_change"" = ?,
                    ""exp_remission"" = ?,
                    ""exp_note"" = ?,
                    ""exp_type"" = ?,
                    ""do_doc_no"" = ?,
                    ""stone_desc"" = ?
                WHERE ""weight_id"" = ?;";

            cmd.Parameters.AddWithValue("", S(d, "car_registration"));
            cmd.Parameters.AddWithValue("", S(d, "car_registration_name"));
            cmd.Parameters.AddWithValue("", S(d, "province"));
            cmd.Parameters.AddWithValue("", S(d, "driver"));
            cmd.Parameters.AddWithValue("", S(d, "driver_name"));
            cmd.Parameters.AddWithValue("", S(d, "customer"));
            cmd.Parameters.AddWithValue("", S(d, "customer_name"));
            cmd.Parameters.AddWithValue("", S(d, "site"));
            cmd.Parameters.AddWithValue("", S(d, "site_name"));
            cmd.Parameters.AddWithValue("", S(d, "mill"));
            cmd.Parameters.AddWithValue("", S(d, "mill_name"));
            cmd.Parameters.AddWithValue("", S(d, "stone_type"));
            cmd.Parameters.AddWithValue("", S(d, "stone_type_name"));
            cmd.Parameters.AddWithValue("", S(d, "pay"));
            cmd.Parameters.AddWithValue("", S(d, "scale_id"));
            cmd.Parameters.AddWithValue("", S(d, "scale_name"));
            cmd.Parameters.AddWithValue("", S(d, "scoop"));
            cmd.Parameters.AddWithValue("", S(d, "scoop_name"));
            cmd.Parameters.AddWithValue("", S(d, "vat_type"));
            cmd.Parameters.AddWithValue("", S(d, "stone_color"));
            cmd.Parameters.AddWithValue("", S(d, "car_team"));
            cmd.Parameters.AddWithValue("", S(d, "car_team_name"));
            cmd.Parameters.AddWithValue("", S(d, "clean_type"));
            cmd.Parameters.AddWithValue("", S(d, "transport"));
            cmd.Parameters.AddWithValue("", S(d, "note"));
            cmd.Parameters.AddWithValue("", S(d, "ship_cost"));
            cmd.Parameters.AddWithValue("", S(d, "carry_type_name"));
            cmd.Parameters.AddWithValue("", S(d, "line_type"));
            cmd.Parameters.AddWithValue("", S(d, "bag_type"));
            cmd.Parameters.AddWithValue("", S(d, "fertilizer_name"));
            cmd.Parameters.AddWithValue("", S(d, "pack_weight"));
            cmd.Parameters.AddWithValue("", S(d, "price_per_ton"));
            cmd.Parameters.AddWithValue("", S(d, "vat"));
            cmd.Parameters.AddWithValue("", S(d, "q"));
            cmd.Parameters.AddWithValue("", S(d, "amount"));
            cmd.Parameters.AddWithValue("", S(d, "amount_vat"));
            cmd.Parameters.AddWithValue("", S(d, "weight_in"));
            cmd.Parameters.AddWithValue("", S(d, "weight_out"));
            cmd.Parameters.AddWithValue("", S(d, "weight_total"));
            cmd.Parameters.AddWithValue("", S(d, "oil_content"));
            cmd.Parameters.AddWithValue("", S(d, "origin_weight"));
            cmd.Parameters.AddWithValue("", S(d, "origin_q"));
            cmd.Parameters.AddWithValue("", S(d, "freight_cost"));
            cmd.Parameters.AddWithValue("", S(d, "ton"));
            cmd.Parameters.AddWithValue("", S(d, "sack"));
            cmd.Parameters.AddWithValue("", S(d, "price_up"));
            cmd.Parameters.AddWithValue("", S(d, "price_down"));
            cmd.Parameters.AddWithValue("", S(d, "price_up_total"));
            cmd.Parameters.AddWithValue("", S(d, "price_down_total"));
            cmd.Parameters.AddWithValue("", S(d, "freight_cost_total"));
            cmd.Parameters.AddWithValue("", S(d, "base_weight_station_name"));
            cmd.Parameters.AddWithValue("", SBool(d, "is_s"));
            cmd.Parameters.AddWithValue("", SBool(d, "is_cancel"));
            cmd.Parameters.AddWithValue("", S(d, "exp_bill"));
            cmd.Parameters.AddWithValue("", S(d, "exp_change"));
            cmd.Parameters.AddWithValue("", S(d, "exp_remission"));
            cmd.Parameters.AddWithValue("", S(d, "exp_note"));
            cmd.Parameters.AddWithValue("", S(d, "exp_type"));
            cmd.Parameters.AddWithValue("", S(d, "do_doc_no"));
            cmd.Parameters.AddWithValue("", S(d, "stone_desc"));
            cmd.Parameters.AddWithValue("", S(d, "weight_id"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("weight", d);
        }

        // เทียบกับ processs_weight_delivery() ใน AU_weight_to_local.py
        private void ProcessWeightDeliveryUpdate(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"UPDATE public.weight_delivery SET ""is_cancel"" = ? WHERE ""weight_id"" = ?";
            cmd.Parameters.AddWithValue("", SBool(d, "is_cancel"));
            cmd.Parameters.AddWithValue("", S(d, "weight_id"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("weight_delivery", d);
        }

        // เทียบกับ uploadBT() / handle_upload() ใน C_weight_to_center_local.py
        private async void btULWeight_Click(object sender, EventArgs e)
        {
            string confirmDateStr = tbdateULWeight.Value.ToString("yyyy-MM-dd");
            DialogResult confirmResult = MessageBox.Show(
                $"ต้องการอัพโหลดรายการชั่งวันที่ {confirmDateStr} หรือไม่?",
                "ยืนยันการอัพโหลด", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes)
                return;

            btULWeight.Enabled = false;

            frmDownloadProgress progress = new frmDownloadProgress();
            progress.Show(FindForm());

            try
            {
                string baseUrl = GetBaseApi(1, 1);
                string apiUsername = GetBaseApi(2, 1);
                string apiPassword = GetBaseApi(3, 1);
                string dateStr = tbdateULWeight.Value.ToString("yyyy-MM-dd");

                progress.Log("Authentication...");

                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    string accessToken = await GetJwtTokenAsync(client, baseUrl, apiUsername, apiPassword);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        progress.Log("Authentication failed.");
                        MessageBox.Show("เข้าสู่ระบบ API ไม่สำเร็จ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    progress.Log("Authentication successful.");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    List<string> jsonDataList = new List<string>();
                    dl.connect();
                    try
                    {
                        jsonDataList = GetWeightJsonDataForDate(dateStr);
                    }
                    finally
                    {
                        dl.close();
                    }

                    int totalUploaded = 0;
                    StringBuilder errors = new StringBuilder();

                    if (jsonDataList.Count == 0)
                    {
                        progress.Log("No 'weight_id' to update");
                    }
                    else
                    {
                        string url = $"{baseUrl}/weight/api/create/";

                        foreach (string jsonDataStr in jsonDataList)
                        {
                            try
                            {
                                var content = new StringContent(jsonDataStr, Encoding.UTF8, "application/json");
                                HttpResponseMessage response = await client.PostAsync(url, content);

                                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                                {
                                    string responseText = await response.Content.ReadAsStringAsync();
                                    JObject responseObj = JsonConvert.DeserializeObject<JObject>(responseText);
                                    string weightId = responseObj?["weight_id"]?.ToString();
                                    totalUploaded++;
                                    progress.Log($"upload weight_id: {weightId}");
                                }
                                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                                {
                                    totalUploaded++;
                                    progress.Log("duplicate weight_id!");
                                }
                                else
                                {
                                    errors.AppendLine($"POST request failed with status code {(int)response.StatusCode}");
                                    progress.Log($"POST request failed with status code {(int)response.StatusCode}");
                                }
                            }
                            catch (Exception exRow)
                            {
                                progress.Log($"Error uploading row: {exRow.Message}");
                                errors.AppendLine(exRow.Message);
                            }
                        }
                    }

                    string summary = $"อัพโหลดข้อมูลสำเร็จ {totalUploaded} รายการ";
                    if (errors.Length > 0)
                        summary += "\r\n\r\nข้อผิดพลาด:\r\n" + errors.ToString();

                    progress.Log(summary);

                    MessageBox.Show(summary, "Upload to WebApp", MessageBoxButtons.OK,
                        errors.Length > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                progress.Log("Error: " + ex.Message);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress.AllowClose();
                progress.Close();
                btULWeight.Enabled = true;
            }
        }

        // เทียบกับ sql_query ใน uploadBT() ของ C_weight_to_center_local.py
        private List<string> GetWeightJsonDataForDate(string dateStr)
        {
            List<string> results = new List<string>();

            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                SELECT jsonb_concat(
                    jsonb_build_object(
                        'weight_id', ""weight_id"",
                        'date', ""วันที่"",
                        'date_in', ""วันที่ชั่งเข้า"",
                        'date_out', ""วันที่ชั่งออก"",
                        'time_in', ""เวลาชั่งเข้า"",
                        'time_out', ""เวลาชั่งออก"",
                        'doc_id', ""เลขที่เอกสาร"",
                        'car_registration', ""รหัสทะเบียนรถ"",
                        'car_registration_name', ""ทะเบียนรถ"",
                        'province', ""จังหวัด"",
                        'driver', ""รหัสคนขับ"",
                        'driver_name', ""คนขับ"",
                        'customer', ""รหัสลูกค้า"",
                        'customer_name', ""ลูกค้า"",
                        'site', ""site_id"",
                        'site_name', ""หน้างาน"",
                        'mill', ""mill_id"",
                        'mill_name', ""โรงโม่"",
                        'stone_type', ""stone_type_id"",
                        'stone_type_name', ""ชนิดหิน"",
                        'pay', ""จ่ายเงิน"",
                        'scale_id', ""รหัสผู้ชั่ง"",
                        'scale_name', ""ชื่อผู้ชั่ง"",
                        'scoop', ""รหัสผู้ตัก"",
                        'scoop_name', ""ชื่อผู้ตัก"",
                        'vat_type', ""ชนิดvat"",
                        'stone_color', ""ประเภทหิน"",
                        'car_team', ""car_team_id"",
                        'car_team_name', ""ทีม"",
                        'clean_type', ""ล้าง"",
                        'transport', ""ขนส่ง"",
                        'note', ""หมายเหตุ"",
                        'ship_cost', ""ค่าขนส่ง"",
                        'carry_type_name', ""carry_type_name"",
                        'line_type', ""line_type""
                    ),
                    jsonb_build_object(
                        'bag_type', ""ลักษณะถุง"",
                        'fertilizer_name', ""ชนิดปุ๋ย"",
                        'pack_weight', ""น้ำหนักบรรจุ"",
                        'price_per_ton', ""ราคาตัน"",
                        'vat', ""vat"",
                        'q', ""คิว"",
                        'amount', ""จำนวณเงิน"",
                        'amount_vat', ""จำนวนเงินสุทธิ"",
                        'weight_in', ""น้ำหนักรถ"",
                        'weight_out', ""น้ำหนักรวม"",
                        'weight_total', ""น้ำหนักสินค้า"",
                        'oil_content', ""oil_content"",
                        'origin_weight', ""origin_weight"",
                        'origin_q', ""origin_q"",
                        'freight_cost', ""ค่าบรรทุก"",
                        'ton', ""จำนวนตัน"",
                        'sack', ""จำนวนกระสอบ"",
                        'price_up', ""ค่าขึ้น"",
                        'price_down', ""ค่าลง"",
                        'price_up_total', ""ค่าขึ้นรวม"",
                        'price_down_total', ""ค่าลงรวม"",
                        'freight_cost_total', ""ค่าบรรทุกรวม"",
                        'base_weight_station_name', ""base_weight_station_name"",
                        'is_s', ""is_s"",
                        'is_cancel', ""is_cancel"",
                        'exp_bill', ""exp_bill"",
                        'exp_change', ""exp_change"",
                        'exp_remission', ""exp_remission"",
                        'exp_note', ""exp_note"",
                        'exp_type', ""exp_type"",
                        'bws', ""bws"",
                        'do_doc_no', ""do_doc_no"",
                        'stone_desc', ""stone_desc""
                    )
                ) AS json_data
                FROM public.weight
                WHERE ""วันที่"" = ?
                  AND (""รหัสลูกค้า"" = '99' OR ""รหัสลูกค้า"" = '99RM' OR ""รหัสลูกค้า"" = '09-V-001' OR ""รหัสลูกค้า"" = '09-A-001' OR ""น้ำหนักรวม"" != 0);";
            cmd.Parameters.AddWithValue("", dateStr);

            using (OdbcDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(reader["json_data"].ToString());
                }
            }

            return results;
        }

        // เทียบกับ processs_delivery_order() ใน AU_weight_to_local.py
        // ตั้งค่าตรงกับ backupSql_m.bat ใน C:\Users\Userpc\Documents\backupSqlNew\script\ แต่รันผ่าน pg_dump.exe โดยตรงจาก C# แทนการเรียก .bat
        // พารามิเตอร์การเชื่อมต่อ (host/port/database/user/password) อ่านจาก ODBC DSN "PostgreSQLS" เดียวกับที่ Datalayer ใช้ แทนการฝังค่าตายตัว
        private const string OdbcDsnName = "PostgreSQLS";
        private const string DefaultPgDumpPath = @"C:\Program Files\PostgreSQL\9.5\bin\pg_dump.exe";
        private const string DefaultBackupDir = @"D:\backupSqlNew";
        private static readonly string BackupConfigPath =
            System.IO.Path.Combine(Application.StartupPath, "configs_backup.txt");

        // โหลดค่า PgDumpPath / BackupDir / AutoBackupEnabled จาก configs_backup.txt (key=value ต่อบรรทัด) ถ้าไม่มีไฟล์ใช้ค่า default
        private void LoadBackupConfig()
        {
            string pgDumpPath = DefaultPgDumpPath;
            string backupDir = DefaultBackupDir;
            bool autoBackupEnabled = true;

            if (System.IO.File.Exists(BackupConfigPath))
            {
                foreach (string line in System.IO.File.ReadAllLines(BackupConfigPath))
                {
                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                        continue;

                    string key = line.Substring(0, idx).Trim();
                    string value = line.Substring(idx + 1).Trim();

                    if (key == "PgDumpPath")
                        pgDumpPath = value;
                    else if (key == "BackupDir")
                        backupDir = value;
                    else if (key == "AutoBackupEnabled")
                        bool.TryParse(value, out autoBackupEnabled);
                }
            }

            tbPgDumpPath.Text = pgDumpPath;
            tbBackupDir.Text = backupDir;
            chkAutoBackup.Checked = autoBackupEnabled;
        }

        private void SaveBackupConfig()
        {
            string[] lines =
            {
                "PgDumpPath=" + tbPgDumpPath.Text.Trim(),
                "BackupDir=" + tbBackupDir.Text.Trim(),
                "AutoBackupEnabled=" + chkAutoBackup.Checked,
            };
            System.IO.File.WriteAllLines(BackupConfigPath, lines);
        }

        private void btnBrowsePgDump_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "pg_dump.exe|pg_dump.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                dlg.FileName = "pg_dump.exe";
                if (System.IO.File.Exists(tbPgDumpPath.Text))
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(tbPgDumpPath.Text);

                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    tbPgDumpPath.Text = dlg.FileName;
            }
        }

        private void btnBrowseBackupDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (System.IO.Directory.Exists(tbBackupDir.Text))
                    dlg.SelectedPath = tbBackupDir.Text;

                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    tbBackupDir.Text = dlg.SelectedPath;
            }
        }

        private void btnSaveBackupConfig_Click(object sender, EventArgs e)
        {
            if (!Globals.isPermissionAddSetting())
            {
                MessageBox.Show("คุณไม่มีสิทธิ์บันทึกการตั้งค่านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPgDumpPath.Text) || string.IsNullOrWhiteSpace(tbBackupDir.Text))
            {
                MessageBox.Show("กรุณาระบุ pg_dump.exe และ Backup Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveBackupConfig();
                MessageBox.Show("บันทึกการตั้งค่าสำเร็จ", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกการตั้งค่าไม่สำเร็จ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // เทียบกับค่าที่ psqlODBC เก็บใน HKCU/HKLM\Software\ODBC\ODBC.INI\<DSN>
        private class PgDsnInfo
        {
            public string Host;
            public string Port;
            public string Database;
            public string Username;
            public string Password;
        }

        private static PgDsnInfo GetPgDsnInfo(string dsnName)
        {
            string[] roots =
            {
                $@"SOFTWARE\ODBC\ODBC.INI\{dsnName}",
                $@"SOFTWARE\WOW6432Node\ODBC\ODBC.INI\{dsnName}",
            };

            foreach (var hive in new[] { Microsoft.Win32.Registry.CurrentUser, Microsoft.Win32.Registry.LocalMachine })
            {
                foreach (var subKey in roots)
                {
                    using (var key = hive.OpenSubKey(subKey))
                    {
                        if (key == null)
                            continue;

                        return new PgDsnInfo
                        {
                            Host = key.GetValue("Servername")?.ToString(),
                            Port = key.GetValue("Port")?.ToString(),
                            Database = key.GetValue("Database")?.ToString(),
                            Username = key.GetValue("Username")?.ToString() ?? key.GetValue("UID")?.ToString(),
                            Password = key.GetValue("Password")?.ToString(),
                        };
                    }
                }
            }

            return null;
        }

        private async void btnBackup_Click(object sender, EventArgs e)
        {
            await DoBackupAsync(isAuto: false);
        }

        // ตารางเวลา backup อัตโนมัติ: ทุก 2 ชั่วโมง เริ่ม 09:00 สิ้นสุด 17:00
        private static readonly int[] AutoBackupHours = { 9, 11, 13, 15, 17 };
        private System.Windows.Forms.Timer autoBackupTimer;
        private string lastAutoBackupKey = "";

        private void InitAutoBackupTimer()
        {
            autoBackupTimer = new System.Windows.Forms.Timer { Interval = 60000 };
            autoBackupTimer.Tick += AutoBackupTimer_Tick;
            autoBackupTimer.Start();

            this.Disposed += (s, e) =>
            {
                autoBackupTimer.Stop();
                autoBackupTimer.Dispose();
            };
        }

        private async void AutoBackupTimer_Tick(object sender, EventArgs e)
        {
            if (!chkAutoBackup.Checked)
                return;

            DateTime now = DateTime.Now;
            if (now.Minute != 0 || !AutoBackupHours.Contains(now.Hour))
                return;

            string key = now.ToString("yyyyMMdd_HH");
            if (key == lastAutoBackupKey)
                return;
            lastAutoBackupKey = key;

            if (!btnBackup.Enabled)
                return; // มีการ backup ทำงานอยู่แล้ว (ผู้ใช้กดเอง หรือรอบก่อนหน้ายังไม่เสร็จ)

            await DoBackupAsync(isAuto: true);
        }

        private static readonly string AutoBackupLogPath =
            System.IO.Path.Combine(Application.StartupPath, "backup_auto.log");

        // auto backup ไม่มีหน้าต่างให้เห็น เขียน log ลงไฟล์แทนไว้ตรวจสอบย้อนหลัง
        private static void LogToFile(string message)
        {
            try
            {
                System.IO.File.AppendAllText(AutoBackupLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n");
            }
            catch (Exception)
            {
                // ไม่ทำให้ auto backup ล้มเหลวเพียงเพราะเขียน log ไม่ได้
            }
        }

        // ใช้ร่วมกันทั้งกดปุ่ม backup เองและ auto backup ตามตารางเวลา
        // isAuto = true จะทำงานเบื้องหลังทั้งหมด ไม่เปิดหน้าต่างและไม่เด้ง MessageBox ให้เห็น (log ลงไฟล์แทน)
        private async Task DoBackupAsync(bool isAuto)
        {
            btnBackup.Enabled = false;

            frmDownloadProgress progress = null;
            Action<string> log;
            if (isAuto)
            {
                log = LogToFile;
            }
            else
            {
                progress = new frmDownloadProgress();
                progress.Show(FindForm());
                log = progress.Log;
            }

            try
            {
                string pgDumpPath = tbPgDumpPath.Text.Trim();
                string backupDir = tbBackupDir.Text.Trim();

                if (string.IsNullOrEmpty(pgDumpPath) || !System.IO.File.Exists(pgDumpPath))
                {
                    log($"ไม่พบไฟล์ {pgDumpPath}");
                    if (!isAuto)
                        MessageBox.Show($"ไม่พบไฟล์ {pgDumpPath}", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(backupDir))
                {
                    log("กรุณาระบุ Backup Folder");
                    if (!isAuto)
                        MessageBox.Show("กรุณาระบุ Backup Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PgDsnInfo dsn = GetPgDsnInfo(OdbcDsnName);
                if (dsn == null || string.IsNullOrEmpty(dsn.Host) || string.IsNullOrEmpty(dsn.Database))
                {
                    log($"ไม่พบการตั้งค่า ODBC DSN \"{OdbcDsnName}\"");
                    if (!isAuto)
                        MessageBox.Show($"ไม่พบการตั้งค่า ODBC DSN \"{OdbcDsnName}\"", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!System.IO.Directory.Exists(backupDir))
                    System.IO.Directory.CreateDirectory(backupDir);

                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = System.IO.Path.Combine(backupDir, $"{dsn.Database}_{dateTime}.backup");

                log("===========================================");
                log(isAuto ? "Auto Backup" : "Manual Backup");
                log($"Backup Database : {dsn.Database}");
                log($"Output File     : {backupFile}");
                log("===========================================");

                int exitCode = await RunPgDumpAsync(pgDumpPath, dsn, backupFile, log);

                if (exitCode == 0)
                {
                    log("===== Backup Success =====");
                    log(backupFile);
                    lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) สำเร็จ";
                    if (!isAuto)
                        MessageBox.Show("สำรองข้อมูลสำเร็จ\r\n" + backupFile, "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    log($"XXXXX Backup Failed (exit code {exitCode}) XXXXX");
                    lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) ไม่สำเร็จ";
                    if (!isAuto)
                        MessageBox.Show($"สำรองข้อมูลไม่สำเร็จ (exit code {exitCode})", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                log("Error: " + ex.Message);
                lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) เกิดข้อผิดพลาด";
                if (!isAuto)
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (progress != null)
                {
                    progress.AllowClose();
                    progress.Close();
                }
                btnBackup.Enabled = true;
            }
        }

        // เทียบกับคำสั่ง pg_dump ใน backupSql_m.bat: -h -p -U -F c -b -v -f <file> <database>
        private Task<int> RunPgDumpAsync(string pgDumpPath, PgDsnInfo dsn, string backupFile, Action<string> log)
        {
            var tcs = new TaskCompletionSource<int>();

            string port = string.IsNullOrEmpty(dsn.Port) ? "5432" : dsn.Port;

            var psi = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {dsn.Host} -p {port} -U {dsn.Username} -F c -b -v -f \"{backupFile}\" {dsn.Database}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            psi.EnvironmentVariables["PGPASSWORD"] = dsn.Password ?? "";

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            process.OutputDataReceived += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(ev.Data))
                    log(ev.Data);
            };
            process.ErrorDataReceived += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(ev.Data))
                    log(ev.Data);
            };
            process.Exited += (s, ev) =>
            {
                tcs.TrySetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private void ProcessDeliveryOrderUpdate(JObject d)
        {
            OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
            cmd.CommandText = @"
                UPDATE public.delivery_order
                SET
                    ""car_company_rem"" = ?,
                    ""car_customer_rem"" = ?,
                    ""car_company_tot"" = ?,
                    ""car_customer_tot"" = ?,
                    ""qty_tot"" = ?
                WHERE ""doc_no"" = ?;";
            cmd.Parameters.AddWithValue("", S(d, "car_company_rem"));
            cmd.Parameters.AddWithValue("", S(d, "car_customer_rem"));
            cmd.Parameters.AddWithValue("", S(d, "car_company_tot"));
            cmd.Parameters.AddWithValue("", S(d, "car_customer_tot"));
            cmd.Parameters.AddWithValue("", S(d, "qty_tot"));
            cmd.Parameters.AddWithValue("", S(d, "doc_no"));
            cmd.ExecuteNonQuery();

            UpdateVStamp("delivery_order", d);
        }

    }
}
