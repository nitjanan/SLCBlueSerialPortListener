
namespace SerialPortListener
{
    partial class TableDeliveryOrder
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.doidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.docNoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deliveryDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deliveryTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.carCompanyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.carCustomerDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customerCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customerNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customerAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.productCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.productNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qtyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saleNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.noteDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deliveryorderBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.doDataSet = new SerialPortListener.DoDataSet();
            this.dgvDO = new System.Windows.Forms.DataGridView();
            this.deliveryorderBindingSource2 = new System.Windows.Forms.BindingSource(this.components);
            this.newDODataSet = new SerialPortListener.newDODataSet();
            this.deliveryorderBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.btSelect = new System.Windows.Forms.Button();
            this.delivery_orderTableAdapter = new SerialPortListener.DoDataSetTableAdapters.delivery_orderTableAdapter();
            this.delivery_orderTableAdapter2 = new SerialPortListener.newDODataSetTableAdapters.delivery_orderTableAdapter();
            this.label3 = new System.Windows.Forms.Label();
            this.cbbSearchDO = new System.Windows.Forms.ComboBox();
            this.btSearchDO = new System.Windows.Forms.Button();
            this.doc_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.delivery_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_customer_rem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_company_rem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customer_code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customer_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.site_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.product_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qty2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qty_tot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unit_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.note2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_customer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_customer_tot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_company = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.car_company_tot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sale_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bws = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.do_id2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.product_code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.site_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.doDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newDODataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.doidDataGridViewTextBoxColumn,
            this.docNoDataGridViewTextBoxColumn,
            this.deliveryDateDataGridViewTextBoxColumn,
            this.deliveryTypeDataGridViewTextBoxColumn,
            this.carCompanyDataGridViewTextBoxColumn,
            this.carCustomerDataGridViewTextBoxColumn,
            this.customerCodeDataGridViewTextBoxColumn,
            this.customerNameDataGridViewTextBoxColumn,
            this.customerAddressDataGridViewTextBoxColumn,
            this.productCodeDataGridViewTextBoxColumn,
            this.productNameDataGridViewTextBoxColumn,
            this.qtyDataGridViewTextBoxColumn,
            this.unitNameDataGridViewTextBoxColumn,
            this.saleNameDataGridViewTextBoxColumn,
            this.noteDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.deliveryorderBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(475, 265);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(12, 12);
            this.dataGridView1.TabIndex = 0;
            // 
            // doidDataGridViewTextBoxColumn
            // 
            this.doidDataGridViewTextBoxColumn.DataPropertyName = "do_id";
            this.doidDataGridViewTextBoxColumn.HeaderText = "do_id";
            this.doidDataGridViewTextBoxColumn.Name = "doidDataGridViewTextBoxColumn";
            // 
            // docNoDataGridViewTextBoxColumn
            // 
            this.docNoDataGridViewTextBoxColumn.DataPropertyName = "docNo";
            this.docNoDataGridViewTextBoxColumn.HeaderText = "docNo";
            this.docNoDataGridViewTextBoxColumn.Name = "docNoDataGridViewTextBoxColumn";
            // 
            // deliveryDateDataGridViewTextBoxColumn
            // 
            this.deliveryDateDataGridViewTextBoxColumn.DataPropertyName = "deliveryDate";
            this.deliveryDateDataGridViewTextBoxColumn.HeaderText = "deliveryDate";
            this.deliveryDateDataGridViewTextBoxColumn.Name = "deliveryDateDataGridViewTextBoxColumn";
            // 
            // deliveryTypeDataGridViewTextBoxColumn
            // 
            this.deliveryTypeDataGridViewTextBoxColumn.DataPropertyName = "deliveryType";
            this.deliveryTypeDataGridViewTextBoxColumn.HeaderText = "deliveryType";
            this.deliveryTypeDataGridViewTextBoxColumn.Name = "deliveryTypeDataGridViewTextBoxColumn";
            // 
            // carCompanyDataGridViewTextBoxColumn
            // 
            this.carCompanyDataGridViewTextBoxColumn.DataPropertyName = "carCompany";
            this.carCompanyDataGridViewTextBoxColumn.HeaderText = "carCompany";
            this.carCompanyDataGridViewTextBoxColumn.Name = "carCompanyDataGridViewTextBoxColumn";
            // 
            // carCustomerDataGridViewTextBoxColumn
            // 
            this.carCustomerDataGridViewTextBoxColumn.DataPropertyName = "carCustomer";
            this.carCustomerDataGridViewTextBoxColumn.HeaderText = "carCustomer";
            this.carCustomerDataGridViewTextBoxColumn.Name = "carCustomerDataGridViewTextBoxColumn";
            // 
            // customerCodeDataGridViewTextBoxColumn
            // 
            this.customerCodeDataGridViewTextBoxColumn.DataPropertyName = "customerCode";
            this.customerCodeDataGridViewTextBoxColumn.HeaderText = "customerCode";
            this.customerCodeDataGridViewTextBoxColumn.Name = "customerCodeDataGridViewTextBoxColumn";
            // 
            // customerNameDataGridViewTextBoxColumn
            // 
            this.customerNameDataGridViewTextBoxColumn.DataPropertyName = "customerName";
            this.customerNameDataGridViewTextBoxColumn.HeaderText = "customerName";
            this.customerNameDataGridViewTextBoxColumn.Name = "customerNameDataGridViewTextBoxColumn";
            // 
            // customerAddressDataGridViewTextBoxColumn
            // 
            this.customerAddressDataGridViewTextBoxColumn.DataPropertyName = "customerAddress";
            this.customerAddressDataGridViewTextBoxColumn.HeaderText = "customerAddress";
            this.customerAddressDataGridViewTextBoxColumn.Name = "customerAddressDataGridViewTextBoxColumn";
            // 
            // productCodeDataGridViewTextBoxColumn
            // 
            this.productCodeDataGridViewTextBoxColumn.DataPropertyName = "productCode";
            this.productCodeDataGridViewTextBoxColumn.HeaderText = "productCode";
            this.productCodeDataGridViewTextBoxColumn.Name = "productCodeDataGridViewTextBoxColumn";
            // 
            // productNameDataGridViewTextBoxColumn
            // 
            this.productNameDataGridViewTextBoxColumn.DataPropertyName = "productName";
            this.productNameDataGridViewTextBoxColumn.HeaderText = "productName";
            this.productNameDataGridViewTextBoxColumn.Name = "productNameDataGridViewTextBoxColumn";
            // 
            // qtyDataGridViewTextBoxColumn
            // 
            this.qtyDataGridViewTextBoxColumn.DataPropertyName = "qty";
            this.qtyDataGridViewTextBoxColumn.HeaderText = "qty";
            this.qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            // 
            // unitNameDataGridViewTextBoxColumn
            // 
            this.unitNameDataGridViewTextBoxColumn.DataPropertyName = "unitName";
            this.unitNameDataGridViewTextBoxColumn.HeaderText = "unitName";
            this.unitNameDataGridViewTextBoxColumn.Name = "unitNameDataGridViewTextBoxColumn";
            // 
            // saleNameDataGridViewTextBoxColumn
            // 
            this.saleNameDataGridViewTextBoxColumn.DataPropertyName = "saleName";
            this.saleNameDataGridViewTextBoxColumn.HeaderText = "saleName";
            this.saleNameDataGridViewTextBoxColumn.Name = "saleNameDataGridViewTextBoxColumn";
            // 
            // noteDataGridViewTextBoxColumn
            // 
            this.noteDataGridViewTextBoxColumn.DataPropertyName = "note";
            this.noteDataGridViewTextBoxColumn.HeaderText = "note";
            this.noteDataGridViewTextBoxColumn.Name = "noteDataGridViewTextBoxColumn";
            // 
            // statusDataGridViewTextBoxColumn
            // 
            this.statusDataGridViewTextBoxColumn.DataPropertyName = "status";
            this.statusDataGridViewTextBoxColumn.HeaderText = "status";
            this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            // 
            // deliveryorderBindingSource
            // 
            this.deliveryorderBindingSource.DataMember = "delivery_order";
            this.deliveryorderBindingSource.DataSource = this.doDataSet;
            // 
            // doDataSet
            // 
            this.doDataSet.DataSetName = "DoDataSet";
            this.doDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // dgvDO
            // 
            this.dgvDO.AutoGenerateColumns = false;
            this.dgvDO.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDO.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.doc_no,
            this.delivery_date,
            this.car_customer_rem,
            this.car_company_rem,
            this.customer_code,
            this.customer_name,
            this.site_name,
            this.product_name,
            this.qty2,
            this.qty_tot,
            this.unit_name,
            this.note2,
            this.car_customer,
            this.car_customer_tot,
            this.car_company,
            this.car_company_tot,
            this.sale_name,
            this.status,
            this.bws,
            this.do_id2,
            this.product_code,
            this.site_id});
            this.dgvDO.DataSource = this.deliveryorderBindingSource2;
            this.dgvDO.Location = new System.Drawing.Point(13, 51);
            this.dgvDO.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvDO.Name = "dgvDO";
            this.dgvDO.ReadOnly = true;
            this.dgvDO.Size = new System.Drawing.Size(1177, 408);
            this.dgvDO.TabIndex = 1;
            this.dgvDO.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDO_CellDoubleClick);
            // 
            // deliveryorderBindingSource2
            // 
            this.deliveryorderBindingSource2.DataMember = "delivery_order";
            this.deliveryorderBindingSource2.DataSource = this.newDODataSet;
            // 
            // newDODataSet
            // 
            this.newDODataSet.DataSetName = "newDODataSet";
            this.newDODataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // deliveryorderBindingSource1
            // 
            this.deliveryorderBindingSource1.DataMember = "delivery_order";
            // 
            // btSelect
            // 
            this.btSelect.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btSelect.BackColor = System.Drawing.Color.DodgerBlue;
            this.btSelect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSelect.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSelect.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btSelect.Image = global::SerialPortListener.Properties.Resources.add_24px;
            this.btSelect.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSelect.Location = new System.Drawing.Point(57, 468);
            this.btSelect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btSelect.Name = "btSelect";
            this.btSelect.Size = new System.Drawing.Size(87, 32);
            this.btSelect.TabIndex = 7;
            this.btSelect.Text = "เลือก";
            this.btSelect.UseVisualStyleBackColor = false;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // delivery_orderTableAdapter
            // 
            this.delivery_orderTableAdapter.ClearBeforeFill = true;
            // 
            // delivery_orderTableAdapter2
            // 
            this.delivery_orderTableAdapter2.ClearBeforeFill = true;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(822, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 21);
            this.label3.TabIndex = 19;
            this.label3.Text = "ใบส่งของ:";
            // 
            // cbbSearchDO
            // 
            this.cbbSearchDO.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cbbSearchDO.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbbSearchDO.FormattingEnabled = true;
            this.cbbSearchDO.Items.AddRange(new object[] {
            "ทั้งหมด",
            "ยังไม่สำเร็จ",
            "สำเร็จแล้ว",
            "ยกเลิก"});
            this.cbbSearchDO.Location = new System.Drawing.Point(892, 13);
            this.cbbSearchDO.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbbSearchDO.Name = "cbbSearchDO";
            this.cbbSearchDO.Size = new System.Drawing.Size(151, 29);
            this.cbbSearchDO.TabIndex = 18;
            // 
            // btSearchDO
            // 
            this.btSearchDO.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btSearchDO.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSearchDO.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSearchDO.Image = global::SerialPortListener.Properties.Resources.search_32px;
            this.btSearchDO.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSearchDO.Location = new System.Drawing.Point(1059, 10);
            this.btSearchDO.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btSearchDO.Name = "btSearchDO";
            this.btSearchDO.Size = new System.Drawing.Size(87, 32);
            this.btSearchDO.TabIndex = 17;
            this.btSearchDO.Text = "ค้นหา";
            this.btSearchDO.UseVisualStyleBackColor = true;
            this.btSearchDO.Click += new System.EventHandler(this.btSearchDO_Click);
            // 
            // doc_no
            // 
            this.doc_no.DataPropertyName = "doc_no";
            this.doc_no.HeaderText = "เลขที่ใบส่งของ";
            this.doc_no.Name = "doc_no";
            this.doc_no.ReadOnly = true;
            this.doc_no.Width = 120;
            // 
            // delivery_date
            // 
            this.delivery_date.DataPropertyName = "delivery_date";
            this.delivery_date.HeaderText = "วันที่ส่ง";
            this.delivery_date.Name = "delivery_date";
            this.delivery_date.ReadOnly = true;
            // 
            // car_customer_rem
            // 
            this.car_customer_rem.DataPropertyName = "car_customer_rem";
            this.car_customer_rem.HeaderText = "รับเองเหลือ (เที่ยว)";
            this.car_customer_rem.Name = "car_customer_rem";
            this.car_customer_rem.ReadOnly = true;
            this.car_customer_rem.Width = 90;
            // 
            // car_company_rem
            // 
            this.car_company_rem.DataPropertyName = "car_company_rem";
            this.car_company_rem.HeaderText = "ส่งให้เหลือ (เที่ยว)";
            this.car_company_rem.Name = "car_company_rem";
            this.car_company_rem.ReadOnly = true;
            this.car_company_rem.Width = 90;
            // 
            // customer_code
            // 
            this.customer_code.DataPropertyName = "customer_code";
            this.customer_code.HeaderText = "รหัสลูกค้า";
            this.customer_code.Name = "customer_code";
            this.customer_code.ReadOnly = true;
            this.customer_code.Width = 90;
            // 
            // customer_name
            // 
            this.customer_name.DataPropertyName = "customer_name";
            this.customer_name.HeaderText = "ชื่อลูกค้า";
            this.customer_name.Name = "customer_name";
            this.customer_name.ReadOnly = true;
            this.customer_name.Width = 150;
            // 
            // site_name
            // 
            this.site_name.DataPropertyName = "site_name";
            this.site_name.HeaderText = "ชื่อหน้างาน";
            this.site_name.Name = "site_name";
            this.site_name.ReadOnly = true;
            this.site_name.Width = 130;
            // 
            // product_name
            // 
            this.product_name.DataPropertyName = "product_name";
            this.product_name.HeaderText = "ชื่อสินค้า";
            this.product_name.Name = "product_name";
            this.product_name.ReadOnly = true;
            this.product_name.Width = 120;
            // 
            // qty2
            // 
            this.qty2.DataPropertyName = "qty";
            this.qty2.HeaderText = "plan จำนวนหิน";
            this.qty2.Name = "qty2";
            this.qty2.ReadOnly = true;
            this.qty2.Width = 95;
            // 
            // qty_tot
            // 
            this.qty_tot.DataPropertyName = "qty_tot";
            this.qty_tot.HeaderText = "จำนวนหินชั่งแล้ว";
            this.qty_tot.Name = "qty_tot";
            this.qty_tot.ReadOnly = true;
            // 
            // unit_name
            // 
            this.unit_name.DataPropertyName = "unit_name";
            this.unit_name.HeaderText = "หน่วย";
            this.unit_name.Name = "unit_name";
            this.unit_name.ReadOnly = true;
            // 
            // note2
            // 
            this.note2.DataPropertyName = "note";
            this.note2.HeaderText = "หมายเหตุ";
            this.note2.Name = "note2";
            this.note2.ReadOnly = true;
            // 
            // car_customer
            // 
            this.car_customer.DataPropertyName = "car_customer";
            this.car_customer.HeaderText = "plan รับเอง(เที่ยว)";
            this.car_customer.Name = "car_customer";
            this.car_customer.ReadOnly = true;
            this.car_customer.Width = 85;
            // 
            // car_customer_tot
            // 
            this.car_customer_tot.DataPropertyName = "car_customer_tot";
            this.car_customer_tot.HeaderText = "ชั่งแล้ว รับเอง(เที่ยว)";
            this.car_customer_tot.Name = "car_customer_tot";
            this.car_customer_tot.ReadOnly = true;
            this.car_customer_tot.Width = 85;
            // 
            // car_company
            // 
            this.car_company.DataPropertyName = "car_company";
            this.car_company.HeaderText = "plan ส่งให้(เที่ยว)";
            this.car_company.Name = "car_company";
            this.car_company.ReadOnly = true;
            this.car_company.Width = 85;
            // 
            // car_company_tot
            // 
            this.car_company_tot.DataPropertyName = "car_company_tot";
            this.car_company_tot.HeaderText = "ชั่งแล้ว ส่งให้(เที่ยว)";
            this.car_company_tot.Name = "car_company_tot";
            this.car_company_tot.ReadOnly = true;
            this.car_company_tot.Width = 85;
            // 
            // sale_name
            // 
            this.sale_name.DataPropertyName = "sale_name";
            this.sale_name.HeaderText = "พนักงานขาย";
            this.sale_name.Name = "sale_name";
            this.sale_name.ReadOnly = true;
            // 
            // status
            // 
            this.status.DataPropertyName = "status";
            this.status.HeaderText = "status";
            this.status.Name = "status";
            this.status.ReadOnly = true;
            // 
            // bws
            // 
            this.bws.DataPropertyName = "bws";
            this.bws.HeaderText = "bws";
            this.bws.Name = "bws";
            this.bws.ReadOnly = true;
            // 
            // do_id2
            // 
            this.do_id2.DataPropertyName = "do_id";
            this.do_id2.HeaderText = "do_id";
            this.do_id2.Name = "do_id2";
            this.do_id2.ReadOnly = true;
            // 
            // product_code
            // 
            this.product_code.DataPropertyName = "product_code";
            this.product_code.HeaderText = "รหัสสินค้า";
            this.product_code.Name = "product_code";
            this.product_code.ReadOnly = true;
            // 
            // site_id
            // 
            this.site_id.DataPropertyName = "site_id";
            this.site_id.HeaderText = "รหัสหน้างาน";
            this.site_id.Name = "site_id";
            this.site_id.ReadOnly = true;
            this.site_id.Width = 90;
            // 
            // TableDeliveryOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Snow;
            this.ClientSize = new System.Drawing.Size(1203, 511);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbbSearchDO);
            this.Controls.Add(this.btSearchDO);
            this.Controls.Add(this.btSelect);
            this.Controls.Add(this.dgvDO);
            this.Controls.Add(this.dataGridView1);
            this.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "TableDeliveryOrder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ข้อมูลใบส่งของ";
            this.Load += new System.EventHandler(this.TableDeliveryOrder_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.doDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newDODataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deliveryorderBindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private DoDataSet doDataSet;
        private System.Windows.Forms.BindingSource deliveryorderBindingSource;
        private DoDataSetTableAdapters.delivery_orderTableAdapter delivery_orderTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn doidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn docNoDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn deliveryDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn deliveryTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn carCompanyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn carCustomerDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn customerCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn customerNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn customerAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn productCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn productNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn saleNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn noteDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridView dgvDO;
        private System.Windows.Forms.Button btSelect;
        private truckDataSet4 truckDataSet4;
        private System.Windows.Forms.BindingSource deliveryorderBindingSource1;
        private truckDataSet4TableAdapters.delivery_orderTableAdapter delivery_orderTableAdapter1;
        private System.Windows.Forms.DataGridViewTextBoxColumn qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn note;
        private System.Windows.Forms.DataGridViewTextBoxColumn do_id;
        private newDODataSet newDODataSet;
        private System.Windows.Forms.BindingSource deliveryorderBindingSource2;
        private newDODataSetTableAdapters.delivery_orderTableAdapter delivery_orderTableAdapter2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbbSearchDO;
        private System.Windows.Forms.Button btSearchDO;
        private System.Windows.Forms.DataGridViewTextBoxColumn doc_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn delivery_date;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_customer_rem;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_company_rem;
        private System.Windows.Forms.DataGridViewTextBoxColumn customer_code;
        private System.Windows.Forms.DataGridViewTextBoxColumn customer_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn site_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn product_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn qty2;
        private System.Windows.Forms.DataGridViewTextBoxColumn qty_tot;
        private System.Windows.Forms.DataGridViewTextBoxColumn unit_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn note2;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_customer;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_customer_tot;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_company;
        private System.Windows.Forms.DataGridViewTextBoxColumn car_company_tot;
        private System.Windows.Forms.DataGridViewTextBoxColumn sale_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn status;
        private System.Windows.Forms.DataGridViewTextBoxColumn bws;
        private System.Windows.Forms.DataGridViewTextBoxColumn do_id2;
        private System.Windows.Forms.DataGridViewTextBoxColumn product_code;
        private System.Windows.Forms.DataGridViewTextBoxColumn site_id;
    }
}