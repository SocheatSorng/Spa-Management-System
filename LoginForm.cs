using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public partial class LoginForm : Form
    {
        private bool loginSuccessful = false;
        public bool LoginSuccessful => loginSuccessful;
        
        public LoginForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Login Required";
            
            // Add required login message
            Label lblRequired = new Label
            {
                AutoSize = true,
                ForeColor = Color.DarkBlue,
                Location = new Point(120, 55),
                Font = new Font(this.Font.FontFamily, 8)
            };
            this.Controls.Add(lblRequired);
            
            // Hide the cancel button as login is now mandatory
            //btnLogin.Location = new Point(170, 170);
            //btnLogin.Size = new Size(120, 30);
        }
        
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", 
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                // Authenticate the user
                string query = "SELECT COUNT(*) FROM tbUser WHERE Username = @Username AND Password = @Password";
                
                using (SqlConnection connection = new SqlConnection(SqlConnectionManager.ConnectionString))
                {
                    connection.Open();
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        
                        int userCount = (int)command.ExecuteScalar();
                        
                        if (userCount > 0)
                        {
                            // Set authentication status in the connection manager
                            SqlConnectionManager.SetAuthenticated(username, true);
                            
                            loginSuccessful = true;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.", 
                                "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", 
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(LoginForm));
            Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges borderEdges1 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties1 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties2 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties3 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties4 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties5 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties6 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties7 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties8 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            lblTitle = new Label();
            bunifuFormControlBox1 = new Bunifu.UI.WinForms.BunifuFormControlBox();
            btnLogin = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
            txtUsername = new Bunifu.UI.WinForms.BunifuTextBox();
            txtPassword = new Bunifu.UI.WinForms.BunifuTextBox();
            bunifuFormDrag1 = new Bunifu.UI.WinForms.BunifuFormDrag();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Black;
            lblTitle.Location = new Point(150, 43);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(51, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Login";
            // 
            // bunifuFormControlBox1
            // 
            bunifuFormControlBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bunifuFormControlBox1.BunifuFormDrag = null;
            bunifuFormControlBox1.CloseBoxOptions.BackColor = Color.Transparent;
            bunifuFormControlBox1.CloseBoxOptions.BorderRadius = 0;
            bunifuFormControlBox1.CloseBoxOptions.Enabled = true;
            bunifuFormControlBox1.CloseBoxOptions.EnableDefaultAction = true;
            bunifuFormControlBox1.CloseBoxOptions.HoverColor = Color.FromArgb(232, 17, 35);
            bunifuFormControlBox1.CloseBoxOptions.Icon = (Image)resources.GetObject("bunifuFormControlBox1.CloseBoxOptions.Icon");
            bunifuFormControlBox1.CloseBoxOptions.IconAlt = null;
            bunifuFormControlBox1.CloseBoxOptions.IconColor = Color.Black;
            bunifuFormControlBox1.CloseBoxOptions.IconHoverColor = Color.White;
            bunifuFormControlBox1.CloseBoxOptions.IconPressedColor = Color.White;
            bunifuFormControlBox1.CloseBoxOptions.IconSize = new Size(18, 18);
            bunifuFormControlBox1.CloseBoxOptions.PressedColor = Color.FromArgb(232, 17, 35);
            bunifuFormControlBox1.HelpBox = false;
            bunifuFormControlBox1.HelpBoxOptions.BackColor = Color.Transparent;
            bunifuFormControlBox1.HelpBoxOptions.BorderRadius = 0;
            bunifuFormControlBox1.HelpBoxOptions.Enabled = true;
            bunifuFormControlBox1.HelpBoxOptions.EnableDefaultAction = true;
            bunifuFormControlBox1.HelpBoxOptions.HoverColor = Color.LightGray;
            bunifuFormControlBox1.HelpBoxOptions.Icon = (Image)resources.GetObject("bunifuFormControlBox1.HelpBoxOptions.Icon");
            bunifuFormControlBox1.HelpBoxOptions.IconAlt = null;
            bunifuFormControlBox1.HelpBoxOptions.IconColor = Color.Black;
            bunifuFormControlBox1.HelpBoxOptions.IconHoverColor = Color.Black;
            bunifuFormControlBox1.HelpBoxOptions.IconPressedColor = Color.Black;
            bunifuFormControlBox1.HelpBoxOptions.IconSize = new Size(22, 22);
            bunifuFormControlBox1.HelpBoxOptions.PressedColor = Color.Silver;
            bunifuFormControlBox1.Location = new Point(290, 0);
            bunifuFormControlBox1.MaximizeBox = false;
            bunifuFormControlBox1.MaximizeBoxOptions.BackColor = Color.Transparent;
            bunifuFormControlBox1.MaximizeBoxOptions.BorderRadius = 0;
            bunifuFormControlBox1.MaximizeBoxOptions.Enabled = true;
            bunifuFormControlBox1.MaximizeBoxOptions.EnableDefaultAction = true;
            bunifuFormControlBox1.MaximizeBoxOptions.HoverColor = Color.LightGray;
            bunifuFormControlBox1.MaximizeBoxOptions.Icon = (Image)resources.GetObject("bunifuFormControlBox1.MaximizeBoxOptions.Icon");
            bunifuFormControlBox1.MaximizeBoxOptions.IconAlt = (Image)resources.GetObject("bunifuFormControlBox1.MaximizeBoxOptions.IconAlt");
            bunifuFormControlBox1.MaximizeBoxOptions.IconColor = Color.Black;
            bunifuFormControlBox1.MaximizeBoxOptions.IconHoverColor = Color.Black;
            bunifuFormControlBox1.MaximizeBoxOptions.IconPressedColor = Color.Black;
            bunifuFormControlBox1.MaximizeBoxOptions.IconSize = new Size(16, 16);
            bunifuFormControlBox1.MaximizeBoxOptions.PressedColor = Color.Silver;
            bunifuFormControlBox1.MinimizeBox = true;
            bunifuFormControlBox1.MinimizeBoxOptions.BackColor = Color.Transparent;
            bunifuFormControlBox1.MinimizeBoxOptions.BorderRadius = 0;
            bunifuFormControlBox1.MinimizeBoxOptions.Enabled = true;
            bunifuFormControlBox1.MinimizeBoxOptions.EnableDefaultAction = true;
            bunifuFormControlBox1.MinimizeBoxOptions.HoverColor = Color.LightGray;
            bunifuFormControlBox1.MinimizeBoxOptions.Icon = (Image)resources.GetObject("bunifuFormControlBox1.MinimizeBoxOptions.Icon");
            bunifuFormControlBox1.MinimizeBoxOptions.IconAlt = null;
            bunifuFormControlBox1.MinimizeBoxOptions.IconColor = Color.Black;
            bunifuFormControlBox1.MinimizeBoxOptions.IconHoverColor = Color.Black;
            bunifuFormControlBox1.MinimizeBoxOptions.IconPressedColor = Color.Black;
            bunifuFormControlBox1.MinimizeBoxOptions.IconSize = new Size(14, 14);
            bunifuFormControlBox1.MinimizeBoxOptions.PressedColor = Color.Silver;
            bunifuFormControlBox1.Name = "bunifuFormControlBox1";
            bunifuFormControlBox1.ShowDesignBorders = false;
            bunifuFormControlBox1.Size = new Size(60, 30);
            bunifuFormControlBox1.TabIndex = 1;
            // 
            // btnLogin
            // 
            btnLogin.AllowAnimations = true;
            btnLogin.AllowMouseEffects = true;
            btnLogin.AllowToggling = false;
            btnLogin.AnimationSpeed = 200;
            btnLogin.AutoGenerateColors = false;
            btnLogin.AutoRoundBorders = false;
            btnLogin.AutoSizeLeftIcon = true;
            btnLogin.AutoSizeRightIcon = true;
            btnLogin.BackColor = Color.Transparent;
            btnLogin.BackColor1 = Color.FromArgb(51, 122, 183);
            btnLogin.BackgroundImage = (Image)resources.GetObject("btnLogin.BackgroundImage");
            btnLogin.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnLogin.ButtonText = "Login";
            btnLogin.ButtonTextMarginLeft = 0;
            btnLogin.ColorContrastOnClick = 45;
            btnLogin.ColorContrastOnHover = 45;
            borderEdges1.BottomLeft = true;
            borderEdges1.BottomRight = true;
            borderEdges1.TopLeft = true;
            borderEdges1.TopRight = true;
            btnLogin.CustomizableEdges = borderEdges1;
            btnLogin.DialogResult = DialogResult.None;
            btnLogin.DisabledBorderColor = Color.FromArgb(191, 191, 191);
            btnLogin.DisabledFillColor = Color.Empty;
            btnLogin.DisabledForecolor = Color.Empty;
            btnLogin.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton.ButtonStates.Pressed;
            btnLogin.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.IconLeft = null;
            btnLogin.IconLeftAlign = ContentAlignment.MiddleLeft;
            btnLogin.IconLeftCursor = Cursors.Default;
            btnLogin.IconLeftPadding = new Padding(11, 3, 3, 3);
            btnLogin.IconMarginLeft = 11;
            btnLogin.IconPadding = 10;
            btnLogin.IconRight = null;
            btnLogin.IconRightAlign = ContentAlignment.MiddleRight;
            btnLogin.IconRightCursor = Cursors.Default;
            btnLogin.IconRightPadding = new Padding(3, 3, 7, 3);
            btnLogin.IconSize = 25;
            btnLogin.IdleBorderColor = Color.Empty;
            btnLogin.IdleBorderRadius = 0;
            btnLogin.IdleBorderThickness = 0;
            btnLogin.IdleFillColor = Color.Empty;
            btnLogin.IdleIconLeftImage = null;
            btnLogin.IdleIconRightImage = null;
            btnLogin.IndicateFocus = false;
            btnLogin.Location = new Point(75, 208);
            btnLogin.Name = "btnLogin";
            btnLogin.OnDisabledState.BorderColor = Color.FromArgb(191, 191, 191);
            btnLogin.OnDisabledState.BorderRadius = 15;
            btnLogin.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnLogin.OnDisabledState.BorderThickness = 1;
            btnLogin.OnDisabledState.FillColor = Color.FromArgb(204, 204, 204);
            btnLogin.OnDisabledState.ForeColor = Color.FromArgb(168, 160, 168);
            btnLogin.OnDisabledState.IconLeftImage = null;
            btnLogin.OnDisabledState.IconRightImage = null;
            btnLogin.onHoverState.BorderColor = Color.DarkGoldenrod;
            btnLogin.onHoverState.BorderRadius = 15;
            btnLogin.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnLogin.onHoverState.BorderThickness = 1;
            btnLogin.onHoverState.FillColor = Color.DarkGoldenrod;
            btnLogin.onHoverState.ForeColor = Color.White;
            btnLogin.onHoverState.IconLeftImage = null;
            btnLogin.onHoverState.IconRightImage = null;
            btnLogin.OnIdleState.BorderColor = Color.DarkGoldenrod;
            btnLogin.OnIdleState.BorderRadius = 15;
            btnLogin.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnLogin.OnIdleState.BorderThickness = 1;
            btnLogin.OnIdleState.FillColor = Color.DarkGoldenrod;
            btnLogin.OnIdleState.ForeColor = Color.White;
            btnLogin.OnIdleState.IconLeftImage = null;
            btnLogin.OnIdleState.IconRightImage = null;
            btnLogin.OnPressedState.BorderColor = Color.DarkGoldenrod;
            btnLogin.OnPressedState.BorderRadius = 15;
            btnLogin.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnLogin.OnPressedState.BorderThickness = 1;
            btnLogin.OnPressedState.FillColor = Color.DarkGoldenrod;
            btnLogin.OnPressedState.ForeColor = Color.White;
            btnLogin.OnPressedState.IconLeftImage = null;
            btnLogin.OnPressedState.IconRightImage = null;
            btnLogin.Size = new Size(200, 30);
            btnLogin.TabIndex = 5;
            btnLogin.TextAlign = ContentAlignment.MiddleCenter;
            btnLogin.TextAlignment = HorizontalAlignment.Center;
            btnLogin.TextMarginLeft = 0;
            btnLogin.TextPadding = new Padding(0);
            btnLogin.UseDefaultRadiusAndThickness = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtUsername
            // 
            txtUsername.AcceptsReturn = false;
            txtUsername.AcceptsTab = false;
            txtUsername.AnimationSpeed = 200;
            txtUsername.AutoCompleteMode = AutoCompleteMode.None;
            txtUsername.AutoCompleteSource = AutoCompleteSource.None;
            txtUsername.AutoSizeHeight = true;
            txtUsername.BackColor = Color.Transparent;
            txtUsername.BackgroundImage = (Image)resources.GetObject("txtUsername.BackgroundImage");
            txtUsername.BorderColorActive = Color.DodgerBlue;
            txtUsername.BorderColorDisabled = Color.FromArgb(204, 204, 204);
            txtUsername.BorderColorHover = Color.FromArgb(105, 181, 255);
            txtUsername.BorderColorIdle = Color.Silver;
            txtUsername.BorderRadius = 15;
            txtUsername.BorderThickness = 1;
            txtUsername.CharacterCase = Bunifu.UI.WinForms.BunifuTextBox.CharacterCases.Normal;
            txtUsername.CharacterCasing = CharacterCasing.Normal;
            txtUsername.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtUsername.DefaultText = "";
            txtUsername.FillColor = Color.White;
            txtUsername.HideSelection = true;
            txtUsername.IconLeft = null;
            txtUsername.IconLeftCursor = Cursors.IBeam;
            txtUsername.IconPadding = 10;
            txtUsername.IconRight = null;
            txtUsername.IconRightCursor = Cursors.IBeam;
            txtUsername.Location = new Point(75, 90);
            txtUsername.MaxLength = 32767;
            txtUsername.MinimumSize = new Size(1, 1);
            txtUsername.Modified = false;
            txtUsername.Multiline = false;
            txtUsername.Name = "txtUsername";
            stateProperties1.BorderColor = Color.DodgerBlue;
            stateProperties1.FillColor = Color.Empty;
            stateProperties1.ForeColor = Color.Empty;
            stateProperties1.PlaceholderForeColor = Color.Empty;
            txtUsername.OnActiveState = stateProperties1;
            stateProperties2.BorderColor = Color.FromArgb(204, 204, 204);
            stateProperties2.FillColor = Color.FromArgb(240, 240, 240);
            stateProperties2.ForeColor = Color.FromArgb(109, 109, 109);
            stateProperties2.PlaceholderForeColor = Color.DarkGray;
            txtUsername.OnDisabledState = stateProperties2;
            stateProperties3.BorderColor = Color.FromArgb(105, 181, 255);
            stateProperties3.FillColor = Color.Empty;
            stateProperties3.ForeColor = Color.Empty;
            stateProperties3.PlaceholderForeColor = Color.Empty;
            txtUsername.OnHoverState = stateProperties3;
            stateProperties4.BorderColor = Color.Silver;
            stateProperties4.FillColor = Color.White;
            stateProperties4.ForeColor = Color.Empty;
            stateProperties4.PlaceholderForeColor = Color.Empty;
            txtUsername.OnIdleState = stateProperties4;
            txtUsername.Padding = new Padding(3);
            txtUsername.PasswordChar = '\0';
            txtUsername.PlaceholderForeColor = Color.Silver;
            txtUsername.PlaceholderText = "Enter username";
            txtUsername.ReadOnly = false;
            txtUsername.ScrollBars = ScrollBars.None;
            txtUsername.SelectedText = "";
            txtUsername.SelectionLength = 0;
            txtUsername.SelectionStart = 0;
            txtUsername.ShortcutsEnabled = true;
            txtUsername.Size = new Size(200, 38);
            txtUsername.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtUsername.TabIndex = 6;
            txtUsername.TextAlign = HorizontalAlignment.Left;
            txtUsername.TextMarginBottom = 0;
            txtUsername.TextMarginLeft = 3;
            txtUsername.TextMarginTop = 1;
            txtUsername.TextPlaceholder = "Enter username";
            txtUsername.UseSystemPasswordChar = false;
            txtUsername.WordWrap = true;
            // 
            // txtPassword
            // 
            txtPassword.AcceptsReturn = false;
            txtPassword.AcceptsTab = false;
            txtPassword.AnimationSpeed = 200;
            txtPassword.AutoCompleteMode = AutoCompleteMode.None;
            txtPassword.AutoCompleteSource = AutoCompleteSource.None;
            txtPassword.AutoSizeHeight = true;
            txtPassword.BackColor = Color.Transparent;
            txtPassword.BackgroundImage = (Image)resources.GetObject("txtPassword.BackgroundImage");
            txtPassword.BorderColorActive = Color.DodgerBlue;
            txtPassword.BorderColorDisabled = Color.FromArgb(204, 204, 204);
            txtPassword.BorderColorHover = Color.FromArgb(105, 181, 255);
            txtPassword.BorderColorIdle = Color.Silver;
            txtPassword.BorderRadius = 15;
            txtPassword.BorderThickness = 1;
            txtPassword.CharacterCase = Bunifu.UI.WinForms.BunifuTextBox.CharacterCases.Normal;
            txtPassword.CharacterCasing = CharacterCasing.Normal;
            txtPassword.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPassword.DefaultText = "";
            txtPassword.FillColor = Color.White;
            txtPassword.HideSelection = true;
            txtPassword.IconLeft = null;
            txtPassword.IconLeftCursor = Cursors.IBeam;
            txtPassword.IconPadding = 10;
            txtPassword.IconRight = null;
            txtPassword.IconRightCursor = Cursors.IBeam;
            txtPassword.Location = new Point(75, 134);
            txtPassword.MaxLength = 32767;
            txtPassword.MinimumSize = new Size(1, 1);
            txtPassword.Modified = false;
            txtPassword.Multiline = false;
            txtPassword.Name = "txtPassword";
            stateProperties5.BorderColor = Color.DodgerBlue;
            stateProperties5.FillColor = Color.Empty;
            stateProperties5.ForeColor = Color.Empty;
            stateProperties5.PlaceholderForeColor = Color.Empty;
            txtPassword.OnActiveState = stateProperties5;
            stateProperties6.BorderColor = Color.FromArgb(204, 204, 204);
            stateProperties6.FillColor = Color.FromArgb(240, 240, 240);
            stateProperties6.ForeColor = Color.FromArgb(109, 109, 109);
            stateProperties6.PlaceholderForeColor = Color.DarkGray;
            txtPassword.OnDisabledState = stateProperties6;
            stateProperties7.BorderColor = Color.FromArgb(105, 181, 255);
            stateProperties7.FillColor = Color.Empty;
            stateProperties7.ForeColor = Color.Empty;
            stateProperties7.PlaceholderForeColor = Color.Empty;
            txtPassword.OnHoverState = stateProperties7;
            stateProperties8.BorderColor = Color.Silver;
            stateProperties8.FillColor = Color.White;
            stateProperties8.ForeColor = Color.Empty;
            stateProperties8.PlaceholderForeColor = Color.Empty;
            txtPassword.OnIdleState = stateProperties8;
            txtPassword.Padding = new Padding(3);
            txtPassword.PasswordChar = '\0';
            txtPassword.PlaceholderForeColor = Color.Silver;
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.ReadOnly = false;
            txtPassword.ScrollBars = ScrollBars.None;
            txtPassword.SelectedText = "";
            txtPassword.SelectionLength = 0;
            txtPassword.SelectionStart = 0;
            txtPassword.ShortcutsEnabled = true;
            txtPassword.Size = new Size(200, 38);
            txtPassword.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtPassword.TabIndex = 7;
            txtPassword.TextAlign = HorizontalAlignment.Left;
            txtPassword.TextMarginBottom = 0;
            txtPassword.TextMarginLeft = 3;
            txtPassword.TextMarginTop = 1;
            txtPassword.TextPlaceholder = "Enter password";
            txtPassword.UseSystemPasswordChar = false;
            txtPassword.WordWrap = true;
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
            // LoginForm
            // 
            ClientSize = new Size(350, 250);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(btnLogin);
            Controls.Add(bunifuFormControlBox1);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();
        }
        private System.Windows.Forms.Label lblTitle;
        private Bunifu.UI.WinForms.BunifuFormControlBox bunifuFormControlBox1;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton btnLogin;
        private Bunifu.UI.WinForms.BunifuTextBox txtUsername;
        private Bunifu.UI.WinForms.BunifuTextBox txtPassword;
        private Bunifu.UI.WinForms.BunifuFormDrag bunifuFormDrag1;
    }
} 