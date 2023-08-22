using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortListener
{
    public partial class FPrintInvoiceReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintInvoiceReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintInvoiceReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'weightScoopDataSet.weight' table. You can move, or remove it, as needed.
            //this.weightTableAdapter.Fill(this.weightScoopDataSet.weight);

            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvInvoiceReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvInvoiceReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvInvoiceReport.LocalReport.DataSources.Clear();
            this.rvInvoiceReport.LocalReport.DataSources.Add(_rs);
            this.rvInvoiceReport.LocalReport.SetParameters(p);
            this.rvInvoiceReport.LocalReport.DisplayName = "รายงานการชั่งสินค้างวดใบแจ้งหนี้ประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvInvoiceReport.RefreshReport();
        }
    }
}
