namespace Spa_Management_System
{
    partial class Invoice
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
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            txtOrderID = new TextBox();
            txtNotes = new TextBox();
            txtInvoiceDate = new TextBox();
            txtID = new TextBox();
            label1 = new Label();
            textBox1 = new TextBox();
            btnClear = new Button();
            btnNew = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnInsert = new Button();
            dgvInvoice = new DataGridView();
            label6 = new Label();
            txtTotalAmount = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvInvoice).BeginInit();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 187);
            label5.Name = "label5";
            label5.Size = new Size(38, 15);
            label5.TabIndex = 68;
            label5.Text = "Notes";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 71);
            label4.Name = "label4";
            label4.Size = new Size(18, 15);
            label4.TabIndex = 67;
            label4.Text = "ID";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 129);
            label3.Name = "label3";
            label3.Size = new Size(72, 15);
            label3.TabIndex = 66;
            label3.Text = "Invoice Date";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 100);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 65;
            label2.Text = "Order ID";
            label2.Click += label2_Click;
            // 
            // txtOrderID
            // 
            txtOrderID.Location = new Point(112, 97);
            txtOrderID.Name = "txtOrderID";
            txtOrderID.Size = new Size(100, 23);
            txtOrderID.TabIndex = 64;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(112, 184);
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(100, 23);
            txtNotes.TabIndex = 63;
            // 
            // txtInvoiceDate
            // 
            txtInvoiceDate.Location = new Point(112, 126);
            txtInvoiceDate.Name = "txtInvoiceDate";
            txtInvoiceDate.Size = new Size(100, 23);
            txtInvoiceDate.TabIndex = 62;
            // 
            // txtID
            // 
            txtID.Location = new Point(112, 68);
            txtID.Name = "txtID";
            txtID.Size = new Size(100, 23);
            txtID.TabIndex = 61;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 16);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 60;
            label1.Text = "search";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(112, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 59;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(392, 184);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 58;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(392, 158);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 23);
            btnNew.TabIndex = 57;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(392, 129);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 56;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(392, 100);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(75, 23);
            btnUpdate.TabIndex = 55;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnInsert
            // 
            btnInsert.Location = new Point(392, 71);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(75, 23);
            btnInsert.TabIndex = 54;
            btnInsert.Text = "Insert";
            btnInsert.UseVisualStyleBackColor = true;
            // 
            // dgvInvoice
            // 
            dgvInvoice.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvInvoice.Location = new Point(17, 213);
            dgvInvoice.Name = "dgvInvoice";
            dgvInvoice.Size = new Size(450, 136);
            dgvInvoice.TabIndex = 53;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(19, 158);
            label6.Name = "label6";
            label6.Size = new Size(79, 15);
            label6.TabIndex = 70;
            label6.Text = "Total Amount";
            // 
            // txtTotalAmount
            // 
            txtTotalAmount.Location = new Point(112, 155);
            txtTotalAmount.Name = "txtTotalAmount";
            txtTotalAmount.Size = new Size(100, 23);
            txtTotalAmount.TabIndex = 69;
            // 
            // Invoice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 361);
            Controls.Add(label6);
            Controls.Add(txtTotalAmount);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtOrderID);
            Controls.Add(txtNotes);
            Controls.Add(txtInvoiceDate);
            Controls.Add(txtID);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(btnClear);
            Controls.Add(btnNew);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnInsert);
            Controls.Add(dgvInvoice);
            Name = "Invoice";
            Text = "Invoice";
            ((System.ComponentModel.ISupportInitialize)dgvInvoice).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox txtOrderID;
        private TextBox txtNotes;
        private TextBox txtInvoiceDate;
        private TextBox txtID;
        private Label label1;
        private TextBox textBox1;
        private Button btnClear;
        private Button btnNew;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnInsert;
        private DataGridView dgvInvoice;
        private Label label6;
        private TextBox txtTotalAmount;
    }
}