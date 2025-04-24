namespace Spa_Management_System
{
    partial class Consumable : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Consumable));
            Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges borderEdges1 = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.BorderEdges();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties1 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties2 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties3 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties4 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderEdges borderEdges3 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderEdges();
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderEdges borderEdges2 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderEdges();
            bunifuPanel2 = new Bunifu.UI.WinForms.BunifuPanel();
            label11 = new Label();
            btnExitProgram = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
            txtSearch = new Bunifu.UI.WinForms.BunifuTextBox();
            dgvConsumable = new Bunifu.UI.WinForms.BunifuDataGridView();
            ConsumableId = new DataGridViewTextBoxColumn();
            Name = new DataGridViewTextBoxColumn();
            Price = new DataGridViewTextBoxColumn();
            Category = new DataGridViewTextBoxColumn();
            StockQuantity = new DataGridViewTextBoxColumn();
            CreatedDate = new DataGridViewTextBoxColumn();
            ModifiedDate = new DataGridViewTextBoxColumn();
            picConsumable = new PictureBox();
            bunifuFormDock1 = new Bunifu.UI.WinForms.BunifuFormDock();
            btnSelectPicture = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            bunifuFormDrag1 = new Bunifu.UI.WinForms.BunifuFormDrag();
            btnDelete = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            bunifuPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConsumable).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picConsumable).BeginInit();
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
            bunifuPanel2.TabIndex = 91;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(355, 20);
            label11.Name = "label11";
            label11.Size = new Size(245, 25);
            label11.TabIndex = 74;
            label11.Text = "Consumable Management";
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
            btnExitProgram.Location = new Point(823, 18);
            btnExitProgram.Name = "btnExitProgram";
            btnExitProgram.RoundBorders = true;
            btnExitProgram.ShowBorders = true;
            btnExitProgram.Size = new Size(26, 26);
            btnExitProgram.Style = Bunifu.UI.WinForms.BunifuButton.BunifuIconButton.ButtonStyles.Round;
            btnExitProgram.TabIndex = 69;
            btnExitProgram.Click += btnExitProgram_Click;
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
            txtSearch.Size = new Size(661, 38);
            txtSearch.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtSearch.TabIndex = 112;
            txtSearch.TextAlign = HorizontalAlignment.Left;
            txtSearch.TextMarginBottom = 0;
            txtSearch.TextMarginLeft = 3;
            txtSearch.TextMarginTop = 1;
            txtSearch.TextPlaceholder = "Search ...";
            txtSearch.UseSystemPasswordChar = false;
            txtSearch.WordWrap = true;
            // 
            // dgvConsumable
            // 
            dgvConsumable.AllowCustomTheming = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(255, 232, 191);
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dgvConsumable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvConsumable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvConsumable.BackgroundColor = Color.White;
            dgvConsumable.BorderStyle = BorderStyle.None;
            dgvConsumable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvConsumable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.Orange;
            dataGridViewCellStyle2.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvConsumable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvConsumable.ColumnHeadersHeight = 40;
            dgvConsumable.Columns.AddRange(new DataGridViewColumn[] { ConsumableId, Name, Price, Category, StockQuantity, CreatedDate, ModifiedDate });
            dgvConsumable.CurrentTheme.AlternatingRowsStyle.BackColor = Color.FromArgb(255, 232, 191);
            dgvConsumable.CurrentTheme.AlternatingRowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvConsumable.CurrentTheme.AlternatingRowsStyle.ForeColor = Color.Black;
            dgvConsumable.CurrentTheme.AlternatingRowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvConsumable.CurrentTheme.AlternatingRowsStyle.SelectionForeColor = Color.Black;
            dgvConsumable.CurrentTheme.BackColor = Color.Orange;
            dgvConsumable.CurrentTheme.GridColor = Color.FromArgb(255, 226, 173);
            dgvConsumable.CurrentTheme.HeaderStyle.BackColor = Color.Orange;
            dgvConsumable.CurrentTheme.HeaderStyle.Font = new Font("Segoe UI Semibold", 11.75F, FontStyle.Bold);
            dgvConsumable.CurrentTheme.HeaderStyle.ForeColor = Color.White;
            dgvConsumable.CurrentTheme.HeaderStyle.SelectionBackColor = Color.FromArgb(204, 132, 0);
            dgvConsumable.CurrentTheme.HeaderStyle.SelectionForeColor = Color.White;
            dgvConsumable.CurrentTheme.Name = null;
            dgvConsumable.CurrentTheme.RowsStyle.BackColor = Color.FromArgb(255, 237, 204);
            dgvConsumable.CurrentTheme.RowsStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dgvConsumable.CurrentTheme.RowsStyle.ForeColor = Color.Black;
            dgvConsumable.CurrentTheme.RowsStyle.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dgvConsumable.CurrentTheme.RowsStyle.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(255, 237, 204);
            dataGridViewCellStyle3.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(255, 201, 102);
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvConsumable.DefaultCellStyle = dataGridViewCellStyle3;
            dgvConsumable.EnableHeadersVisualStyles = false;
            dgvConsumable.GridColor = Color.FromArgb(255, 226, 173);
            dgvConsumable.HeaderBackColor = Color.Orange;
            dgvConsumable.HeaderBgColor = Color.Empty;
            dgvConsumable.HeaderForeColor = Color.White;
            dgvConsumable.Location = new Point(12, 115);
            dgvConsumable.Name = "dgvConsumable";
            dgvConsumable.RowHeadersVisible = false;
            dgvConsumable.RowTemplate.Height = 40;
            dgvConsumable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConsumable.Size = new Size(665, 373);
            dgvConsumable.TabIndex = 118;
            dgvConsumable.Theme = Bunifu.UI.WinForms.BunifuDataGridView.PresetThemes.Orange;
            // 
            // ConsumableId
            // 
            ConsumableId.HeaderText = "ID";
            ConsumableId.Name = "ConsumableId";
            // 
            // Name
            // 
            Name.HeaderText = "Name";
            Name.Name = "Name";
            // 
            // Price
            // 
            Price.HeaderText = "Price";
            Price.Name = "Price";
            // 
            // Category
            // 
            Category.HeaderText = "Category";
            Category.Name = "Category";
            // 
            // StockQuantity
            // 
            StockQuantity.HeaderText = "Quantity";
            StockQuantity.Name = "StockQuantity";
            // 
            // CreatedDate
            // 
            CreatedDate.HeaderText = "Created";
            CreatedDate.Name = "CreatedDate";
            // 
            // ModifiedDate
            // 
            ModifiedDate.HeaderText = "Modified";
            ModifiedDate.Name = "ModifiedDate";
            // 
            // picConsumable
            // 
            picConsumable.BorderStyle = BorderStyle.FixedSingle;
            picConsumable.Location = new Point(699, 115);
            picConsumable.Name = "picConsumable";
            picConsumable.Size = new Size(150, 150);
            picConsumable.TabIndex = 136;
            picConsumable.TabStop = false;
            // 
            // bunifuFormDock1
            // 
            bunifuFormDock1.AllowFormDragging = true;
            bunifuFormDock1.AllowFormDropShadow = true;
            bunifuFormDock1.AllowFormResizing = true;
            bunifuFormDock1.AllowHidingBottomRegion = true;
            bunifuFormDock1.AllowOpacityChangesWhileDragging = true;
            bunifuFormDock1.BorderOptions.BottomBorder.BorderColor = Color.Silver;
            bunifuFormDock1.BorderOptions.BottomBorder.BorderThickness = 1;
            bunifuFormDock1.BorderOptions.BottomBorder.ShowBorder = true;
            bunifuFormDock1.BorderOptions.LeftBorder.BorderColor = Color.Silver;
            bunifuFormDock1.BorderOptions.LeftBorder.BorderThickness = 1;
            bunifuFormDock1.BorderOptions.LeftBorder.ShowBorder = true;
            bunifuFormDock1.BorderOptions.RightBorder.BorderColor = Color.Silver;
            bunifuFormDock1.BorderOptions.RightBorder.BorderThickness = 1;
            bunifuFormDock1.BorderOptions.RightBorder.ShowBorder = true;
            bunifuFormDock1.BorderOptions.TopBorder.BorderColor = Color.Silver;
            bunifuFormDock1.BorderOptions.TopBorder.BorderThickness = 1;
            bunifuFormDock1.BorderOptions.TopBorder.ShowBorder = true;
            bunifuFormDock1.ContainerControl = this;
            bunifuFormDock1.DockingIndicatorsColor = Color.FromArgb(202, 215, 233);
            bunifuFormDock1.DockingIndicatorsOpacity = 0.5D;
            bunifuFormDock1.DockingOptions.DockAll = true;
            bunifuFormDock1.DockingOptions.DockBottomLeft = true;
            bunifuFormDock1.DockingOptions.DockBottomRight = true;
            bunifuFormDock1.DockingOptions.DockFullScreen = true;
            bunifuFormDock1.DockingOptions.DockLeft = true;
            bunifuFormDock1.DockingOptions.DockRight = true;
            bunifuFormDock1.DockingOptions.DockTopLeft = true;
            bunifuFormDock1.DockingOptions.DockTopRight = true;
            bunifuFormDock1.FormDraggingOpacity = 0.9D;
            bunifuFormDock1.ParentForm = this;
            bunifuFormDock1.ShowCursorChanges = true;
            bunifuFormDock1.ShowDockingIndicators = true;
            bunifuFormDock1.TitleBarOptions.AllowFormDragging = true;
            bunifuFormDock1.TitleBarOptions.BunifuFormDock = bunifuFormDock1;
            bunifuFormDock1.TitleBarOptions.DoubleClickToExpandWindow = true;
            bunifuFormDock1.TitleBarOptions.TitleBarControl = null;
            bunifuFormDock1.TitleBarOptions.UseBackColorOnDockingIndicators = false;
            // 
            // btnSelectPicture
            // 
            btnSelectPicture.AllowAnimations = true;
            btnSelectPicture.AllowMouseEffects = true;
            btnSelectPicture.AllowToggling = false;
            btnSelectPicture.AnimationSpeed = 200;
            btnSelectPicture.AutoGenerateColors = false;
            btnSelectPicture.AutoRoundBorders = false;
            btnSelectPicture.AutoSizeLeftIcon = true;
            btnSelectPicture.AutoSizeRightIcon = true;
            btnSelectPicture.BackColor = Color.Transparent;
            btnSelectPicture.BackColor1 = Color.White;
            btnSelectPicture.BackgroundImage = (Image)resources.GetObject("btnSelectPicture.BackgroundImage");
            btnSelectPicture.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnSelectPicture.ButtonText = "Select Image";
            btnSelectPicture.ButtonTextMarginLeft = 0;
            btnSelectPicture.ColorContrastOnClick = 45;
            btnSelectPicture.ColorContrastOnHover = 45;
            btnSelectPicture.Cursor = Cursors.Hand;
            borderEdges3.BottomLeft = true;
            borderEdges3.BottomRight = true;
            borderEdges3.TopLeft = true;
            borderEdges3.TopRight = true;
            btnSelectPicture.CustomizableEdges = borderEdges3;
            btnSelectPicture.DialogResult = DialogResult.None;
            btnSelectPicture.DisabledBorderColor = Color.FromArgb(191, 191, 191);
            btnSelectPicture.DisabledFillColor = Color.FromArgb(204, 204, 204);
            btnSelectPicture.DisabledForecolor = Color.FromArgb(168, 160, 168);
            btnSelectPicture.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.ButtonStates.Pressed;
            btnSelectPicture.Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSelectPicture.ForeColor = Color.DarkGoldenrod;
            btnSelectPicture.IconLeftAlign = ContentAlignment.MiddleLeft;
            btnSelectPicture.IconLeftCursor = Cursors.Default;
            btnSelectPicture.IconLeftPadding = new Padding(15, 3, 3, 3);
            btnSelectPicture.IconMarginLeft = 11;
            btnSelectPicture.IconPadding = 10;
            btnSelectPicture.IconRightAlign = ContentAlignment.MiddleRight;
            btnSelectPicture.IconRightCursor = Cursors.Default;
            btnSelectPicture.IconRightPadding = new Padding(3, 3, 7, 3);
            btnSelectPicture.IconSize = 25;
            btnSelectPicture.IdleBorderColor = Color.DarkGoldenrod;
            btnSelectPicture.IdleBorderRadius = 15;
            btnSelectPicture.IdleBorderThickness = 1;
            btnSelectPicture.IdleFillColor = Color.White;
            btnSelectPicture.IdleIconLeftImage = Properties.Resources.image_file;
            btnSelectPicture.IdleIconRightImage = null;
            btnSelectPicture.IndicateFocus = false;
            btnSelectPicture.Location = new Point(699, 271);
            btnSelectPicture.Name = "btnSelectPicture";
            btnSelectPicture.OnDisabledState.BorderColor = Color.FromArgb(191, 191, 191);
            btnSelectPicture.OnDisabledState.BorderRadius = 15;
            btnSelectPicture.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnSelectPicture.OnDisabledState.BorderThickness = 1;
            btnSelectPicture.OnDisabledState.FillColor = Color.FromArgb(204, 204, 204);
            btnSelectPicture.OnDisabledState.ForeColor = Color.FromArgb(168, 160, 168);
            btnSelectPicture.OnDisabledState.IconLeftImage = null;
            btnSelectPicture.OnDisabledState.IconRightImage = null;
            btnSelectPicture.onHoverState.BorderColor = Color.DarkGoldenrod;
            btnSelectPicture.onHoverState.BorderRadius = 15;
            btnSelectPicture.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnSelectPicture.onHoverState.BorderThickness = 1;
            btnSelectPicture.onHoverState.FillColor = Color.DarkGoldenrod;
            btnSelectPicture.onHoverState.ForeColor = Color.White;
            btnSelectPicture.onHoverState.IconLeftImage = Properties.Resources.convenience;
            btnSelectPicture.onHoverState.IconRightImage = null;
            btnSelectPicture.OnIdleState.BorderColor = Color.DarkGoldenrod;
            btnSelectPicture.OnIdleState.BorderRadius = 15;
            btnSelectPicture.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnSelectPicture.OnIdleState.BorderThickness = 1;
            btnSelectPicture.OnIdleState.FillColor = Color.White;
            btnSelectPicture.OnIdleState.ForeColor = Color.DarkGoldenrod;
            btnSelectPicture.OnIdleState.IconLeftImage = Properties.Resources.image_file;
            btnSelectPicture.OnIdleState.IconRightImage = null;
            btnSelectPicture.OnPressedState.BorderColor = Color.Gray;
            btnSelectPicture.OnPressedState.BorderRadius = 15;
            btnSelectPicture.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnSelectPicture.OnPressedState.BorderThickness = 1;
            btnSelectPicture.OnPressedState.FillColor = Color.DarkGoldenrod;
            btnSelectPicture.OnPressedState.ForeColor = Color.White;
            btnSelectPicture.OnPressedState.IconLeftImage = null;
            btnSelectPicture.OnPressedState.IconRightImage = null;
            btnSelectPicture.Size = new Size(150, 39);
            btnSelectPicture.TabIndex = 137;
            btnSelectPicture.TextAlign = ContentAlignment.MiddleCenter;
            btnSelectPicture.TextAlignment = HorizontalAlignment.Center;
            btnSelectPicture.TextMarginLeft = 0;
            btnSelectPicture.TextPadding = new Padding(10, 0, 0, 0);
            btnSelectPicture.UseDefaultRadiusAndThickness = true;
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
            // btnDelete
            // 
            btnDelete.AllowAnimations = true;
            btnDelete.AllowMouseEffects = true;
            btnDelete.AllowToggling = false;
            btnDelete.AnimationSpeed = 200;
            btnDelete.AutoGenerateColors = false;
            btnDelete.AutoRoundBorders = false;
            btnDelete.AutoSizeLeftIcon = true;
            btnDelete.AutoSizeRightIcon = true;
            btnDelete.BackColor = Color.Transparent;
            btnDelete.BackColor1 = Color.Red;
            btnDelete.BackgroundImage = (Image)resources.GetObject("btnDelete.BackgroundImage");
            btnDelete.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnDelete.ButtonText = "Delete";
            btnDelete.ButtonTextMarginLeft = 0;
            btnDelete.ColorContrastOnClick = 45;
            btnDelete.ColorContrastOnHover = 45;
            borderEdges2.BottomLeft = true;
            borderEdges2.BottomRight = true;
            borderEdges2.TopLeft = true;
            borderEdges2.TopRight = true;
            btnDelete.CustomizableEdges = borderEdges2;
            btnDelete.DialogResult = DialogResult.None;
            btnDelete.DisabledBorderColor = Color.FromArgb(191, 191, 191);
            btnDelete.DisabledFillColor = Color.FromArgb(204, 204, 204);
            btnDelete.DisabledForecolor = Color.FromArgb(168, 160, 168);
            btnDelete.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.ButtonStates.Pressed;
            btnDelete.Font = new Font("Segoe UI", 9F);
            btnDelete.ForeColor = Color.White;
            btnDelete.IconLeftAlign = ContentAlignment.MiddleLeft;
            btnDelete.IconLeftCursor = Cursors.Default;
            btnDelete.IconLeftPadding = new Padding(11, 3, 3, 3);
            btnDelete.IconMarginLeft = 11;
            btnDelete.IconPadding = 10;
            btnDelete.IconRightAlign = ContentAlignment.MiddleRight;
            btnDelete.IconRightCursor = Cursors.Default;
            btnDelete.IconRightPadding = new Padding(3, 3, 7, 3);
            btnDelete.IconSize = 25;
            btnDelete.IdleBorderColor = Color.Red;
            btnDelete.IdleBorderRadius = 15;
            btnDelete.IdleBorderThickness = 1;
            btnDelete.IdleFillColor = Color.Red;
            btnDelete.IdleIconLeftImage = null;
            btnDelete.IdleIconRightImage = null;
            btnDelete.IndicateFocus = false;
            btnDelete.Location = new Point(699, 316);
            btnDelete.Name = "btnDelete";
            btnDelete.OnDisabledState.BorderColor = Color.FromArgb(191, 191, 191);
            btnDelete.OnDisabledState.BorderRadius = 15;
            btnDelete.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnDelete.OnDisabledState.BorderThickness = 1;
            btnDelete.OnDisabledState.FillColor = Color.FromArgb(204, 204, 204);
            btnDelete.OnDisabledState.ForeColor = Color.FromArgb(168, 160, 168);
            btnDelete.OnDisabledState.IconLeftImage = null;
            btnDelete.OnDisabledState.IconRightImage = null;
            btnDelete.onHoverState.BorderColor = Color.FromArgb(105, 181, 255);
            btnDelete.onHoverState.BorderRadius = 15;
            btnDelete.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnDelete.onHoverState.BorderThickness = 1;
            btnDelete.onHoverState.FillColor = Color.FromArgb(105, 181, 255);
            btnDelete.onHoverState.ForeColor = Color.White;
            btnDelete.onHoverState.IconLeftImage = null;
            btnDelete.onHoverState.IconRightImage = null;
            btnDelete.OnIdleState.BorderColor = Color.Red;
            btnDelete.OnIdleState.BorderRadius = 15;
            btnDelete.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnDelete.OnIdleState.BorderThickness = 1;
            btnDelete.OnIdleState.FillColor = Color.Red;
            btnDelete.OnIdleState.ForeColor = Color.White;
            btnDelete.OnIdleState.IconLeftImage = null;
            btnDelete.OnIdleState.IconRightImage = null;
            btnDelete.OnPressedState.BorderColor = Color.FromArgb(40, 96, 144);
            btnDelete.OnPressedState.BorderRadius = 15;
            btnDelete.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton2.BorderStyles.Solid;
            btnDelete.OnPressedState.BorderThickness = 1;
            btnDelete.OnPressedState.FillColor = Color.FromArgb(40, 96, 144);
            btnDelete.OnPressedState.ForeColor = Color.White;
            btnDelete.OnPressedState.IconLeftImage = null;
            btnDelete.OnPressedState.IconRightImage = null;
            btnDelete.Size = new Size(150, 39);
            btnDelete.TabIndex = 138;
            btnDelete.TextAlign = ContentAlignment.MiddleCenter;
            btnDelete.TextAlignment = HorizontalAlignment.Center;
            btnDelete.TextMarginLeft = 0;
            btnDelete.TextPadding = new Padding(0);
            btnDelete.UseDefaultRadiusAndThickness = true;
            // 
            // Consumable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 500);
            Controls.Add(btnDelete);
            Controls.Add(btnSelectPicture);
            Controls.Add(picConsumable);
            Controls.Add(dgvConsumable);
            Controls.Add(txtSearch);
            Controls.Add(bunifuPanel2);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Consumable Management";
            Load += Consumable_Load;
            bunifuPanel2.ResumeLayout(false);
            bunifuPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConsumable).EndInit();
            ((System.ComponentModel.ISupportInitialize)picConsumable).EndInit();
            ResumeLayout(false);
        }

        #endregion
        //private TextBox txtID;
        //private TextBox txtName;
        //private TextBox txtPrice;
        //private TextBox txtCategory;
        //private TextBox txtQuantity;
        //private TextBox txtDescription;
        //private TextBox txtCreatedAt;
        private Label label9;
        private Bunifu.UI.WinForms.BunifuPanel bunifuPanel2;
        private Label label11;
        private Bunifu.UI.WinForms.BunifuButton.BunifuIconButton btnExitProgram;
        private Bunifu.UI.WinForms.BunifuTextBox txtSearch;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnClear;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnUpdate;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnInsert;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnNew;
        private Bunifu.UI.WinForms.BunifuDataGridView dgvConsumable;
        private PictureBox picConsumable;
        private Bunifu.UI.WinForms.BunifuFormDock bunifuFormDock1;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnSelectPicture;
        private Bunifu.UI.WinForms.BunifuFormDrag bunifuFormDrag1;
        private DataGridViewTextBoxColumn ConsumableId;
        private DataGridViewTextBoxColumn Name;
        private DataGridViewTextBoxColumn Price;
        private DataGridViewTextBoxColumn Category;
        private DataGridViewTextBoxColumn StockQuantity;
        private DataGridViewTextBoxColumn CreatedDate;
        private DataGridViewTextBoxColumn ModifiedDate;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnDelete;
    }
}
