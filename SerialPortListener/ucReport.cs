using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class ucReport : UserControl
    {
        Datalayer dl;

        private static ucReport _instance;
        private static ucReport Instance
        {

            get
            {
                if (_instance == null)
                    _instance = new ucReport();
                return _instance;
            }
        }
        // Bind default keywords
        List<string> listOriginalCustomerReport = new List<string>();
        // save new keywords
        List<string> listNewCustomerNameReport = new List<string>();

        // Bind default keywords
        List<string> listOriginalInvoiceReport = new List<string>();
        // save new keywords
        List<string> listNewInvoiceReport = new List<string>();

        public ucReport()
        {
            dl = new Datalayer();
            InitializeComponent();
            setDefaultFormatDTGV();
            /* autoComplete ลูกค้า */
            autoCompleteSetting(tbCustomerId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbCustomerName, "ชื่อลูกค้า", "base_customer");
            /* autoComplete แจ้งหนี้ */
            autoCompleteSetting(tbInvoiceCutomerId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbInvoiceCutomerName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete ทะเบียนรถ */
            autoCompleteSettingDistinct(tbJointCarRegistration, "ทะเบียนรถ", "weight");

            /* autoComplete ชื่อผู้ตัก */
            autoCompleteSettingDistinct(tbScoopName, "ชื่อผู้ตัก", "weight");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbCustomerName, listOriginalCustomerReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbInvoiceCutomerName, listOriginalInvoiceReport);

        }

        /*3 search anywhere customer */
        public void setautoCompleteCustomer(string fieldId, string fieldName, string tableName, ComboBox cbb, List<string> listOriginal)
        {
            cbb.Items.Clear();
            listOriginal.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE weight_type = 1 or weight_type = 3 ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() + " : " + reader[fieldName].ToString();
                    listOriginal.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();

            cbb.Items.AddRange(listOriginal.ToArray());
        }

        private void setDefaultFormatDTGV() {
            dgvDailyReport.Columns["ราคาตัน"].DefaultCellStyle.Format = "n2";
            dgvDailyReport.Columns["จำนวนเงิน"].DefaultCellStyle.Format = "n2";
            dgvDailyReport.Columns["จำนวนเงินสุทธิ"].DefaultCellStyle.Format = "n2";
            dgvDailyReport.Columns["vat"].DefaultCellStyle.Format = "n2";
            dgvDailyReport.Columns["คิว"].DefaultCellStyle.Format = "n2";

            dgvDailyReport.Columns["น้ำหนักรถ"].DefaultCellStyle.Format = "n3";
            dgvDailyReport.Columns["น้ำหนักรวม"].DefaultCellStyle.Format = "n3";
            dgvDailyReport.Columns["น้ำหนักสินค้า"].DefaultCellStyle.Format = "n3";
        }

        /* autoComplete Setting */
        private void autoCompleteSetting(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString().Trim();
                    if (!string.IsNullOrEmpty(rdStr))
                    {
                        coll.Add(rdStr);
                    }
                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /* autoComplete Setting  แบบข้อมูลไม่ซ้ำกัน */
        private void autoCompleteSettingDistinct(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT DISTINCT "+ field + " FROM public." + tableName;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString().Trim();
                    if (!string.IsNullOrEmpty(rdStr))
                    {
                        coll.Add(rdStr);
                    }
                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        private void setDataWeightToDTGV()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.weight WHERE วันที่ >= '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "' AND  วันที่ <= '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "' ORDER BY วันที่, เลขที่เอกสาร");
                OdbcDataAdapter adt = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                DataTable dt = new DataTable();
                adt.Fill(dt);
                dgvDailyReport.DataSource = dt;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void ucReport_Load(object sender, EventArgs e)
        {
            //this.weightTableAdapter.Fill(this.truckDataSet.weight);
            //this.weightBindingSource.Filter = string.Format("วันที่ = '" + tbdateTo.Text + "' AND ( NOT น้ำหนักรวม = 0.00 OR  รหัสลูกค้า = '999' ) ");
            setDataWeightToDTGV();
        }

        private void tcReport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcReport.SelectedTab == tabPage6){
                fillCarTeamCombo(cbbCarTeamName);
            }else if(tcReport.SelectedTab == tabPage12){
                fillCarTeamCombo(cbbCarTeamNameByTeam);
            }
            else if (tcReport.SelectedTab == tabPage8)
            {
                fillMillCombo(cbMill);
            }
        }

        private void fillCarTeamCombo(ComboBox cbb)
        {
            //ล้างก่อน
            cbb.Items.Clear();

            //
            cbb.Items.Add("ทั้งหมด");
            cbb.SelectedIndex = 0;

            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_car_team ORDER BY รหัสทีม";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["ชื่อทีม"].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillMillCombo(ComboBox cbb)
        {
            //ล้างก่อน
            cbb.Items.Clear();

            cbb.Items.Add("ทั้งหมด");
            cbb.SelectedIndex = 0;

            //
            //cbb.Items.Add("ทั้งหมด");

            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_mill ORDER BY รหัสโรงโม่";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["ชื่อโรงโม่"].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            cbb.SelectedIndex = 0;
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            /*
            this.weightTableAdapter.Fill(this.truckDataSet.weight);
            StringBuilder sql = new StringBuilder();
            sql.Append("วันที่ >= '" + tbdateFrom.Text + "' AND  วันที่ <= '" + tbdateTo.Text + "' AND ( NOT น้ำหนักรวม = 0.00 OR  รหัสลูกค้า = '999' ) ");
            this.weightBindingSource.Filter = sql.ToString();
            */
            setDataWeightToDTGV();
        }

        private void btPrint_Click(object sender, EventArgs e)
        {
            /*  print แบบเก่า
             
            Microsoft.Reporting.WinForms.ReportDataSource rs = new Microsoft.Reporting.WinForms.ReportDataSource();
            double sumTotal = 0;
            double sumQ = 0;
            double sumAmount = 0;
            double sumAmountVat = 0;
            List<WeightDailyReport> listWDR = new List<WeightDailyReport>();
            listWDR.Clear();

            for (int i = 0; i < dgvDailyReport.Rows.Count - 1; i++) {
                listWDR.Add(new WeightDailyReport()
                {
                    วันที่ = dateFormatToPrint(dgvDailyReport.Rows[i].Cells["วันที่"].Value.ToString()),
                    เลขที่เอกสาร = dgvDailyReport.Rows[i].Cells["เลขที่เอกสาร"].Value.ToString(),
                    รหัสลูกค้า = dgvDailyReport.Rows[i].Cells["รหัสลูกค้า"].Value.ToString(),
                    ลูกค้า = dgvDailyReport.Rows[i].Cells["ลูกค้า"].Value.ToString(),
                    ทะเบียนรถ = dgvDailyReport.Rows[i].Cells["ทะเบียนรถ"].Value.ToString(),
                    จังหวัด = dgvDailyReport.Rows[i].Cells["จังหวัด"].Value.ToString(),
                    ทีม = dgvDailyReport.Rows[i].Cells["ทีม"].Value.ToString(),
                    ชนิดหิน = dgvDailyReport.Rows[i].Cells["ชนิดหิน"].Value.ToString(),
                    ราคาตัน = dgvDailyReport.Rows[i].Cells["ราคาตัน"].Value.ToString(),
                    น้ำหนักสินค้า = dgvDailyReport.Rows[i].Cells["น้ำหนักสินค้า"].Value.ToString(),
                    คิว = dgvDailyReport.Rows[i].Cells["คิว"].Value.ToString(),
                    จำนวนเงิน = dgvDailyReport.Rows[i].Cells["จำนวนเงิน"].Value.ToString(),
                    จำนวนเงินสุทธิ = dgvDailyReport.Rows[i].Cells["จำนวนเงินสุทธิ"].Value.ToString(),
                    จ่ายเงิน = dgvDailyReport.Rows[i].Cells["จ่ายเงิน"].Value.ToString(),
                    ชื่อผู้ตัก = dgvDailyReport.Rows[i].Cells["ชื่อผู้ตัก"].Value.ToString(),
                });
                sumTotal += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["น้ำหนักสินค้า"].Value);
                sumQ += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["คิว"].Value);
                sumAmount += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["จำนวนเงิน"].Value);
                sumAmountVat += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["จำนวนเงินสุทธิ"].Value);
            }
            WeightDailyReport.DateFrom = tbdateFrom.Text;
            WeightDailyReport.DateTo = tbdateTo.Text;
            WeightDailyReport.SumToTal = sumTotal.ToString("#,##0.00");
            WeightDailyReport.SumQ = sumQ.ToString("#,##0.00");
            WeightDailyReport.SumAmount = sumAmount.ToString("#,##0.00");
            WeightDailyReport.SumAmountVat = sumAmountVat.ToString("#,##0.00");
            WeightDailyReport.CountRount = (dgvDailyReport.Rows.Count-1).ToString();
            rs.Name = "DailyDataSet";
            rs.Value = listWDR;
            FPrintDailyReport fp = new FPrintDailyReport(listWDR, rs);
            fp.ShowDialog();

            */

            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "' AND '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "' ORDER BY วันที่, เลขที่เอกสาร";
            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightDailyReport.DateFrom = tbdateFrom.Text;
            WeightDailyReport.DateTo = tbdateTo.Text;

            //open winform
            List<WeightDailyReport> listWDR = new List<WeightDailyReport>();
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("dailyDataSet", dt);
            FPrintDailyReport fp = new FPrintDailyReport(listWDR, rds);
            fp.ShowDialog();

        }

        private string dateFormatToPrint(string strDate) {
            DateTime date = Convert.ToDateTime(strDate);
            string dateFormat = date.ToString("dd/MM/yyyy");
            return dateFormat;
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            double sumTotal = 0;
            double sumQ = 0;
            double sumAmount = 0;
            double sumAmountVat = 0;
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;
            worksheet.Name = "DailyReportSheet";

            worksheet.Cells[1, 1] = "รายงานการชั่งสินค้าประจำวันที่ "+ tbdateFrom.Text + " - "+ tbdateTo.Text;
            for (int i = 1; i< dgvDailyReport.Columns.Count + 1; i++) {
                worksheet.Cells[3,i] = dgvDailyReport.Columns[i - 1].HeaderText;
            }

            for (int i = 0; i < dgvDailyReport.Rows.Count; i++) {
                sumTotal += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["น้ำหนักสินค้า"].Value);
                sumQ += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["คิว"].Value);
                sumAmount += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["จำนวนเงิน"].Value);
                sumAmountVat += Convert.ToDouble(dgvDailyReport.Rows[i].Cells["จำนวนเงินสุทธิ"].Value);

                for (int j = 0; j < dgvDailyReport.Columns.Count; j++) {
                    string tmpStr = null;
                    if (dgvDailyReport.Rows[i].Cells[j].Value == null)
                        tmpStr = "";
                    else if (dgvDailyReport.Rows[i].Cells[j].ValueType.ToString().Equals("System.DateTime"))
                        tmpStr = dateFormatToPrint(dgvDailyReport.Rows[i].Cells[j].Value.ToString());
                    else
                        tmpStr = dgvDailyReport.Rows[i].Cells[j].Value.ToString();
                    worksheet.Cells[i + 4, j + 1] = tmpStr;
                }
            }
            WeightDailyReport.SumToTal = sumTotal.ToString("#,##0.00");
            WeightDailyReport.SumQ = sumQ.ToString("#,##0.00");
            WeightDailyReport.SumAmount = sumAmount.ToString("#,##0.00");
            WeightDailyReport.SumAmountVat = sumAmountVat.ToString("#,##0.00");

            int coutRows = dgvDailyReport.Rows.Count+4;
            worksheet.Cells[coutRows, 1] = "รวม";
            worksheet.Cells[coutRows, 10] = WeightDailyReport.SumToTal;
            worksheet.Cells[coutRows, 11] = WeightDailyReport.SumQ;
            worksheet.Cells[coutRows, 12] = WeightDailyReport.SumAmount;
            worksheet.Cells[coutRows, 13] = WeightDailyReport.SumAmountVat;

            worksheet.Cells[coutRows + 1, 1] = "จำนวนเที่ยว";
            worksheet.Cells[coutRows + 1, 2] = (dgvDailyReport.Rows.Count - 1).ToString();
            worksheet.Cells[coutRows + 1, 3] = "เที่ยว";

            var saveFileDialoge = new SaveFileDialog();
            saveFileDialoge.FileName = "DailyReport";
            saveFileDialoge.DefaultExt = ".xlsx";
            if (saveFileDialoge.ShowDialog() == DialogResult.OK) {
                workbook.SaveAs(saveFileDialoge.FileName,Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            app.Quit();

        }

        private void btPrintScoop_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromScoop.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToScoop.Value.ToString("yyyy-MM-dd") + "'";
            if (tbScoopName.Text != "")
                sql += " AND ชื่อผู้ตัก LIKE '%" + tbScoopName.Text + "%' ";
            sql += " ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromScoop.Text;
            WeightTempReport.DateTo = dtToScoop.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("scoopDataSet", dt);
            FPrintScoopReport fp = new FPrintScoopReport(rds);
            fp.ShowDialog();
        }

        private void btPrintJointCar_Click(object sender, EventArgs e)
        {

            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromJointCar.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToJointCar.Value.ToString("yyyy-MM-dd") + "'";
            if (tbJointCarRegistration.Text != "")
                sql += " AND ทะเบียนรถ LIKE '%" + tbJointCarRegistration.Text + "%' ";
            sql += " ORDER BY วันที่, เลขที่เอกสาร ";
            
            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromJointCar.Text;
            WeightTempReport.DateTo = dtToJointCar.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("jointCarDataSet", dt);
            FPrintJointCarReport fp = new FPrintJointCarReport(rds);
            fp.ShowDialog();
            
        }

        private void btPrintCarTeam_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "SELECT base_car.รหัสรถร่วม, base_car.ชื่อรถร่วม, base_car.รหัสทีม, base_car_team.ชื่อทีม FROM base_car INNER JOIN base_car_team ON base_car.รหัสทีม = base_car_team.รหัสทีม ";
            if (cbbCarTeamName.SelectedIndex != 0)
                sql += "WHERE base_car_team.ชื่อทีม = '" + cbbCarTeamName.Text + "'";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();


            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("carTeamDataSet", dt);
            FPrintCarTeamReport fp = new FPrintCarTeamReport(rds);
            fp.ShowDialog();
        }


        private void btPrintInvoice_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromInvoice.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToInvoice.Value.ToString("yyyy-MM-dd") + "'";
            if (tbInvoiceCutomerId.Text != "")
                sql += " AND รหัสลูกค้า = '" + tbInvoiceCutomerId.Text + "' ";
            sql += " ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromInvoice.Text;
            WeightTempReport.DateTo = dtToInvoice.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("invoiceDataSet", dt);
            FPrintInvoiceReport fp = new FPrintInvoiceReport(rds);
            fp.ShowDialog();
        }

        private void btPrintCustomer_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromCustomer.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToCustomer.Value.ToString("yyyy-MM-dd") + "' ";
            if (tbCustomerId.Text != "")
                sql += " AND รหัสลูกค้า = '" + tbCustomerId.Text + "' ";
            sql += " ORDER BY วันที่ ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromCustomer.Text;
            WeightTempReport.DateTo = dtToCustomer.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("customerDataSet", dt);
            FPrintCustomerReport fp = new FPrintCustomerReport(rds);
            fp.ShowDialog();
        }

        private void setCustomerIdToTextbox(TextBox customerId, TextBox customernName){

            if (customerId != null && customerId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where รหัสลูกค้า = '" + customerId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อลูกค้า"].ToString();

                        customernName.Text = rdStr;
                    }

                }
                catch (Exception)
                {
                }

                dl.close();
            }
            else
            {
                customernName.Text = "";
            }
        }

        private void tbCustomerId_TextChanged(object sender, EventArgs e)
        {
            setCustomerIdToTextbox(tbCustomerId, tbCustomerName);
        }

        private void setCustomerNameToTextbox(TextBox customerId, TextBox customernName)
        {
            if (customernName != null && customernName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where ชื่อลูกค้า = '" + customernName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสลูกค้า"].ToString();
                        customerId.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                customerId.Text = "";
            }
        }
        private void tbCustomerName_TextChanged(object sender, EventArgs e)
        {
            setCustomerNameToTextbox(tbCustomerId, tbCustomerName);
        }

        private void btPrintClean_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromClean.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToClean.Value.ToString("yyyy-MM-dd") + "' AND ล้าง = '" + getRadioValueClean() + "' ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromClean.Text;
            WeightTempReport.DateTo = dtToClean.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("cleanDataSet", dt);
            FPrintCleanReport fp = new FPrintCleanReport(rds);
            fp.ShowDialog();
        }

        private String getRadioValueClean() {
            string value = "";
            if (rbCleanStone.Checked)
                value = rbCleanStone.Text;
            else if (rbCleanWater.Checked)
                value = rbCleanWater.Text;
            else if (rbCleanNo.Checked)
                value = rbCleanNo.Text;
            return value;
        }

        private void btPrintMill_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromMill.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToMill.Value.ToString("yyyy-MM-dd") + "' ";
            if (cbMill.SelectedIndex != 0)
                sql += " AND โรงโม่ = '" + cbMill.Text + "'";
            sql += " ORDER BY วันที่, เลขที่เอกสาร ";


            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromMill.Text;
            WeightTempReport.DateTo = dtToMill.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("millDataSet", dt);
            FPrintMillReport fp = new FPrintMillReport(rds);
            fp.ShowDialog();
        }

        private void btPrintSite_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromSite.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToSite.Value.ToString("yyyy-MM-dd") + "' ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromSite.Text;
            WeightTempReport.DateTo = dtToSite.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("siteDataSet", dt);
            FPrintSiteReport fp = new FPrintSiteReport(rds);
            fp.ShowDialog();
        }

        private void tbInvoiceCutomerId_TextChanged(object sender, EventArgs e)
        {
            setCustomerIdToTextbox(tbInvoiceCutomerId, tbInvoiceCutomerName);
        }

        private void tbInvoiceCutomerName_TextChanged(object sender, EventArgs e)
        {
            setCustomerNameToTextbox(tbInvoiceCutomerId, tbInvoiceCutomerName);
        }

        private void btPrintStoneType_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromStoneType.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToStoneType.Value.ToString("yyyy-MM-dd") + "' AND NOT น้ำหนักรวม = 0.00 AND NOT คิว = 0.00 AND NOT โรงโม่ = ' ' AND NOT รหัสลูกค้า LIKE '10%' AND (NOT site_id = '300PL' OR site_id IS NULL) ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromStoneType.Text;
            WeightTempReport.DateTo = dtToStoneType.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("stoneTypeDataSet", dt);
            FPrintStoneTypeReport fp = new FPrintStoneTypeReport(rds);
            fp.ShowDialog();
        }

        private void btPrintTransport_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromTransport.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToTransport.Value.ToString("yyyy-MM-dd") + "' ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromTransport.Text;
            WeightTempReport.DateTo = dtToTransport.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("transportDataSet", dt);
            FPrintTransportReport fp = new FPrintTransportReport(rds);
            fp.ShowDialog();

        }

        private void btPrintTransportByTeam_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromTransportByTeam.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToTransportByTeam.Value.ToString("yyyy-MM-dd") + "' AND NOT รหัสลูกค้า = '09-A-001' AND NOT รหัสลูกค้า = '09-V-001' ";
            if (cbbCarTeamNameByTeam.SelectedIndex != 0)
                sql += " AND ทีม = '" + cbbCarTeamNameByTeam.Text + "'";
            sql += " ORDER BY วันที่, เลขที่เอกสาร ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromTransportByTeam.Text;
            WeightTempReport.DateTo = dtToTransportByTeam.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("transportByTeamDataSet", dt);
            FPrintTransportByTeamReport fp = new FPrintTransportByTeamReport(rds);
            fp.ShowDialog();
        }

        private void cbbCustomerName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbCustomerName, listOriginalCustomerReport, listNewCustomerNameReport);
        }

        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew)
        {

            try
            {
                //clear combobox
                cb.Items.Clear();
                //clear listNew
                listNew.Clear();

                foreach (var item in listOriginal)
                {
                    // call ToLower() .. not case sensitive
                    if (item.ToLower().Contains(cb.Text))
                    {
                        //add to ListNew
                        listNew.Add(item);
                    }
                }

                if (listNew.Count > 0)
                {
                    cb.Items.AddRange(listNew.ToArray());
                    cb.SelectionStart = cb.Text.Length;
                    Cursor = Cursors.Default;
                    // Automatically pop up drop-down
                    cb.DroppedDown = true;
                }
                else
                {
                    cb.Items.AddRange(listOriginal.ToArray());
                    cb.DroppedDown = false;
                }


            }
            catch (Exception)
            {

            }
        }

        private void cbbCustomerName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbCustomerId.Text = cbbCustomerName.Text.Substring(0, cbbCustomerName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbCustomerId.Text = "";
                cbbCustomerName.Text = "";
            }
        }

        private void cbbInvoiceCutomerName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbInvoiceCutomerName, listOriginalInvoiceReport, listNewInvoiceReport);
        }

        private void cbbInvoiceCutomerName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbInvoiceCutomerId.Text = cbbInvoiceCutomerName.Text.Substring(0, cbbInvoiceCutomerName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbInvoiceCutomerId.Text = "";
                cbbInvoiceCutomerName.Text = "";
            }
        }
    }  
}
