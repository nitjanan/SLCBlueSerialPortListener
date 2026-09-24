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

        // Program Files (ที่ติดตั้งโปรแกรม) เขียนไฟล์ไม่ได้ถ้าไม่ใช่ admin จึงเก็บ config ไว้ใน AppData ของผู้ใช้แทน
        // AppDataDir is per-build (see Utils.AppDataDir) so Blue and Pink never share the same config file.
        private static readonly string AppDataDir = Utils.AppDataDir;
        private static readonly string PortConfigPath =
            System.IO.Path.Combine(AppDataDir, "config_port.txt");

        public ucHelp()
        {
            InitializeComponent();
        
            LoadSerialHandlerSetting();
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
                    cboBaud.SelectedItem = 2400;
                }

                cboParity.SelectedItem = settings.Parity;

                if (cboDataBits.Items.Contains(settings.DataBits))
                {
                    cboDataBits.SelectedItem = settings.DataBits;
                }
                else
                {
                    cboDataBits.SelectedItem = 7;
                }

                cboStopBits.SelectedItem = settings.StopBits;
            }
            else
            {
                // Fallbacks if no manager is set yet
                if (cboPort.Items.Count > 0) cboPort.SelectedIndex = 0;
                cboBaud.SelectedItem = 2400;
                cboParity.SelectedItem = Parity.None;
                cboDataBits.SelectedItem = 7;
                cboStopBits.SelectedItem = StopBits.One;
            }

            // config_serial.txt เก็บพารามิเตอร์สายสัญญาณที่บันทึกไว้ ให้ใช้แทนค่าเริ่มต้นข้างบน
            ApplySavedPortSettings();

            // config_port.txt เก็บพอร์ตที่บันทึกไว้ล่าสุด ถ้ามีไฟล์นี้ให้ใช้แทนค่าจาก _spManager
            string savedPort = LoadSavedPort();
            if (!string.IsNullOrEmpty(savedPort) && ports.Contains(savedPort))
            {
                cboPort.SelectedItem = savedPort;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;

            ApplyPortConfigPermission();
        }

        // ตอน constructor ทำงาน (สร้าง ucHelp เป็นลูกของ MainForm) ยังไม่ผ่าน Login
        // Globals.Permission จึงยังไม่ถูกตั้งค่า เช็คสิทธิ์ใหม่ทุกครั้งที่แสดงหน้านี้แทน
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                ApplyPortConfigPermission();
        }

        // เฉพาะ user ที่มีสิทธิ์ add_setting เท่านั้นที่แก้ไข/บันทึกพอร์ตได้ user อื่นดูได้อย่างเดียว
        private void ApplyPortConfigPermission()
        {
            bool canEdit = Globals.isPermissionAddSetting();

            cboPort.Enabled = canEdit && btnStop.Enabled == false;
            btnSavePort.Visible = canEdit;
            btnSavePort.Enabled = canEdit;

            // กล่องรูปแบบตาชั่งเป็นการตั้งค่าระดับเครื่อง ให้เห็นเฉพาะผู้ที่มีสิทธิ add_setting
            gbScale.Visible = canEdit;
        }

        // อ่านค่า COM port ที่บันทึกไว้จาก config_port.txt (บรรทัดเดียว เช่น "COM4") ถ้าไม่มีไฟล์หรืออ่านไม่ได้คืนค่า null
        private static string LoadSavedPort()
        {
            try
            {
                if (!System.IO.File.Exists(PortConfigPath))
                    return null;

                string[] lines = System.IO.File.ReadAllLines(PortConfigPath);
                return lines.Length > 0 ? lines[0].Trim() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void btnSavePort_Click(object sender, EventArgs e)
        {
            if (!Globals.isPermissionAddSetting())
            {
                MessageBox.Show("คุณไม่มีสิทธิ์บันทึกการตั้งค่านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboPort.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!System.IO.Directory.Exists(AppDataDir))
                    System.IO.Directory.CreateDirectory(AppDataDir);
                System.IO.File.WriteAllLines(PortConfigPath, new[] { cboPort.SelectedItem.ToString() });

                // Baud/Parity/DataBits/StopBits เก็บรวมไว้ที่ config_serial.txt
                if (!SaveCurrentPortSettings())
                {
                    MessageBox.Show("บันทึกพารามิเตอร์สายสัญญาณไม่สำเร็จ", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ปุ่มเดียวเก็บให้ครบ: รูปแบบตาชั่งบันทึกต่อท้ายพอร์ตในครั้งเดียวกัน
                string savedHandler = SaveSelectedScaleHandler();
                if (savedHandler == null)
                {
                    MessageBox.Show("บันทึกรูปแบบตาชั่งไม่สำเร็จ", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string done = "บันทึกการตั้งค่าสำเร็จ";
                if (savedHandler.Length > 0)
                    done += Environment.NewLine + "รูปแบบตาชั่ง : " + savedHandler;
                MessageBox.Show(done, "Port", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกการตั้งค่าไม่สำเร็จ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // บันทึกรูปแบบตาชั่งที่เลือกอยู่ แล้วให้หน้าชั่งใช้วิธีใหม่ทันทีโดยไม่ต้องปิดเปิดโปรแกรม
        // คืนชื่อรูปแบบที่บันทึก, คืน "" เมื่อไม่มีอะไรให้บันทึก, คืน null เมื่อบันทึกไม่สำเร็จ
        private string SaveSelectedScaleHandler()
        {
            int i = cboSerialHandler.SelectedIndex;
            if (i < 0 || i >= SerialDataHandler.Handlers.Length)
                return "";

            SerialDataHandler.HandlerInfo h = SerialDataHandler.Handlers[i];
            if (!SerialDataHandler.SaveSelectedHandler(h.Key))
                return null;

            MainForm mf = this.FindForm() as MainForm;
            if (mf != null)
                mf.ReloadSerialHandler();

            return h.DisplayName;
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
                cboBaud.Enabled = true;
                cboParity.Enabled = true;
                cboDataBits.Enabled = true;
                cboStopBits.Enabled = true;
                ApplyPortConfigPermission();

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

            UpdateWeightPreview(pending);
        }
        // ---- รูปแบบการอ่านค่าจากตาชั่ง ----
        // แต่ละสาขาใช้ตาชั่งคนละรุ่น จึงให้เลือกวิธีอ่านได้จากหน้านี้
        // รายการทั้งหมดและตัวแยกค่าอยู่ที่ SerialDataHandler
        private void LoadSerialHandlerSetting()
        {
            cboSerialHandler.Items.Clear();
            foreach (SerialDataHandler.HandlerInfo h in SerialDataHandler.Handlers)
                cboSerialHandler.Items.Add(h.DisplayName);

            string key = SerialDataHandler.GetSelectedKey();
            for (int i = 0; i < SerialDataHandler.Handlers.Length; i++)
            {
                if (SerialDataHandler.Handlers[i].Key == key)
                {
                    cboSerialHandler.SelectedIndex = i;
                    break;
                }
            }
            if (cboSerialHandler.SelectedIndex < 0 && cboSerialHandler.Items.Count > 0)
                cboSerialHandler.SelectedIndex = 0;
        }

        // ---- ช่องแสดงตัวอย่างน้ำหนัก ----
        // ใช้รูปแบบที่ "กำลังเลือกอยู่ในคอมโบ" ไม่ใช่ที่บันทึกไว้
        // ผู้ใช้จึงลองเปลี่ยนดูได้ว่ารูปแบบไหนตัดค่าถูกต้อง แล้วค่อยกดบันทึก
        private SerialDataHandler.HandlerInfo GetPreviewHandler()
        {
            int i = cboSerialHandler.SelectedIndex;
            if (i >= 0 && i < SerialDataHandler.Handlers.Length)
                return SerialDataHandler.Handlers[i];
            return SerialDataHandler.GetSelectedHandler();
        }

        /// <summary>แยกค่าจากข้อมูลที่รับมาแล้วแสดงในช่องตัวอย่าง</summary>
        private void UpdateWeightPreview(string chunk)
        {
            try
            {
                SerialDataHandler.HandlerInfo h = GetPreviewHandler();
                SerialDataHandler.ParseResult r = SerialDataHandler.Parse(h, tbRx.Text, chunk);

                if (r.HasValue)
                {
                    tbWeightPreview.Text = r.Text;
                    tbWeightPreview.ForeColor = r.IsNegative ? Color.Orange : Color.LightGreen;
                }
                else if (r.IsError)
                {
                    tbWeightPreview.Text = "Error";
                    tbWeightPreview.ForeColor = Color.OrangeRed;
                }
                else
                {
                    // ยังตัดค่าไม่ได้ อาจเป็นเพราะเลือกรูปแบบไม่ตรงกับตาชั่ง หรือข้อมูลยังมาไม่ครบ
                    tbWeightPreview.Text = "- - -";
                    tbWeightPreview.ForeColor = Color.Gray;
                }
            }
            catch (Exception)
            {
            }
        }

        private void cboSerialHandler_SelectedIndexChanged(object sender, EventArgs e)
        {
            // คำนวณใหม่จากข้อมูลที่ค้างอยู่ในหน้าจอ จะได้เห็นผลทันทีโดยไม่ต้องรอข้อมูลก้อนถัดไป
            UpdateWeightPreview(string.Empty);
        }
        // ---- พารามิเตอร์สายสัญญาณ (Baud / Parity / DataBits / StopBits) ----
        // เก็บรวมไว้ที่ config_serial.txt ไฟล์เดียวกับรูปแบบตาชั่ง
        // ส่วนชื่อพอร์ตยังอยู่ที่ config_port.txt ตามเดิม

        /// <summary>เอาค่าที่บันทึกไว้มาใส่คอมโบ และดันเข้า _spManager ให้ใช้ได้ทันที</summary>
        private void ApplySavedPortSettings()
        {
            SerialDataHandler.PortSettings ps = SerialDataHandler.GetPortSettings();

            if (cboBaud.Items.Contains(ps.BaudRate))
                cboBaud.SelectedItem = ps.BaudRate;
            if (cboParity.Items.Contains(ps.Parity))
                cboParity.SelectedItem = ps.Parity;
            if (cboDataBits.Items.Contains(ps.DataBits))
                cboDataBits.SelectedItem = ps.DataBits;
            if (cboStopBits.Items.Contains(ps.StopBits))
                cboStopBits.SelectedItem = ps.StopBits;

            // MainForm อาจเริ่มรับข้อมูลเองโดยไม่ผ่านปุ่ม start จึงต้องตั้งค่าให้ด้วย
            if (_spManager != null && _spManager.CurrentSerialSettings != null)
            {
                SerialSettings settings = _spManager.CurrentSerialSettings;
                settings.BaudRate = ps.BaudRate;
                settings.Parity = ps.Parity;
                settings.DataBits = ps.DataBits;
                settings.StopBits = ps.StopBits;
            }
        }

        /// <summary>เก็บค่าที่เลือกอยู่ในคอมโบลง config_serial.txt</summary>
        private bool SaveCurrentPortSettings()
        {
            SerialDataHandler.PortSettings ps = new SerialDataHandler.PortSettings();
            if (cboBaud.SelectedItem != null)
                ps.BaudRate = (int)cboBaud.SelectedItem;
            if (cboParity.SelectedItem != null)
                ps.Parity = (Parity)cboParity.SelectedItem;
            if (cboDataBits.SelectedItem != null)
                ps.DataBits = (int)cboDataBits.SelectedItem;
            if (cboStopBits.SelectedItem != null)
                ps.StopBits = (StopBits)cboStopBits.SelectedItem;
            return SerialDataHandler.SavePortSettings(ps);
        }







    }
}
