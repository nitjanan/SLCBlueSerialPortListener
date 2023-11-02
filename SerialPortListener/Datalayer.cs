using Devart.Data.PostgreSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace SerialPortListener
{
    public class Datalayer
    {
        OdbcConnection conn;
        OdbcCommand cmd;
        public string getmassage { get; set; }
        public Datalayer() {
            //String cs = "User Id=postgres;Host=localhost;Database=truck;Password=postgres;Initial Schema=public;charset=UTF8";
            //String cs = "User Id=sa;Host=192.168.10.132;Database=truck;Password=123456;Initial Schema=public;charset=UTF8";
            //string cs = "DSN=PostgreSQLM";
            //string cs = "DSN=PostgreSQLS";
            //string cs = "DSN=PostgreSQLCenterS";
            //string cs = "DSN=PostgreSQLCenterM";
            //string cs = "DSN=PostgreSQLCenterSTOCK";
            string cs = "DSN=PostgreSQLCenterALL";
            conn = new OdbcConnection(cs);
            cmd = new OdbcCommand();
        }
        public bool connect() {
            try {
                conn.Open();
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
            try
            {
                conn.Close();
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
