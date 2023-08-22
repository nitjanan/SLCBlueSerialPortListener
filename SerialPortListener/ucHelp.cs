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
    public partial class ucHelp : UserControl
    {
        private static ucHelp _instance;
        private static ucHelp Instance
        {

            get
            {
                if (_instance == null)
                    _instance = new ucHelp();
                return _instance;
            }
        }
        public ucHelp()
        {
            InitializeComponent();
        }

        private void ucHelp_Load(object sender, EventArgs e)
        {

        }
    }
}
