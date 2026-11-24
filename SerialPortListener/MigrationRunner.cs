using System;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;

namespace SerialPortListener
{
    // migration ฐานในเครื่องที่รันตอนเปิดโปรแกรม (ซ้ำได้ ไม่ทำให้โปรแกรมเปิดไม่ได้ถ้าล้มเหลว)
    static class MigrationRunner
    {
        // ตาราง sync_outbox + trigger บน weight สำหรับ WeightMirrorSync ; ไม่มีในเครื่องจะสร้างให้อัตโนมัติ
        // ข้ามเฉพาะเมื่อมีครบทั้งตารางและ trigger ทั้งสอง เพราะ DROP/CREATE TRIGGER ต้อง lock ตาราง weight
        // (เช่นกู้ฐานจาก backup เก่าที่มี trigger แต่ไม่มีตาราง : trigger จะกลืน error ทิ้ง รายการชั่งไม่เข้าคิวเลย)
        // ถูกเรียกตอนเปิดโปรแกรม และจาก WeightMirrorSync เมื่อพบว่าตารางหายระหว่างใช้งาน
        public static void RunWeightMirrorMigration(OdbcConnection connection)
        {
            if (!WeightMirrorSync.Enabled)
            {
                RemoveWeightMirrorTriggers(connection);
                return;
            }
            try
            {
                if (IsWeightMirrorInstalled(connection))
                    return;

                string sql = ReadEmbeddedSql("2026-11-23-weight-mirror-outbox.sql");
                if (sql == null)
                    return;

                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Weight mirror migration failed (non-fatal): " + ex.Message);
            }
        }

        // ตอนปิดระบบสำเนา : ลบ trigger ออก ไม่ให้รายการชั่งเข้าคิว sync_outbox ค้างพองไปเรื่อย ๆ โดยไม่มีใครส่ง
        // และล้างแถวที่ค้างในคิวทิ้ง (ไม่มีตัวส่งแล้ว) ; ตัวตาราง sync_outbox เก็บไว้ ไม่กระทบการชั่ง
        // ทำเฉพาะเมื่อยังมี trigger / แถวค้างอยู่ เพราะ DROP TRIGGER ต้อง lock ตาราง weight
        private static void RemoveWeightMirrorTriggers(OdbcConnection connection)
        {
            try
            {
                using (OdbcCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT count(*) FROM pg_trigger WHERE tgrelid = 'public.weight'::regclass" +
                        " AND tgname IN ('slc_weight_mirror_ins_del', 'slc_weight_mirror_upd')";
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    {
                        cmd.CommandText =
                            "DROP TRIGGER IF EXISTS slc_weight_mirror_ins_del ON public.weight;" +
                            " DROP TRIGGER IF EXISTS slc_weight_mirror_upd ON public.weight;";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT CASE WHEN to_regclass('public.sync_outbox') IS NULL THEN 0 ELSE 1 END";
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    {
                        cmd.CommandText = "DELETE FROM public.sync_outbox";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Remove weight mirror triggers failed (non-fatal): " + ex.Message);
            }
        }

        // psqlODBC คืน boolean เป็นตัวอักษร (BoolsAsChar) จึงนับเป็นตัวเลข
        public static bool IsWeightMirrorInstalled(OdbcConnection connection)
        {
            using (OdbcCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT CASE WHEN to_regclass('public.sync_outbox') IS NULL THEN 0 ELSE 1 END" +
                    " + (SELECT count(*) FROM pg_trigger WHERE tgrelid = 'public.weight'::regclass" +
                    "    AND tgname IN ('slc_weight_mirror_ins_del', 'slc_weight_mirror_upd'))";
                return Convert.ToInt32(cmd.ExecuteScalar()) == 3;
            }
        }

        private static string ReadEmbeddedSql(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                return null;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.IO.StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
