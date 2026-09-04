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

        // Bind default keywords
        List<string> listOriginalCorpReport = new List<string>();
        // save new keywords
        List<string> listNewCorpReport = new List<string>();

        // Bind default keywords
        List<string> listOriginalCCReport = new List<string>();
        // save new keywords
        List<string> listNewCCReport = new List<string>();

        // Bind default keywords
        List<string> listOriginalProductReport = new List<string>();
        // save new keywords
        List<string> listNewProductReport = new List<string>();

        // Bind default keywords
        List<string> listOriginalDOReport = new List<string>();
        // save new keywords
        List<string> listNewDOReport = new List<string>();

        public ucReport()
        {
            dl = new Datalayer();
            InitializeComponent();
            setDefaultFormatDTGV();

            //tabPage1
            cbbCustomerType.SelectedIndex = 0;
            cbbWeight.SelectedIndex = 1;
            cbbLineType.SelectedIndex = 0;

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

            fillTableCombo(cbbStoneType, "base_stone_type", "ชื่อหิน");
            fillTableCombo(cbbCarTeam, "base_car_team", "ชื่อทีม");
            fillTableCombo(cbbCarryTeam, "base_car_team", "ชื่อทีม");
            fillTableCombo(cbbProductStoneType, "base_stone_type", "ชื่อหิน");
            fillTableCombo(cbbCorpStone, "base_stone_type", "ชื่อหิน");

            /* autoComplete ลูกค้า */
            autoCompleteSetting(tbbCustomerId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbbCustomerName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete ลูกค้า */
            autoCompleteSetting(tbbFromCustomerId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbbToCustomerId, "รหัสลูกค้า", "base_customer");

            /* autoComplete ลูกค้า รายงานตามบริษัท*/
            autoCompleteSetting(tbCorpId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbCorpName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete ลูกค้า รายงานตามบริษัทและการขนส่ง*/
            autoCompleteSetting(tbCCId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbCCName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete ลูกค้า รายงานแยกตามสินค้า*/
            autoCompleteSetting(tbProductCustomerId, "รหัสลูกค้า", "base_customer");
            autoCompleteSetting(tbProductCustomerName, "ชื่อลูกค้า", "base_customer");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbCustomerName, listOriginalCustomerReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbInvoiceCutomerName, listOriginalInvoiceReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbCorpName, listOriginalCorpReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbCCName, listOriginalCCReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbProductName, listOriginalProductReport);
            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", cbbDOCutomerName, listOriginalDOReport);

            this.cbbCorpName.SelectedIndexChanged += new System.EventHandler(this.cbbCorpName_SelectedIndexChanged);
            fillCorpSiteCombo();

            dtFromOut.Value = Convert.ToDateTime(System.DateTime.Today.ToShortDateString() + " 06:00 AM");
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
            pgCommand.CommandText = "SELECT * FROM public." + tableName + " where weight_type = 1 or weight_type = 3 ";
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

        private string getDataCustomerType(string str)
        {
            if (str == "ขาย")
                return "1";
            else if (str == "สต็อก")
                return "2";
            else
                return "";
        }

        private void setDataWeightToDTGV()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT weight.*  FROM public.weight ");
                if (cbbCustomerType.SelectedIndex != 0)
                    sql.Append(" INNER JOIN public.base_customer ON weight.รหัสลูกค้า = base_customer.รหัสลูกค้า ");
                sql.Append(" WHERE (วันที่ = '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "' and เวลาชั่งออก >= '" + dtFromOut.Value.ToString("HH:mm") + "' or วันที่  > '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "') AND (วันที่ = '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "' and  เวลาชั่งออก <= '" + dtToOut.Value.ToString("HH:mm") + "' or  วันที่ < '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "') ");
                if (cbbCustomerType.SelectedIndex != 0)
                    sql.Append(" AND base_customer.ประเภทลูกค้า = '" + getDataCustomerType(cbbCustomerType.Text) + "' ");
                if (tbCarRegistration.Text != "")
                    sql.Append(" AND ทะเบียนรถ LIKE '" + tbCarRegistration.Text + "%' ");
                if (cbbCarTeam.SelectedIndex != 0)
                    sql.Append(" AND ทีม = '" + cbbCarTeam.Text + "' ");
                if (tbDriver.Text != "")
                    sql.Append(" AND คนขับ LIKE '" + tbDriver.Text + "%' ");
                if (cbbStoneType.SelectedIndex != 0)
                    sql.Append(" AND ชนิดหิน = '" + cbbStoneType.Text + "' ");
                if (tbbCustomerName.Text != "")
                    sql.Append(" AND ลูกค้า = '" + tbbCustomerName.Text + "' ");
                if (tbbFromCustomerId.Text != "" && tbbToCustomerId.Text != "")
                    sql.Append(" AND weight.รหัสลูกค้า BETWEEN '" + tbbFromCustomerId.Text + "' AND '" + tbbToCustomerId.Text + "' ");
                if (cbbWeight.SelectedIndex == 0)
                    sql.Append(" AND NOT น้ำหนักรถ  = '0.00' AND NOT น้ำหนักรวม = '0.00' ");
                if (cbbWeight.SelectedIndex == 2)
                    sql.Append(" AND น้ำหนักรวม = '0.00' ");
                if (cbbLineType.SelectedIndex != 0)
                    sql.Append(" AND line_type = '" + cbbLineType.Text + "' ");

                sql.Append(" ORDER BY วันที่, เลขที่เอกสาร ");

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
            else if (tcReport.SelectedTab == tabPage13)
            {
                fillCorpSiteCombo();
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
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT weight.*  FROM public.weight ");
            if (cbbCustomerType.SelectedIndex != 0)
                sql.Append(" INNER JOIN public.base_customer ON weight.รหัสลูกค้า = base_customer.รหัสลูกค้า ");
            sql.Append(" WHERE (วันที่ = '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "' and เวลาชั่งออก >= '" + dtFromOut.Value.ToString("HH:mm") + "' or วันที่  > '" + tbdateFrom.Value.ToString("yyyy-MM-dd") + "') AND (วันที่ = '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "' and  เวลาชั่งออก <= '" + dtToOut.Value.ToString("HH:mm") + "' or  วันที่ < '" + tbdateTo.Value.ToString("yyyy-MM-dd") + "') ");
            if (cbbCustomerType.SelectedIndex != 0)
                sql.Append(" AND base_customer.ประเภทลูกค้า = '" + getDataCustomerType(cbbCustomerType.Text) + "' ");
            if (tbCarRegistration.Text != "")
                sql.Append(" AND ทะเบียนรถ LIKE '" + tbCarRegistration.Text + "%' ");
            if (cbbCarTeam.SelectedIndex != 0)
                sql.Append(" AND ทีม = '" + cbbCarTeam.Text + "' ");
            if (tbDriver.Text != "")
                sql.Append(" AND คนขับ LIKE '" + tbDriver.Text + "%' ");
            if (cbbStoneType.SelectedIndex != 0)
                sql.Append(" AND ชนิดหิน = '" + cbbStoneType.Text + "' ");
            if (tbbCustomerName.Text != "")
                sql.Append(" AND ลูกค้า = '" + tbbCustomerName.Text + "' ");
            if (tbbFromCustomerId.Text != "" && tbbToCustomerId.Text != "")
                sql.Append(" AND weight.รหัสลูกค้า BETWEEN '" + tbbFromCustomerId.Text + "' AND '" + tbbToCustomerId.Text + "' ");
            if (cbbWeight.SelectedIndex == 0)
                sql.Append(" AND NOT น้ำหนักรถ  = '0.00' AND NOT น้ำหนักรวม = '0.00' ");
            if (cbbWeight.SelectedIndex == 2)
                sql.Append(" AND น้ำหนักรวม = '0.00' ");
            if (cbbLineType.SelectedIndex != 0)
                sql.Append(" AND line_type = '" + cbbLineType.Text + "' ");

            sql.Append(" ORDER BY วันที่, เลขที่เอกสาร ");

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
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

        private void btPrintDO_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT dord.doc_no AS do_no, dord.customer_name, dord.product_name, dord.customer_address AS site_address, dord.site_name AS site_name, ");
            sql.Append("wd.weight_doc_id, wd.delivery_date, wd.carry_type_name, wd.weight_ton, wd.weight_q, wd.unit_name, wd.bws, w.stone_desc ");
            sql.Append("FROM weight_delivery wd INNER JOIN delivery_order dord ON wd.do_doc_no = dord.doc_no ");
            sql.Append("LEFT JOIN weight w ON wd.weight_id = w.weight_id ");
            sql.Append("WHERE wd.delivery_date BETWEEN ? AND ? ");
            sql.Append("AND (wd.is_cancel = false OR wd.is_cancel IS NULL) ");
            if (tbDOCutomerId.Text != "")
                sql.Append("AND dord.customer_code = ? ");
            sql.Append("ORDER BY dord.doc_no, wd.weight_doc_id");

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = sql.ToString();
            pgCommand.Parameters.AddWithValue("", dtFromDO.Value.ToString("yyyy-MM-dd"));
            pgCommand.Parameters.AddWithValue("", dtToDO.Value.ToString("yyyy-MM-dd"));
            if (tbDOCutomerId.Text != "")
                pgCommand.Parameters.AddWithValue("", tbDOCutomerId.Text);

            DataTable dt = new DataTable();
            try
            {
                OdbcDataAdapter cmd = new OdbcDataAdapter(pgCommand);
                cmd.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                dl.close();
                return;
            }
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromDO.Text;
            WeightTempReport.DateTo = dtToDO.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("WeightDeliveryDataSet", dt);
            FPrintWeightDelivery fp = new FPrintWeightDelivery(rds);
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

        private string numberFormat(string numStr, int format)
        {
            double deci = Convert.ToDouble(numStr);
            string str = "";
            if (format == 1)
                str = deci.ToString();
            else if (format == 2)
                str = deci.ToString("#,##0.00");
            else if (format == 3)
                str = deci.ToString("#,##0");
            return str;
        }

        private string strNotEmty(string str)
        {
            return str == "" ? " " : str;
        }

        private string getVatRadioValuePrint()
        {
            string value = null;
            /*
            if (rbbNonVat.Checked)
            {
                value = "ใบส่งของ";
                Company.CompanyName = " ";
                Company.Address = " ";
                Company.Email = " ";
                Company.Telephone = " ";
                Company.TTelephone = " ";
                Company.TEmail = " ";
            }
            else if (rbbVat.Checked)
            {
            */
            value = "ใบส่งสินค้า";
            getDefaultCompany();
            /*
            }
            */
            return value;
        }

        private void getDefaultCompany()
        {
            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_company where base_company_id = 1 ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    Company.CompanyName = strNotEmty(reader["company_name"].ToString());
                    Company.Address = strNotEmty(reader["address"].ToString());
                    Company.Telephone = strNotEmty(reader["telephone"].ToString());
                    Company.Email = strNotEmty(reader["email"].ToString());
                }
            }
            catch (Exception)
            {
            }
            dl.close();

        }

        private string getStoneTypeId(string stoneType)
        {
            string stoneTypeId = " ";
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสหิน FROM public.base_stone_type  where ชื่อหิน = '" + stoneType + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    stoneTypeId = reader["รหัสหิน"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            return stoneTypeId;
        }

        private string tonTokg(string tonStr)
        {
            double tmp = Convert.ToDouble(tonStr);
            double deci = tmp * 1000;
            return deci.ToString("#,##0");
        }

        private void preparePrint(int mode)
        {
            Weight.DocNum = "N" + dgvDailyReport.CurrentRow.Cells["เลขที่เอกสาร"].Value.ToString();
            Weight.Amount = numberFormat(dgvDailyReport.CurrentRow.Cells["จำนวนเงิน"].Value.ToString(), 2);
            Weight.CarCity = strNotEmty(dgvDailyReport.CurrentRow.Cells["จังหวัด"].Value.ToString());
            Weight.CarLicense = strNotEmty(dgvDailyReport.CurrentRow.Cells["ทะเบียนรถ"].Value.ToString());
            Weight.CustomerName = strNotEmty(dgvDailyReport.CurrentRow.Cells["ลูกค้า"].Value.ToString());
            Weight.Date = dgvDailyReport.CurrentRow.Cells["วันที่"].Value.ToString();
            Weight.DriverName = strNotEmty(dgvDailyReport.CurrentRow.Cells["คนขับ"].Value.ToString());
            Weight.Mill = strNotEmty(dgvDailyReport.CurrentRow.Cells["โรงโม่"].Value.ToString());
            Weight.Pay = strNotEmty(Convert.ToString(dgvDailyReport.CurrentRow.Cells["จ่ายเงิน"].Value));
            Weight.Price = numberFormat(dgvDailyReport.CurrentRow.Cells["ราคาตัน"].Value.ToString(), 2);
            Weight.StoneType = strNotEmty(dgvDailyReport.CurrentRow.Cells["ชนิดหิน"].Value.ToString());
            Weight.WeightIn = dgvDailyReport.CurrentRow.Cells["น้ำหนักรถ"].Value.ToString();
            Weight.DateIn = dateFormatToPrint(dgvDailyReport.CurrentRow.Cells["วันที่ชั่งเข้า"].Value.ToString());
            Weight.TimeIn = strNotEmty(dgvDailyReport.CurrentRow.Cells["เวลาชั่งเข้า"].Value.ToString());
            Weight.WeightOut = dgvDailyReport.CurrentRow.Cells["น้ำหนักรวม"].Value.ToString();
            Weight.DateOut = dateFormatToPrint(dgvDailyReport.CurrentRow.Cells["วันที่ชั่งออก"].Value.ToString());
            Weight.TimeOut = strNotEmty(dgvDailyReport.CurrentRow.Cells["เวลาชั่งออก"].Value.ToString());
            Weight.WeightTotal = dgvDailyReport.CurrentRow.Cells["น้ำหนักสินค้า"].Value.ToString();
            Weight.Q = dgvDailyReport.CurrentRow.Cells["คิว"].Value.ToString();
            Weight.ApproveName = strNotEmty(dgvDailyReport.CurrentRow.Cells["ชื่อผู้อนุมัติจ่าย"].Value.ToString());
            Weight.AmountVat = numberFormat(dgvDailyReport.CurrentRow.Cells["จำนวนเงินสุทธิ"].Value.ToString(), 2);
            Weight.VatType = getVatRadioValuePrint();
            Weight.StoneColor = strNotEmty(dgvDailyReport.CurrentRow.Cells["ประเภทหิน"].Value.ToString());
            Weight.Site = strNotEmty(dgvDailyReport.CurrentRow.Cells["หน้างาน"].Value.ToString());
            Weight.Team = strNotEmty(dgvDailyReport.CurrentRow.Cells["ทีม"].Value.ToString());
            Weight.Clean = strNotEmty(dgvDailyReport.CurrentRow.Cells["ล้าง"].Value.ToString());
            Weight.Vat = numberFormat(dgvDailyReport.CurrentRow.Cells["vat"].Value.ToString(), 2);
            Weight.Transport = strNotEmty(dgvDailyReport.CurrentRow.Cells["ขนส่ง"].Value.ToString());
            Weight.ScoopName = strNotEmty(dgvDailyReport.CurrentRow.Cells["ชื่อผู้ตัก"].Value.ToString());
            Weight.OilContent = numberFormat(dgvDailyReport.CurrentRow.Cells["oil_content"].Value.ToString(), 2);
            Weight.Id = strNotEmty(dgvDailyReport.CurrentRow.Cells["weight_id"].Value.ToString());

            try
            {
                Weight.StoneDesc = strNotEmty(Convert.ToString(dgvDailyReport.CurrentRow.Cells["stone_desc"].Value));
            }
            catch
            {
                Weight.StoneDesc = " ";
            }


            if (mode.Equals(3))
            {
                Company.TTelephone = "โทร";
                Company.TEmail = "E-mail";

                Company.TDocName = "เลขที่การชั่ง";
                Company.TLogo = "(Sandvik)";
            }
            else if (mode.Equals(4))
            {
               
                Weight.DocNum = dgvDailyReport.CurrentRow.Cells["เลขที่เอกสาร"].Value.ToString();
                Weight.CustomerId = strNotEmty(dgvDailyReport.CurrentRow.Cells["รหัสลูกค้า"].Value.ToString());
                Weight.Note = strNotEmty(dgvDailyReport.CurrentRow.Cells["หมายเหตุ"].Value.ToString());
                Weight.StoneTypeId = getStoneTypeId(dgvDailyReport.CurrentRow.Cells["ชนิดหิน"].Value.ToString());
                Weight.WeightIn = tonTokg(dgvDailyReport.CurrentRow.Cells["น้ำหนักรถ"].Value.ToString());
                Weight.WeightOut = tonTokg(dgvDailyReport.CurrentRow.Cells["น้ำหนักรวม"].Value.ToString());
                Weight.WeightTotal = tonTokg(dgvDailyReport.CurrentRow.Cells["น้ำหนักสินค้า"].Value.ToString());
                Weight.Price = numberFormat(dgvDailyReport.CurrentRow.Cells["ราคาตัน"].Value.ToString(), 3);
                Weight.AmountVat = numberFormat(dgvDailyReport.CurrentRow.Cells["จำนวนเงินสุทธิ"].Value.ToString(), 3);
            }

        }

        private void btPrintVRock_Click(object sender, EventArgs e)
        {
            if (dgvDailyReport.Rows.Count > 1)
            {
                preparePrint(4);

                FPrintVRock f = new FPrintVRock();
                f.ShowDialog();

            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลที่ต้องการปริ้นบิล  กรุณาเลือกรายการ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tbbFromCustomerId_Leave(object sender, EventArgs e)
        {
            if (tbbFromCustomerId.Text != "")
            {
                tbbCustomerId.Text = "";
                tbbCustomerName.Text = "";
            }
        }

        private void tbbToCustomerId_Leave(object sender, EventArgs e)
        {
            if (tbbToCustomerId.Text != "")
            {
                tbbCustomerId.Text = "";
                tbbCustomerName.Text = "";
            }
        }

        private void tbbCustomerId_Leave(object sender, EventArgs e)
        {
            if (tbbToCustomerId.Text != "")
            {
                tbbCustomerId.Text = "";
                tbbCustomerName.Text = "";
            }
            setCustomerIdToTextbox(tbbCustomerId, tbbCustomerName);
        }

        private void tbbCustomerName_Leave(object sender, EventArgs e)
        {
            if (tbbCustomerName.Text != "")
            {
                tbbFromCustomerId.Text = "";
                tbbToCustomerId.Text = "";
            }
            setCustomerNameToTextbox(tbbCustomerId, tbbCustomerName);
        }

        private void btPrintBil_Click(object sender, EventArgs e)
        {
            if (dgvDailyReport.Rows.Count > 1)
            {
                preparePrint(3);

                HandleSuccessfulPrint();

                FPrint f = new FPrint();
                f.ShowDialog();

            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลที่ต้องการปริ้นบิล  กรุณาเลือกรายการ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HandleSuccessfulPrint()
        {
            int copy_num = findLastCopyByWeightId();
            copy_num++;

            Weight.DatePrint = DateTime.Now.ToString("yyyy-MM-dd");
            Weight.DatePrintAndCopyNum = DateTime.Now.ToString("dd/MM") + "#" + copy_num;
            Weight.TimePrint = DateTime.Now.ToString("HH:mm:ss");

            //save weight copy
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight_copy (copy_num, date_print, time_print, user_print, weight_id )" +
                                     "VALUES ('" + copy_num + "','" + Weight.DatePrint + "','" + Weight.TimePrint + "','" + Globals.UserId + "','" + Weight.Id + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private int findLastCopyByWeightId()
        {
            int copy_num = 0;

            if (Weight.Id != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "select copy_num from weight_copy where weight_id = '" + Weight.Id + "' ORDER BY weight_copy_id DESC LIMIT 1";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        copy_num = Convert.ToInt32(reader["copy_num"].ToString());
                    }
                    else
                    {
                        copy_num = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                dl.close();
            }

            return copy_num;
        }


        private void btPrintCorp_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromCorp.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToCorp.Value.ToString("yyyy-MM-dd") + "' ";
            if (tbCorpId.Text != "")
                sql += " AND รหัสลูกค้า = '" + tbCorpId.Text + "' ";
            if (cbbCorpSite.Text != "" && cbbCorpSite.Text != "ทั้งหมด")
                sql += " AND หน้างาน = '" + cbbCorpSite.Text + "' ";
            if (cbbCorpStone.Text != "" && cbbCorpStone.Text != "ทั้งหมด")
                sql += " AND ชนิดหิน = '" + cbbCorpStone.Text + "' ";
            sql += " ORDER BY วันที่ ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromCorp.Text;
            WeightTempReport.DateTo = dtToCorp.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("CorpDataSet", dt);
            FPrintCorpReport fp = new FPrintCorpReport(rds);
            fp.ShowDialog();
        }

        private void btPrintCC_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            string sql = "select * from weight where วันที่ BETWEEN '" + dtFromCC.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToCC.Value.ToString("yyyy-MM-dd") + "' ";
            if (tbCCId.Text != "")
                sql += " AND รหัสลูกค้า = '" + tbCCId.Text + "' ";
            sql += " ORDER BY วันที่ ";

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql, dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromCC.Text;
            WeightTempReport.DateTo = dtToCC.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("customerCompanyDataSet", dt);
            FPrintCCReport fp = new FPrintCCReport(rds);
            fp.ShowDialog();
        }


        private void btPrintProduct_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();

            StringBuilder sql = new StringBuilder();
            sql.Append("select * from weight where วันที่ BETWEEN '" + dtFromProduct.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToProduct.Value.ToString("yyyy-MM-dd") + "' ");
            if (tbProductCustomerId.Text != "")
                sql.Append(" AND รหัสลูกค้า = '" + tbProductCustomerId.Text + "' ");
            if (cbbProductStoneType.SelectedIndex != 0)
                sql.Append(" AND ชนิดหิน = '" + cbbProductStoneType.Text + "' ");
            sql.Append(" ORDER BY วันที่ ");

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromProduct.Text;
            WeightTempReport.DateTo = dtToProduct.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("productDataSet", dt);
            FPrintProduct fp = new FPrintProduct(rds);
            fp.ShowDialog();
        }

        private void fillCorpSiteCombo()
        {
            cbbCorpSite.Items.Clear();
            cbbCorpSite.Items.Add("ทั้งหมด");
            cbbCorpSite.SelectedIndex = 0;

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            string query = "SELECT DISTINCT หน้างาน FROM public.weight WHERE หน้างาน IS NOT NULL AND หน้างาน <> ''";
            if (!string.IsNullOrEmpty(tbCorpId.Text))
            {
                query += " AND รหัสลูกค้า = '" + tbCorpId.Text + "'";
            }
            query += " ORDER BY หน้างาน";
            pgCommand.CommandText = query;

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string site = reader["หน้างาน"].ToString().Trim();
                    if (!string.IsNullOrEmpty(site))
                    {
                        cbbCorpSite.Items.Add(site);
                    }
                }
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void cbbCorpName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbbCorpName.SelectedItem != null)
                {
                    string selected = cbbCorpName.SelectedItem.ToString();
                    tbCorpId.Text = selected.Substring(0, selected.IndexOf(" : "));
                }
            }
            catch (Exception)
            {
                tbCorpId.Text = "";
            }
            fillCorpSiteCombo();
        }

        private void tbCorpId_Leave(object sender, EventArgs e)
        {
            setCustomerIdToTextbox(tbCorpId, tbCorpName);
            fillCorpSiteCombo();
        }

        private void tbCorpName_Leave(object sender, EventArgs e)
        {
            setCustomerNameToTextbox(tbCorpId, tbCorpName);
            fillCorpSiteCombo();
        }

        private void tbCCId_Leave(object sender, EventArgs e)
        {
            setCustomerIdToTextbox(tbCCId, tbCCName);
        }

        private void tbCCName_Leave(object sender, EventArgs e)
        {
            setCustomerNameToTextbox(tbCCId, tbCCName);
        }

        private void btPrintCarry_Click(object sender, EventArgs e)
        {
            //sql
            dl.connect();
            StringBuilder sql = new StringBuilder();
            sql.Append("select * from weight where วันที่ BETWEEN '" + dtFromCarry.Value.ToString("yyyy-MM-dd") + "' AND '" + dtToCarry.Value.ToString("yyyy-MM-dd") + "' ");
            if (cbbCarryTeam.SelectedIndex != 0)
                sql.Append(" AND ทีม = '" + cbbCarryTeam.Text + "' ");
            sql.Append(" ORDER BY วันที่, เลขที่เอกสาร ");

            OdbcDataAdapter cmd = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
            DataTable dt = new DataTable();
            cmd.Fill(dt);
            dl.close();

            //set parameter
            WeightTempReport.DateFrom = dtFromCarry.Text;
            WeightTempReport.DateTo = dtToCarry.Text;

            //open winform
            Microsoft.Reporting.WinForms.ReportDataSource rds = new Microsoft.Reporting.WinForms.ReportDataSource("carryDataSet", dt);
            FPrintCarryReport fp = new FPrintCarryReport(rds);
            fp.ShowDialog();
        }

        private void tbProductCustomerId_Leave(object sender, EventArgs e)
        {
            setCustomerIdToTextbox(tbProductCustomerId, tbProductCustomerName);
        }

        private void tbProductCustomerName_Leave(object sender, EventArgs e)
        {
            setCustomerNameToTextbox(tbProductCustomerId, tbProductCustomerName);
        }

        private void cbbCorpName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbCorpId.Text = cbbCorpName.Text.Substring(0, cbbCorpName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbCorpId.Text = "";
                cbbCorpName.Text = "";
            }
            fillCorpSiteCombo();
        }

        private void cbbCorpName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbCorpName, listOriginalCorpReport, listNewCorpReport);
        }

        private void cbbCCName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbCCId.Text = cbbCCName.Text.Substring(0, cbbCCName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbCCId.Text = "";
                cbbCCName.Text = "";
            }
        }

        private void cbbCCName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbCCName, listOriginalCCReport, listNewCCReport);
        }

        private void cbbProductName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbProductCustomerId.Text = cbbProductName.Text.Substring(0, cbbProductName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbProductCustomerId.Text = "";
                cbbProductName.Text = "";
            }
        }

        private void cbbProductName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbProductName, listOriginalProductReport, listNewProductReport);
        }

        private void cbbDOCutomerName_Leave(object sender, EventArgs e)
        {
            try
            {
                tbDOCutomerId.Text = cbbDOCutomerName.Text.Substring(0, cbbDOCutomerName.Text.IndexOf(" : "));
            }
            catch (Exception)
            {
                tbDOCutomerId.Text = "";
                cbbDOCutomerName.Text = "";
            }
        }

        private void cbbDOCutomerName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbDOCutomerName, listOriginalDOReport, listNewDOReport);
        }
    }  
}
