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

        // Runs the Krabi STP mode DB migration once before any form is shown.
        // Non-fatal on failure - see MigrationRunner.
        static void RunStartupMigration()
        {
            Datalayer dl = new Datalayer();
            try
            {
                dl.connect();
                MigrationRunner.RunKrabiStpMigration(dl.sqlConn());
            }
            finally
            {
                dl.close();
            }
        }
    }
}
