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
    public partial class FPrintJointCarReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintJointCarReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintJointCarReport_Load(object sender, EventArgs e)
        {

            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvJointCarReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvJointCarReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvJointCarReport.LocalReport.DataSources.Clear();
            this.rvJointCarReport.LocalReport.DataSources.Add(_rs);
            this.rvJointCarReport.LocalReport.SetParameters(p);
            this.rvJointCarReport.LocalReport.DisplayName = "รายงานทะเบียนรถร่วมประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvJointCarReport.RefreshReport();
        }
    }
}
