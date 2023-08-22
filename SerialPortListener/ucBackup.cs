using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class ucBackup : UserControl
    {
        Datalayer dl;
        public ucBackup()
        {
            InitializeComponent();
            dl = new Datalayer();
        }

        private void btBrowBackup_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK) {
                tbLocalBackup.Text = dlg.SelectedPath;
                btBackup.Enabled = true;
            }
        }

        private void btBackup_Click(object sender, EventArgs e)
        {
            string database = dl.sqlConn().Database.ToString();
            try
            {
                if (tbLocalBackup.Text == string.Empty)
                {
                    MessageBox.Show("กรุณาเลือกโฟลเดอร์ที่ต้องการ backup ข้อมูล");
                }
                else {
                    /*
                    MessageBox.Show(database);
                    string cmd = "BACKUP DATABASE " + database + " TO DISK = '" + tbLocalBackup.Text + "\\" + "database" + "-" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".bak'";
                    MessageBox.Show(cmd);
                    using (OdbcCommand command = new OdbcCommand(cmd, dl.sqlConn()))
                    {
                        MessageBox.Show("3");
                        if (dl.sqlConn().State != ConnectionState.Open) {
                            dl.connect();
                            MessageBox.Show("4");
                        }
                        MessageBox.Show("5");
                        command.ExecuteNonQuery();
                        MessageBox.Show("6");
                        MessageBox.Show("backup ข้อมูลเรียบร้อยแล้ว");
                        dl.close();
                        MessageBox.Show("7");
                        MessageBox.Show("backup ข้อมูลเรียบร้อยแล้ว");
                        MessageBox.Show("8");
                        btBackup.Enabled = false;



                    }
                    */
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "BACKUP DATABASE " + database + " TO DISK = '" + tbLocalBackup.Text + "\\" + "database" + "-" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".bak'";
                    try
                    {
                        dl.connect();
                        pgCommand.ExecuteNonQuery();
                        MessageBox.Show("backup ข้อมูลเรียบร้อยแล้ว");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();
                }
            }
            catch (Exception ex) { 
            
            }
        }

    }
}
