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
    public partial class FPrintCarTeamReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource(); 
        public FPrintCarTeamReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCarTeamReport_Load(object sender, EventArgs e)
        {
            this.rvCarTeamReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCarTeamReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCarTeamReport.LocalReport.DataSources.Clear();
            this.rvCarTeamReport.LocalReport.DataSources.Add(_rs);
            this.rvCarTeamReport.LocalReport.DisplayName = "รายงานชื่อทีมรถร่วม";
            //this.rvJointCarReport.LocalReport.SetParameters(p);
            this.rvCarTeamReport.RefreshReport();
        }
    }
}
