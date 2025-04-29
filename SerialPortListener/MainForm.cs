using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPortListener.Serial;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using Devart.Data.PostgreSql;
using static SerialPortListener.TableFromDB;
using System.Data.Odbc;
using Microsoft.VisualBasic;


namespace SerialPortListener
{

    public partial class MainForm : Form
    {
        SerialPortManager _spManager;
        Datalayer dl;
        String strCalQ = "1.00";
        AutoCompleteStringCollection collCarTeam = new AutoCompleteStringCollection();
        bool isCheckedCash = false;
        bool isCheckedTrans = false;
        bool isCheckedCredit = false;
        bool isCheckedMill1 = false;
        bool isCheckedMill2 = false;
        bool isCheckedMill3 = false;
        bool isCheckedMillNo = false;
        bool isCheckedCleanStone = false;
        bool isCheckedCleanWater = false;
        bool isCheckedCleanNo = false;
        bool isCheckedSelfPick = false;
        bool isCheckedSendTo = false;

        /*1 search anywhere customer */
        // Bind default keywords
        List<string> listOriginalCustomerName = new List<string>();
        // save new keywords
        List<string> listNewCustomerName = new List<string>();


        class ComboboxValue
        {
            public string Id { get; private set; }
            public string Name { get; private set; }

            public ComboboxValue(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public MainForm(string username, String firstname)
        {
            dl = new Datalayer();
            InitializeComponent();

            UserInitialization();

            setDefaultFromDB(username, firstname);

            getSettingDefault();

            _spManager.StartListening();

            timerWeight.Interval = 5000; // 5 seconds
            timerWeight.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _spManager.StopListening();
            _spManager.StartListening();
        }

        public void getSettingDefault()
        {
            lbCompanyCode.Text = Company.Code;

            if (Globals.isPermissionAutoWeight()) //สิทธิ user รายการซื้อให้ set น้ำหนักเป็น 100 auto
                tbWeigtData.Text = "100";

            /* autoComplete ผู้ตัก */
            autoCompleteSettingCompany(tbScoopId, "รหัสผู้ตัก", "base_scoop");
            autoCompleteSettingCompany(tbScoopName, "ชื่อผู้ตัก", "base_scoop");

            /* autoComplete ผู้ชั่ง */
            autoCompleteSetting(tbScaleId, "username", "users");
            autoCompleteSetting(tbScaleName, "firstname", "users");

            /* autoComplete ผู้อนุมัติ */
            autoCompleteSetting(tbApproveId, "รหัสผู้อนุมัติจ่าย", "base_approve");
            autoCompleteSetting(tbApproveName, "ชื่อผู้อนุมัติจ่าย", "base_approve");

            /* autoComplete จังหวัด */
            autoCompleteSetting(tbCarCity, "ชื่อจังหวัด", "base_car_city");

            /* autoComplete ลูกค้า */
            //autoCompleteSetting(tbCustomerId, "รหัสลูกค้า", "base_customer");
            //autoCompleteSetting(tbCustomerName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete โรงโม่ */
            //autoCompleteSettingWeightType(tbMillId, "รหัสโรงโม่", "base_mill");
            //autoCompleteSettingWeightType(tbMillName, "ชื่อโรงโม่", "base_mill");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer");

            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

            tbWeigtData.Enter += (s, e) => { tbWeigtData.Parent.Focus(); };

        }
        public void EnableWeightInAndOut()
        {
            btReadIn.Enabled = true;
            btReadOut.Enabled = true;
        }

        public void disableReadWeightIn() {
            btReadIn.Enabled = false;
        }

        public void disableReadWeightOut() {
            btReadOut.Enabled = false;
        }

        public void resetMainForm() {
            tbId.Text = "";
            tbDocNum.Text = "";
            rbMill1.Checked = false;
            rbMill2.Checked = false;
            rbMill3.Checked = false;
            rbMillNo.Checked = false;
            rbCash.Checked = false;
            rbCredit.Checked = false;
            rbTrans.Checked = false;
            rbVat.Checked = false;
            cbbStoneType.Text = "";
            cbbStoneColor.Text = "";
            cbbTransport.Text = "";
            tbRefNum.Text = "";
            tbCustomerId.Text = "";
            tbCustomerName.Text = "";
            cbbCustomerName.Text = "";
            tbCarLicense.Text = "";
            tbCarCity.Text = "";
            tbDriverName.Text = "";
            cbbMill.Text = "";
            //tbMillId.Text = "";
            //tbMillName.Text = "";

            // login admin ให้เปลี่ยน
            if (Globals.isPermissionEditWeight())
            {
                tbScaleId.Text = "003";
                tbScaleName.Text = "รุ่งฤดี";
            }
            else {
                tbScaleId.Text = Globals.Username;
                tbScaleName.Text = Globals.Firstname;
            }

            tbScoopId.Text = "";
            tbScoopName.Text = "";
            tbWeightIn.Text = "0.00";
            tbWeightOut.Text = "0.00";
            tbWeightTotal.Text = "0.00";
            tbPricePerTon.Text = "0.00";
            tbAmountVat.Text = "0.00";
            tbAmount.Text = "0.00";
            tbShipCost.Text = "0.00";
            tbAmount.Text = "0.00";
            tbVat.Text = "0.00";
            tbApproveId.Text = "";
            tbApproveName.Text = "";
            dtDate.Text = DateTime.Now.ToShortDateString();
            dtWeightInDate.Text = DateTime.Now.ToShortDateString();
            dtWeightOutDate.Text = DateTime.Now.ToShortDateString();
            dtWeightInTime.Text = DateTime.Now.ToLongTimeString();
            dtWeightOutTime.Text = DateTime.Now.ToLongTimeString();
            tbQ.Text = "0.00";
            rbbNonVat.Checked = false;
            rbbVat.Checked = true;
            rbCleanStone.Checked = false;
            rbCleanWater.Checked = false;
            rbCleanNo.Checked = false;
            cbbSite.Text = "";
            cbbCarTeam.Text = "";
            tbNote.Text = "";
            tbOilContent.Text = "0.00";
            fillStoneCombo();
            fillTransportCombo();
            fillMillCombo();
            calculatenumQ();

            disableBtAfterRead(0);
            //if user admin enable all 
            if (Globals.isPermissionTop())
                disableBtAfterRead(3);
            if (Globals.isPermissionEditWeight())
                disableBtAfterRead(999);

        }

        public void runningDocNumber() {
            Boolean IsnewYear = false;
            string todayYear = DateTime.Now.ToString("yyyy");

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.seq_doc_num where run_year = '" + todayYear + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                if (reader.Read())
                {
                    int rdNum = Convert.ToInt32(reader["run_number"].ToString());
                    rdNum++;
                    int lengthRdNum = reader["run_number"].ToString().Length;
                    string format = "D" + lengthRdNum.ToString();
                    tbDocNum.Text = rdNum.ToString(format);
                }
                else
                {
                    IsnewYear = true;
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            if (IsnewYear)
                generateNewSeqNumber();

        }

        private void generateNewSeqNumber() {
            string todayYear = DateTime.Now.ToString("yyyy");
            string runningNumber = "0000000000";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO public.seq_doc_num (run_number, run_year) " +
                    "VALUES ('" + runningNumber + "', '" + todayYear + "') ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception) {
            }
            dl.close();

            //แก้ใน form
            int rdNumNew = Convert.ToInt32(runningNumber);
            rdNumNew++;
            int lengthRdNum = runningNumber.ToString().Length;
            string format = "D" + lengthRdNum.ToString();
            tbDocNum.Text = rdNumNew.ToString(format);
        }

        public void checkDocNumEmty()
        {
            if (tbDocNum.Text == "")
            {
                tbDocNum.Enabled = true;
            }
        }

        private void fillStoneCombo() {
            //ล้างก่อน
            cbbStoneType.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_stone_type where inactive = false ORDER BY รหัสหิน";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read()) {
                    string id = reader["รหัสหิน"].ToString();
                    string des = reader["ชื่อหิน"].ToString();
                    cbbStoneType.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception) {

            }
            dl.close();
        }

