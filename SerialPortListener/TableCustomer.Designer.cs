namespace SerialPortListener
{
    partial class TableCustomer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dgvCustomer = new System.Windows.Forms.DataGridView();
            this.รหัสลูกค้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ชื่อลูกค้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ที่อยู่ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ส่งที่ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.basecustomerBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.dataSet1 = new SerialPortListener.DataSet1();
            this.basecustomerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.customerDataSet = new SerialPortListener.customerDataSet();
            this.base_customerTableAdapter = new SerialPortListener.customerDataSetTableAdapters.base_customerTableAdapter();
            this.tcSettingCustomer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tbJobId = new System.Windows.Forms.TextBox();
            this.tbCustomerAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbbVatType = new System.Windows.Forms.ComboBox();
            this.btClearCustomer = new System.Windows.Forms.Button();
            this.tbCustomerName = new System.Windows.Forms.TextBox();
            this.tbCustomerId = new System.Windows.Forms.TextBox();
            this.cbbJobType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btDelCustomer = new System.Windows.Forms.Button();
            this.btSaveCustomer = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btClearSite = new System.Windows.Forms.Button();
            this.tbCustomerSiteId = new System.Windows.Forms.TextBox();
            this.tbSiteName = new System.Windows.Forms.TextBox();
            this.tbSiteId = new System.Windows.Forms.TextBox();
            this.cbbCustomerSiteName = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.dgvSite = new System.Windows.Forms.DataGridView();
            this.base_site_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.base_site_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.base_customer_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.basesiteBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.dataSet2 = new SerialPortListener.DataSet2();
            this.btDelSite = new System.Windows.Forms.Button();
            this.btSaveSite = new System.Windows.Forms.Button();
            this.basesiteBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.baseSiteDataSet = new SerialPortListener.truckDataSet2();
            this.base_siteTableAdapter = new SerialPortListener.truckDataSet2TableAdapters.base_siteTableAdapter();
            this.base_customerTableAdapter1 = new SerialPortListener.DataSet1TableAdapters.base_customerTableAdapter();
            this.base_siteTableAdapter1 = new SerialPortListener.DataSet2TableAdapters.base_siteTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basecustomerBindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basecustomerBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.customerDataSet)).BeginInit();
            this.tcSettingCustomer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basesiteBindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basesiteBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.baseSiteDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCustomer
            // 
            this.dgvCustomer.AutoGenerateColumns = false;
            this.dgvCustomer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCustomer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.รหัสลูกค้า,
            this.ชื่อลูกค้า,
            this.ที่อยู่,
            this.ส่งที่});
            this.dgvCustomer.DataSource = this.basecustomerBindingSource1;
            this.dgvCustomer.Location = new System.Drawing.Point(9, 103);
            this.dgvCustomer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvCustomer.Name = "dgvCustomer";
            this.dgvCustomer.ReadOnly = true;
            this.dgvCustomer.ShowRowErrors = false;
            this.dgvCustomer.Size = new System.Drawing.Size(774, 386);
            this.dgvCustomer.TabIndex = 2;
            this.dgvCustomer.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCustomer_CellClick);
            this.dgvCustomer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvCustomer_KeyDown);
            // 
            // รหัสลูกค้า
            // 
            this.รหัสลูกค้า.DataPropertyName = "รหัสลูกค้า";
            this.รหัสลูกค้า.HeaderText = "รหัสลูกค้า";
            this.รหัสลูกค้า.Name = "รหัสลูกค้า";
            this.รหัสลูกค้า.ReadOnly = true;
            this.รหัสลูกค้า.Width = 150;
            // 
            // ชื่อลูกค้า
            // 
            this.ชื่อลูกค้า.DataPropertyName = "ชื่อลูกค้า";
            this.ชื่อลูกค้า.HeaderText = "ชื่อลูกค้า";
            this.ชื่อลูกค้า.Name = "ชื่อลูกค้า";
            this.ชื่อลูกค้า.ReadOnly = true;
            this.ชื่อลูกค้า.Width = 300;
            // 
            // ที่อยู่
            // 
            this.ที่อยู่.DataPropertyName = "ที่อยู่";
            this.ที่อยู่.HeaderText = "ที่อยู่";
            this.ที่อยู่.Name = "ที่อยู่";
            this.ที่อยู่.ReadOnly = true;
            this.ที่อยู่.Width = 260;
            // 
            // ส่งที่
            // 
            this.ส่งที่.DataPropertyName = "ส่งที่";
            this.ส่งที่.HeaderText = "ส่งที่";
            this.ส่งที่.Name = "ส่งที่";
            this.ส่งที่.ReadOnly = true;
            this.ส่งที่.Visible = false;
            this.ส่งที่.Width = 150;
            // 
            // basecustomerBindingSource1
            // 
            this.basecustomerBindingSource1.DataMember = "base_customer";
            this.basecustomerBindingSource1.DataSource = this.dataSet1;
            // 
            // dataSet1
            // 
            this.dataSet1.DataSetName = "DataSet1";
            this.dataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // basecustomerBindingSource
            // 
            this.basecustomerBindingSource.DataMember = "base_customer";
            this.basecustomerBindingSource.DataSource = this.customerDataSet;
            // 
            // customerDataSet
            // 
            this.customerDataSet.DataSetName = "customerDataSet";
            this.customerDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // base_customerTableAdapter
            // 
            this.base_customerTableAdapter.ClearBeforeFill = true;
            // 
            // tcSettingCustomer
            // 
            this.tcSettingCustomer.Controls.Add(this.tabPage1);
            this.tcSettingCustomer.Controls.Add(this.tabPage2);
            this.tcSettingCustomer.Location = new System.Drawing.Point(-1, 1);
            this.tcSettingCustomer.Name = "tcSettingCustomer";
            this.tcSettingCustomer.SelectedIndex = 0;
            this.tcSettingCustomer.Size = new System.Drawing.Size(798, 533);
            this.tcSettingCustomer.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Thistle;
            this.tabPage1.Controls.Add(this.tbJobId);
            this.tabPage1.Controls.Add(this.tbCustomerAddress);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.cbbVatType);
            this.tabPage1.Controls.Add(this.btClearCustomer);
            this.tabPage1.Controls.Add(this.tbCustomerName);
            this.tabPage1.Controls.Add(this.tbCustomerId);
            this.tabPage1.Controls.Add(this.cbbJobType);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.btDelCustomer);
            this.tabPage1.Controls.Add(this.btSaveCustomer);
            this.tabPage1.Controls.Add(this.dgvCustomer);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(790, 500);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ลูกค้า";
            // 
            // tbJobId
            // 
            this.tbJobId.Enabled = false;
            this.tbJobId.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbJobId.Location = new System.Drawing.Point(292, 18);
            this.tbJobId.Name = "tbJobId";
            this.tbJobId.Size = new System.Drawing.Size(31, 26);
            this.tbJobId.TabIndex = 60;
            // 
            // tbCustomerAddress
            // 
            this.tbCustomerAddress.Location = new System.Drawing.Point(528, 63);
            this.tbCustomerAddress.Name = "tbCustomerAddress";
            this.tbCustomerAddress.Size = new System.Drawing.Size(244, 26);
            this.tbCustomerAddress.TabIndex = 59;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(488, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 20);
            this.label1.TabIndex = 58;
            this.label1.Text = "ที่อยู่";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 20);
            this.label5.TabIndex = 57;
            this.label5.Text = "ชนิด";
            // 
            // cbbVatType
            // 
            this.cbbVatType.FormattingEnabled = true;
            this.cbbVatType.Location = new System.Drawing.Point(103, 18);
            this.cbbVatType.Name = "cbbVatType";
            this.cbbVatType.Size = new System.Drawing.Size(99, 28);
            this.cbbVatType.TabIndex = 56;
            this.cbbVatType.SelectedIndexChanged += new System.EventHandler(this.cbbVatType_SelectedIndexChanged);
            // 
            // btClearCustomer
            // 
            this.btClearCustomer.BackColor = System.Drawing.Color.DimGray;
            this.btClearCustomer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btClearCustomer.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClearCustomer.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btClearCustomer.Image = global::SerialPortListener.Properties.Resources.erase_24px;
            this.btClearCustomer.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btClearCustomer.Location = new System.Drawing.Point(529, 21);
            this.btClearCustomer.Name = "btClearCustomer";
            this.btClearCustomer.Size = new System.Drawing.Size(60, 30);
            this.btClearCustomer.TabIndex = 55;
            this.btClearCustomer.Text = "ล้าง";
            this.btClearCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btClearCustomer.UseVisualStyleBackColor = false;
            this.btClearCustomer.Click += new System.EventHandler(this.btClearCustomer_Click);
            // 
            // tbCustomerName
            // 
            this.tbCustomerName.Location = new System.Drawing.Point(292, 63);
            this.tbCustomerName.Name = "tbCustomerName";
            this.tbCustomerName.Size = new System.Drawing.Size(187, 26);
            this.tbCustomerName.TabIndex = 52;
            // 
            // tbCustomerId
            // 
            this.tbCustomerId.Location = new System.Drawing.Point(103, 60);
            this.tbCustomerId.Name = "tbCustomerId";
            this.tbCustomerId.Size = new System.Drawing.Size(99, 26);
            this.tbCustomerId.TabIndex = 51;
            // 
            // cbbJobType
            // 
            this.cbbJobType.FormattingEnabled = true;
            this.cbbJobType.Location = new System.Drawing.Point(329, 18);
            this.cbbJobType.Name = "cbbJobType";
            this.cbbJobType.Size = new System.Drawing.Size(150, 28);
            this.cbbJobType.TabIndex = 50;
            this.cbbJobType.SelectedIndexChanged += new System.EventHandler(this.cbbJobType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(213, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 20);
            this.label2.TabIndex = 49;
            this.label2.Text = "ชื่อลูกค้า";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 20);
            this.label3.TabIndex = 48;
            this.label3.Text = "รหัสลูกค้า";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 20);
            this.label4.TabIndex = 47;
            this.label4.Text = "ประเภทงาน";
            // 
            // btDelCustomer
            // 
            this.btDelCustomer.BackColor = System.Drawing.Color.IndianRed;
            this.btDelCustomer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btDelCustomer.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btDelCustomer.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btDelCustomer.Image = global::SerialPortListener.Properties.Resources.delete_bin_24px;
            this.btDelCustomer.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btDelCustomer.Location = new System.Drawing.Point(712, 21);
            this.btDelCustomer.Name = "btDelCustomer";
            this.btDelCustomer.Size = new System.Drawing.Size(60, 30);
            this.btDelCustomer.TabIndex = 46;
            this.btDelCustomer.Text = "ลบ";
            this.btDelCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btDelCustomer.UseVisualStyleBackColor = false;
            this.btDelCustomer.Click += new System.EventHandler(this.btDelCustomer_Click);
            // 
            // btSaveCustomer
            // 
            this.btSaveCustomer.BackColor = System.Drawing.Color.DarkSlateBlue;
            this.btSaveCustomer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSaveCustomer.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSaveCustomer.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btSaveCustomer.Image = global::SerialPortListener.Properties.Resources.save_24px;
            this.btSaveCustomer.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSaveCustomer.Location = new System.Drawing.Point(610, 21);
            this.btSaveCustomer.Name = "btSaveCustomer";
            this.btSaveCustomer.Size = new System.Drawing.Size(75, 30);
            this.btSaveCustomer.TabIndex = 45;
            this.btSaveCustomer.Text = "บันทึก";
            this.btSaveCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSaveCustomer.UseVisualStyleBackColor = false;
            this.btSaveCustomer.Click += new System.EventHandler(this.btSaveCustomer_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tabPage2.Controls.Add(this.btClearSite);
            this.tabPage2.Controls.Add(this.tbCustomerSiteId);
            this.tabPage2.Controls.Add(this.tbSiteName);
            this.tabPage2.Controls.Add(this.tbSiteId);
            this.tabPage2.Controls.Add(this.cbbCustomerSiteName);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.dgvSite);
            this.tabPage2.Controls.Add(this.btDelSite);
            this.tabPage2.Controls.Add(this.btSaveSite);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(790, 500);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "หน้างานตามลูกค้า";
            // 
            // btClearSite
            // 
            this.btClearSite.BackColor = System.Drawing.Color.DimGray;
            this.btClearSite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btClearSite.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClearSite.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btClearSite.Image = global::SerialPortListener.Properties.Resources.erase_24px;
            this.btClearSite.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btClearSite.Location = new System.Drawing.Point(583, 61);
            this.btClearSite.Name = "btClearSite";
            this.btClearSite.Size = new System.Drawing.Size(60, 30);
            this.btClearSite.TabIndex = 44;
            this.btClearSite.Text = "ล้าง";
            this.btClearSite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btClearSite.UseVisualStyleBackColor = false;
            this.btClearSite.Click += new System.EventHandler(this.btClearSite_Click);
            // 
            // tbCustomerSiteId
            // 
            this.tbCustomerSiteId.Location = new System.Drawing.Point(583, 19);
            this.tbCustomerSiteId.Name = "tbCustomerSiteId";
            this.tbCustomerSiteId.ReadOnly = true;
            this.tbCustomerSiteId.Size = new System.Drawing.Size(200, 26);
            this.tbCustomerSiteId.TabIndex = 43;
            this.tbCustomerSiteId.Visible = false;
            // 
            // tbSiteName
            // 
            this.tbSiteName.Location = new System.Drawing.Point(381, 64);
            this.tbSiteName.Name = "tbSiteName";
            this.tbSiteName.Size = new System.Drawing.Size(187, 26);
            this.tbSiteName.TabIndex = 41;
            // 
            // tbSiteId
            // 
            this.tbSiteId.Location = new System.Drawing.Point(107, 64);
            this.tbSiteId.Name = "tbSiteId";
            this.tbSiteId.Size = new System.Drawing.Size(174, 26);
            this.tbSiteId.TabIndex = 40;
            // 
            // cbbCustomerSiteName
            // 
            this.cbbCustomerSiteName.DropDownHeight = 300;
            this.cbbCustomerSiteName.FormattingEnabled = true;
            this.cbbCustomerSiteName.IntegralHeight = false;
            this.cbbCustomerSiteName.Location = new System.Drawing.Point(107, 17);
            this.cbbCustomerSiteName.Name = "cbbCustomerSiteName";
            this.cbbCustomerSiteName.Size = new System.Drawing.Size(461, 28);
            this.cbbCustomerSiteName.TabIndex = 39;
            this.cbbCustomerSiteName.TextUpdate += new System.EventHandler(this.cbbCustomerSiteName_TextUpdate);
            this.cbbCustomerSiteName.Leave += new System.EventHandler(this.cbbCustomerSiteName_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(302, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 20);
            this.label8.TabIndex = 38;
            this.label8.Text = "ชื่อหน้างาน";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 20);
            this.label9.TabIndex = 37;
            this.label9.Text = "รหัสหน้างาน";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(23, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 20);
            this.label10.TabIndex = 36;
            this.label10.Text = "ชื่อลูกค้า";
            // 
            // dgvSite
            // 
            this.dgvSite.AutoGenerateColumns = false;
            this.dgvSite.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSite.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.base_site_id,
            this.base_site_name,
            this.base_customer_id});
            this.dgvSite.DataSource = this.basesiteBindingSource1;
            this.dgvSite.Location = new System.Drawing.Point(9, 115);
            this.dgvSite.Name = "dgvSite";
            this.dgvSite.ReadOnly = true;
            this.dgvSite.Size = new System.Drawing.Size(774, 379);
            this.dgvSite.TabIndex = 35;
            this.dgvSite.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSite_CellClick);
            // 
            // base_site_id
            // 
            this.base_site_id.DataPropertyName = "base_site_id";
            this.base_site_id.HeaderText = "รหัสหน้างาน";
            this.base_site_id.Name = "base_site_id";
            this.base_site_id.ReadOnly = true;
            this.base_site_id.Width = 300;
            // 
            // base_site_name
            // 
            this.base_site_name.DataPropertyName = "base_site_name";
            this.base_site_name.HeaderText = "ชื่อหน้างาน";
            this.base_site_name.Name = "base_site_name";
            this.base_site_name.ReadOnly = true;
            this.base_site_name.Width = 600;
            // 
            // base_customer_id
            // 
            this.base_customer_id.DataPropertyName = "base_customer_id";
            this.base_customer_id.HeaderText = "base_customer_id";
            this.base_customer_id.Name = "base_customer_id";
            this.base_customer_id.ReadOnly = true;
            this.base_customer_id.Visible = false;
            // 
            // basesiteBindingSource1
            // 
            this.basesiteBindingSource1.DataMember = "base_site";
            this.basesiteBindingSource1.DataSource = this.dataSet2;
            // 
            // dataSet2
            // 
            this.dataSet2.DataSetName = "DataSet2";
            this.dataSet2.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // btDelSite
            // 
            this.btDelSite.BackColor = System.Drawing.Color.IndianRed;
            this.btDelSite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btDelSite.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btDelSite.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btDelSite.Image = global::SerialPortListener.Properties.Resources.delete_bin_24px;
            this.btDelSite.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btDelSite.Location = new System.Drawing.Point(730, 61);
            this.btDelSite.Name = "btDelSite";
            this.btDelSite.Size = new System.Drawing.Size(53, 30);
            this.btDelSite.TabIndex = 34;
            this.btDelSite.Text = "ลบ";
            this.btDelSite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btDelSite.UseVisualStyleBackColor = false;
            this.btDelSite.Click += new System.EventHandler(this.btDelSite_Click);
            // 
            // btSaveSite
            // 
            this.btSaveSite.BackColor = System.Drawing.Color.Teal;
            this.btSaveSite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSaveSite.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSaveSite.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btSaveSite.Image = global::SerialPortListener.Properties.Resources.save_24px;
            this.btSaveSite.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSaveSite.Location = new System.Drawing.Point(649, 61);
            this.btSaveSite.Name = "btSaveSite";
            this.btSaveSite.Size = new System.Drawing.Size(75, 30);
            this.btSaveSite.TabIndex = 33;
            this.btSaveSite.Text = "บันทึก";
            this.btSaveSite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSaveSite.UseVisualStyleBackColor = false;
            this.btSaveSite.Click += new System.EventHandler(this.btSaveSite_Click);
            // 
            // basesiteBindingSource
            // 
            this.basesiteBindingSource.DataMember = "base_site";
            this.basesiteBindingSource.DataSource = this.baseSiteDataSet;
            // 
            // baseSiteDataSet
            // 
            this.baseSiteDataSet.DataSetName = "baseSiteDataSet";
            this.baseSiteDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // base_siteTableAdapter
            // 
            this.base_siteTableAdapter.ClearBeforeFill = true;
            // 
            // base_customerTableAdapter1
            // 
            this.base_customerTableAdapter1.ClearBeforeFill = true;
            // 
            // base_siteTableAdapter1
            // 
            this.base_siteTableAdapter1.ClearBeforeFill = true;
            // 
            // TableCustomer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(798, 532);
            this.Controls.Add(this.tcSettingCustomer);
            this.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "TableCustomer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "แก้ไขลูกค้า";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TableCustomer_FormClosed);
            this.Load += new System.EventHandler(this.TableCustomer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basecustomerBindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basecustomerBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.customerDataSet)).EndInit();
            this.tcSettingCustomer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basesiteBindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basesiteBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.baseSiteDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvCustomer;
        private customerDataSet customerDataSet;
        private System.Windows.Forms.BindingSource basecustomerBindingSource;
        private customerDataSetTableAdapters.base_customerTableAdapter base_customerTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn รหัสลูกค้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn ชื่อลูกค้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn ที่อยู่;
        private System.Windows.Forms.DataGridViewTextBoxColumn ส่งที่;
        private System.Windows.Forms.TabControl tcSettingCustomer;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btClearSite;
        private System.Windows.Forms.TextBox tbCustomerSiteId;
        private System.Windows.Forms.TextBox tbSiteName;
        private System.Windows.Forms.TextBox tbSiteId;
        private System.Windows.Forms.ComboBox cbbCustomerSiteName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridView dgvSite;
        private System.Windows.Forms.Button btDelSite;
        private System.Windows.Forms.Button btSaveSite;
        private truckDataSet2 baseSiteDataSet;
        private System.Windows.Forms.BindingSource basesiteBindingSource;
        private truckDataSet2TableAdapters.base_siteTableAdapter base_siteTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn base_site_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn base_site_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn base_customer_id;
        private System.Windows.Forms.Button btClearCustomer;
        private System.Windows.Forms.TextBox tbCustomerName;
        private System.Windows.Forms.TextBox tbCustomerId;
        private System.Windows.Forms.ComboBox cbbJobType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btDelCustomer;
        private System.Windows.Forms.Button btSaveCustomer;
        private System.Windows.Forms.ComboBox cbbVatType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbCustomerAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbJobId;
        private DataSet1 dataSet1;
        private System.Windows.Forms.BindingSource basecustomerBindingSource1;
        private DataSet1TableAdapters.base_customerTableAdapter base_customerTableAdapter1;
        private DataSet2 dataSet2;
        private System.Windows.Forms.BindingSource basesiteBindingSource1;
        private DataSet2TableAdapters.base_siteTableAdapter base_siteTableAdapter1;
    }
}