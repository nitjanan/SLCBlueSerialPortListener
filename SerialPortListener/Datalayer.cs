using Devart.Data.PostgreSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;

namespace SerialPortListener
{
    public class Datalayer
    {
        OdbcConnection conn;
        OdbcCommand cmd;
        int connDepth = 0;
        public string getmassage { get; set; }
        public Datalayer() {
            //String cs = "User Id=postgres;Host=localhost;Database=truck;Password=postgres;Initial Schema=public;charset=UTF8";
            //String cs = "User Id=sa;Host=192.168.10.132;Database=truck;Password=123456;Initial Schema=public;charset=UTF8";
            string cs = "DSN=PostgreSQLM";
            //string cs = "DSN=PostgreSQLS";
            conn = new OdbcConnection(cs);
            cmd = new OdbcCommand();
        }
        public bool connect() {
            // Reentrant: if a caller already opened the connection (e.g. to batch
            // several queries), nested connect() calls just bump the depth instead
            // of paying for another network round-trip to open the connection.
            if (conn.State == ConnectionState.Open)
            {
                connDepth++;
                getmassage = "Connect successfully";
                return true;
            }
            try {
                conn.Open();
                connDepth = 1;
                getmassage = "Connect successfully";
                return true;
            }
            catch (Exception) {
                getmassage = "Connect fail";
                return false;
            }
        }

        public OdbcConnection sqlConn()
        {
            return conn;
        }
        public bool close()
        {
            // Only physically close once the outermost connect() is matched.
            if (connDepth > 1)
            {
                connDepth--;
                getmassage = "Close successfully";
                return true;
            }
            try
            {
                conn.Close();
                connDepth = 0;
                getmassage = "Close successfully";
                return true;
            }
            catch (Exception)
            {
                getmassage = "Close fail";
                return false;
            }
        }
    }
}
