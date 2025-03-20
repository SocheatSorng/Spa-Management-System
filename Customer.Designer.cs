namespace Spa_Management_System
{
    partial class Customer
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
            txtCustomerID = new TextBox();
            txtNote = new TextBox();
            txtIssuedTime = new TextBox();
            txtID = new TextBox();
            label1 = new Label();
            btnSearch = new TextBox();
            Clear = new Button();
            btnNew = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnInsert = new Button();
            dgvCustomer = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvCustomer).BeginInit();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 158);
            label5.Name = "label5";
            label5.Size = new Size(33, 15);
            label5.TabIndex = 52;
            label5.Text = "Note";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 71);
            label4.Name = "label4";
            label4.Size = new Size(18, 15);
            label4.TabIndex = 51;
            label4.Text = "ID";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 129);
            label3.Name = "label3";
            label3.Size = new Size(31, 15);
            label3.TabIndex = 50;
            label3.Text = "Date";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 100);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.TabIndex = 49;
            label2.Text = "Customer ID";
            // 
            // txtCustomerID
            // 
            txtCustomerID.Location = new Point(112, 97);
            txtCustomerID.Name = "txtCustomerID";
            txtCustomerID.Size = new Size(100, 23);
            txtCustomerID.TabIndex = 47;
            // 
            // txtNote
            // 
            txtNote.Location = new Point(112, 155);
            txtNote.Name = "txtNote";
            txtNote.Size = new Size(100, 23);
            txtNote.TabIndex = 46;
            // 
            // txtIssuedTime
            // 
            txtIssuedTime.Location = new Point(112, 126);
            txtIssuedTime.Name = "txtIssuedTime";
            txtIssuedTime.Size = new Size(100, 23);
            txtIssuedTime.TabIndex = 45;
            // 
            // txtID
            // 
            txtID.Location = new Point(112, 68);
            txtID.Name = "txtID";
            txtID.Size = new Size(100, 23);
            txtID.TabIndex = 44;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 16);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 43;
            label1.Text = "search";
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(112, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 23);
            btnSearch.TabIndex = 42;
            // 
            // Clear
            // 
            Clear.Location = new Point(392, 184);
            Clear.Name = "Clear";
            Clear.Size = new Size(75, 23);
            Clear.TabIndex = 41;
            Clear.Text = "Clear";
            Clear.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(392, 158);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 23);
            btnNew.TabIndex = 40;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(392, 129);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 39;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(392, 100);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(75, 23);
            btnUpdate.TabIndex = 38;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnInsert
            // 
            btnInsert.Location = new Point(392, 71);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(75, 23);
            btnInsert.TabIndex = 37;
            btnInsert.Text = "Insert";
            btnInsert.UseVisualStyleBackColor = true;
            // 
            // dgvCustomer
            // 
            dgvCustomer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCustomer.Location = new Point(17, 213);
            dgvCustomer.Name = "dgvCustomer";
            dgvCustomer.Size = new Size(450, 136);
            dgvCustomer.TabIndex = 36;
            // 
            // Customer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 361);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtCustomerID);
            Controls.Add(txtNote);
            Controls.Add(txtIssuedTime);
            Controls.Add(txtID);
            Controls.Add(label1);
            Controls.Add(btnSearch);
            Controls.Add(Clear);
            Controls.Add(btnNew);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnInsert);
            Controls.Add(dgvCustomer);
            Name = "Customer";
            Text = "Customer";
            ((System.ComponentModel.ISupportInitialize)dgvCustomer).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox txtCustomerID;
        private TextBox txtNote;
        private TextBox txtIssuedTime;
        private TextBox txtID;
        private Label label1;
        private TextBox btnSearch;
        private Button Clear;
        private Button btnNew;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnInsert;
        private DataGridView dgvCustomer;
    }
}