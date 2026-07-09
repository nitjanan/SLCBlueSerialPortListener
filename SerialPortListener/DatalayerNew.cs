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
        private string connectionString = "DSN=PostgreSQLM";

        public OdbcConnection CreateConnection()
        {
            return new OdbcConnection(connectionString);
        }
    }
}
