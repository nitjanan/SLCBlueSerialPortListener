using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SerialPortListener
{
    public partial class ucTruck : UserControl
    {
        private static ucTruck _instance;
        private static ucTruck Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucTruck();
                return _instance;
            }
            set
            { }
        }
        public ucTruck()
        {
            InitializeComponent();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void ucTruck_Load(object sender, EventArgs e)
        {

        }
    }
}
