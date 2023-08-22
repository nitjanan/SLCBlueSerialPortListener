using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class TableCustomer : Form
    {
        MainForm mainForm;

        Datalayer dl = null;

        //customer
        OdbcDataAdapter adtCustomer;
        DataTable dtCustomer;
        OdbcCommandBuilder cmbCustomer;

        //customer
        OdbcDataAdapter adtSite;
        DataTable dtSite;
        OdbcCommandBuilder cmbSite;

        // Bind default keywords
        List<string> listOriginalCustomerName = new List<string>();
        // save new keywords
        List<string> listNewCustomerName = new List<string>();

        public TableCustomer(MainForm parent)
        {
            InitializeComponent();
            mainForm = parent;
            dl = new Datalayer();
        }

        /*Base Customer*/
        private void setDataSouceForDGVCustomer()
        {
            if (cbbJobType.SelectedIndex != -1 && cbbVatType.SelectedIndex != -1) { 
                try
                {
                    dl.connect();
                    StringBuilder sql = new StringBuilder();
                    sql.Append(" SELECT base_customer.*  FROM ((base_customer ");
                    sql.Append(" INNER JOIN base_job_type ON base_customer.base_job_type_id = base_job_type.base_job_type_id) ");
                    sql.Append(" INNER JOIN base_vat_type ON base_customer.base_vat_type_id = base_vat_type.base_vat_type_id) ");
                    sql.Append(" WHERE base_job_type_name = '" + cbbJobType.Text + "' AND base_vat_type_name = '" + cbbVatType.Text + "'  ORDER BY รหัสลูกค้า ");
                    adtCustomer = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                    dtCustomer = new DataTable();
                    adtCustomer.Fill(dtCustomer);
                    dgvCustomer.DataSource = dtCustomer;
                }
                catch (Exception)
                {
                }
                dl.close();      
            }

        }

        private void setDataCustomer()
        {
            if (dgvCustomer.Rows.Count > 1)
            {
                tbCustomerId.Text = dgvCustomer.CurrentRow.Cells["รหัสลูกค้า"].Value.ToString();
                tbCustomerName.Text = dgvCustomer.CurrentRow.Cells["ชื่อลูกค้า"].Value.ToString();
                tbCustomerAddress.Text = dgvCustomer.CurrentRow.Cells["ที่อยู่"].Value.ToString();

                //เปิดปิดช่องเมื่อมีไม่มีค่า
                if (tbCustomerId.Text != "")
                    tbCustomerId.ReadOnly = true;
                else
                    tbCustomerId.ReadOnly = false;
            }
            else {
                tbCustomerId.Text = "";
                tbCustomerName.Text = "";
                tbCustomerAddress.Text = "";
                tbCustomerId.ReadOnly = false;
            }
        }


        private void TableCustomer_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet2.base_site' table. You can move, or remove it, as needed.
            //this.base_siteTableAdapter1.Fill(this.dataSet2.base_site);
            // TODO: This line of code loads data into the 'dataSet1.base_customer' table. You can move, or remove it, as needed.
            //this.base_customerTableAdapter1.Fill(this.dataSet1.base_customer);
            // TODO: This line of code loads data into the 'truckDataSet2.base_site' table. You can move, or remove it, as needed.
            //this.base_siteTableAdapter.Fill(this.baseSiteDataSet.base_site);
            // TODO: This line of code loads data into the 'customerDataSet.base_customer' table. You can move, or remove it, as needed.
            //this.base_customerTableAdapter.Fill(this.customerDataSet.base_customer);

            setDataSouceForDGVCustomer();

            //set ค่าให้ช่อง combobox ลูกค้า
            setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerName);
            // set cobbobox vat type
            fillComboAll(cbbVatType, "base_vat_type", "base_vat_type_name");
            // set cobbobox job type
            fillComboAll(cbbJobType, "base_job_type", "base_job_type_name");

        }

        private void setautoComplete(ComboBox cb, string fieldId, string fieldName, string tableName, List<string> listOriginal)
        {
            cb.Items.Clear();
            listOriginal.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT DISTINCT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE base_job_type_id IS NOT NULL AND base_vat_type_id  IS NOT NULL ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() + " : " + reader[fieldName].ToString();
                    listOriginal.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();
            cb.Items.AddRange(listOriginal.ToArray());
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) {
                /*
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    basecustomerBindingSource.RemoveCurrent();
                    saveAction();
                }
                */
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    deleteDTGV(dgvCustomer, "รหัสลูกค้า", "base_customer");
                }
            }
        }
        private void saveAction() {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecustomerBindingSource.EndEdit();
                base_customerTableAdapter.Update(this.customerDataSet.base_customer);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void saveAndUpdateDTGV(OdbcCommandBuilder cmb, OdbcDataAdapter adt, DataTable dt, DataGridView dgv, String tableName)
        {
            try
            {
                dl.connect();
                cmb = new OdbcCommandBuilder(adt);
                adt.Update(dt);
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dl.close();
            }
            catch (System.InvalidOperationException exUpdate)
            {
                updateDTGV(dgv, tableName);
            }
            catch (OdbcException exDuplicate)
            {
                MessageBox.Show("มีรหัสที่ซ้ำกัน กรุณากรอกข้อมูลใหม่", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง" + ex, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void updateDTGV(DataGridView dgv, String tableName)
        {
            //dgv.CurrentRow.ErrorText = "";

            int numCol = dgv.Columns.Count;

            string idName = dgv.Columns[0].Name;
            string idValue = dgv.CurrentRow.Cells[idName].Value.ToString();

            string colOneName = dgv.Columns[1].Name;
            string colOneValue = dgv.CurrentRow.Cells[colOneName].Value.ToString();

            string colTwoName = null;
            string colTwoValue = null;

            if (numCol > 2)
            {
                colTwoName = dgv.Columns[2].Name;
                colTwoValue = dgv.CurrentRow.Cells[colTwoName].Value.ToString();
            }

            //sql update
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sqlTxt = new StringBuilder();
            sqlTxt.Append("UPDATE public." + tableName + " SET " + colOneName + " = '" + colOneValue + "' ");
            if (numCol > 2)
                sqlTxt.Append(" , " + colTwoName + " = '" + colTwoValue + "' ");
            sqlTxt.Append(" WHERE " + idName + " = '" + idValue + "' ");
            pgCommand.CommandText = sqlTxt.ToString();
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("อัพเดทข้อมูลเรียบร้อย", "ลบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dl.close();
        }

        private void deleteDTGV(DataGridView dgv, String cellName, String tableName)
        {
            int rowIndex = dgv.CurrentCell.RowIndex;
            string id = dgv.CurrentRow.Cells[cellName].Value.ToString();

            //sql delete
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM public." + tableName + " where " + cellName + " = '" + id + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                dgv.Rows.RemoveAt(rowIndex);
                MessageBox.Show("ลบข้อมูลเรียบร้อย", "ลบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dl.close();
        }

        private void cbbCustomerSiteName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbCustomerSiteName, listOriginalCustomerName, listNewCustomerName);
        }

        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew)
        {
            try {
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
                else
                {
                    cb.Items.AddRange(listOriginal.ToArray());
                    cb.DroppedDown = false;
                }

            }
            catch (Exception)
            {

            }
 
        }

        private void cbbCustomerSiteName_Leave(object sender, EventArgs e)
        {
            try { 
                tbCustomerSiteId.Text = cbbCustomerSiteName.Text.Substring(0, cbbCustomerSiteName.Text.IndexOf(" : "));

                //ดึงเฉพาะหน้างานของลูกค้านั้นๆ
                setDataSouceForDGVSite();

                //set ค่าจากตารางใส่ในช่อง textbox
                setDataCustomerSite();
            } catch (Exception)
            {
                tbCustomerSiteId.Text = "";
                cbbCustomerSiteName.Text = "";
            }
            /*
            if (cbbCustomerSiteName.Text != null && cbbCustomerSiteName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where ชื่อลูกค้า = '" + cbbCustomerSiteName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสลูกค้า"].ToString();
                        tbCustomerSiteId.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีชื่อลูกค้า " + cbbCustomerSiteName.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerSiteId.Text = "";
                        cbbCustomerSiteName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();

                //ดึงเฉพาะหน้างานของลูกค้านั้นๆ
                setDataSouceForDGVSite();

                //set ค่าจากตารางใส่ในช่อง textbox
                setDataCustomerSite();

            }
            else
            {
                tbCustomerSiteId.Text = "";
            }
            */
        }

        /*Base Site*/
        private void setDataSouceForDGVSite()
        {
            if (cbbCustomerSiteName.SelectedIndex != -1) {
                try
                {
                    dl.connect();
                    StringBuilder sql = new StringBuilder();
                    sql.Append("SELECT *  FROM public.base_site where base_customer_id = '" + tbCustomerSiteId.Text + "' ORDER BY base_site_id ");
                    adtSite = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                    dtSite = new DataTable();
                    adtSite.Fill(dtSite);
                    dgvSite.DataSource = dtSite;
                }
                catch (Exception)
                {
                }
                dl.close();            
            }
        }

        private void setDataCustomerSite()
        {
            if (dgvSite.Rows.Count > 1)
            {
                tbSiteId.Text = dgvSite.CurrentRow.Cells["base_site_id"].Value.ToString();
                tbSiteName.Text = dgvSite.CurrentRow.Cells["base_site_name"].Value.ToString();

                //เปิดปิดช่องเมื่อมีไม่มีค่า
                if (tbSiteId.Text != "")
                    tbSiteId.ReadOnly = true;
                else
                    tbSiteId.ReadOnly = false;
            }
            else {
                tbSiteId.Text = "";
                tbSiteName.Text = "";
                tbSiteId.ReadOnly = false;
            }
        }

        private void dgvSite_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_car_team*/
            if (cbbCustomerSiteName.SelectedIndex != -1)
                setDataCustomerSite();
        }

        private void btClearSite_Click(object sender, EventArgs e)
        {
            tbSiteId.Text = "";
            tbSiteId.ReadOnly = false;
            tbSiteName.Text = "";
        }

        private void btSaveSite_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (tbCustomerSiteId.Text == "" || cbbCustomerSiteName.SelectedIndex == -1)
            {
                MessageBox.Show("กรุณาเลือกชื่อลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbSiteId.Text == "")
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ กรุณาใส่รหัสหน้างาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT base_site_id FROM public.base_site where base_site_id = '" + tbSiteId.Text + "' AND base_customer_id  = '" + tbCustomerSiteId.Text + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    isUpdate = reader.Read();
                }
                catch (Exception)
                {
                }
                dl.close();

                //update or save
                if (isUpdate && tbSiteId.ReadOnly)
                    updateBaseSiteAction();
                else
                    saveBaseSiteAction();

            }
        }

        private void updateBaseSiteAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_site SET base_site_name = '" + tbSiteName.Text + "' WHERE base_site_id = '" + tbSiteId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVSite();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseSiteAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO base_site (base_site_id, base_site_name, base_customer_id)" +
                                     "VALUES ('" + tbSiteId.Text + "','" + tbSiteName.Text + "','" + tbCustomerSiteId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVSite();
            }
            catch (Exception ex)
            {
                MessageBox.Show("รหัสหน้างานนี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสหน้างาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();
        }

        private void btDelSite_Click(object sender, EventArgs e)
        {
            if (tbSiteId.Text == "")
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_site WHERE base_site_id = '" + tbSiteId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ทีมรถ*/
                        //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                        //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                        setDataSouceForDGVSite();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();

                    //clear ช่อง
                    clearTwoTextbox(tbSiteId, tbSiteName);
                }
            }
        }

        private void fillComboAll(ComboBox cbb, String tableName, String fieldName)
        {
            //ล้างก่อน
            cbb.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT "+ fieldName + " FROM public."+ tableName + "";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader[fieldName].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void cbbVatType_SelectedIndexChanged(object sender, EventArgs e)
        {
            setDataSouceForDGVCustomer();
            setDataCustomer();
        }

        private void cbbJobType_SelectedIndexChanged(object sender, EventArgs e)
        {
            setDataSouceForDGVCustomer();
            setDataCustomer();
            getJobTypeIdFromName();
        }
        private void getJobTypeIdFromName() {

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_job_type_id FROM public.base_job_type WHERE base_job_type_name = '" + cbbJobType.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    tbJobId.Text = reader["base_job_type_id"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_customer*/
            if (cbbVatType.SelectedIndex != -1 && cbbJobType.SelectedIndex != -1)
                setDataCustomer();
        }

        private void btClearCustomer_Click(object sender, EventArgs e)
        {
            tbCustomerId.Text = "";
            tbCustomerId.ReadOnly = false;
            tbCustomerName.Text = "";
            tbCustomerAddress.Text = "";
        }

        private void btSaveCustomer_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (cbbJobType.SelectedIndex == -1 || cbbVatType.SelectedIndex == -1)
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ กรุณาเลือกชนิดและประเภทลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text == "")
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ กรุณาใส่รหัสลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text.Any(char.IsLower))
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องจากรหัสเป็นตัวพิมเล็ก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT รหัสลูกค้า FROM public.base_customer where รหัสลูกค้า = '" + tbCustomerId.Text + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    isUpdate = reader.Read();
                }
                catch (Exception)
                {
                }
                dl.close();

                //update or save
                if (isUpdate && tbCustomerId.ReadOnly)
                    updateBaseCustomerAction();
                else
                    saveBaseCustomerAction();

                //set ค่าให้ช่อง combobox ลูกค้า
                setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerName);

            }
        }

        private void updateBaseCustomerAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_customer SET ชื่อลูกค้า = '" + tbCustomerName.Text + "' , ที่อยู่ = '" + tbCustomerAddress.Text + "' WHERE รหัสลูกค้า = '" + tbCustomerId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCustomer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseCustomerAction()
        {
            if (tbCustomerId.Text.Length > 8) {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องจากรหัสลูกค้ามีมากกว่า 8 หลัก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }else if (tbCustomerId.Text.Contains(":") || tbCustomerName.Text.Contains(":")) {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องมีตัวอักษร : ในรหัสลูกค้าหรือชื่อลูกค้านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }else {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "INSERT INTO base_customer (รหัสลูกค้า, ชื่อลูกค้า, ที่อยู่, base_job_type_id, base_vat_type_id)" +
                                         "VALUES ('" + tbCustomerId.Text + "','" + tbCustomerName.Text + "','" + tbCustomerAddress.Text + "', (SELECT base_job_type_id FROM base_job_type WHERE base_job_type_name = '" + cbbJobType.Text + "' ) , (SELECT base_vat_type_id FROM base_vat_type WHERE base_vat_type_name = '" + cbbVatType.Text + "' ) )";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    /*กรองตาม ทีมรถ*/
                    //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                    //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                    setDataSouceForDGVCustomer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("รหัสลูกค้านี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสลูกค้าใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                dl.close();
            }

        }

        private void cbbJobType_Leave(object sender, EventArgs e)
        {
            getJobTypeIdFromName();
        }

        private void btDelCustomer_Click(object sender, EventArgs e)
        {
            if (tbCustomerId.Text == "")
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    //ลบหน้างานที่ผูกกับลูกค้านั้นๆก่อน
                    deleteSiteBeforeCustomer();

                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_customer WHERE รหัสลูกค้า = '" + tbCustomerId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ทีมรถ*/
                        //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                        //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                        setDataSouceForDGVCustomer();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();

                    //set ค่าให้ช่อง combobox ลูกค้า
                    setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerName);

                    //clear ช่อง
                    clearThreeTextbox(tbCustomerId, tbCustomerName, tbCustomerAddress);
                }
            }
        }

        private void deleteSiteBeforeCustomer() {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM base_site WHERE base_customer_id = '" + tbCustomerId.Text + "' ; ";
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

        private void TableCustomer_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer");
        }

        /* start clearThreeTextbox */
        private void clearThreeTextbox(TextBox tbId, TextBox tbFirst, TextBox tbSecond)
        {
            tbId.Text = "";
            tbId.ReadOnly = false;
            tbFirst.Text = "";
            tbSecond.Text = "";
        }
        /* end clearThreeTextbox */

        /* start clearThreeTextbox */
        private void clearTwoTextbox(TextBox tbId, TextBox tbFirst)
        {
            tbId.Text = "";
            tbId.ReadOnly = false;
            tbFirst.Text = "";
        }
        /* end clearThreeTextbox */

    }
}