        private void fillMillCombo()
        {
            //ล้างก่อน
            cbbMill.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_mill where weight_type = 1 or weight_type = 3 ORDER BY รหัสโรงโม่";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["รหัสโรงโม่"].ToString();
                    string des = reader["ชื่อโรงโม่"].ToString();
                    cbbMill.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillTransportCombo()
        {
            //ล้างก่อน
            cbbTransport.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_transport_name FROM public.base_transport ORDER BY base_transport_id";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["base_transport_name"].ToString();
                    cbbTransport.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        //เรียกจาก TableFromDB
        public void AfterGetDataFromTable() {
            ucTruck.Hide();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
        }

        public void setDataFromClassTableFromDB(DataToUpdate data) {
            tbId.Text = data.id;
            dtDate.Text = data.date;
            tbDocNum.Text = data.docNum;
            tbCarLicense.Text = data.carLicense;
            tbCarCity.Text = data.carCity;
            tbDriverName.Text = data.driverName;
            tbCustomerId.Text = data.customerId;
            tbCustomerName.Text = data.customerName;

            if(data.customerId != "" && data.customerName != "" )
                cbbCustomerName.Text = data.customerId + " : " + data.customerName;

            tbWeightIn.Text = tonTokg(data.weightIn);
            tbWeightOut.Text = tonTokg(data.weightOut);
            tbWeightTotal.Text = tonTokg(data.weightTotal);
            tbRefNum.Text = data.refNum;
            tbScaleId.Text = data.scaleId;
            tbScaleName.Text = data.scaleName;
            tbScoopId.Text = data.scoopId;
            tbScoopName.Text = data.scoopName;
            tbPricePerTon.Text = numberFormat(data.pricePerTon, 2);
            tbAmountVat.Text = numberFormat(data.amountVat, 2);
            tbAmount.Text = numberFormat(data.amount, 2);
            tbVat.Text = numberFormat(data.vat, 2);
            tbShipCost.Text = data.shipCost;
            dtWeightInDate.Text = data.weightInDate;
            dtWeightInTime.Text = data.weightInTime;
            if (tbWeightOut.Text == "0.00")
            {
                dtWeightOutDate.Text = DateTime.Now.ToShortDateString();
                dtWeightOutTime.Text = DateTime.Now.ToShortTimeString();
                btReadOut.Enabled = true;

                if (!checkEmptyTB(tbCarLicense))
                {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }
            }
            else {
                dtWeightOutDate.Text = data.weightOutDate;
                dtWeightOutTime.Text = data.weightOutTime;
                //disable after read out
                disableBtAfterRead(2);

                if (!checkEmptyTB(tbCarLicense))
                {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }
            }
            cbbStoneType.Text = data.stoneType;//111111111111
            tbQ.Text = numberFormat(data.q, 2);
            tbApproveId.Text = data.approveId;
            tbApproveName.Text = data.approveName;
            cbbStoneColor.Text = data.stoneColor;
            cbbTransport.Text = data.transport;
            cbbSite.Text = data.site;//111111111111
            cbbCarTeam.Text = data.team;//111111111111
            //tbMillName.Text = data.mill;//111111111111
            //tbMillId.Text = data.millId;//111111111111
            cbbMill.Text = data.mill;
            tbNote.Text = data.note;
            tbOilContent.Text = numberFormat(data.oilContent, 2);

            //setDataMillToRB(data.mill);
            setDataPayToRB(data.payType);
            setDataVatToRB(data.vatType);
            setDataCleanToRB(data.clean);
            //ดึงหน้างาน
            fillSiteCombo();

            AfterGetDataFromTable();

            //disable after read in
            disableBtAfterRead(1);

            //รหัสยกเลิกให้ปิดช่องให้หมด
            disableCancelId();

            //รหัสแก้ไขน้ำหนักได้
            if (Globals.isPermissionEditWeight())
                disableBtAfterRead(999);

            //if user admin enable all
            /* 111111111
            if (Globals.isPermissionTop())
            {
                disableBtAfterRead(999);
            }
            */
        }

        private string getComboboxSiteUpdate() {

            string selectedName = cbbSite.Text;
            string selectedId = "";

            foreach (ComboboxValue item in cbbSite.Items)
            {

                if (item.Name == selectedName)
                {
                    selectedId = item.Id;
                    break;
                }
            }
            return selectedId;
        }

        private string getComboboxStoneTypeUpdate()
        {

            string selectedName = cbbStoneType.Text;
            string selectedId = "";

            foreach (ComboboxValue item in cbbStoneType.Items)
            {

                if (item.Name == selectedName)
                {
                    selectedId = item.Id;
                    break;
                }
            }
            return selectedId;
        }

        private string getComboboxMillUpdate()
        {

            string selectedName = cbbMill.Text;
            string selectedId = "";

            foreach (ComboboxValue item in cbbMill.Items)
            {

                if (item.Name == selectedName)
                {
                    selectedId = item.Id;
                    break;
                }
            }
            return selectedId;
        }

        private string getComboboxCarTeamUpdate()
        {

            string selectedName = cbbCarTeam.Text;
            string selectedId = "";

            //มี list Items ที่สร้างเอง 1 list เลยต้องใส่ if
            if (cbbCarTeam.Items.Count > 1)
            {
                foreach (ComboboxValue item in cbbCarTeam.Items)
                {

                    if (selectedName == "")
                    {
                        selectedId = "";
                        break;
                    }
                    else if (item.Name == selectedName)
                    {
                        selectedId = item.Id;
                        break;
                    }
                }
            }

            return selectedId;
        }

        private string tonTokg(string tonStr)
        {
            double tmp = Convert.ToDouble(tonStr);
            double deci = tmp * 1000;
            string str = deci.ToString("#,##0.00");
            return str;
        }

        private string numberFormat(string numStr, int format)
        {
            double deci = Convert.ToDouble(numStr);
            string str = "";
            if (format == 1)
                str = deci.ToString();
            else if (format == 2)
                str = deci.ToString("#,##0.00");
            return str;
        }

        private void setDataMillToRB(string dataMill) {
            cbbMill.Text = dataMill; //111111111111
            /*
            if (dataMill.Equals("โรงโม่ 1"))
                rbMill1.Checked = true;
            else if (dataMill.Equals("โรงโม่ 2"))
                rbMill2.Checked = true;
            else if (dataMill.Equals("โรงโม่ 3"))
                rbMill3.Checked = true;
            else if (dataMill.Equals("ไม่มี"))
                rbMillNo.Checked = true;
            */
        }
        private void setDataPayToRB(string dataPay)
        {
            if (dataPay.Equals("เงินสด"))
                rbCash.Checked = true;
            else if (dataPay.Equals("เงินเชื่อ"))
                rbCredit.Checked = true;
            else if (dataPay.Equals("เงินโอน"))
                rbTrans.Checked = true;
            else if (dataPay.Equals("Vat"))
                rbVat.Checked = true;
        }

        private void setDataVatToRB(string dataVat)
        {
            if (dataVat.Equals("ไม่รวมภาษี"))
                rbbVat.Checked = true;
            else if (dataVat.Equals("รวมภาษี"))
                rbbNonVat.Checked = true;
        }

        private void setDataCleanToRB(string dataClean)
        {
            if (dataClean.Equals("ล้างหิน"))
                rbCleanStone.Checked = true;
            else if (dataClean.Equals("สเปรย์น้ำ"))
                rbCleanWater.Checked = true;
            else if (dataClean.Equals("ไม่มี"))
                rbCleanNo.Checked = true;
        }

        private void setDefaultFromDB(string username, String firstname)
        {
            btMenu2.BackColor = Color.LightSkyBlue;
            btMenu3.BackColor = Color.LightSkyBlue;
            btMenu4.BackColor = Color.LightSkyBlue;
            btMenu5.BackColor = Color.LightSkyBlue;
            ucTruck.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucTruck.BringToFront();

            tbScaleId.Text = username;
            tbScaleName.Text = firstname;

            if (Globals.isPermissionSales()) {
                btMenu1.Enabled = false;
                btMenu3.Enabled = false;
            }

            if (!Globals.isPermissionAddSetting())
            {
                btMenu3.Enabled = false;
                btLoadCustomer.Enabled = false;
            }


        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void UserInitialization()
        {
            //Serial Port
            _spManager = new SerialPortManager();
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            /*
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));
            */

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);

        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (Globals.isPermissionAutoWeight()) //สิทธิ user รายการซื้อให้ set น้ำหนักเป็น 100 auto
            {
                tbWeigtData.Text = "100";
            }
            else {

                if (this.InvokeRequired)
                {
                    // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                    this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                    return;
                }

                int maxTextLength = 50; // maximum text length in text box
                if (tbData.TextLength > maxTextLength)
                    tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

                // This application is connected to a GPS sending ASCCI characters, so data is converted to text
                string str = Encoding.ASCII.GetString(e.Data);
                tbData.AppendText(str);
                tbData.ScrollToCaret();

                /*
                try
                {
                    //แสดงเลขน้ำหนักที่กำลังวิ่ง
                    //JOB ขาออก (เครื่องแม่)
                    //string newString = tbData.Text.Remove(tbData.Text.LastIndexOf(""));
                    //string remainingText = newString.Substring(newString.LastIndexOf("q"));


                    //JOB ล่างสุด
                    string newString = tbData.Text.Remove(tbData.Text.LastIndexOf("\r"));
                    string remainingText = newString.Substring(newString.LastIndexOf("p"));
                    MatchCollection mc = Regex.Matches(remainingText, @"\d+");

                    if (mc.Count > 0)
                    {
                        if (Int32.Parse(mc[0].Value) % 10 != 0 || Int32.Parse(mc[0].Value) > 100000)
                        {
                            string tmp = mc[0].Value;
                            tbWeigtData.Text = tmp.Remove(tmp.Length - 1);
                        }
                        else if (Int32.Parse(mc[0].Value) < 10)
                        {
                            tbWeigtData.Text = "0";
                        }
                        else if (String.Compare(tbWeigtData.Text, mc[0].Value) != 0)
                        {
                            tbWeigtData.Text = mc[0].Value.TrimStart('0').PadLeft(1, '0');
                        }

                    }
                }
                catch (Exception ex)
                {

                }
                */

                
                // JOB ขาเข้า
                try
                {
                    //แสดงเลขน้ำหนักที่กำลังวิ่ง


                    string newString = tbData.Text.Remove(tbData.Text.LastIndexOf("\r"));
                    string remainingText = newString.Substring(newString.LastIndexOf("(") + 3);

                    MatchCollection mc = Regex.Matches(remainingText, @"\d+");


                    if (mc.Count > 0)
                    {
                        if (String.Compare(tbWeigtData.Text, mc[0].Value) != 0)
                        {
                            tbWeigtData.Text = mc[0].Value.TrimStart('0').PadLeft(1, '0');
                                //tbWeigtData.ForeColor = Color.LightCoral;
                        }
                        else
                        {
                            tbWeigtData.ForeColor = Color.LightGreen;
                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }

        }

        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            _spManager.StartListening();
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            _spManager.StopListening();
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            try
            {
                _spManager.StopListening();

                /*
                int length = tbData.Text.Length;
                string substring = tbData.Text.Substring(length - 15, 7);
                tbWeightIn.Text = Regex.Match(substring, @"\d+").Value;
                tbData.Text = "";
                */

                tbWeightIn.Text = numberFormat(tbWeigtData.Text, 2);
                dtWeightInTime.Text = DateTime.Now.ToLongTimeString();

                calculateWeight();
                _spManager.StartListening();

                //disable after read in
                if (!Globals.isPermissionTop())
                    disableBtAfterRead(1);
            }
            catch (Exception) {
            }
        }

        /* 
         * mode 0 -> enable all
         * mode 1 -> disable after read in
         * mode 2 -> disable after read out
         */
        private void disableBtAfterRead(int mode) {
            if (mode.Equals(0))
            {
                tbCarLicense.Enabled = true;
                tbCarCity.Enabled = true;

                tbCarCity.Enabled = true;
                dtWeightInDate.Enabled = true;
                dtWeightInTime.Enabled = true;
                dtWeightOutDate.Enabled = true;
                dtWeightOutTime.Enabled = true;
            }
            else if (mode.Equals(1))// weight in
            {
                disableReadWeightIn();
                dtWeightInDate.Enabled = false;
                dtWeightInTime.Enabled = false;

                if(!checkZeroStr(tbWeightIn.Text))
                    tbWeightIn.Enabled = false;
                if (!checkEmptyTB(tbCarLicense)) {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }

            }
            else if (mode.Equals(2))// weight out
            {
                disableReadWeightOut();
                dtWeightOutDate.Enabled = false;
                dtWeightOutTime.Enabled = false;

                tbWeightOut.Enabled = false;
                tbWeightOut.Enabled = false;
                tbWeightTotal.Enabled = false;
                tbQ.Enabled = false;
            }
            else if (mode.Equals(3))//open all admin add
            {
                tbWeightIn.Enabled = true;
                tbWeightOut.Enabled = true;
                tbWeightTotal.Enabled = true;
                tbQ.Enabled = true;
            }
            else if (mode.Equals(4))//disable all
            {
                dtWeightInDate.Enabled = false;
                dtWeightInTime.Enabled = false;

                dtWeightOutDate.Enabled = false;
                dtWeightOutTime.Enabled = false;

                tbWeightIn.Enabled = false;
                tbWeightOut.Enabled = false;
                tbWeightTotal.Enabled = false;
                tbQ.Enabled = false;
            }
            else if (mode.Equals(999)) {//open all edit weight
                tbWeightIn.Enabled = true;
                tbWeightOut.Enabled = true;
                tbWeightTotal.Enabled = true;
                tbQ.Enabled = true;

                tbCarLicense.Enabled = true;
                tbCarCity.Enabled = true;
            }
        }


        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btMenu1_Click(object sender, EventArgs e)
        {
            btMenu1.BackColor = Color.White;
            btMenu2.BackColor = Color.LightSkyBlue;
            btMenu3.BackColor = Color.LightSkyBlue;
            btMenu4.BackColor = Color.LightSkyBlue;
            btMenu5.BackColor = Color.LightSkyBlue;

            ucTruck.BringToFront();
            ucTruck.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();

            TableFromDB mf = new TableFromDB(this);
            mf.ShowDialog();
        }

        private void btMenu2_Click(object sender, EventArgs e)
        {
            btMenu2.BackColor = Color.White;
            btMenu1.BackColor = Color.LightSkyBlue;
            btMenu3.BackColor = Color.LightSkyBlue;
            btMenu4.BackColor = Color.LightSkyBlue;
            btMenu5.BackColor = Color.LightSkyBlue;

            ucReport.Show();
            ucTruck.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucReport.BringToFront();

        }
        private void btMenu3_Click(object sender, EventArgs e)
        {
            btMenu3.BackColor = Color.White;
            btMenu1.BackColor = Color.LightSkyBlue;
            btMenu2.BackColor = Color.LightSkyBlue;
            btMenu4.BackColor = Color.LightSkyBlue;
            btMenu5.BackColor = Color.LightSkyBlue;

            ucSetting.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucTruck.Hide();
            ucBackup.Hide();
            ucSetting.BringToFront();
        }
        private void btMenu4_Click(object sender, EventArgs e)
        {
            btMenu4.BackColor = Color.White;
            btMenu1.BackColor = Color.LightSkyBlue;
            btMenu2.BackColor = Color.LightSkyBlue;
            btMenu3.BackColor = Color.LightSkyBlue;
            btMenu5.BackColor = Color.LightSkyBlue;

            ucHelp.Show();
            ucTruck.Hide();
            ucReport.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucHelp.BringToFront();
        }

        private void btMenu5_Click(object sender, EventArgs e)
        {
            btMenu5.BackColor = Color.White;
            btMenu1.BackColor = Color.LightSkyBlue;
            btMenu2.BackColor = Color.LightSkyBlue;
            btMenu3.BackColor = Color.LightSkyBlue;
            btMenu4.BackColor = Color.LightSkyBlue;

            ucBackup.Show();
            ucHelp.Hide();
            ucTruck.Hide();
            ucReport.Hide();
            ucSetting.Hide();
            ucBackup.BringToFront();
        }



        private void pnHelp_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ucHelp_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void btReadOut_Click(object sender, EventArgs e)
        {
            try {
                _spManager.StopListening();

                /*
                int length = tbData.Text.Length;
                string substring = tbData.Text.Substring(length - 15, 7);
                tbWeightOut.Text = Regex.Match(substring, @"\d+").Value;
                */
                tbWeightOut.Text = numberFormat(tbWeigtData.Text, 2);
                dtWeightOutTime.Text = DateTime.Now.ToLongTimeString();

                calculateWeight();
                _spManager.StartListening();

                //disable after read out
                if (!Globals.isPermissionTop())
                    disableBtAfterRead(2);
            }
            catch (Exception) {
            }
        }

        private void calculateWeight() {
            string weightIn = tbWeightIn.Text;
            string weightOut = tbWeightOut.Text;
            double numWeightIn = 0;
            double numWeightOut = 0;

            if (weightIn != "" && weightIn != null && weightOut != "" && weightOut != null) {
                try {

                    numWeightIn = Convert.ToDouble(weightIn);
                    numWeightOut = Convert.ToDouble(weightOut);
                    double numWeight = 0;
                    if (numWeightIn > numWeightOut)
                        numWeight = numWeightIn - numWeightOut;
                    else if (numWeightIn < numWeightOut)
                        numWeight = numWeightOut - numWeightIn;
                    tbWeightTotal.Text = numWeight.ToString("#,##0.00");

                }
                catch (Exception) {
                }
            }

        }

        private Boolean checkDuplicateRunningNumber() {
            Boolean isDuplicate = false;
            string todayYear = DateTime.Now.ToString("yyyy");
            string startDate = todayYear + "-01-01";
            string endDate = todayYear + "-12-31";

            //sql get weight id
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT เลขที่เอกสาร FROM weight WHERE เลขที่เอกสาร = '" + tbDocNum.Text + "' AND วันที่ BETWEEN '" + startDate + "' AND '" + endDate + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    isDuplicate = true;
                    //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
            }
            dl.close();
            return isDuplicate;
        }


        private void btSave_Click(object sender, EventArgs e)
        {
            autoSave();
        }

        private void autoSave()
        {
            Boolean isPasswordCorrect = true;
            if (tbId.Text == "")
            {
                isPasswordCorrect = checkCancelAction();

                //เช็คค่าว่าง
                if (tbDocNum.Text == "")
                    MessageBox.Show("เลขที่การชั่งเป็นค่าว่าง กรุณใส่เลขที่การชั่ง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else if (!isPasswordCorrect)
                    MessageBox.Show("รหัสยกเลิกผิด ไม่สามารถบันทึกข้อมูลได้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //เช็คเลขซ้ำกัน
                else if (checkDuplicateRunningNumber())
                    MessageBox.Show("เลขที่การชั่งนี้ใช้ไปแล้ว กรุณาเข้าหน้าต่างใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    saveAction();
            }
            else
            {
                isPasswordCorrect = checkCancelAction();
                if (isPasswordCorrect)
                    updateAction();
                else
                    MessageBox.Show("รหัสยกเลิกผิด ไม่สามารถบันทึกข้อมูลได้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void saveAction() {

            Boolean isSuccess = false;
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight (วันที่, เลขที่เอกสาร, ทะเบียนรถ, จังหวัด, คนขับ, ลูกค้า, น้ำหนักรถ, น้ำหนักรวม, น้ำหนักสินค้า , เลขที่ใบตัก, โรงโม่, ชนิดหิน, จ่ายเงิน, รหัสผู้ชั่ง, รหัสผู้ตัก, ราคาตัน, จำนวณเงิน, ค่าขนส่ง, วันที่ชั่งเข้า, เวลาชั่งเข้า, วันที่ชั่งออก, เวลาชั่งออก, รหัสลูกค้า, ชื่อผู้ชั่ง, ชื่อผู้ตัก, vat, รหัสผู้อนุมัติจ่าย, ชื่อผู้อนุมัติจ่าย, คิว, ชนิดvat, จำนวนเงินสุทธิ, ประเภทหิน, หน้างาน, ทีม, ล้าง, ขนส่ง, หมายเหตุ, carry_type_name, base_weight_station_name, bws, oil_content, site_id, stone_type_id, mill_id, car_team_id)" +
                                     "VALUES ('" + dtDate.Value.ToString("yyyy-MM-dd") + "','" + tbDocNum.Text + "','" + tbCarLicense.Text.TrimEnd() + "','" + tbCarCity.Text + "','" + tbDriverName.Text + "','" + tbCustomerName.Text + "','" + kgToTon(tbWeightIn) + "'" + ",'"
                                     + kgToTon(tbWeightOut) + "','" + kgToTon(tbWeightTotal) + "','" + tbRefNum.Text + "','" + cbbMill.Text + "','" + cbbStoneType.Text + "','" + getPayRadioValue() + "','" + tbScaleId.Text + "','"
                                     + tbScoopId.Text + "','" + numberFormat(tbPricePerTon.Text, 1) + "','" + numberFormat(tbAmount.Text, 1) + "','" + tbShipCost.Text + "','" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightInTime.Text + "','" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightOutTime.Text + "','"
                                     + tbCustomerId.Text + "','" + tbScaleName.Text + "','" + tbScoopName.Text + "','" + numberFormat(tbVat.Text, 1) + "','" + tbApproveId.Text + "','" + tbApproveName.Text + "','" + numberFormat(tbQ.Text, 1) + "','" + getVatRadioValue() + "','" + numberFormat(tbAmountVat.Text, 1) + "','"
                                     + cbbStoneColor.Text + "','" + cbbSite.Text + "','" + cbbCarTeam.Text + "','" + getCleanRadioValue() + "','" + cbbTransport.Text + "','" + tbNote.Text + "','" + findcarryTypeByTransport() + "', (SELECT base_weight_station_name FROM base_weight_station WHERE base_weight_station_id = 1 ) , (SELECT code FROM base_weight_station WHERE base_weight_station_id = 1 ) ,'"
                                     + numberFormat(tbOilContent.Text, 1) + "','" + getComboboxId(cbbSite) + "','" + getComboboxId(cbbStoneType) + "','" + getComboboxId(cbbMill) + "','" + getComboboxId(cbbCarTeam) + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                isSuccess = runningDocNumberAfterSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();

            //set WeightId
            if (isSuccess)
                setWeightId();

            //ปิดช่องหลัง save
            disableAfterSave();
        }

        private void saveWeightHistory()
        {
            
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight_log (weight_id, วันที่, เลขที่เอกสาร, ทะเบียนรถ, จังหวัด, คนขับ, ลูกค้า, น้ำหนักรถ, น้ำหนักรวม, น้ำหนักสินค้า , เลขที่ใบตัก, โรงโม่, ชนิดหิน, จ่ายเงิน, รหัสผู้ชั่ง, รหัสผู้ตัก, ราคาตัน, จำนวณเงิน, ค่าขนส่ง, วันที่ชั่งเข้า, เวลาชั่งเข้า, วันที่ชั่งออก, เวลาชั่งออก, รหัสลูกค้า, ชื่อผู้ชั่ง, ชื่อผู้ตัก, vat, รหัสผู้อนุมัติจ่าย, ชื่อผู้อนุมัติจ่าย, คิว, ชนิดvat, จำนวนเงินสุทธิ, ประเภทหิน, หน้างาน, ทีม, ล้าง, ขนส่ง, หมายเหตุ, carry_type_name, base_weight_station_name, oil_content, site_id, stone_type_id, mill_id, car_team_id)" +
                                     "VALUES ('" + tbId.Text + "','" + dtDate.Value.ToString("yyyy-MM-dd") + "','" + tbDocNum.Text + "','" + tbCarLicense.Text.TrimEnd() + "','" + tbCarCity.Text + "','" + tbDriverName.Text + "','" + tbCustomerName.Text + "','" + kgToTon(tbWeightIn) + "'" + ",'"
                                     + kgToTon(tbWeightOut) + "','" + kgToTon(tbWeightTotal) + "','" + tbRefNum.Text + "','" + cbbMill.Text + "','" + cbbStoneType.Text + "','" + getPayRadioValue() + "','" + tbScaleId.Text + "','"
                                     + tbScoopId.Text + "','" + numberFormat(tbPricePerTon.Text, 1) + "','" + numberFormat(tbAmount.Text, 1) + "','" + tbShipCost.Text + "','" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightInTime.Text + "','" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightOutTime.Text + "','"
                                     + tbCustomerId.Text + "','" + tbScaleName.Text + "','" + tbScoopName.Text + "','" + numberFormat(tbVat.Text, 1) + "','" + tbApproveId.Text + "','" + tbApproveName.Text + "','" + numberFormat(tbQ.Text, 1) + "','" + getVatRadioValue() + "','" + numberFormat(tbAmountVat.Text, 1) + "','"
                                     + cbbStoneColor.Text + "','" + cbbSite.Text + "','" + cbbCarTeam.Text + "','" + getCleanRadioValue() + "','" + cbbTransport.Text + "','" + tbNote.Text + "','" + findcarryTypeByTransport() + "', (SELECT base_weight_station_name FROM base_weight_station WHERE base_weight_station_id = 1 ) ,'"
                                     + numberFormat(tbOilContent.Text, 1) + "','" + getComboboxId(cbbSite) + "','" + getComboboxId(cbbStoneType) + "','" + getComboboxId(cbbMill) + "','" + getComboboxId(cbbCarTeam) + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            dl.close();

        }

        private void setWeightId() {
            //sql get weight id
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT weight_id FROM public.weight WHERE เลขที่เอกสาร = '" + tbDocNum.Text + "' AND วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["weight_id"].ToString();
                    tbId.Text = rdStr;
                    //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void updateAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE weight SET ทะเบียนรถ = '" + tbCarLicense.Text.TrimEnd() + "' , จังหวัด = '" + tbCarCity.Text + "' , คนขับ = '" + tbDriverName.Text + "', ลูกค้า = '" + tbCustomerName.Text + "' , น้ำหนักรถ = '" + kgToTon(tbWeightIn) + "' , น้ำหนักรวม = '" + kgToTon(tbWeightOut) + "'" +
                                    " , น้ำหนักสินค้า = '" + kgToTon(tbWeightTotal) + "' , เลขที่ใบตัก = '" + tbRefNum.Text + "' , โรงโม่ = '" + cbbMill.Text + "' , ชนิดหิน = '" + cbbStoneType.Text + "' , จ่ายเงิน = '" + getPayRadioValue() + "' , รหัสผู้ชั่ง = '" + tbScaleId.Text + "'" +
                                    " , รหัสผู้ตัก = '" + tbScoopId.Text + "' , ราคาตัน = '" + numberFormat(tbPricePerTon.Text, 1) + "' , จำนวณเงิน = '" + numberFormat(tbAmount.Text, 1) + "' , ค่าขนส่ง = '" + tbShipCost.Text + "' , วันที่ชั่งเข้า = '" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งเข้า = '" + dtWeightInTime.Text + "'" +
                                    " , วันที่ชั่งออก = '" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งออก = '" + dtWeightOutTime.Text + "'  , รหัสลูกค้า = '" + tbCustomerId.Text + "'  , ชื่อผู้ชั่ง = '" + tbScaleName.Text + "' , ชื่อผู้ตัก = '" + tbScoopName.Text + "' , vat = '" + numberFormat(tbVat.Text, 1) +
                                    "' , รหัสผู้อนุมัติจ่าย = '" + tbApproveId.Text + "' , ชื่อผู้อนุมัติจ่าย = '" + tbApproveName.Text + "' , คิว = '" + numberFormat(tbQ.Text, 1) + "' , ชนิดvat = '" + getVatRadioValue() + "' , จำนวนเงินสุทธิ = '" + numberFormat(tbAmountVat.Text, 1) + "' , ประเภทหิน = '" + cbbStoneColor.Text +
                                    "' , หน้างาน = '" + cbbSite.Text + "' , ทีม = '" + cbbCarTeam.Text + "' , ล้าง = '" + getCleanRadioValue() + "' , ขนส่ง = '" + cbbTransport.Text + "' , carry_type_name = '" + findcarryTypeByTransport() + "' , หมายเหตุ = '" + tbNote.Text + "' , oil_content = '" + numberFormat(tbOilContent.Text, 1) +
                                    "' , site_id = '" + getComboboxSiteUpdate() + "' , stone_type_id = '" + getComboboxStoneTypeUpdate() + "' , mill_id = '" + getComboboxMillUpdate() + "' , car_team_id = '" + getComboboxCarTeamUpdate() +
                                    "' WHERE วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND weight_id = " + tbId.Text + " ; ";

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                while (reader.Read())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();

            //ปิดช่องหลัง save
            disableAfterSave();
        }

        //get combobox id use to save or update
        private string getComboboxId(ComboBox cbb)
        {
            string tmp = "";

            try
            {
                if (cbb.SelectedIndex > -1)
                {
                    ComboboxValue tmpComboboxValue = (ComboboxValue)cbb.SelectedItem;
                    tmp = tmpComboboxValue.Id;
                }
            }
            catch (Exception ex)
            {

            }
            return tmp;
        }

        private void disableAfterSave()
        {
            if (!Globals.isPermissionEditWeight()) {
                if (!checkZeroStr(tbWeightIn.Text))
                    disableBtAfterRead(1);
                if (!checkZeroStr(tbWeightOut.Text))
                    disableBtAfterRead(2);     
            }

            //รหัสยกเลิกให้ปิดช่องให้หมด
            disableCancelId();

            //19-09-2023 มาเก็บ weight history ตรงนี้นะ
            saveWeightHistory();
        }

        private void disableCancelId()
        {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001")
            {
                disableBtAfterRead(4);
                if (checkEmptyTB(tbNote))
                {
                    MessageBox.Show("กรุณาใส่เหตุผลในการยกเลิก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbNote.Select();
                }
                updateStatusCancel(true);
            }
            else {
                updateStatusCancel(false);
            }
        }

        private void updateStatusCancel(Boolean status)
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE weight SET is_cancel =  " + status + "  WHERE วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND weight_id = " + tbId.Text + " ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                while (reader.Read())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private Boolean checkZeroStr(string str)
        {
            Double temp;
            Boolean isOk = Double.TryParse(str, out temp);
            Int32 value = isOk ? (Int32)temp : 0;

            return value == 0 ? true : false;
        }

        private Boolean checkEmptyTB(TextBox tb) {
            return string.IsNullOrEmpty(tb.Text) == true ? true : false;
        }


        private Boolean runningDocNumberAfterSave()
        {
            Boolean isSuccess = false;
            string todayYear = DateTime.Now.ToString("yyyy");
            //sql find
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.seq_doc_num where run_year = '" + todayYear + "'";
            try
            {

                //sql update
                pgCommand.CommandText = "UPDATE public.seq_doc_num SET run_number = '" + tbDocNum.Text + "' where run_year = '" + todayYear + "'";
                OdbcDataReader reader = pgCommand.ExecuteReader();
                isSuccess = true;

            }
            catch (Exception)
            {
            }
            return isSuccess;
        }

        private string getMillRadioValue() {
            string value = "";
            if (rbMill1.Checked)
                value = rbMill1.Text;
            else if (rbMill2.Checked)
                value = rbMill2.Text;
            else if (rbMill3.Checked)
                value = rbMill3.Text;
            else if (rbMillNo.Checked)
                value = rbMillNo.Text;
            return value;
        }

        private string getCleanRadioValue()
        {
            string value = "";
            if (rbCleanStone.Checked)
                value = rbCleanStone.Text;
            else if (rbCleanWater.Checked)
                value = rbCleanWater.Text;
            else if (rbCleanNo.Checked)
                value = rbCleanNo.Text;
            return value;
        }
        private string getPayRadioValue()
        {
            string value = "";
            if (rbCash.Checked)
                value = rbCash.Text;
            else if (rbCredit.Checked)
                value = rbCredit.Text;
            if (rbTrans.Checked)
                value = rbTrans.Text;
            return value;
        }

        private string getVatRadioValue()
        {
            string value = null;
            if (rbbNonVat.Checked)
                value = rbbNonVat.Text;
            else if (rbbVat.Checked)
                value = rbbVat.Text;
            return value;
        }

        private string getVatRadioValuePrint()
        {
            string value = null;
            if (rbbNonVat.Checked)
            {
                value = "ใบส่งของ";
                Company.CompanyName = " ";
                Company.Address = " ";
                Company.Email = " ";
                Company.Telephone = " ";
                Company.TTelephone = " ";
                Company.TEmail = " ";
            }
            else if (rbbVat.Checked)
            {
                value = "ใบส่งสินค้า";
                getDefaultCompany();
            }
            return value;
        }


        private void getDefaultCompany()
        {
            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_company where base_company_id = 1 ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    Company.CompanyName = reader["company_name"].ToString();
                    Company.Address = reader["address"].ToString();
                    Company.Telephone = reader["telephone"].ToString();
                    Company.Email = reader["email"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

        }


        /* autoComplete Setting */
        private void autoCompleteSetting(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString();
                    coll.Add(rdStr);

                }
            }
            catch (Exception) {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /* autoComplete Setting */
        private void autoCompleteSettingCompany(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName + " where company = '"+ Company.Code +"' " ;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString();
                    coll.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /* autoComplete Setting Weight Type*/
        private void autoCompleteSettingWeightType(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName +" WHERE weight_type = 1 or weight_type = 3 ";

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString();
                    coll.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /*3 search anywhere customer */
        public void setautoCompleteCustomer(string fieldId, string fieldName, string tableName)
        {
            cbbCustomerName.Items.Clear();
            listOriginalCustomerName.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            //old -- pgCommand.CommandText = "SELECT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE base_job_type_id IS NOT NULL AND base_vat_type_id  IS NOT NULL ORDER BY " + fieldId;
            //20-09 not show inactive not confirm
            pgCommand.CommandText = "SELECT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE weight_type = 1 or weight_type = 3 ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() + " : " + reader[fieldName].ToString();
                    listOriginalCustomerName.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();

            cbbCustomerName.Items.AddRange(listOriginalCustomerName.ToArray());
        }

        private void tbCustomerName_TextChanged(object sender, EventArgs e)
        {
            //customerNameTextChanged();
        }

        private void customerNameTextChanged()
        {
            try
            {
                tbCustomerId.Text = cbbCustomerName.Text.Substring(0, cbbCustomerName.Text.IndexOf(" : "));

                int start = cbbCustomerName.Text.IndexOf(" : ") + 3;
                int end = cbbCustomerName.Text.Length - 11;
                tbCustomerName.Text = cbbCustomerName.Text.Substring(start, end);

                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

                Boolean isWrong = checkInputWrong(tbCustomerName, "ชื่อลูกค้า", "base_customer", tbCustomerId, "รหัสลูกค้า");
                if (isWrong)
                    cbbCustomerName.Text = "";
            }
            catch (Exception ex) {
                tbCustomerId.Text = "";
                tbCustomerName.Text = "";
                cbbCustomerName.Text = "";
            }

            /*
            Boolean isWrongId = checkInputWrong(tbCustomerId, "รหัสลูกค้า", "base_customer", tbCustomerName);
            Boolean isWrongName = checkInputWrong(tbCustomerName, "ชื่อลูกค้า", "base_customer", tbCustomerId);

            if (isWrongId || isWrongName)
               cbbCustomerName.Text = "";
            */


            /*
            if (cbbCustomerName.Text != null && cbbCustomerName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where ชื่อลูกค้า = '" + cbbCustomerName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสลูกค้า"].ToString();
                        tbCustomerId.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีชื่อลูกค้า " + cbbCustomerName.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerId.Text = "";
                        cbbCustomerName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            }
            else
            {
                tbCustomerId.Text = "";
                Weight.CustomerAddress = " ";
            }
            */
        }
        private Boolean checkInputWrong(TextBox tb, string field, string table, TextBox tbsecond, string fieldSecond)
        {
            Boolean isWrong = false;
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + field + " FROM public." + table + " where " + field + " = '" + tb.Text + "' AND " + fieldSecond + " = '" + tbsecond.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();

                //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                if (!reader.HasRows)
                {
                    //MessageBox.Show("ไม่มี " + tb.AccessibleName + " นี้ กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isWrong = true;
                    tb.Text = "";
                    tbsecond.Text = "";
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            return isWrong;
        }

        private void tbCustomerId_TextChanged(object sender, EventArgs e)
        {
            //customerIdTextChanged();

            //หาการล้าง,สเปรย์จากลูกค้าและชนิดหิน
            setDataCleanByCustomerAndStoneType();
        }

        private void customerIdTextChanged()
        {

            if (tbCustomerId != null && tbCustomerId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where รหัสลูกค้า = '" + tbCustomerId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อลูกค้า"].ToString();
                        cbbCustomerName.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีรหัสลูกค้า " + tbCustomerId.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerId.Text = "";
                        cbbCustomerName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            }
            else
            {
                cbbCustomerName.Text = "";
                Weight.CustomerAddress = " ";
            }
        }


        private void tbScaleId_TextChanged(object sender, EventArgs e)
        {
            if (tbScaleId != null && tbScaleId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.users where username = '" + tbScaleId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["firstname"].ToString();
                        tbScaleName.Text = rdStr;
                    }
                }
                catch (Exception) {
                }
                dl.close();
            }
            else {
                tbScaleName.Text = "";
            }
        }

        private void tbScaleName_TextChanged(object sender, EventArgs e)
        {
            if (tbScaleName != null && tbScaleName.Text != "") {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.users where firstname = '" + tbScaleName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["username"].ToString();
                        tbScaleId.Text = rdStr;
                    }
                }
                catch (Exception) {
                }
                dl.close();
            }
            else {
                tbScaleId.Text = "";
            }

        }

        private void tbPricePerTon_TextChanged(object sender, EventArgs e)
        {
            //calculateAmount();
            calculateVat();
        }
        private void tbWeightTotal_TextChanged(object sender, EventArgs e)
        {
            //calculateAmount();
            calculateVat();
            if (cbbStoneType.SelectedIndex != -1)
                calculatenumQ();
        }

        //ไม่ใช้แล้ว 03-09-2024 เนื่องจากมีการคำนวน vat (รวมภาษี) แบบใหม่
        private void calculateAmount() {
            try {
                double total = 0;
                total = Convert.ToDouble(tbWeightTotal.Text);
                double price = 0;
                price = Convert.ToDouble(tbPricePerTon.Text);
                double amount = 0;
                amount = (total / 1000) * price;
                tbAmountVat.Text = amount.ToString("#,##0.00");

                //set Temp
                tbAmount.Text = tbAmountVat.Text;
            }
            catch (Exception ex) {
                //MessageBox.Show(ex.ToString());
            }
        }

        private double getAmount()
        {
            double amount = 0;
            try
            {
                double total = 0;
                total = Convert.ToDouble(tbWeightTotal.Text);
                double price = 0;
                price = Convert.ToDouble(tbPricePerTon.Text);
                amount = (total / 1000) * price;
                tbAmount.Text = amount.ToString("#,##0.00");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return amount;
        }

        private void calculatenumQ() {
            try {
                if (!checkZeroStr(tbWeightIn.Text) && !checkZeroStr(tbWeightOut.Text) && !string.IsNullOrEmpty(strCalQ))
                {
                    double numCalQ = Convert.ToDouble(strCalQ);
                    double numWeightTotal = Convert.ToDouble(tbWeightTotal.Text);
                    double numQ = numWeightTotal / (numCalQ * 1000);
                    tbQ.Text = numQ.ToString("#,##0.00");
                }
                else {
                    tbQ.Text = "0.00";
                }

            }
            catch (Exception e) {

            }
        }

        private void btPrintIn_Click(object sender, EventArgs e)
        {
            //เช็คค่าว่าง
            showErrorWeightInEmty();

            //ปริ้น
            preparePrint(1);

            if (checkDuplicateRunningNumber() && tbId.Text == "") {
                //ไม่ต้องทำไร
            }
            else {
                //save อัตโนมัติ
                autoSave();

                FPrint f = new FPrint();
                f.ShowDialog();
            }

        }

        private void preparePrint(int mode) {
            Company.TTelephone = "โทร";
            Company.TEmail = "E-mail";
            Weight.Date = dtDate.Text;
            Weight.DocNum = "J" + tbDocNum.Text;
            Weight.Mill = strNotEmty(cbbMill.Text);
            Weight.DriverName = strNotEmty(tbDriverName.Text);
            Weight.CustomerName = strNotEmty(tbCustomerName.Text);
            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            Weight.StoneType = strNotEmty(cbbStoneType.Text);
            Weight.CarLicense = strNotEmty(tbCarLicense.Text);
            Weight.CarCity = strNotEmty(tbCarCity.Text);
            Weight.DateIn = strNotEmty(dtWeightInDate.Text);
            Weight.TimeIn = strNotEmty(dtWeightInTime.Text);
            Weight.DateOut = strNotEmty(dtWeightOutDate.Text);
            Weight.TimeOut = strNotEmty(dtWeightOutTime.Text);
            Weight.WeightIn = kgToTon(tbWeightIn);
            Weight.WeightOut = kgToTon(tbWeightOut);
            Weight.WeightTotal = kgToTon(tbWeightTotal);
            Weight.Price = tbPricePerTon.Text;
            Weight.Amount = tbAmount.Text;
            Weight.Vat = tbVat.Text;
            Weight.AmountVat = tbAmountVat.Text;
            Weight.Q = tbQ.Text;
            Weight.Team = strNotEmty(cbbCarTeam.Text);
            Weight.StoneColor = strNotEmty(cbbStoneColor.Text);
            Weight.Site = strNotEmty(cbbSite.Text);
            Weight.ApproveName = strNotEmty(tbApproveName.Text);
            Weight.Pay = strNotEmty(getPayRadioValue());
            Weight.VatType = getVatRadioValuePrint();
            Weight.Clean = strNotEmty(getCleanRadioValue());
            Weight.Transport = strNotEmty(cbbTransport.Text);
            Weight.OilContent = zeroNotEmty(tbOilContent.Text);
            Weight.Id = tbId.Text;
            Weight.Note = strNotEmty(tbNote.Text);


            if (mode.Equals(3))
            {
                //ปริ้นทั้ง IN และ OUT
                Company.TDocName = "เลขที่การชั่ง";
                Company.TLogo = "(Sandvik)";
            }
            else if (mode.Equals(2)) {
                //ปริ้น OUT
                Weight.Pay = " ";
                Weight.DocNum = " ";
                Weight.DateIn = " ";
                Weight.TimeIn = " ";
                Weight.WeightIn = " ";
                Weight.CustomerName = " ";
                Weight.CustomerAddress = " ";
                Weight.Site = " ";
                Weight.StoneType = " ";
                Weight.CarLicense = " ";
                Weight.CarCity = " ";
                Weight.DriverName = " ";
                Weight.Team = " ";
                Weight.Transport = " ";
                Company.TDocName = " ";
                Company.TLogo = " ";
            }
            else if (mode.Equals(1)) {
                //ปริ้น IN
                Weight.Mill = " ";
                Weight.StoneColor = " ";
                Weight.Clean = " ";
                Weight.ApproveName = " ";
                Weight.DateOut = " ";
                Weight.TimeOut = " ";
                Weight.WeightOut = " ";
                Weight.WeightTotal = " ";
                Weight.Q = " ";
                Weight.Price = " ";
                Weight.Amount = " ";
                Weight.Vat = " ";
                Weight.AmountVat = " ";
                Weight.OilContent = " ";
                Company.TDocName = "เลขที่การชั่ง";
                Company.TLogo = "(Sandvik)";
                Weight.DatePrintAndCopyNum = " ";
                Weight.Note = " ";

            }
            else if (mode.Equals(4))
            {
                Weight.WeightIn = decimalToInt(tbWeightIn.Text);
                Weight.WeightOut = decimalToInt(tbWeightOut.Text);
                Weight.WeightTotal = decimalToInt(tbWeightTotal.Text);
                Weight.CustomerId = strNotEmty(tbCustomerId.Text);
                Weight.StoneTypeId = getStoneTypeId();
                Weight.DocNum = tbDocNum.Text;
                Weight.Price = decimalToInt(tbPricePerTon.Text);
                Weight.AmountVat = decimalToInt(tbAmountVat.Text);
            }


        }

        private string strNotEmty(string str) {
            return str == "" ? " " : str;
        }

        private string zeroNotEmty(string str)
        {
            return str == "0.00" || str == "0" ? " " : str + " (L)";
        }

        private string decimalToInt(string str)
        {
            int nt = Convert.ToInt32(Convert.ToDecimal(str));
            return nt.ToString("#,##0");
        }


        private string getPrintFromDB(string database, string field, string fieldCondition, string condition) {
            //sql
            string rdStr = " ";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + field + " FROM public." + database + " where " + fieldCondition + " = '" + condition + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    rdStr = reader[field].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();


            if (rdStr == null || rdStr == "") {
                rdStr = " ";
            }

            return rdStr;

        }

        private string kgToTon(TextBox tb)
        {
            double tmp = Convert.ToDouble(tb.Text);
            double deci = tmp / 1000;
            string str = string.Format("{0:0.000}", deci);
            return str;
        }

        private void btLoadCustomer_Click(object sender, EventArgs e)
        {
            TableCustomer tc = new TableCustomer(this);
            tc.ShowDialog();
        }

        private void cbbStoneType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT ค่าคำนวณคิว FROM public.base_stone_type where ชื่อหิน = '" + cbbStoneType.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["ค่าคำนวณคิว"].ToString();
                    strCalQ = rdStr;
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            //Weight.StoneColor = getPrintFromDB("base_stone_type", "ประเภทหิน", "ชื่อหิน", cbbStoneType.Text);
            //คำนวณค่าคิว
            calculatenumQ();

            //หาการล้าง,สเปรย์จากลูกค้าและชนิดหิน
            setDataCleanByCustomerAndStoneType();
        }
        private void textboxFormatDecimal(object sender, KeyPressEventArgs e, TextBox textBox)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            if (!char.IsControl(e.KeyChar))
            {

                textBox = (TextBox)sender;

                if (textBox.Text.IndexOf('.') > -1 &&
                         textBox.Text.Substring(textBox.Text.IndexOf('.')).Length >= 3)
                {
                    e.Handled = true;
                }

            }

        }

        private void tbPricePerTon_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbPricePerTon);
        }

        private void rbbNonVat_CheckedChanged(object sender, EventArgs e)
        {
            if (rbbNonVat.Checked)
            {
                try
                {
                    double tempAmount = getAmount();
                    double vat = tempAmount - (tempAmount / 107) * 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount - vat;
                    tbAmount.Text = total.ToString("#,##0.00");
                    tbAmountVat.Text = tempAmount.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
        }

        private void rbbVat_CheckedChanged(object sender, EventArgs e)
        {
            if (rbbVat.Checked)
            {
                try
                {
                    double tempAmount = getAmount();
                    double vat = (tempAmount * 7.0) / 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount + vat;
                    tbAmount.Text = tempAmount.ToString("#,##0.00");
                    tbAmountVat.Text = total.ToString("#,##0.00");
                }
                catch (Exception ec)
                {

                }
            }
        }

        private void calculateVat() {
            double tempAmount = getAmount();
            if (rbbVat.Checked)
            {
                try
                {
                    double vat = (tempAmount * 7.0) / 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount + vat;
                    tbAmount.Text = tempAmount.ToString("#,##0.00");
                    tbAmountVat.Text = total.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
            else if (rbbNonVat.Checked)
            {
                try
                {
                    double vat = tempAmount - (tempAmount / 107) * 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount - vat;
                    tbAmount.Text = total.ToString("#,##0.00");
                    tbAmountVat.Text = tempAmount.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
        }

        private void tbApproveId_TextChanged(object sender, EventArgs e)
        {
            if (tbApproveId != null && tbApproveId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_approve where รหัสผู้อนุมัติจ่าย = '" + tbApproveId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อผู้อนุมัติจ่าย"].ToString();
                        tbApproveName.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbApproveName.Text = "";
            }

        }

        private void tbApproveName_TextChanged(object sender, EventArgs e)
        {
            if (tbApproveName != null && tbApproveName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_approve where ชื่อผู้อนุมัติจ่าย = '" + tbApproveName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสผู้อนุมัติจ่าย"].ToString();
                        tbApproveId.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbApproveId.Text = "";
            }
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }
        private void convertFormatToDecimal(TextBox tb) {
            try
            {
                double d = Convert.ToDouble(tb.Text);
                tb.Text = d.ToString("#,##0.00");
            }
            catch (Exception ex) {
                MessageBox.Show("ชนิดของข้อมูลผิด กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "0.00";
            }
        }

        private void tbWeightTotal_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightTotal);
        }

        private void tbAmount_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbAmount);
        }

        private void tbVat_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbVat);
        }

        private void tbAmountVat_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbAmountVat);
        }

        private void tbQ_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbQ);
        }

        private void tbWeightOut_TextChanged(object sender, EventArgs e)
        {
            calculateWeight();
        }

        private void tbWeightIn_TextChanged(object sender, EventArgs e)
        {
            calculateWeight();
        }

        private void tbCarLicense_TextChanged(object sender, EventArgs e)
        {
            fillCarTeamCombo();

            /*
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                //pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car where ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                pgCommand.CommandText = "SELECT base_car_team.ชื่อทีม FROM base_car INNER JOIN base_car_team ON base_car.รหัสทีม = base_car_team.รหัสทีม WHERE base_car.ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อทีม"].ToString();
                        tbCarTeam.Text = rdStr;
                        collCarTeam.Add(rdStr);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbCarTeam.Text = "";
            }
            */
            
        }

        private void tbCarTeam_Click(object sender, EventArgs e)
        {
            tbCarTeam.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbCarTeam.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbCarTeam.AutoCompleteCustomSource = collCarTeam;
        }

        //timer old Tick 21-05-2024
        private void timerWeight_Tick(object sender, EventArgs e)
        {
            tbWeigtData.Text = tbWeigtData.Text;
        }

        private Boolean checkCancelAction()
        {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001")
            {
                /*
                using (var form = new FCancelPassword())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string password = form.ReturnPassword;
                        if (password == "pdg]bd=yj'")
                        {
                */
                            tbWeightIn.Text = "0.00";
                            tbWeightOut.Text = "0.00";
                            tbWeightTotal.Text = "0.00";
                            tbPricePerTon.Text = "0.00";
                            tbAmount.Text = "0.00";
                            tbAmountVat.Text = "0.00";
                            tbVat.Text = "0.00";
                            tbQ.Text = "0.00";
                            return true;
                /*
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }
                */
            }
            return true;
        }

        private void checkResetWeight() {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001") {
                tbWeightIn.Text = "0.00";
                tbWeightOut.Text = "0.00";
                tbWeightTotal.Text = "0.00";
                tbPricePerTon.Text = "0.00";
                tbAmount.Text = "0.00";
                tbAmountVat.Text = "0.00";
                tbVat.Text = "0.00";
            }
        }

        private void tbCustomerId_Leave(object sender, EventArgs e)
        {
            /*
            checkResetWeight();
            customerIdTextChanged();
            */
        }

        private void tbCustomerName_Leave(object sender, EventArgs e)
        {
            checkResetWeight();
            customerNameTextChanged();
        }

        private void rbMill1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void rbMill1_Click(object sender, EventArgs e)
        {
            
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked)
            {
                radio.Checked = false;
            }


        }

        private void rbCash_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCash = radio.Checked;
        }

