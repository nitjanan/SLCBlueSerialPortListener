using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class Login : Form
    {
        Datalayer dl;
        public Login()
        {
            dl = new Datalayer();
            InitializeComponent();
            setDefault();
        }
        private void setDefault(){
            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_company where base_company_id = 1 ";
            try
             {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    Company.CompanyName  = reader["company_name"].ToString();
                    Company.Address = reader["address"].ToString();
                    Company.Telephone = reader["telephone"].ToString();
                    Company.Email = reader["email"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

        }
        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            try {

                //MessageBox.Show("test = "+ dl.connect());
                string sql = "select username, firstname, password, permission from public.users where username = '" + tbUsername.Text + "' and password = '" + Utils.hashPassword(tbPassword.Text) + "'";
                OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
                DataTable dt = new DataTable();
                dl.connect();
                cmd.Fill(dt);

                if (dt.Rows.Count == 1)
                {

                    this.Hide();
                    String rUsername = dt.Rows[0][0].ToString();
                    String rFirstname = dt.Rows[0][1].ToString();
                    String rPassword = dt.Rows[0][2].ToString();
                    String rPermission = dt.Rows[0][3].ToString();
                    Globals.Username = rUsername;
                    Globals.Firstname = rFirstname;
                    Globals.Password = rPassword;
                    Globals.Permission = rPermission;
                    MainForm mf = new MainForm(rUsername, rFirstname);
                    mf.ShowDialog();
                }
                else {
                    MessageBox.Show("username หรือ password ไม่ถูกต้อง", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dl.close();

            }catch (Exception){
                
            }

                /*
                //sql find company
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "select username, firstname, password, permission from public.users where username = '" + tbUsername.Text + "' and password = '" + Utils.hashPassword(tbPassword.Text) + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        this.Hide();
                        String rUsername = reader.GetString("username");
                        String rFirstname = reader.GetString("firstname");
                        String rPassword = reader.GetString("password");
                        String rPermission = reader.GetString("permission");
                        Globals.Username = rUsername;
                        Globals.Firstname = rFirstname;
                        Globals.Password = rPassword;
                        Globals.Permission = rPermission;
                        MainForm mf = new MainForm(rUsername, rFirstname);
                        mf.ShowDialog();
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
                */
            }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) {
                button1_Click(sender, e);
            }
        }
    }
}
