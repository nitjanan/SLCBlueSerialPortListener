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
    public partial class FPrintProduct : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintProduct(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintProduct_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'truckDataSet.weight' table. You can move, or remove it, as needed.
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
                new Microsoft.Reporting.WinForms.ReportParameter("PMainComp",WeightTempReport.MainComp), //set หัวกระดาษรายงาน 17-11-2025
            };

            this.rvProductReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvProductReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvProductReport.LocalReport.DataSources.Clear();
            this.rvProductReport.LocalReport.DataSources.Add(_rs);
            this.rvProductReport.LocalReport.SetParameters(p);
            this.rvProductReport.LocalReport.DisplayName = "รายงานตามบริษัทประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvProductReport.RefreshReport();
        }
    }
}
