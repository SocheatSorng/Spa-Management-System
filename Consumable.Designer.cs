namespace Spa_Management_System
{
    partial class Consumable
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvConsumable = new DataGridView();
            btnInsert = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnNew = new Button();
            btnClear = new Button();
            txtSearch = new TextBox();
            label1 = new Label();
            txtID = new TextBox();
            txtName = new TextBox();
            txtPrice = new TextBox();
            txtCategory = new TextBox();
            txtQuantity = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            txtDescription = new TextBox();
            label8 = new Label();
            txtCreatedAt = new TextBox();
            label9 = new Label();
            txtModifiedAt = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvConsumable).BeginInit();
            SuspendLayout();
            // 
            // dgvConsumable
            // 
            dgvConsumable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvConsumable.Location = new Point(12, 300);
            dgvConsumable.Name = "dgvConsumable";
            dgvConsumable.Size = new Size(460, 49);
            dgvConsumable.TabIndex = 0;
            // 
            // btnInsert
            // 
            btnInsert.Location = new Point(397, 71);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(75, 23);
            btnInsert.TabIndex = 1;
            btnInsert.Text = "Insert";
            btnInsert.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(397, 100);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(75, 23);
            btnUpdate.TabIndex = 2;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(397, 129);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(397, 158);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 23);
            btnNew.TabIndex = 4;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(397, 191);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 5;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(107, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(100, 23);
            txtSearch.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 16);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 7;
            label1.Text = "search";
            // 
            // txtID
            // 
            txtID.Location = new Point(107, 68);
            txtID.Name = "txtID";
            txtID.Size = new Size(100, 23);
            txtID.TabIndex = 8;
            // 
            // txtName
            // 
            txtName.Location = new Point(107, 126);
            txtName.Name = "txtName";
            txtName.Size = new Size(100, 23);
            txtName.TabIndex = 9;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(107, 155);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(100, 23);
            txtPrice.TabIndex = 10;
            // 
            // txtCategory
            // 
            txtCategory.Location = new Point(107, 97);
            txtCategory.Name = "txtCategory";
            txtCategory.Size = new Size(100, 23);
            txtCategory.TabIndex = 11;
            // 
            // txtQuantity
            // 
            txtQuantity.Location = new Point(107, 184);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(100, 23);
            txtQuantity.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 100);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 13;
            label2.Text = "Category";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 129);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 14;
            label3.Text = "Name";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 71);
            label4.Name = "label4";
            label4.Size = new Size(18, 15);
            label4.TabIndex = 15;
            label4.Text = "ID";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(14, 158);
            label5.Name = "label5";
            label5.Size = new Size(33, 15);
            label5.TabIndex = 16;
            label5.Text = "Price";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 187);
            label6.Name = "label6";
            label6.Size = new Size(53, 15);
            label6.TabIndex = 17;
            label6.Text = "Quantity";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 216);
            label7.Name = "label7";
            label7.Size = new Size(67, 15);
            label7.TabIndex = 39;
            label7.Text = "Description";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(107, 213);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(100, 23);
            txtDescription.TabIndex = 38;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(12, 245);
            label8.Name = "label8";
            label8.Size = new Size(48, 15);
            label8.TabIndex = 41;
            label8.Text = "Created";
            // 
            // txtCreatedAt
            // 
            txtCreatedAt.Location = new Point(107, 242);
            txtCreatedAt.Name = "txtCreatedAt";
            txtCreatedAt.Size = new Size(100, 23);
            txtCreatedAt.TabIndex = 40;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(12, 274);
            label9.Name = "label9";
            label9.Size = new Size(55, 15);
            label9.TabIndex = 43;
            label9.Text = "Modified";
            // 
            // txtModifiedAt
            // 
            txtModifiedAt.Location = new Point(107, 271);
            txtModifiedAt.Name = "txtModifiedAt";
            txtModifiedAt.Size = new Size(100, 23);
            txtModifiedAt.TabIndex = 42;
            // 
            // Consumable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 361);
            Controls.Add(label9);
            Controls.Add(txtModifiedAt);
            Controls.Add(label8);
            Controls.Add(txtCreatedAt);
            Controls.Add(label7);
            Controls.Add(txtDescription);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtQuantity);
            Controls.Add(txtCategory);
            Controls.Add(txtPrice);
            Controls.Add(txtName);
            Controls.Add(txtID);
            Controls.Add(label1);
            Controls.Add(txtSearch);
            Controls.Add(btnClear);
            Controls.Add(btnNew);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnInsert);
            Controls.Add(dgvConsumable);
            Name = "Consumable";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvConsumable).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvConsumable;
        private Button btnInsert;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnNew;
        private Button btnClear;
        private TextBox txtSearch;
        private Label label1;
        private TextBox txtID;
        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtCategory;
        private TextBox txtQuantity;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private TextBox txtDescription;
        private Label label8;
        private TextBox txtCreatedAt;
        private Label label9;
        private TextBox txtModifiedAt;
    }
}
