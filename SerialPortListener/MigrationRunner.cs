using System;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;

namespace SerialPortListener
{
    // Runs the additive, idempotent Krabi STP mode DB migration once per startup.
    // Never blocks the app from starting on failure - logs and continues, since the
    // app's Standard-mode behavior does not depend on these objects existing.
    static class MigrationRunner
    {
        public static void RunKrabiStpMigration(OdbcConnection connection)
        {
            try
            {
                // Skip entirely if already applied - avoids taking a table lock on every single
                // startup once the migration has already run once.
                OdbcCommand checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'weight' AND column_name = 'origin_weight'";
                object result = checkCmd.ExecuteScalar();
                if (result != null)
                    return; // already migrated

                string sql;
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("2026-09-29-krabi-stp-mode.sql", StringComparison.OrdinalIgnoreCase));
                if (resourceName == null)
                    return;

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    sql = reader.ReadToEnd();
                }

                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Krabi STP migration failed (non-fatal): " + ex.Message);
            }
        }
    }
}
