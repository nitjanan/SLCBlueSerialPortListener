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
    public partial class FPrintStoneTypeReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintStoneTypeReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintStoneTypeReport_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvStoneTypeReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvStoneTypeReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvStoneTypeReport.LocalReport.DataSources.Clear();
            this.rvStoneTypeReport.LocalReport.DataSources.Add(_rs);
            this.rvStoneTypeReport.LocalReport.SetParameters(p);
            this.rvStoneTypeReport.LocalReport.DisplayName = "รายงานสรุปตามประเภทหินประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');

            this.rvStoneTypeReport.RefreshReport();
        }
    }
}
