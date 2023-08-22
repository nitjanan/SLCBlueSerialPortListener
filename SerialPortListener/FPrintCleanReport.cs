using Microsoft.Reporting.WinForms;
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
    public partial class FPrintCleanReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();

        public FPrintCleanReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCleanReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'cleanDataSet.weight' table. You can move, or remove it, as needed.
            //this.weightTableAdapter.Fill(this.cleanDataSet.weight);
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvCleanReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCleanReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCleanReport.LocalReport.DataSources.Clear();
            this.rvCleanReport.LocalReport.DataSources.Add(_rs);
            this.rvCleanReport.LocalReport.DisplayName = "รายงานล้างสเปร์ยประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvCleanReport.LocalReport.SetParameters(p);
            this.rvCleanReport.RefreshReport();
        }
    }
}
