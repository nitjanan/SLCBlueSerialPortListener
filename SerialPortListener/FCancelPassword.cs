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
    public partial class FCancelPassword : Form
    {
        public string ReturnPassword { get; set; }
        public FCancelPassword()
        {
            InitializeComponent();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            this.ReturnPassword = tbPassword.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btOk.PerformClick();
        }
    }
}
