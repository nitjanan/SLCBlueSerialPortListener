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
    public partial class FPrintCCReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintCCReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCCReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'truckDataSet.weight' table. You can move, or remove it, as needed.
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvCCReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCCReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCCReport.LocalReport.DataSources.Clear();
            this.rvCCReport.LocalReport.DataSources.Add(_rs);
            this.rvCCReport.LocalReport.SetParameters(p);
            this.rvCCReport.LocalReport.DisplayName = "รายงานตามบริษัทและขนส่งประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvCCReport.RefreshReport();
        }
    }
}
