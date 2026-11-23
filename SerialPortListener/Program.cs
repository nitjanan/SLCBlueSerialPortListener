using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SerialPortListener
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunStartupMigration();

            Application.Run(new Login());
        }

        // สร้างตาราง sync_outbox + trigger ถ้ายังไม่มี (ไม่ทำให้โปรแกรมเปิดไม่ได้ถ้าล้มเหลว ดู MigrationRunner)
        static void RunStartupMigration()
        {
            Datalayer dl = new Datalayer();
            try
            {
                dl.connect();
                MigrationRunner.RunWeightMirrorMigration(dl.sqlConn());
            }
            finally
            {
                dl.close();
            }
        }
    }
}
