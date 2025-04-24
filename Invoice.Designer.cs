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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Invoice));
            Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges borderEdges1 = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties1 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties2 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties3 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties4 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            bunifuPanel2 = new Bunifu.UI.WinForms.BunifuPanel();
            label11 = new Label();
            btnExitProgram = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
            FormDock = new Bunifu.UI.WinForms.BunifuFormDock();
            txtSearch = new Bunifu.UI.WinForms.BunifuTextBox();
            dgvInvoice = new Bunifu.UI.WinForms.BunifuDataGridView();
            InvoiceId = new DataGridViewTextBoxColumn();
            TotalAmount = new DataGridViewTextBoxColumn();
            InvoiceDate = new DataGridViewTextBoxColumn();
            Notes = new DataGridViewTextBoxColumn();
            bunifuFormDrag1 = new Bunifu.UI.WinForms.BunifuFormDrag();
            bunifuPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvInvoice).BeginInit();
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
            bunifuPanel2.TabIndex = 90;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            label11.Location = new Point(355, 20);
            label11.Name = "label11";
            label11.Size = new Size(199, 25);
            label11.TabIndex = 74;
            label11.Text = "Invoice Management";
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
            // FormDock
            // 
            FormDock.AllowFormDragging = true;
            FormDock.AllowFormDropShadow = true;
            FormDock.AllowFormResizing = true;
            FormDock.AllowHidingBottomRegion = true;
            FormDock.AllowOpacityChangesWhileDragging = false;
            FormDock.BorderOptions.BottomBorder.BorderColor = Color.Silver;
            FormDock.BorderOptions.BottomBorder.BorderThickness = 1;
            FormDock.BorderOptions.BottomBorder.ShowBorder = true;
            FormDock.BorderOptions.LeftBorder.BorderColor = Color.Silver;
            FormDock.BorderOptions.LeftBorder.BorderThickness = 1;
            FormDock.BorderOptions.LeftBorder.ShowBorder = true;
            FormDock.BorderOptions.RightBorder.BorderColor = Color.Silver;
            FormDock.BorderOptions.RightBorder.BorderThickness = 1;
            FormDock.BorderOptions.RightBorder.ShowBorder = true;
            FormDock.BorderOptions.TopBorder.BorderColor = Color.Silver;
            FormDock.BorderOptions.TopBorder.BorderThickness = 1;
            FormDock.BorderOptions.TopBorder.ShowBorder = true;
            FormDock.ContainerControl = this;
            FormDock.DockingIndicatorsColor = Color.FromArgb(202, 215, 233);
            FormDock.DockingIndicatorsOpacity = 0.5D;
            FormDock.DockingOptions.DockAll = true;
            FormDock.DockingOptions.DockBottomLeft = true;
            FormDock.DockingOptions.DockBottomRight = true;
            FormDock.DockingOptions.DockFullScreen = true;
            FormDock.DockingOptions.DockLeft = true;
            FormDock.DockingOptions.DockRight = true;
            FormDock.DockingOptions.DockTopLeft = true;
            FormDock.DockingOptions.DockTopRight = true;
            FormDock.FormDraggingOpacity = 0.9D;
            FormDock.ParentForm = this;
            FormDock.ShowCursorChanges = true;
            FormDock.ShowDockingIndicators = true;
            FormDock.TitleBarOptions.AllowFormDragging = true;
            FormDock.TitleBarOptions.BunifuFormDock = FormDock;
            FormDock.TitleBarOptions.DoubleClickToExpandWindow = true;
            FormDock.TitleBarOptions.TitleBarControl = null;
            FormDock.TitleBarOptions.UseBackColorOnDockingIndicators = false;
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
            txtSearch.TabIndex = 113;
            txtSearch.TextAlign = HorizontalAlignment.Left;
            txtSearch.TextMarginBottom = 0;
            txtSearch.TextMarginLeft = 3;
            txtSearch.TextMarginTop = 1;
            txtSearch.TextPlaceholder = "Search ...";
            txtSearch.UseSystemPasswordChar = false;
            txtSearch.WordWrap = true;
            // 
            // dgvInvoice
            // 
            dgvInvoice.AllowCustomTheming = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(255, 232, 191);
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dgvInvoice.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvInvoice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInvoice.BackgroundColor = Color.White;
            dgvInvoice.BorderStyle = BorderStyle.None;
            dgvInvoice.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvInvoice.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.Orange;
            dataGridViewCellStyle2.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvInvoice.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvInvoice.ColumnHeadersHeight = 40;
            dgvInvoice.Columns.AddRange(new DataGridViewColumn[] { InvoiceId, TotalAmount, InvoiceDate, Notes });
            dgvInvoice.CurrentTheme.AlternatingRowsStyle.BackColor = Color.FromArgb(255, 232, 191);
            dgvInvoice.CurrentTheme.AlternatingRowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvInvoice.CurrentTheme.AlternatingRowsStyle.ForeColor = Color.Black;
            dgvInvoice.CurrentTheme.AlternatingRowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvInvoice.CurrentTheme.AlternatingRowsStyle.SelectionForeColor = Color.Black;
            dgvInvoice.CurrentTheme.BackColor = Color.Orange;
            dgvInvoice.CurrentTheme.GridColor = Color.FromArgb(255, 226, 173);
            dgvInvoice.CurrentTheme.HeaderStyle.BackColor = Color.Orange;
            dgvInvoice.CurrentTheme.HeaderStyle.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dgvInvoice.CurrentTheme.HeaderStyle.ForeColor = Color.White;
            dgvInvoice.CurrentTheme.HeaderStyle.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dgvInvoice.CurrentTheme.HeaderStyle.SelectionForeColor = Color.White;
            dgvInvoice.CurrentTheme.Name = null;
            dgvInvoice.CurrentTheme.RowsStyle.BackColor = Color.FromArgb(255, 237, 204);
            dgvInvoice.CurrentTheme.RowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvInvoice.CurrentTheme.RowsStyle.ForeColor = Color.Black;
            dgvInvoice.CurrentTheme.RowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvInvoice.CurrentTheme.RowsStyle.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(255, 237, 204);
            dataGridViewCellStyle3.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvInvoice.DefaultCellStyle = dataGridViewCellStyle3;
            dgvInvoice.EnableHeadersVisualStyles = false;
            dgvInvoice.GridColor = Color.FromArgb(255, 226, 173);
            dgvInvoice.HeaderBackColor = Color.Orange;
            dgvInvoice.HeaderBgColor = Color.Empty;
            dgvInvoice.HeaderForeColor = Color.White;
            dgvInvoice.Location = new Point(12, 115);
            dgvInvoice.Name = "dgvInvoice";
            dgvInvoice.RowHeadersVisible = false;
            dgvInvoice.RowTemplate.Height = 40;
            dgvInvoice.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoice.Size = new Size(846, 373);
            dgvInvoice.TabIndex = 119;
            dgvInvoice.Theme = Bunifu.UI.WinForms.BunifuDataGridView.PresetThemes.Orange;
            // 
            // InvoiceId
            // 
            InvoiceId.HeaderText = "ID";
            InvoiceId.Name = "InvoiceId";
            // 
            // TotalAmount
            // 
            TotalAmount.HeaderText = "Total Amount";
            TotalAmount.Name = "TotalAmount";
            // 
            // InvoiceDate
            // 
            InvoiceDate.HeaderText = "Date";
            InvoiceDate.Name = "InvoiceDate";
            // 
            // Notes
            // 
            Notes.HeaderText = "Notes";
            Notes.Name = "Notes";
            // 
            // bunifuFormDrag1
            // 
            bunifuFormDrag1.AllowOpacityChangesWhileDragging = false;
            bunifuFormDrag1.ContainerControl = this;
            bunifuFormDrag1.DockIndicatorsOpacity = 0.5D;
            bunifuFormDrag1.DockingIndicatorsColor = Color.FromArgb(202, 215, 233);
            bunifuFormDrag1.DockingOptions.DockAll = true;
            bunifuFormDrag1.DockingOptions.DockBottomLeft = true;
            bunifuFormDrag1.DockingOptions.DockBottomRight = true;
            bunifuFormDrag1.DockingOptions.DockFullScreen = true;
            bunifuFormDrag1.DockingOptions.DockLeft = true;
            bunifuFormDrag1.DockingOptions.DockRight = true;
            bunifuFormDrag1.DockingOptions.DockTopLeft = true;
            bunifuFormDrag1.DockingOptions.DockTopRight = true;
            bunifuFormDrag1.DragOpacity = 0.9D;
            bunifuFormDrag1.Enabled = true;
            bunifuFormDrag1.ParentForm = this;
            bunifuFormDrag1.ShowCursorChanges = true;
            bunifuFormDrag1.ShowDockingIndicators = true;
            bunifuFormDrag1.TitleBarOptions.BunifuFormDrag = bunifuFormDrag1;
            bunifuFormDrag1.TitleBarOptions.DoubleClickToExpandWindow = true;
            bunifuFormDrag1.TitleBarOptions.Enabled = true;
            bunifuFormDrag1.TitleBarOptions.TitleBarControl = null;
            bunifuFormDrag1.TitleBarOptions.UseBackColorOnDockingIndicators = false;
            // 
            // Invoice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 500);
            Controls.Add(dgvInvoice);
            Controls.Add(txtSearch);
            Controls.Add(bunifuPanel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Invoice";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Invoice Management";
            bunifuPanel2.ResumeLayout(false);
            bunifuPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvInvoice).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Bunifu.UI.WinForms.BunifuPanel bunifuPanel2;
        private Label label11;
        private Bunifu.UI.WinForms.BunifuButton.BunifuIconButton btnExitProgram;
        private Bunifu.UI.WinForms.BunifuFormDock FormDock;
        private Bunifu.UI.WinForms.BunifuTextBox txtSearch;
        private Bunifu.UI.WinForms.BunifuDataGridView dgvInvoice;
        private DataGridViewTextBoxColumn InvoiceId;
        private DataGridViewTextBoxColumn TotalAmount;
        private DataGridViewTextBoxColumn InvoiceDate;
        private DataGridViewTextBoxColumn Notes;
        private Bunifu.UI.WinForms.BunifuFormDrag bunifuFormDrag1;
    }
}