using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class FSettingLine : Form
    {
        MainForm mainForm;
        Datalayer dl = null;

        public FSettingLine(MainForm parent)
        {
            InitializeComponent();
            mainForm = parent;
            dl = new Datalayer();
            //fillTableCombo(cbbSite, "base_site", "base_site_name");
            fillTableComboByWeightType(cbbSite, "base_site", "base_site_name");
            setDataSetting();
        }

        private void setDataSetting()
        {
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * from base_setting_line WHERE base_setting_line_id = 1";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {

                    dtDateFrom.Text = reader["base_setting_line_date_from"].ToString();
                    dtFromOut.Text = reader["base_setting_line_time_from"].ToString();
                    cbbSite.Text = reader["base_site_name"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillTableCombo(ComboBox cbb, string tableName, string field)
        {
            //ล้างก่อน
            cbb.Items.Clear();

            //
            cbb.Items.Add("ทั้งหมด");
            cbb.SelectedIndex = 0;

            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader[field].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillTableComboByWeightType(ComboBox cbb, string tableName, string field)
        {
            //ล้างก่อน
            cbb.Items.Clear();

            //
            cbb.Items.Add("ทั้งหมด");
            cbb.SelectedIndex = 0;

            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName + " where weight_type = 4";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader[field].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();

        }

        private void btSaveLine_Click(object sender, EventArgs e)
        {
            string dateFrom = dtDateFrom.Value.ToString("yyyy-MM-dd");
            string timeFrom = dtFromOut.Text;
            string siteName = cbbSite.Text;

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_setting_line_id from base_setting_line WHERE base_setting_line_id = 1";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                bool hasRows = reader.HasRows;
                dl.close();

                dl.connect();
                pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                //update
                if (hasRows)
                {
                    pgCommand.CommandText = "UPDATE base_setting_line SET base_setting_line_date_from = ? , base_setting_line_time_from = ? , base_site_name = ? WHERE base_setting_line_id = 1";
                }
                //save
                else
                {
                    pgCommand.CommandText = "INSERT into base_setting_line (base_setting_line_id, base_setting_line_date_from, base_setting_line_time_from, base_site_name) VALUES (1, ?, ?, ? )";
                }
                pgCommand.Parameters.Add("@dateFrom", OdbcType.VarChar).Value = dateFrom;
                pgCommand.Parameters.Add("@timeFrom", OdbcType.VarChar).Value = timeFrom;
                pgCommand.Parameters.Add("@siteName", OdbcType.VarChar).Value = siteName;
                pgCommand.ExecuteNonQuery();
                MessageBox.Show("บันทึกเรียบร้อย", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ กรุณาลองใหม่อีกครั้ง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();

            mainForm.RefreshLineTypeTotals();

        }
    }
}