        private void rbCash_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCash)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCash = false;
            }
        }

        private void rbTrans_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedTrans = radio.Checked;
        }

        private void rbTrans_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedTrans)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedTrans = false;
            }
        }

        private void rbCredit_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCredit = radio.Checked;
        }

        private void rbCredit_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCredit)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCredit = false;
            }
        }

        private void rbMill1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill1 = radio.Checked;
        }

        private void rbMill1_Click_1(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill1)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill1 = false;
            }
        }

        private void rbMill2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill2 = radio.Checked;
        }

        private void rbMill2_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill2)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill2 = false;
            }
        }

        private void rbMill3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill3 = radio.Checked;
        }

        private void rbMill3_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill3)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill3 = false;
            }
        }

        private void rbCleanStone_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanStone = radio.Checked;
        }

        private void rbCleanStone_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanStone)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanStone = false;
            }
        }

        private void rbCleanWater_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanWater = radio.Checked;
        }

        private void rbCleanWater_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanWater)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanWater = false;
            }
        }

        private void btPrintOut_Click(object sender, EventArgs e)
        {
            //เช็คค่าว่าง
            showErrorWeightOutEmty();

            //ปริ้น
            preparePrint(2);
            if (checkDuplicateRunningNumber() && tbId.Text == ""){
                //ไม่ต้องทำไร
            }
            else{
                //save อัตโนมัติ
                autoSave();
                HandleSuccessfulPrint();
                //Print
                FPrint f = new FPrint();
                f.ShowDialog();
            }

        }

        private void btPrintAll_Click(object sender, EventArgs e)
        {
            //เช็คค่าว่าง
            showErrorWeightInEmty();
            showErrorWeightOutEmty();

            //ปริ้น
            preparePrint(3);
            if (checkDuplicateRunningNumber() && tbId.Text == ""){
                //ไม่ต้องทำไร
            }
            else {
                //save อัตโนมัติ
                autoSave();
                HandleSuccessfulPrint();
                //Print
                FPrint f = new FPrint();
                f.ShowDialog();
            }
        }

        private void HandleSuccessfulPrint()
        {
            int copy_num = findLastCopyByWeightId();
            copy_num++;

            Weight.DatePrint = DateTime.Now.ToString("yyyy-MM-dd");
            Weight.DatePrintAndCopyNum = DateTime.Now.ToString("dd/MM") + "#"+ copy_num;
            Weight.TimePrint = DateTime.Now.ToString("HH:mm:ss");

            //save weight copy
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight_copy (copy_num, date_print, time_print, user_print, weight_id )" +
                                     "VALUES ('" + copy_num + "','" + Weight.DatePrint + "','" + Weight.TimePrint + "','" + Globals.UserId + "','" + tbId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }


        private int findLastCopyByWeightId()
        {
            int copy_num = 0;

            if (tbId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "select copy_num from weight_copy where weight_id = '" + tbId.Text + "' ORDER BY weight_copy_id DESC LIMIT 1";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        copy_num = Convert.ToInt32(reader["copy_num"].ToString());
                    }
                    else
                    {
                        copy_num = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                dl.close();
            }

            return copy_num;
        }

        private void rbMillNo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMillNo = radio.Checked;
        }

        private void rbMillNo_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMillNo)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMillNo = false;
            }
        }

        private void rbCleanNo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanNo = radio.Checked;
        }

        private void rbCleanNo_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanNo)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanNo = false;
            }
        }

        private void showErrorEmtyTextBox(TextBox tb)
        {
            if (string.IsNullOrEmpty(tb.Text) || tb.Text == "0.00")
                MessageBox.Show("' "+ tb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorEmtyComboBox(ComboBox cbb)
        {
            if (string.IsNullOrEmpty(cbb.Text))
                MessageBox.Show("' " + cbb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorEmtyRadioButton(GroupBox gb)
        {
            var rd = gb.Controls.OfType<RadioButton>()
                    .FirstOrDefault(n => n.Checked);
            if(rd == null)
                MessageBox.Show("' " + gb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorWeightInEmty() {
            showErrorEmtyRadioButton(groupBox2);
            showErrorEmtyComboBox(cbbStoneType);
            showErrorEmtyComboBox(cbbTransport);
            showErrorEmtyTextBox(tbCarLicense);
            //showErrorEmtyTextBox(tbCarCity);
            showErrorEmtyTextBox(tbWeightIn);
        }

        private void showErrorWeightOutEmty()
        {
            showErrorEmtyRadioButton(groupBox2);
            showErrorEmtyTextBox(tbScoopId);
            showErrorEmtyTextBox(tbScoopName);
            //showErrorEmtyRadioButton(groupBox1);
            //showErrorEmtyComboBox(cbbMill);
            showErrorEmtyComboBox(cbbMill);
            showErrorEmtyRadioButton(groupBox4);
            showErrorEmtyTextBox(tbQ);

        }

        /*4 search anywhere customer */
        private void cbbCustomerName_TextUpdate(object sender, EventArgs e)
        {   
             setSearchAnywhereToCombobox(cbbCustomerName,listOriginalCustomerName,listNewCustomerName);
        }

        /*5 search anywhere customer */
        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew) {

            try
            {
                //clear combobox
                cb.Items.Clear();
                //clear listNew
                listNew.Clear();

                    foreach (var item in listOriginal)
                    {
                        // call ToLower() .. not case sensitive
                        if (item.ToLower().Contains(cb.Text))
                        {
                            //add to ListNew
                            listNew.Add(item);
                        }
                    }

                if (listNew.Count > 0)
                {
                    cb.Items.AddRange(listNew.ToArray());
                    cb.SelectionStart = cb.Text.Length;
                    Cursor = Cursors.Default;
                    // Automatically pop up drop-down
                    cb.DroppedDown = true;
                }
                else {

                    cb.Items.AddRange(listOriginal.ToArray());
                    cb.DroppedDown = false;
                }
                

            }
            catch (Exception)
            {
            }

        }

        private void cbbCustomerName_Leave(object sender, EventArgs e)
        {
                checkResetWeight();
                customerNameTextChanged();
                fillSiteCombo();
        }

        private void fillSiteCombo()
        {
            //ล้างก่อน
            cbbSite.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_site.base_site_id, base_site.base_site_name FROM public.base_customer_site INNER JOIN public.base_site ON base_customer_site.site_id = base_site.base_site_id where customer_id = '" + tbCustomerId.Text + "'";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["base_site_id"].ToString();
                    string des = reader["base_site_name"].ToString();
                    //cbbSite.Items.Add(des);
                    cbbSite.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillCarTeamCombo()
        {

                //ล้างก่อน
                cbbCarTeam.Items.Clear();

                //เพิ่ม combobox
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT base_car_team.รหัสทีม , base_car_team.ชื่อทีม FROM base_car INNER JOIN base_car_team ON base_car.รหัสทีม = base_car_team.รหัสทีม WHERE base_car.ชื่อรถร่วม = '" + tbCarLicense.Text + "' order by base_car_team.รหัสทีม";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string id = reader["รหัสทีม"].ToString();
                        string des = reader["ชื่อทีม"].ToString();
                        //cbbSite.Items.Add(des);
                        cbbCarTeam.Items.Add(new ComboboxValue(id, des));
                        cbbCarTeam.SelectedIndex = 0;
                    }
                }
                catch (Exception)
                {

                }
                dl.close();
                cbbCarTeam.Items.Add("");
        }

        private void cbbCustomerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbbSite.Text = "";
        }

        private string findcarryTypeByTransport()
        {
            string carryTypeName = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_carry_type.base_carry_type_name FROM base_carry_type INNER JOIN base_transport ON base_carry_type.base_carry_type_id = base_transport.base_carry_type_id where base_transport_name = '" + cbbTransport.Text + "'";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                     carryTypeName = reader["base_carry_type_name"].ToString();
                }
            }
            catch(Exception)
            {

            }
            dl.close();

            return carryTypeName;
        }

        private void tbOilContent_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbOilContent);
        }

        private void tbWeightIn_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightIn);
            checkNumWeightMany(tbWeightIn);
        }

        private void tbWeightOut_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightOut);
            checkNumWeightMany(tbWeightOut);
        }

        private void checkNumWeightMany(TextBox tb) {
            if(tb.Text.Length > 9)
            {
                MessageBox.Show("ช่อง "+ tb.AccessibleName + "มีน้ำหนักเกิน กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "0.00";
            }
        
        }

        private void tbCarLicense_KeyUp(object sender, KeyEventArgs e)
        {
            findLastCityByCarLicense();
            findLastTransportByCarLicense();
        }

        private void findLastCityByCarLicense()
        {
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                //pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car where ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                pgCommand.CommandText = "SELECT จังหวัด from weight where ทะเบียนรถ = '" + tbCarLicense.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["จังหวัด"].ToString();
                        tbCarCity.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbCarCity.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbCarCity.Text = "";
            }
        }

        private void findLastTransportByCarLicense()
        {
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT ขนส่ง from weight where ทะเบียนรถ = '" + tbCarLicense.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ขนส่ง"].ToString();
                        cbbTransport.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbTransport.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                cbbTransport.Text = "";
            }
        }

        private void setDataCleanByCustomerAndStoneType()
        {
            if (tbCustomerId != null && tbCustomerId.Text != "" && cbbStoneType != null && cbbStoneType.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT ล้าง from weight where รหัสลูกค้า = '" + tbCustomerId.Text + "' and ชนิดหิน = '" + cbbStoneType.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ล้าง"].ToString();
                        //set data clean
                        setDataCleanToRB(rdStr);
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        setDataCleanToRB("ไม่มี");
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void tbScoopId_Leave(object sender, EventArgs e)
        {
            if (tbScoopId != null && tbScoopId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_scoop where รหัสผู้ตัก = '" + tbScoopId.Text + "' and company = '" + Company.Code + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อผู้ตัก"].ToString();
                        tbScoopName.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbScoopId.Text = "";
                        tbScoopName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbScoopName.Text = "";
            }
        }

        private void tbScoopName_Leave(object sender, EventArgs e)
        {
            if (tbScoopName != null && tbScoopName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_scoop where ชื่อผู้ตัก = '" + tbScoopName.Text + "' and company = '" + Company.Code + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสผู้ตัก"].ToString();
                        tbScoopId.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbScoopId.Text = "";
                        tbScoopName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbScoopId.Text = "";
            }
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            /* autoComplete ผู้ตัก */
            autoCompleteSettingCompany(tbScoopId, "รหัสผู้ตัก", "base_scoop");
            autoCompleteSettingCompany(tbScoopName, "ชื่อผู้ตัก", "base_scoop");

            /* autoComplete โรงโม่ */
            //autoCompleteSettingWeightType(tbMillId, "รหัสโรงโม่", "base_mill");
            //autoCompleteSettingWeightType(tbMillName, "ชื่อโรงโม่", "base_mill");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer");

            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

            fillStoneCombo();
            fillTransportCombo();
            fillMillCombo();
        }

        private void tbMillId_Leave(object sender, EventArgs e)
        {
            if (tbMillId != null && tbMillId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and รหัสโรงโม่ = '" + tbMillId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อโรงโม่"].ToString();
                        tbMillName.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbMillId.Text = "";
                        tbMillName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbMillName.Text = "";
            }
        }

        private void tbMillName_Leave(object sender, EventArgs e)
        {
            if (tbMillName != null && tbMillName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and ชื่อโรงโม่ = '" + tbMillName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสโรงโม่"].ToString();
                        tbMillId.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbMillId.Text = "";
                        tbMillName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbMillId.Text = "";
            }
        }

        private void btPrintVRock_Click(object sender, EventArgs e)
        {
            //ปริ้น
            preparePrint(4);
            if (checkDuplicateRunningNumber() && tbId.Text == "")
            {
                //ไม่ต้องทำไร
            }
            else
            {
                FPrintVRock f = new FPrintVRock();
                f.ShowDialog();
            }

            //save อัตโนมัติ
            autoSave();

        }

        private string getStoneTypeId()
        {
            string stoneTypeId = " ";
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสหิน FROM public.base_stone_type  where ชื่อหิน = '" + cbbStoneType.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    stoneTypeId = reader["รหัสหิน"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            return stoneTypeId;
        }

        private void cbbStoneType_Leave(object sender, EventArgs e)
        {
            if (cbbStoneType != null && cbbStoneType.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_stone_type where inactive = false and ชื่อหิน = '" + cbbStoneType.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbStoneType.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลชนิดหินนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void cbbMill_Leave(object sender, EventArgs e)
        {
            if (cbbMill != null && cbbMill.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and ชื่อโรงโม่ = '" + cbbMill.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbMill.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลต้นทางนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void cbbSite_Leave(object sender, EventArgs e)
        {
            if (cbbSite != null && cbbSite.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_site where (weight_type = 1 or weight_type = 3) and base_site_name = '" + cbbSite.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbSite.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลปลายทางนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }
    }
}
