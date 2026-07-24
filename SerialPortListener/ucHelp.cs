using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using SerialPortListener.Serial;

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

        private SerialPortManager _spManager;

        private readonly object _rxLock = new object();
        private StringBuilder _rxBuffer = new StringBuilder();
        private const int MaxRxTextLength = 2000;

        public ucHelp()
        {
            InitializeComponent();
        }

        public void SetSerialPortManager(SerialPortManager spManager)
        {
            this._spManager = spManager;
            if (this._spManager != null)
            {
                this._spManager.NewSerialDataRecieved += _spManager_NewSerialDataRecieved;
            }
        }

        private void ucHelp_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(193, 216, 240);

            // Populate Ports
            cboPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                cboPort.Items.AddRange(ports);
            }

            // Populate Baud Rates
            cboBaud.Items.Clear();
            object[] bauds = new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            cboBaud.Items.AddRange(bauds);

            // Populate Parity
            cboParity.Items.Clear();
            foreach (var val in Enum.GetValues(typeof(Parity)))
            {
                cboParity.Items.Add(val);
            }

            // Populate DataBits
            cboDataBits.Items.Clear();
            object[] dbits = new object[] { 5, 6, 7, 8 };
            cboDataBits.Items.AddRange(dbits);

            // Populate StopBits
            cboStopBits.Items.Clear();
            foreach (var val in Enum.GetValues(typeof(StopBits)))
            {
                cboStopBits.Items.Add(val);
            }

            // Initialize selections from _spManager settings if available
            if (_spManager != null && _spManager.CurrentSerialSettings != null)
            {
                var settings = _spManager.CurrentSerialSettings;
                if (ports.Contains(settings.PortName))
                {
                    cboPort.SelectedItem = settings.PortName;
                }
                else if (cboPort.Items.Count > 0)
                {
                    cboPort.SelectedIndex = 0;
                }

                if (cboBaud.Items.Contains(settings.BaudRate))
                {
                    cboBaud.SelectedItem = settings.BaudRate;
                }
                else
                {
                    cboBaud.SelectedItem = 1200;
                }

                cboParity.SelectedItem = settings.Parity;

                if (cboDataBits.Items.Contains(settings.DataBits))
                {
                    cboDataBits.SelectedItem = settings.DataBits;
                }
                else
                {
                    cboDataBits.SelectedItem = 8;
                }

                cboStopBits.SelectedItem = settings.StopBits;
            }
            else
            {
                // Fallbacks if no manager is set yet
                if (cboPort.Items.Count > 0) cboPort.SelectedIndex = 0;
                cboBaud.SelectedItem = 1200;
                cboParity.SelectedItem = Parity.None;
                cboDataBits.SelectedItem = 8;
                cboStopBits.SelectedItem = StopBits.One;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cboPort.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_spManager == null)
            {
                MessageBox.Show("Serial Port Manager is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var settings = _spManager.CurrentSerialSettings;
                if (settings != null)
                {
                    settings.PortName = cboPort.SelectedItem.ToString();
                    settings.BaudRate = (int)cboBaud.SelectedItem;
                    settings.Parity = (Parity)cboParity.SelectedItem;
                    settings.DataBits = (int)cboDataBits.SelectedItem;
                    settings.StopBits = (StopBits)cboStopBits.SelectedItem;
                }

                _spManager.StartListening();
                timerRx.Start();

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                cboPort.Enabled = false;
                cboBaud.Enabled = false;
                cboParity.Enabled = false;
                cboDataBits.Enabled = false;
                cboStopBits.Enabled = false;

                tbRx.AppendText($"--- Port {cboPort.SelectedItem} started ---\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open serial port: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_spManager == null)
                return;

            try
            {
                _spManager.StopListening();
                timerRx.Stop();

                btnStart.Enabled = true;
                btnStop.Enabled = false;
                cboPort.Enabled = true;
                cboBaud.Enabled = true;
                cboParity.Enabled = true;
                cboDataBits.Enabled = true;
                cboStopBits.Enabled = true;

                tbRx.AppendText("--- Port stopped ---\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing serial port: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Runs on the SerialPort's background thread. Only buffers data - no UI access here,
        // so a burst of fast-arriving data can't flood the UI thread's Invoke queue.
        private void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            try
            {
                string str = Encoding.ASCII.GetString(e.Data);
                lock (_rxLock)
                {
                    _rxBuffer.Append(str);
                }
            }
            catch (Exception)
            {
            }
        }

        // Runs on the UI thread on a fixed interval, draining whatever arrived since the last
        // tick in one go, instead of once per DataReceived event.
        private void timerRx_Tick(object sender, EventArgs e)
        {
            string pending;
            lock (_rxLock)
            {
                if (_rxBuffer.Length == 0)
                    return;
                pending = _rxBuffer.ToString();
                _rxBuffer.Clear();
            }

            try
            {
                tbRx.AppendText(pending);
                if (tbRx.TextLength > MaxRxTextLength)
                    tbRx.Text = tbRx.Text.Remove(0, tbRx.TextLength - MaxRxTextLength);
                tbRx.SelectionStart = tbRx.Text.Length;
                tbRx.ScrollToCaret();
            }
            catch (Exception)
            {
            }
        }
    }
}
