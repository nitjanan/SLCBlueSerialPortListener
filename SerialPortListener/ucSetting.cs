using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;
using System.Diagnostics;

namespace SerialPortListener
{
    public partial class ucSetting : UserControl
    {
        private static ucSetting _instance;
        Datalayer dl = null;
        //scale
        OdbcDataAdapter adtScale;
        DataTable dtScale;
        OdbcCommandBuilder cmbScale;
        //scoop
        OdbcDataAdapter adtScoop;
        DataTable dtScoop;
        OdbcCommandBuilder cmbScoop;
        //stone type
        OdbcDataAdapter adtStoneType;
        DataTable dtStoneType;
        OdbcCommandBuilder cmbStoneType;
        //approve
        OdbcDataAdapter adtApprove;
        DataTable dtApprove;
        OdbcCommandBuilder cmbApprove;
        //customer
        OdbcDataAdapter adtCustomer;
        DataTable dtCustomer;
        OdbcCommandBuilder cmbCustomer;
        //car city
        OdbcDataAdapter adtCarCity;
        DataTable dtCarCity;
        OdbcCommandBuilder cmbCarCity;
        //users
        OdbcDataAdapter adtUsers;
        DataTable dtUsers;
        OdbcCommandBuilder cmbUsers;
        //car team
        OdbcDataAdapter adtCarTeam;
        DataTable dtCarTeam;
        OdbcCommandBuilder cmbCarTeam;
        //car
        OdbcDataAdapter adtCar;
        DataTable dtCar;
        OdbcCommandBuilder cmbCar;
        //mill
        OdbcDataAdapter adtMill;
        DataTable dtMill;
        OdbcCommandBuilder cmbMill;

        //site
        OdbcDataAdapter adtSite;
        DataTable dtSite;
        OdbcCommandBuilder cmbSite;

        //site
        OdbcDataAdapter adtJobType;
        DataTable dtJobType;
        OdbcCommandBuilder cmbJobType;

        // Bind default keywords
        List<string> listOriginalCustomerNameSetting = new List<string>();
        // save new keywords
        List<string> listNewCustomerNameSetting = new List<string>();

        private static ucSetting Instance
        {
            set 
            {
                if (_instance == null)
                    _instance = new ucSetting();
            }
            get
            {
                if (_instance == null)
                    _instance = new ucSetting();
                return _instance;
            }
        }
        public ucSetting()
        {
            dl = new Datalayer();
            InitializeComponent();
            //set ค่าให้ช่อง combobox ลูกค้า
            setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);
        }

        private void ucSetting_Load(object sender, EventArgs e)
        {
            //this.base_scaleTableAdapter.Fill(this.baseScaleDataSet.base_scale);
            setDataSouceForDGVScale();
            if (!Globals.isPermissionTop())
            {
                //แถบรหัสพนักงาน
                tcSetting.TabPages.Remove(tabPage1);
                //แถบuser
                tcSetting.TabPages.Remove(tabPage7);
            }
        }

