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
    public partial class FPrintMillReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintMillReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintMillReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'truckDataSet.weight' table. You can move, or remove it, as needed.
            //this.weightTableAdapter.Fill(this.truckDataSet.weight);

            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvMillReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvMillReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvMillReport.LocalReport.DataSources.Clear();
            this.rvMillReport.LocalReport.DataSources.Add(_rs);
            this.rvMillReport.LocalReport.DisplayName = "รายงานโรงโม่ประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvMillReport.LocalReport.SetParameters(p);
            this.rvMillReport.RefreshReport();

        }
    }
}
