using System;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SerialPortListener
{
    public class AppReleaseInfo
    {
        public string version { get; set; }
        public string channel { get; set; }
        public string release_notes { get; set; }
        public string download_url { get; set; }
        public string file_hash_sha256 { get; set; }
        public string sql_script_url { get; set; }
        public string sql_script_hash_sha256 { get; set; }
        public bool is_mandatory { get; set; }
    }

    // เรียก Django weight webapp API เพื่อเช็คเวอร์ชันล่าสุดและดาวน์โหลดตัวติดตั้งใหม่
    public static class AppUpdateService
    {
        public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        public static async Task<AppReleaseInfo> GetLatestReleaseAsync(
            HttpClient client, string baseUrl, string accessToken,
            string product = "slcblue")
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            string url = $"{baseUrl}/api/updates/latest/?product={product}";
             
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AppReleaseInfo>(json);
        }

        public static async Task LogUpdateAsync(
            HttpClient client, string baseUrl, string accessToken,
            string machineName, string fromVersion, string toVersion, bool updateApplied,
            string weightStationCode = null, bool sqlApplied = false)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new
            {
                machine_name = machineName,
                from_version = fromVersion,
                to_version = toVersion,
                update_applied = updateApplied,
                weight_station = weightStationCode,
                sql_applied = sqlApplied
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync($"{baseUrl}/api/updates/log/", content);
            }
            catch
            {
                // การ log ไม่สำเร็จไม่ควรทำให้ขั้นตอนอัพเดทหยุด
            }
        }

        public static bool IsNewerVersion(string remoteVersion, Version currentVersion)
        {
            return Version.TryParse(remoteVersion, out Version remote) && remote > currentVersion;
        }

        // ดาวน์โหลด zip ของโฟลเดอร์ installer ทั้งหมด (setup.exe + XXX.msi + setup.ini ที่ต้องอยู่ด้วยกัน)
        // ตรวจสอบ SHA256 ของ zip แล้วแตกไฟล์ไปที่โฟลเดอร์ temp ใหม่ คืน path ของ setup.exe ที่แตกออกมา
        public static async Task<string> DownloadInstallerAsync(HttpClient client, AppReleaseInfo release, string baseUrl, IProgress<int> progress = null)
        {
            Uri expectedBase = new Uri(baseUrl);
            Uri downloadUri = new Uri(release.download_url);
            if (!string.Equals(downloadUri.Scheme, expectedBase.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(downloadUri.Host, expectedBase.Host, StringComparison.OrdinalIgnoreCase) ||
                downloadUri.Port != expectedBase.Port)
            {
                throw new InvalidOperationException("Download URL host ไม่ตรงกับ Server ที่ตั้งค่าไว้ — ปฏิเสธการอัพเดท");
            }

            if (string.IsNullOrEmpty(release.file_hash_sha256))
            {
                throw new InvalidOperationException("Server ไม่ได้ส่งค่า SHA256 มาด้วย — ปฏิเสธการติดตั้งไฟล์ที่ไม่ได้ตรวจสอบ");
            }

            string zipPath = Path.Combine(Path.GetTempPath(), $"SLCBlueSetup_{Guid.NewGuid():N}.zip");

            using (HttpResponseMessage response = await client.GetAsync(release.download_url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? -1;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        if (totalBytes > 0)
                            progress?.Report((int)(totalRead * 100 / totalBytes));
                    }
                }
            }

            string actualHash = ComputeSha256(zipPath);
            if (!string.Equals(actualHash, release.file_hash_sha256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(zipPath);
                throw new InvalidOperationException("ไฟล์ติดตั้งที่ดาวน์โหลดมามีค่า SHA256 ไม่ตรงกับที่กำหนด");
            }

            string extractDir = Path.Combine(Path.GetTempPath(), $"SLCBlueSetup_{Guid.NewGuid():N}");
            Directory.CreateDirectory(extractDir);
            ExtractZipSafely(zipPath, extractDir);
            File.Delete(zipPath);

            string setupExePath = Directory.GetFiles(extractDir, "setup.exe", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (setupExePath == null)
            {
                throw new InvalidOperationException("ไม่พบไฟล์ setup.exe ในไฟล์ติดตั้งที่ดาวน์โหลดมา");
            }

            return setupExePath;
        }

        // ดาวน์โหลดสคริปต์ SQL (ถ้ามี), ตรวจสอบ SHA256 แล้วรันกับ PostgreSQL local ผ่าน connection ที่ส่งเข้ามา
        // คืนค่า true ถ้ารันสำเร็จ, false ถ้าไม่มีสคริปต์ SQL สำหรับ release นี้
        public static async Task<bool> DownloadAndRunSqlScriptAsync(HttpClient client, AppReleaseInfo release, string baseUrl, OdbcConnection connection)
        {
            if (string.IsNullOrEmpty(release.sql_script_url))
                return false;

            Uri expectedBase = new Uri(baseUrl);
            Uri sqlUri = new Uri(release.sql_script_url);
            if (!string.Equals(sqlUri.Scheme, expectedBase.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(sqlUri.Host, expectedBase.Host, StringComparison.OrdinalIgnoreCase) ||
                sqlUri.Port != expectedBase.Port)
            {
                throw new InvalidOperationException("SQL script URL host ไม่ตรงกับ Server ที่ตั้งค่าไว้ — ปฏิเสธการรัน SQL");
            }

            if (string.IsNullOrEmpty(release.sql_script_hash_sha256))
            {
                throw new InvalidOperationException("Server ไม่ได้ส่งค่า SHA256 ของสคริปต์ SQL มาด้วย — ปฏิเสธการรันสคริปต์ที่ไม่ได้ตรวจสอบ");
            }

            string sqlPath = Path.Combine(Path.GetTempPath(), $"SLCBlueUpdate_{Guid.NewGuid():N}.sql");

            using (HttpResponseMessage response = await client.GetAsync(release.sql_script_url))
            {
                response.EnsureSuccessStatusCode();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(sqlPath, bytes);
            }

            try
            {
                string actualHash = ComputeSha256(sqlPath);
                if (!string.Equals(actualHash, release.sql_script_hash_sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("สคริปต์ SQL ที่ดาวน์โหลดมามีค่า SHA256 ไม่ตรงกับที่กำหนด");
                }

                string sqlText = File.ReadAllText(sqlPath, Encoding.UTF8);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // รันทีละคำสั่งแยก transaction ต่อคำสั่ง (ไม่ใช้ transaction ก้อนใหญ่ก้อนเดียว)
                // เพราะถ้าคำสั่งไหน error ใน PostgreSQL transaction จะเข้าสถานะ aborted
                // ทำให้คำสั่งถัดไปใน transaction เดียวกันรันไม่ได้ต่อ แม้จะ catch exception ไว้ก็ตาม
                foreach (string statement in SplitSqlStatements(sqlText))
                {
                    using (OdbcTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (OdbcCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = statement;
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (Exception stmtEx)
                        {
                            transaction.Rollback();

                            // ข้ามเฉพาะ error ที่แปลว่า column/table/object มีอยู่แล้ว (idempotent)
                            // SQLSTATE: 42701=duplicate column, 42P07=duplicate table, 42710=duplicate object
                            if (!IsAlreadyExistsError(stmtEx))
                            {
                                string preview = statement.Length > 300 ? statement.Substring(0, 300) + "..." : statement;
                                throw new InvalidOperationException(
                                    $"รันคำสั่ง SQL ล้มเหลว: {stmtEx.Message}\r\n\r\nคำสั่งที่รัน:\r\n{preview}", stmtEx);
                            }
                        }
                    }
                }

                return true;
            }
            finally
            {
                File.Delete(sqlPath);
            }
        }

        // เช็คว่า error เป็นแบบ "มีอยู่แล้ว" (idempotent) ที่ข้ามได้อย่างปลอดภัยหรือไม่
        // SQLSTATE ของ PostgreSQL: 42701=duplicate column, 42P07=duplicate table, 42710=duplicate object,
        // 42P06=duplicate schema, 42723=duplicate function, 42P04=duplicate database, 42P16=invalid table definition(บางกรณี)
        private static bool IsAlreadyExistsError(Exception ex)
        {
            string message = ex.Message ?? string.Empty;
            if (message.IndexOf("42701", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("42P07", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("42710", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("42P06", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("42723", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("42P04", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // แยกสคริปต์ SQL ด้วย ';' ที่จบบรรทัดคำสั่ง (ไม่รองรับ ';' ภายใน string literal ที่ซับซ้อน — ใช้กับสคริปต์ migration ธรรมดาเท่านั้น)
        // แยกสคริปต์ SQL ด้วย ';' แต่รู้จัก string literal ('...'), quoted identifier ("..."),
        // comment (-- และ /* */) และ dollar-quoted block ($$...$$ หรือ $tag$...$tag$ ของ PL/pgSQL)
        // เพื่อไม่ตัดกลางคำสั่งเมื่อมี ';' ปนอยู่ในสิ่งเหล่านี้
        private static System.Collections.Generic.IEnumerable<string> SplitSqlStatements(string sqlText)
        {
            var statements = new System.Collections.Generic.List<string>();
            var current = new StringBuilder();
            int i = 0;
            int len = sqlText.Length;
            string dollarTag = null;

            while (i < len)
            {
                char c = sqlText[i];

                if (dollarTag != null)
                {
                    if (c == '$' && string.CompareOrdinal(sqlText, i, dollarTag, 0, dollarTag.Length) == 0)
                    {
                        current.Append(dollarTag);
                        i += dollarTag.Length;
                        dollarTag = null;
                        continue;
                    }
                    current.Append(c);
                    i++;
                    continue;
                }

                if (c == '-' && i + 1 < len && sqlText[i + 1] == '-')
                {
                    int lineEnd = sqlText.IndexOf('\n', i);
                    if (lineEnd < 0) lineEnd = len;
                    current.Append(sqlText.Substring(i, lineEnd - i));
                    i = lineEnd;
                    continue;
                }

                if (c == '/' && i + 1 < len && sqlText[i + 1] == '*')
                {
                    int blockEnd = sqlText.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (blockEnd < 0) blockEnd = len - 2;
                    current.Append(sqlText.Substring(i, blockEnd + 2 - i));
                    i = blockEnd + 2;
                    continue;
                }

                if (c == '\'' || c == '"')
                {
                    char quote = c;
                    current.Append(c);
                    i++;
                    while (i < len)
                    {
                        current.Append(sqlText[i]);
                        if (sqlText[i] == quote)
                        {
                            if (i + 1 < len && sqlText[i + 1] == quote)
                            {
                                current.Append(sqlText[i + 1]);
                                i += 2;
                                continue;
                            }
                            i++;
                            break;
                        }
                        i++;
                    }
                    continue;
                }

                if (c == '$')
                {
                    int tagEnd = i + 1;
                    while (tagEnd < len && (char.IsLetterOrDigit(sqlText[tagEnd]) || sqlText[tagEnd] == '_'))
                        tagEnd++;
                    if (tagEnd < len && sqlText[tagEnd] == '$')
                    {
                        dollarTag = sqlText.Substring(i, tagEnd - i + 1);
                        current.Append(dollarTag);
                        i = tagEnd + 1;
                        continue;
                    }
                }

                if (c == ';')
                {
                    string statement = current.ToString().Trim();
                    if (!string.IsNullOrEmpty(statement))
                        statements.Add(statement);
                    current.Clear();
                    i++;
                    continue;
                }

                current.Append(c);
                i++;
            }

            string tail = current.ToString().Trim();
            if (!string.IsNullOrEmpty(tail))
                statements.Add(tail);

            return statements;
        }

        // แตกไฟล์ zip ทีละ entry พร้อมตรวจสอบว่าไม่มี entry ไหน (เช่น ../../evil.exe) หลุดออกนอก extractDir
        private static void ExtractZipSafely(string zipPath, string extractDir)
        {
            string fullExtractDir = Path.GetFullPath(extractDir) + Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string destPath = Path.GetFullPath(Path.Combine(extractDir, entry.FullName));
                    if (!destPath.StartsWith(fullExtractDir, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"พบไฟล์ในตัวติดตั้งที่พยายามหลุดออกนอกโฟลเดอร์ที่กำหนด: '{entry.FullName}'");
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    entry.ExtractToFile(destPath, overwrite: false);
                }
            }
        }

        private static string ComputeSha256(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // เรียก installer ผ่าน batch ที่รอให้ process ของโปรแกรมปัจจุบัน exit จริงๆ ก่อน
        // (ป้องกันหน้าต่าง "Files in Use" ของ Windows Installer เพราะ exe ยังไม่ถูกปลดจาก memory ทันทีหลัง Application.Exit())
        public static void RunInstallerAndExit(string installerPath)
        {
            int currentPid = Process.GetCurrentProcess().Id;
            string exeName = Process.GetCurrentProcess().ProcessName + ".exe";
            string batchPath = Path.Combine(Path.GetTempPath(), $"SLCBlueWaitInstall_{Guid.NewGuid():N}.bat");

            string batchContent =
                "@echo off\r\n" +
                ":wait\r\n" +
                $"tasklist /FI \"PID eq {currentPid}\" | find \"{currentPid}\" >nul\r\n" +
                "if not errorlevel 1 (\r\n" +
                "  timeout /t 1 /nobreak >nul\r\n" +
                "  goto wait\r\n" +
                ")\r\n" +
                // เผื่อมีอีก instance ของโปรแกรมที่เปิดค้างไว้ต่างหาก (ไม่ใช่ตัวที่สั่งอัพเดท) ให้ปิดทิ้งด้วย
                // กันไม่ให้ installer เจอไฟล์ถูกล็อกจาก process อื่นที่ไม่ใช่ตัวเอง
                $"taskkill /F /IM \"{exeName}\" >nul 2>&1\r\n" +
                // process หายจาก tasklist แล้ว แต่ OS อาจยังไม่ปลด file handle ทันที รอเผื่อไว้อีกนิด
                "timeout /t 3 /nobreak >nul\r\n" +
                $"start \"\" \"{installerPath}\"\r\n" +
                $"del \"%~f0\"\r\n";

            File.WriteAllText(batchPath, batchContent, Encoding.ASCII);

            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });

            // Application.Exit() แค่ปิด message loop — ถ้าแอปมี foreground thread ค้างอยู่ (เช่น thread อ่าน serial port)
            // process จะไม่ตายจริง ทำให้ installer ยังเห็นว่าไฟล์ถูกล็อกอยู่ ต้องบังคับปิด process ทันทีด้วย Environment.Exit
            Environment.Exit(0);
        }
    }
}