        private void setautoComplete(ComboBox cb, string fieldId, string fieldName, string tableName, List<string> listOriginal)
        {
            listOriginal.Clear();
            cb.Items.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT DISTINCT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE base_job_type_id IS NOT NULL AND base_vat_type_id  IS NOT NULL ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() +" : " + reader[fieldName].ToString();
                    listOriginal.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();
            cb.Items.AddRange(listOriginal.ToArray());
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tcSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            resetDTGV();
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
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetDTGV() {
            if (tcSetting.SelectedTab == tabPage1)
            {
                //this.base_scaleTableAdapter.Fill(this.baseScaleDataSet.base_scale);
                setDataSouceForDGVScale();
            }
            else if (tcSetting.SelectedTab == tabPage2)
            {
                //this.base_stone_typeTableAdapter.Fill(this.baseStoneTypeDataSet.base_stone_type);
                setDataSouceForDGVStoneType();
            }
            else if (tcSetting.SelectedTab == tabPage3)
            {
                //this.base_scoopTableAdapter.Fill(this.baseScoopDataSet.base_scoop);
                setDataSouceForDGVScoop();
            }
            else if (tcSetting.SelectedTab == tabPage4)
            {
                //this.base_approveTableAdapter.Fill(this.baseApproveDataSet.base_approve);
                setDataSouceForDGVApprove();
            }
            else if (tcSetting.SelectedTab == tabPage5)
            {
                //this.base_customerTableAdapter.Fill(this.baseCustomerDataSet.base_customer);
                setDataSouceForDGVCustomer();
                // set cobbobox vat type
                fillComboAll(cbbVatType, "base_vat_type", "base_vat_type_name");
                // set cobbobox job type
                fillComboAll(cbbJobType, "base_job_type", "base_job_type_name");
            }
            else if (tcSetting.SelectedTab == tabPage6)
            {
                //this.base_car_cityTableAdapter .Fill(this.baseCarCityDataSet.base_car_city);
                setDataSouceForDGVCarCity();

            }
            else if (tcSetting.SelectedTab == tabPage7)
            {
                //this.usersTableAdapter.Fill(this.usersDataSet.users);
                setDataSouceForDGVUsers();
            }
            else if (tcSetting.SelectedTab == tabPage8)
            {
                //this.base_car_teamTableAdapter.Fill(this.baseCarTeamDataSet.base_car_team);
                setDataSouceForDGVCarTeam();
            }
            else if (tcSetting.SelectedTab == tabPage9)
            {
                fillCarTeamCombo();
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                setDataSouceForDGVCar();
            }
            else if (tcSetting.SelectedTab == tabPage10)
            {
                setDataSouceForDGVMill();
            }
            else if (tcSetting.SelectedTab == tabPage11)
            {
                
            }
            else if (tcSetting.SelectedTab == tabPage12)
            {
                setDataSouceForDGVJobType();
            }
        }

        private void deleteDTGVOld(DataGridView dgv)
        {
            try
            {
                int rowIndex = dgv.CurrentCell.RowIndex;
                dgv.Rows.RemoveAt(rowIndex);
            }
            catch (Exception ex)
            {
            }
        }


        private void deleteDTGV(DataGridView dgv, String cellName, String tableName)
        {
            int rowIndex = dgv.CurrentCell.RowIndex;
            string id = dgv.CurrentRow.Cells[cellName].Value.ToString();

            //sql delete
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM public."+ tableName + " where "+ cellName + " = '" + id + "' ";
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

            //reset ก่อน save ใหม่
            resetDTGV();
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

        private void fillCarTeamCombo()
        {
            //ล้างก่อน
            cbbCarTeamName.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_car_team";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["ชื่อทีม"].ToString();
                    cbbCarTeamName.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
            cbbCarTeamName.SelectedIndex = 0;
        }

        /*Base Scale*/
        private void setDataSouceForDGVScale()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_scale ");
                adtScale = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtScale = new DataTable();
                adtScale.Fill(dtScale);
                dgvScale.DataSource = dtScale;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveScale_Click(object sender, EventArgs e)
        {
            //saveActionScale();
            saveAndUpdateDTGV(cmbScale, adtScale, dtScale, dgvScale , "base_scale");
        }

        private void saveActionScale()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basescaleBindingSource.EndEdit();
                base_scaleTableAdapter.Update(this.baseScaleDataSet.base_scale);
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }


        private void dgvScale_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basescaleBindingSource.RemoveCurrent();
                    saveActionScale();
                }
            }
            */
        }

        private void btDelScale_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basescaleBindingSource.RemoveCurrent();
                //saveActionScale();
                deleteDTGV(dgvScale, "รหัสพนักงาน", "base_scale");
            }
        }

        /* Base Stone Type*/
        private void setDataSouceForDGVStoneType()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_stone_type ");
                adtStoneType = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtStoneType = new DataTable();
                adtStoneType.Fill(dtStoneType);
                dgvStoneType.DataSource = dtStoneType;
            }
            catch (Exception)
            {
            }
            dl.close();
        } 
        private void btSaveStoneType_Click(object sender, EventArgs e)
        {
            //saveActionStoneType();
            saveAndUpdateDTGV(cmbStoneType, adtStoneType, dtStoneType , dgvStoneType, "base_stone_type");
        }
        private void saveActionStoneType()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basestonetypeBindingSource.EndEdit();
                base_stone_typeTableAdapter.Update(this.baseStoneTypeDataSet.base_stone_type);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvStoneType_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basestonetypeBindingSource.RemoveCurrent();
                    saveActionStoneType();
                }
            }
            */
        }

        private void btDelStoneType_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basestonetypeBindingSource.RemoveCurrent();
                //saveActionStoneType();
                deleteDTGV(dgvStoneType, "รหัสหิน", "base_stone_type");
            }
        }

        /*Base Scoop*/
        private void setDataSouceForDGVScoop()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_scoop ");
                adtScoop = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtScoop = new DataTable();
                adtScoop.Fill(dtScoop);
                dgvScoop.DataSource = dtScoop;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveScoop_Click(object sender, EventArgs e)
        {
            //saveActionScoop();
            saveAndUpdateDTGV(cmbScoop, adtScoop, dtScoop, dgvScoop, "base_scoop");
        }
        private void saveActionScoop()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basescoopBindingSource.EndEdit();
                base_scoopTableAdapter.Update(this.baseScoopDataSet.base_scoop);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvScoop_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basescoopBindingSource.RemoveCurrent();
                    saveActionScoop();
                }
            }
            */
        }

        private void btDelScoop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basescoopBindingSource.RemoveCurrent();
                //saveActionScoop();
                deleteDTGV(dgvScoop, "รหัสผู้ตัก", "base_scoop");
            }
        }

        /*Base Approve*/
        private void setDataSouceForDGVApprove()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_approve ");
                adtApprove = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtApprove = new DataTable();
                adtApprove.Fill(dtApprove);
                dgvApprove.DataSource = dtApprove;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveApprove_Click(object sender, EventArgs e)
        {
            //saveActionApprove();
            saveAndUpdateDTGV(cmbApprove, adtApprove, dtApprove, dgvApprove, "base_approve");
        }        
        private void saveActionApprove()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                baseapproveBindingSource.EndEdit();
                base_approveTableAdapter.Update(this.baseApproveDataSet.base_approve);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvApprove_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    baseapproveBindingSource.RemoveCurrent();
                    saveActionApprove();
                }
            }
            */
        }

        private void btDelApprove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvApprove, "รหัสผู้อนุมัติจ่าย", "base_approve");
            }
        }

        /*Base Customer*/
        private void setDataSouceForDGVCustomer()
        {
            if (cbbJobType.SelectedIndex != -1 && cbbVatType.SelectedIndex != -1)
            {
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

        private void saveActionCustomer()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecustomerBindingSource.EndEdit();
                base_customerTableAdapter.Update(this.baseCustomerDataSet.base_customer);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
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
                setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);

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
            if (tbCustomerId.Text.Length > 8)
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องจากรหัสลูกค้ามีมากกว่า 8 หลัก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text.Contains(":") || tbCustomerName.Text.Contains(":"))
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องมีตัวอักษร : ในรหัสลูกค้าหรือชื่อลูกค้านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
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

        private void dgvCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basecustomerBindingSource.RemoveCurrent();
                    saveActionCustomer();
                }
            }
            */
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
                        /*กรองตาม ลูกค้า*/
                        setDataSouceForDGVCustomer();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();
                }

                //set ค่าให้ช่อง combobox ลูกค้า
                setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);

                //clear ช่อง
                clearThreeTextbox(tbCustomerId, tbCustomerName, tbCustomerAddress);

            }
        }

        private void deleteSiteBeforeCustomer()
        {
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

        /*Base Car City*/
        private void setDataSouceForDGVCarCity()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car_city ");
                adtCarCity = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCarCity = new DataTable();
                adtCarCity.Fill(dtCarCity);
                dgvCarCity.DataSource = dtCarCity;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCity_Click(object sender, EventArgs e)
        {
            //saveActionCarCity();
            saveAndUpdateDTGV(cmbCarCity, adtCarCity, dtCarCity, dgvCarCity, "base_car_city");
        }
        private void saveActionCarCity()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecarcityBindingSource.EndEdit();
                base_car_cityTableAdapter.Update(this.baseCarCityDataSet.base_car_city);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvCarCity_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basecarcityBindingSource.RemoveCurrent();
                    saveActionCarCity();
                }
            }
            */
        }

        private void btDelCity_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basecarcityBindingSource.RemoveCurrent();
                //saveActionCarCity();
                deleteDTGV(dgvCarCity, "รหัสจังหวัด", "base_car_city");
            }
        }

        /*Users*/
        private void setDataSouceForDGVUsers()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.users ");
                adtUsers = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtUsers = new DataTable();
                adtUsers.Fill(dtUsers);
                dgvUsers.DataSource = dtUsers;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void saveActionUsers()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                usersBindingSource.EndEdit();
                usersTableAdapter.Update(this.usersDataSet.users);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btSaveUsers_Click(object sender, EventArgs e)
        {
            //saveActionUsers();
            saveAndUpdateDTGV(cmbUsers, adtUsers, dtUsers, dgvUsers, "users");
        }

        private void dgvUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //usersBindingSource.RemoveCurrent();
                    //saveActionUsers();
                    deleteDTGV(dgvUsers, "users_id", "users");
                }
            }
        }

        private void tbText_Leave(object sender, EventArgs e)
        {
            tbEncryption.Text = Utils.hashPassword(tbText.Text);
        }

        /*Base Car Team*/
        private void setDataSouceForDGVCarTeam()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car_team ");
                adtCarTeam = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCarTeam = new DataTable();
                adtCarTeam.Fill(dtCarTeam);
                dgvTeamCar.DataSource = dtCarTeam;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCarTeam_Click(object sender, EventArgs e)
        {
            //saveActionCarTeam();
            saveAndUpdateDTGV(cmbCarTeam, adtCarTeam, dtCarTeam, dgvTeamCar, "base_car_team");
        }
        private void saveActionCarTeam()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecarteamBindingSource.EndEdit();
                base_car_teamTableAdapter.Update(this.baseCarTeamDataSet.base_car_team);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btDelCarTeam_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basecarteamBindingSource.RemoveCurrent();
                //saveActionCarTeam();

                deleteDTGV(dgvTeamCar, "รหัสทีม", "base_car_team");
            }
        }

        private void dgvCar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_car_team*/
            if(cbbCarTeamName.SelectedIndex != -1)
                setDataCarTeam();
        }

        private void setDataCarTeam()
        {
            if (dgvCar.Rows.Count > 1)
            {
                    tbCarId.Text = dgvCar.CurrentRow.Cells["รหัสรถร่วม"].Value.ToString();
                    tbCarName.Text = dgvCar.CurrentRow.Cells["ชื่อรถร่วม"].Value.ToString();

                    //เปิดปิดช่องเมื่อมีไม่มีค่า
                    if (tbCarId.Text != "")
                        tbCarId.ReadOnly = true;
                    else
                        tbCarId.ReadOnly = false;
            }
        }

        /*Base Car*/
        private void setDataSouceForDGVCar()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car where รหัสทีม = '" + tbCarTeamId.Text + "'");
                adtCar = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCar = new DataTable();
                adtCar.Fill(dtCar);
                dgvCar.DataSource = dtCar;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCar_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (tbCarTeamId.Text == "" || cbbCarTeamName.SelectedIndex == -1)
            {
                MessageBox.Show("กรุณาเลือกชื่อทีม", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT รหัสรถร่วม FROM public.base_car where รหัสรถร่วม = '" + tbCarId.Text + "' AND รหัสทีม  = '" + tbCarTeamId.Text + "'";
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
                if (isUpdate && tbCarId.ReadOnly)
                    updateBaseCarAction();
                else
                    saveBaseCarAction();

            }
        }

        private void updateBaseCarAction() {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_car SET ชื่อรถร่วม = '" + tbCarName.Text + "' WHERE รหัสรถร่วม = '" + tbCarId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseCarAction(){
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO base_car (รหัสรถร่วม, ชื่อรถร่วม, รหัสทีม)" +
                                     "VALUES ('" + tbCarId.Text + "','" + tbCarName.Text + "','" + tbCarTeamId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("รหัสรถร่วมนี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสรถร่วมใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();
        }

        private void btClearCar_Click(object sender, EventArgs e)
        {
            tbCarId.Text = "";
            tbCarId.ReadOnly = false;
            tbCarName.Text = "";
        }

        private void btDelCar_Click(object sender, EventArgs e)
        {
            if (tbCarId.Text == "") {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_car WHERE รหัสรถร่วม = '" + tbCarId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ทีมรถ*/
                        //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                        //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                        setDataSouceForDGVCar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();

                    //clear ช่อง
                    clearTwoTextbox(tbCarId, tbCarName);
                }
            }
        }

        /*Base Mill*/
        private void setDataSouceForDGVMill()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_mill ");
                adtMill = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtMill = new DataTable();
                adtMill.Fill(dtMill);
                dgvMill.DataSource = dtMill;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void cbbCarTeamName_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbCarId.Text = "";
            tbCarName.Text = "";

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car_team where ชื่อทีม = '" + cbbCarTeamName.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["รหัสทีม"].ToString();
                    tbCarTeamId.Text = rdStr;
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            /*กรองตาม ทีมรถ*/
            //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
            setDataSouceForDGVCar();

            /*set ค่าที่มาจาก Table base_car_team*/
            setDataCarTeam();
        }

        private void btSaveMill_Click(object sender, EventArgs e)
        {
            saveAndUpdateDTGV(cmbMill, adtMill, dtMill, dgvMill, "base_mill");
        }

        private void btDelMill_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvMill, "รหัสโรงโม่", "base_mill");
            }
        }

        private void cbbCustomerSiteName_TextUpdate(object sender, EventArgs e)
        {
             setSearchAnywhereToCombobox(cbbCustomerSiteName, listOriginalCustomerNameSetting, listNewCustomerNameSetting);
        }

        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew)
        {
            //clear combobox
            cb.Items.Clear();
            //clear listNew
            listNew.Clear();

            try {
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
            catch (Exception) { 
            
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

            }
            catch (Exception)
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
                MessageBox.Show("กรุณาใส่รหัสหน้างาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("รหัสหน้างานนี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสหน้างานใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();
        }

        /*Base Job Type*/
        private void setDataSouceForDGVJobType()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_job_type order by base_job_type_id ");
                adtJobType = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtJobType = new DataTable();
                adtJobType.Fill(dtJobType);
                dgvJobType.DataSource = dtJobType;
            }
            catch (Exception)
            {
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

        private void btSaveJobType_Click(object sender, EventArgs e)
        {
            saveAndUpdateDTGV(cmbJobType, adtJobType, dtJobType, dgvJobType, "base_job_type");
        }

        private void btDelJobType_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvJobType, "base_job_type_id", "base_job_type");
            }
        }

        private void fillComboAll(ComboBox cbb, String tableName, String fieldName)
        {
            //ล้างก่อน
            cbb.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + fieldName + " FROM public." + tableName + "";
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
            }else
            {
                tbCustomerId.Text = "";
                tbCustomerName.Text = "";
                tbCustomerAddress.Text = "";
                tbCustomerId.ReadOnly = false;
            }
        }

        private void getJobTypeIdFromName()
        {
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
