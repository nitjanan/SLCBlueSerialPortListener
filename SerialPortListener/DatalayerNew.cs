using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace SerialPortListener
{
    public class DatalayerNew
    {
        private string connectionString = "DSN=PostgreSQLS";

        public OdbcConnection CreateConnection()
        {
            var conn = new OdbcConnection(connectionString);
            conn.StateChange += (s, e) =>
            {
                if (e.CurrentState == System.Data.ConnectionState.Open)
                    DbDate.ApplySessionDateStyle(conn);
            };
            return conn;
        }
    }
}
