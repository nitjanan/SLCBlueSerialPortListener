using System;
using System.Data.Odbc;
using System.IO;
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
                string sqlPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "sql", "2026-09-29-krabi-stp-mode.sql");

                if (!File.Exists(sqlPath))
                    return;

                string sql = File.ReadAllText(sqlPath);
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
