namespace Spa_Management_System
{
    partial class Order
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Order));
            Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges borderEdges1 = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties1 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties2 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties3 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties4 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            bunifuPanel2 = new Bunifu.UI.WinForms.BunifuPanel();
            label11 = new Label();
            btnExitProgram = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
            dgvPayment = new Bunifu.UI.WinForms.BunifuDataGridView();
            txtSearch = new Bunifu.UI.WinForms.BunifuTextBox();
            bunifuPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPayment).BeginInit();
            SuspendLayout();
            // 
            // bunifuPanel2
            // 
            bunifuPanel2.BackgroundColor = Color.White;
            bunifuPanel2.BackgroundImage = (Image)resources.GetObject("bunifuPanel2.BackgroundImage");
            bunifuPanel2.BackgroundImageLayout = ImageLayout.Stretch;
            bunifuPanel2.BorderColor = Color.Transparent;
            bunifuPanel2.BorderRadius = 3;
            bunifuPanel2.BorderThickness = 1;
            bunifuPanel2.Controls.Add(label11);
            bunifuPanel2.Controls.Add(btnExitProgram);
            bunifuPanel2.Dock = DockStyle.Top;
            bunifuPanel2.Location = new Point(0, 0);
            bunifuPanel2.Name = "bunifuPanel2";
            bunifuPanel2.ShowBorders = true;
            bunifuPanel2.Size = new Size(870, 65);
            bunifuPanel2.TabIndex = 89;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(355, 20);
            label11.Name = "label11";
            label11.Size = new Size(191, 24);
            label11.TabIndex = 74;
            label11.Text = "Order Management";
            // 
            // btnExitProgram
            // 
            btnExitProgram.AllowAnimations = true;
            btnExitProgram.AllowBorderColorChanges = true;
            btnExitProgram.AllowMouseEffects = true;
            btnExitProgram.AnimationSpeed = 200;
            btnExitProgram.BackColor = Color.Transparent;
            btnExitProgram.BackgroundColor = Color.Transparent;
            btnExitProgram.BorderColor = Color.Transparent;
            btnExitProgram.BorderRadius = 1;
            btnExitProgram.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderStyles.Solid;
            btnExitProgram.BorderThickness = 1;
            btnExitProgram.ColorContrastOnClick = 30;
            btnExitProgram.ColorContrastOnHover = -30;
            borderEdges1.BottomLeft = true;
            borderEdges1.BottomRight = true;
            borderEdges1.TopLeft = true;
            borderEdges1.TopRight = true;
            btnExitProgram.CustomizableEdges = borderEdges1;
            btnExitProgram.DialogResult = DialogResult.None;
            btnExitProgram.Image = (Image)resources.GetObject("btnExitProgram.Image");
            btnExitProgram.ImageMargin = new Padding(0);
            btnExitProgram.Location = new Point(832, 20);
            btnExitProgram.Name = "btnExitProgram";
            btnExitProgram.RoundBorders = true;
            btnExitProgram.ShowBorders = true;
            btnExitProgram.Size = new Size(26, 26);
            btnExitProgram.Style = Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.ButtonStyles.Round;
            btnExitProgram.TabIndex = 69;
            btnExitProgram.Click += btnExitProgram_Click;
            // 
            // dgvPayment
            // 
            dgvPayment.AllowCustomTheming = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(255, 232, 191);
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dgvPayment.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPayment.BackgroundColor = Color.White;
            dgvPayment.BorderStyle = BorderStyle.None;
            dgvPayment.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPayment.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.Orange;
            dataGridViewCellStyle2.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvPayment.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvPayment.ColumnHeadersHeight = 40;
            dgvPayment.CurrentTheme.AlternatingRowsStyle.BackColor = Color.FromArgb(255, 232, 191);
            dgvPayment.CurrentTheme.AlternatingRowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvPayment.CurrentTheme.AlternatingRowsStyle.ForeColor = Color.Black;
            dgvPayment.CurrentTheme.AlternatingRowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvPayment.CurrentTheme.AlternatingRowsStyle.SelectionForeColor = Color.Black;
            dgvPayment.CurrentTheme.BackColor = Color.Orange;
            dgvPayment.CurrentTheme.GridColor = Color.FromArgb(255, 226, 173);
            dgvPayment.CurrentTheme.HeaderStyle.BackColor = Color.Orange;
            dgvPayment.CurrentTheme.HeaderStyle.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dgvPayment.CurrentTheme.HeaderStyle.ForeColor = Color.White;
            dgvPayment.CurrentTheme.HeaderStyle.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dgvPayment.CurrentTheme.HeaderStyle.SelectionForeColor = Color.White;
            dgvPayment.CurrentTheme.Name = null;
            dgvPayment.CurrentTheme.RowsStyle.BackColor = Color.FromArgb(255, 237, 204);
            dgvPayment.CurrentTheme.RowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvPayment.CurrentTheme.RowsStyle.ForeColor = Color.Black;
            dgvPayment.CurrentTheme.RowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvPayment.CurrentTheme.RowsStyle.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(255, 237, 204);
            dataGridViewCellStyle3.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvPayment.DefaultCellStyle = dataGridViewCellStyle3;
            dgvPayment.EnableHeadersVisualStyles = false;
            dgvPayment.GridColor = Color.FromArgb(255, 226, 173);
            dgvPayment.HeaderBackColor = Color.Orange;
            dgvPayment.HeaderBgColor = Color.Empty;
            dgvPayment.HeaderForeColor = Color.White;
            dgvPayment.Location = new Point(12, 115);
            dgvPayment.Name = "dgvPayment";
            dgvPayment.RowHeadersVisible = false;
            dgvPayment.RowTemplate.Height = 40;
            dgvPayment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPayment.Size = new Size(846, 373);
            dgvPayment.TabIndex = 99;
            dgvPayment.Theme = Bunifu.UI.WinForms.BunifuDataGridView.PresetThemes.Orange;
            // 
            // txtSearch
            // 
            txtSearch.AcceptsReturn = false;
            txtSearch.AcceptsTab = false;
            txtSearch.AnimationSpeed = 200;
            txtSearch.AutoCompleteMode = AutoCompleteMode.None;
            txtSearch.AutoCompleteSource = AutoCompleteSource.None;
            txtSearch.AutoSizeHeight = true;
            txtSearch.BackColor = Color.Transparent;
            txtSearch.BackgroundImage = (Image)resources.GetObject("txtSearch.BackgroundImage");
            txtSearch.BorderColorActive = Color.DarkGoldenrod;
            txtSearch.BorderColorDisabled = Color.FromArgb(204, 204, 204);
            txtSearch.BorderColorHover = Color.DarkGoldenrod;
            txtSearch.BorderColorIdle = Color.Silver;
            txtSearch.BorderRadius = 15;
            txtSearch.BorderThickness = 1;
            txtSearch.CharacterCase = Bunifu.UI.WinForms.BunifuTextBox.CharacterCases.Normal;
            txtSearch.CharacterCasing = CharacterCasing.Normal;
            txtSearch.DefaultFont = new Font("Segoe UI", 9F);
            txtSearch.DefaultText = "";
            txtSearch.FillColor = Color.White;
            txtSearch.HideSelection = true;
            txtSearch.IconLeft = null;
            txtSearch.IconLeftCursor = Cursors.IBeam;
            txtSearch.IconPadding = 10;
            txtSearch.IconRight = null;
            txtSearch.IconRightCursor = Cursors.IBeam;
            txtSearch.Location = new Point(12, 71);
            txtSearch.MaxLength = 32767;
            txtSearch.MinimumSize = new Size(1, 1);
            txtSearch.Modified = false;
            txtSearch.Multiline = false;
            txtSearch.Name = "txtSearch";
            stateProperties1.BorderColor = Color.DarkGoldenrod;
            stateProperties1.FillColor = Color.Empty;
            stateProperties1.ForeColor = Color.Empty;
            stateProperties1.PlaceholderForeColor = Color.Empty;
            txtSearch.OnActiveState = stateProperties1;
            stateProperties2.BorderColor = Color.FromArgb(204, 204, 204);
            stateProperties2.FillColor = Color.FromArgb(240, 240, 240);
            stateProperties2.ForeColor = Color.FromArgb(109, 109, 109);
            stateProperties2.PlaceholderForeColor = Color.DarkGray;
            txtSearch.OnDisabledState = stateProperties2;
            stateProperties3.BorderColor = Color.DarkGoldenrod;
            stateProperties3.FillColor = Color.Empty;
            stateProperties3.ForeColor = Color.Empty;
            stateProperties3.PlaceholderForeColor = Color.Empty;
            txtSearch.OnHoverState = stateProperties3;
            stateProperties4.BorderColor = Color.Silver;
            stateProperties4.FillColor = Color.White;
            stateProperties4.ForeColor = Color.Empty;
            stateProperties4.PlaceholderForeColor = Color.Empty;
            txtSearch.OnIdleState = stateProperties4;
            txtSearch.Padding = new Padding(3);
            txtSearch.PasswordChar = '\0';
            txtSearch.PlaceholderForeColor = Color.Silver;
            txtSearch.PlaceholderText = "Search ...";
            txtSearch.ReadOnly = false;
            txtSearch.ScrollBars = ScrollBars.None;
            txtSearch.SelectedText = "";
            txtSearch.SelectionLength = 0;
            txtSearch.SelectionStart = 0;
            txtSearch.ShortcutsEnabled = true;
            txtSearch.Size = new Size(846, 38);
            txtSearch.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtSearch.TabIndex = 115;
            txtSearch.TextAlign = HorizontalAlignment.Left;
            txtSearch.TextMarginBottom = 0;
            txtSearch.TextMarginLeft = 3;
            txtSearch.TextMarginTop = 1;
            txtSearch.TextPlaceholder = "Search ...";
            txtSearch.UseSystemPasswordChar = false;
            txtSearch.WordWrap = true;
            // 
            // Order
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 500);
            Controls.Add(txtSearch);
            Controls.Add(dgvPayment);
            Controls.Add(bunifuPanel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Order";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Order Management";
            bunifuPanel2.ResumeLayout(false);
            bunifuPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPayment).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Bunifu.UI.WinForms.BunifuPanel bunifuPanel2;
        private Label label11;
        private Bunifu.UI.WinForms.BunifuButton.BunifuIconButton btnExitProgram;
        private Bunifu.UI.WinForms.BunifuDataGridView dgvPayment;
        private Bunifu.UI.WinForms.BunifuTextBox txtSearch;
    }
}