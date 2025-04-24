using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace Spa_Management_System
{
    public partial class ConnectionDialog : Form
    {
        public string ConnectionString { get; private set; }
        private List<string> availableServers = new List<string>();
        private bool isNewDatabase = false;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton btnConnect;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton btnCreateDatabase;
        private Bunifu.UI.WinForms.BunifuLabel bunifuLabel1;
        private Bunifu.UI.WinForms.BunifuTextBox txtPassword;
        private Bunifu.UI.WinForms.BunifuTextBox txtUsername;
        private Bunifu.UI.WinForms.BunifuDropdown txtServer;
        private Bunifu.UI.WinForms.BunifuDropdown txtDatabase;
        private Bunifu.UI.WinForms.BunifuCheckBox chkIntegratedSecurity;
        private Bunifu.UI.WinForms.BunifuLabel lblUseWinAuth;
        private Bunifu.UI.WinForms.BunifuFormDrag bunifuFormDrag1;
        private Bunifu.UI.WinForms.BunifuFormControlBox bunifuFormControlBox1;

        public ConnectionDialog()
        {
            InitializeComponent();
            this.Load += ConnectionDialog_Load;
            
            // Set up the Escape key to close the form
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            };
            this.KeyPreview = true;
            
            // Wire up the click event for the connect button
            btnConnect.Click += btnConnect_Click;
        }

        private async void ConnectionDialog_Load(object sender, EventArgs e)
        {
            // Center the form on screen
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Set initial state of username and password fields based on checkbox
            txtUsername.Enabled = !chkIntegratedSecurity.Checked;
            txtPassword.Enabled = !chkIntegratedSecurity.Checked;
            
            // Add instructions label
            Label lblInstructions = new Label();
            lblInstructions.AutoSize = true;
            lblInstructions.Location = new System.Drawing.Point(20, 95);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new System.Drawing.Size(350, 17);
            lblInstructions.ForeColor = System.Drawing.Color.Navy;
            this.Controls.Add(lblInstructions);
            
            // Update the form text
            this.Text = "Database Connection Settings";

            // Initialize default server text but allow editing
            txtServer.Text = "localhost";
            txtServer.Enabled = true; // Always enable the server field
            btnConnect.Enabled = true; // Always enable the connect button

            // Wire up the click event for the create database button
            btnCreateDatabase.Click += btnCreateDatabase_Click;

            // Add server selection handler
            txtServer.SelectedIndexChanged += (s, args) => 
            {
                if (txtServer.SelectedItem != null)
                {
                    // When server changes, update available databases
                    PopulateDatabaseDropdown();
                }
            };
            
            // Add text changed handler to detect manual entry
            txtServer.TextChanged += (s, args) =>
            {
                // When text changes manually, update databases if needed
                if (txtServer.Text.Length > 0 && txtServer.Text != "Scanning for SQL Servers...")
                {
                    PopulateDatabaseDropdown();
                }
            };

            // Add a message about scanning
            Label lblScanning = new Label();
            lblScanning.AutoSize = true;
            lblScanning.Location = new System.Drawing.Point(150, 47);
            lblScanning.Size = new System.Drawing.Size(250, 15);
            lblScanning.Text = "Scanning for SQL servers in background...";
            lblScanning.ForeColor = System.Drawing.Color.DarkGray;
            lblScanning.Font = new System.Drawing.Font(lblScanning.Font.FontFamily, 8);
            this.Controls.Add(lblScanning);

            // Scan for SQL Servers asynchronously
            await Task.Run(() => 
            {
                try
                {
                    // Find local SQL Server instances
                    availableServers = FindSqlServerInstances();
                    
                    // Update UI on main thread
                    this.Invoke(new Action(() => 
                    {
                        lblScanning.Visible = false;
                        
                        if (availableServers.Count > 0)
                        {
                            // Save the current text in case user has already typed something
                            string currentText = txtServer.Text;
                            
                            // Update items without changing selection
                            txtServer.BeginUpdate();
                            txtServer.Items.Clear();
                            foreach (string server in availableServers)
                            {
                                txtServer.Items.Add(server);
                            }
                            txtServer.EndUpdate();
                            
                            // If the user hasn't typed anything specific, select the first server
                            if (currentText == "localhost" || currentText == "Scanning for SQL Servers...")
                            {
                                txtServer.SelectedIndex = 0;
                            }
                            else
                            {
                                // Keep what the user typed
                                txtServer.Text = currentText;
                            }
                        }
                        else
                        {
                            lblScanning.Text = "No SQL Servers found. Please enter server name manually.";
                            lblScanning.Visible = true;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => 
                    {
                        lblScanning.Text = "Error scanning for servers. Please enter server name manually.";
                        lblScanning.ForeColor = System.Drawing.Color.Red;
                        lblScanning.Visible = true;
                    }));
                }
            });
        }

        private List<string> FindSqlServerInstances()
        {
            List<string> instances = new List<string>();
            
            try
            {
                // Method 1: Try to find SQL Server instances from registry
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL"))
                {
                    if (rk != null)
                    {
                        foreach (string instanceName in rk.GetValueNames())
                        {
                            if (instanceName.Equals("MSSQLSERVER"))
                                instances.Add(Environment.MachineName); // Default instance
                            else
                                instances.Add(Environment.MachineName + "\\" + instanceName);
                        }
                    }
                }
                
                // Method 2: If registry method doesn't work or finds nothing, try SQL Browser method
                if (instances.Count == 0)
                {
                    // This is a simplified version - in a real app you'd use SQL Browser Service
                    // For demo, add some common defaults
                    instances.Add(Environment.MachineName);
                    instances.Add(Environment.MachineName + "\\SQLEXPRESS");
                    instances.Add(Environment.MachineName + "\\MSSQLSERVER");
                    instances.Add("localhost");
                    instances.Add("(local)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding SQL Server instances: {ex.Message}");
                // Add fallback options
                instances.Add(Environment.MachineName);
                instances.Add("localhost");
                instances.Add("(local)");
            }
            
            return instances;
        }

        private void PopulateDatabaseDropdown()
        {
            if (txtServer.Text == "No SQL Servers found" || string.IsNullOrEmpty(txtServer.Text))
            {
                return;
            }

            // Cache values from UI before starting the thread
            string serverName = txtServer.Text;
            bool useIntegratedSecurity = chkIntegratedSecurity.Checked;
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Update UI from the UI thread
            txtDatabase.Items.Clear();
            txtDatabase.Text = "Loading databases...";
            txtDatabase.Enabled = false;
            
            // Use Task to avoid freezing the UI
            Task.Run(() =>
            {
                List<string> databases = new List<string>();
                Exception caughtException = null;
                
                try
                {
                    // Build connection string to master database
                    SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = serverName,
                        InitialCatalog = "master",
                        TrustServerCertificate = true,
                        ConnectTimeout = 30 // Reasonable timeout
                    };
                    
                    if (!useIntegratedSecurity)
                    {
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            masterBuilder.UserID = username;
                            masterBuilder.Password = password;
                        }
                        else
                        {
                            masterBuilder.IntegratedSecurity = true;
                        }
                    }
                    else
                    {
                        masterBuilder.IntegratedSecurity = true;
                    }

                    using (SqlConnection connection = new SqlConnection(masterBuilder.ConnectionString))
                    {
                        connection.Open();
                        
                        // Query to get user databases
                        string query = @"
                            SELECT name 
                            FROM sys.databases 
                            WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                            ORDER BY name";
                        
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = 30; // Reasonable timeout
                            
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    databases.Add(reader["name"].ToString());
                                }
                            }
                        }
                    }
                    
                    // Add SpaManagement if not already in the list
                    if (!databases.Contains("SpaManagement"))
                    {
                        databases.Add("SpaManagement");
                    }
                }
                catch (Exception ex)
                {
                    // Catch and store the exception
                    caughtException = ex;
                    
                    // Add SpaManagement as a fallback if no databases were found
                    if (databases.Count == 0)
                    {
                        databases.Add("SpaManagement");
                    }
                }
                
                // Capture the final list to avoid any thread issues
                List<string> finalDatabases = new List<string>(databases);
                
                // Always use BeginInvoke for updating UI from background thread
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // Update the database dropdown
                            txtDatabase.Items.Clear();
                            
                            foreach (string db in finalDatabases)
                            {
                                txtDatabase.Items.Add(db);
                            }
                            
                            // Select SpaManagement if available, otherwise first item
                            int spaManagementIndex = txtDatabase.Items.IndexOf("SpaManagement");
                            if (spaManagementIndex >= 0)
                            {
                                txtDatabase.SelectedIndex = spaManagementIndex;
                            }
                            else if (txtDatabase.Items.Count > 0)
                            {
                                txtDatabase.SelectedIndex = 0;
                            }
                            
                            txtDatabase.Enabled = true;
                        }
                        catch (Exception uiEx)
                        {
                            Console.WriteLine($"Error updating UI: {uiEx.Message}");
                        }
                    }));
                }
            });
        }

        private void btnCreateDatabase_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text) || txtServer.Text == "No SQL Servers found")
            {
                MessageBox.Show("Please select a valid SQL Server first.", 
                    "Server Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the SpaManagement.sql file exists
            string currentDirectory = Directory.GetCurrentDirectory();
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath = Path.Combine(projectDirectory, "SpaManagement.sql");
            
            string scriptPath = null;
            bool scriptExists = false;
            
            // Debug information
            Console.WriteLine($"Current Directory: {currentDirectory}");
            Console.WriteLine($"Project Directory: {projectDirectory}");
            Console.WriteLine($"Absolute Path: {absolutePath}");
            
            // Try different paths in priority order
            string[] possiblePaths = new string[] 
            {
                "SpaManagement.sql",  // Current directory
                absolutePath, // Absolute path
                Path.Combine(Directory.GetCurrentDirectory(), "SpaManagement.sql"), // Current directory explicit
                Path.Combine(Application.StartupPath, "SpaManagement.sql"), // Application directory
                Path.GetFullPath("SpaManagement.sql"), // Resolved path
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SpaManagement.sql"), // Executable directory
                Path.Combine(Environment.CurrentDirectory, "SpaManagement.sql"), // Environment current directory
            };
            
            foreach (string path in possiblePaths)
            {
                Console.WriteLine($"Checking path: {path}, Exists: {File.Exists(path)}");
                if (File.Exists(path))
                {
                    scriptPath = path;
                    scriptExists = true;
                    break;
                }
            }
            
            // If not found in standard locations, try parent directories
            if (!scriptExists)
            {
                DialogResult result = MessageBox.Show(
                    "SpaManagement.sql file not found. Do you want to browse for it?",
                    "File Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.Title = "Select SpaManagement.sql file";
                        dlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                        
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            scriptPath = dlg.FileName;
                            scriptExists = true;
                            
                            // Try to copy to application directory for future use
                            try
                            {
                                string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpaManagement.sql");
                                File.Copy(scriptPath, targetPath, true);
                                scriptPath = targetPath;
                                Console.WriteLine($"Copied file to: {targetPath}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to copy file: {ex.Message}");
                                // Continue using the selected file even if copy fails
                            }
                        }
                        else
                        {
                            // User canceled the dialog, continue with no SQL file
                            scriptExists = false;
                        }
                    }
                }
            }
            
            if (scriptPath == null)
            {
                string errorMessage = "Could not find SpaManagement.sql file in any location. Please ensure the file exists in the project directory.";
                MessageBox.Show(errorMessage, "Script Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create a dialog for database creation
            using (Form createDbDialog = new Form())
            {
                createDbDialog.Text = "Create New Database";
                createDbDialog.Size = new System.Drawing.Size(400, 350);
                createDbDialog.StartPosition = FormStartPosition.CenterParent;
                createDbDialog.FormBorderStyle = FormBorderStyle.None;
                createDbDialog.MaximizeBox = false;
                createDbDialog.MinimizeBox = false;

                // Create controls
                Bunifu.UI.WinForms.BunifuFormControlBox bunifuControlBox = new Bunifu.UI.WinForms.BunifuFormControlBox();
                bunifuControlBox.Location = new Point(340, 0);
                bunifuControlBox.Size = new Size(60, 30);
                bunifuControlBox.MaximizeBox = false;
                bunifuControlBox.BackColor = Color.Transparent;
                bunifuControlBox.ShowDesignBorders = false;
                createDbDialog.Controls.Add(bunifuControlBox);
                
                // Add BunifuFormDrag to make the form draggable
                Bunifu.UI.WinForms.BunifuFormDrag formDrag = new Bunifu.UI.WinForms.BunifuFormDrag();
                formDrag.AllowOpacityChangesWhileDragging = false;
                formDrag.ContainerControl = createDbDialog;
                formDrag.DockIndicatorsOpacity = 0.5D;
                formDrag.DockingIndicatorsColor = Color.FromArgb(202, 215, 233);
                formDrag.DockingOptions.DockAll = true;
                formDrag.DragOpacity = 0.9D;
                formDrag.Enabled = true;
                formDrag.ParentForm = createDbDialog;
                formDrag.ShowCursorChanges = true;
                formDrag.ShowDockingIndicators = true;
                formDrag.TitleBarOptions.DoubleClickToExpandWindow = true;
                formDrag.TitleBarOptions.Enabled = true;
                formDrag.TitleBarOptions.UseBackColorOnDockingIndicators = false;

                // Create title label
                Bunifu.UI.WinForms.BunifuLabel lblTitle = new Bunifu.UI.WinForms.BunifuLabel();
                lblTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
                lblTitle.Location = new Point(105, 37);
                lblTitle.Size = new Size(190, 21);
                lblTitle.Text = "Create New Database";
                lblTitle.TextFormat = Bunifu.UI.WinForms.BunifuLabel.TextFormattingOptions.Default;
                createDbDialog.Controls.Add(lblTitle);

                // Database name field
                Bunifu.UI.WinForms.BunifuLabel lblDbName = new Bunifu.UI.WinForms.BunifuLabel();
                lblDbName.Text = "Database Name:";
                lblDbName.Location = new Point(20, 80);
                lblDbName.Size = new Size(120, 20);
                lblDbName.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                createDbDialog.Controls.Add(lblDbName);

                Bunifu.UI.WinForms.BunifuTextBox txtDbName = new Bunifu.UI.WinForms.BunifuTextBox();
                txtDbName.Location = new Point(150, 75);
                txtDbName.Size = new Size(200, 38);
                txtDbName.Text = "SpaManagement";
                txtDbName.BorderRadius = 15;
                txtDbName.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                txtDbName.BorderColorActive = Color.DarkGoldenrod;
                txtDbName.BorderColorHover = Color.DarkGoldenrod;
                txtDbName.PlaceholderText = "Enter database name";
                createDbDialog.Controls.Add(txtDbName);

                // For SQL Authentication checkbox
                Bunifu.UI.WinForms.BunifuCheckBox chkSqlAuth = new Bunifu.UI.WinForms.BunifuCheckBox();
                chkSqlAuth.Location = new Point(20, 125);
                chkSqlAuth.Size = new Size(21, 21);
                chkSqlAuth.Checked = false;
                chkSqlAuth.OnCheck.BorderColor = Color.DarkGoldenrod;
                chkSqlAuth.OnCheck.CheckBoxColor = Color.DarkGoldenrod;
                chkSqlAuth.OnCheck.CheckmarkColor = Color.White;
                chkSqlAuth.BorderRadius = 12;
                createDbDialog.Controls.Add(chkSqlAuth);

                Bunifu.UI.WinForms.BunifuLabel lblSqlAuth = new Bunifu.UI.WinForms.BunifuLabel();
                lblSqlAuth.Text = "Use SQL Server Authentication";
                lblSqlAuth.Location = new Point(45, 128);
                lblSqlAuth.Size = new Size(200, 20);
                lblSqlAuth.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                createDbDialog.Controls.Add(lblSqlAuth);

                // Username and password fields
                Bunifu.UI.WinForms.BunifuLabel lblUser = new Bunifu.UI.WinForms.BunifuLabel();
                lblUser.Text = "Username:";
                lblUser.Location = new Point(45, 165);
                lblUser.Size = new Size(80, 20);
                lblUser.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                lblUser.Enabled = false;
                createDbDialog.Controls.Add(lblUser);

                Bunifu.UI.WinForms.BunifuTextBox txtUser = new Bunifu.UI.WinForms.BunifuTextBox();
                txtUser.Location = new Point(150, 160);
                txtUser.Size = new Size(200, 38);
                txtUser.BorderRadius = 15;
                txtUser.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                txtUser.BorderColorActive = Color.DarkGoldenrod;
                txtUser.BorderColorHover = Color.DarkGoldenrod;
                txtUser.PlaceholderText = "Enter username";
                txtUser.Enabled = false;
                createDbDialog.Controls.Add(txtUser);

                Bunifu.UI.WinForms.BunifuLabel lblPass = new Bunifu.UI.WinForms.BunifuLabel();
                lblPass.Text = "Password:";
                lblPass.Location = new Point(45, 205);
                lblPass.Size = new Size(80, 20);
                lblPass.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                lblPass.Enabled = false;
                createDbDialog.Controls.Add(lblPass);

                Bunifu.UI.WinForms.BunifuTextBox txtPass = new Bunifu.UI.WinForms.BunifuTextBox();
                txtPass.Location = new Point(150, 200);
                txtPass.Size = new Size(200, 38);
                txtPass.BorderRadius = 15;
                txtPass.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                txtPass.BorderColorActive = Color.DarkGoldenrod;
                txtPass.BorderColorHover = Color.DarkGoldenrod;
                txtPass.PlaceholderText = "Enter password";
                txtPass.PasswordChar = '•';
                txtPass.UseSystemPasswordChar = true;
                txtPass.Enabled = false;
                createDbDialog.Controls.Add(txtPass);

                // Schema creation option
                Bunifu.UI.WinForms.BunifuCheckBox chkCreateSchema = new Bunifu.UI.WinForms.BunifuCheckBox();
                chkCreateSchema.Location = new Point(20, 250);
                chkCreateSchema.Size = new Size(21, 21);
                chkCreateSchema.Checked = true;
                chkCreateSchema.OnCheck.BorderColor = Color.DarkGoldenrod;
                chkCreateSchema.OnCheck.CheckBoxColor = Color.DarkGoldenrod;
                chkCreateSchema.OnCheck.CheckmarkColor = Color.White;
                chkCreateSchema.BorderRadius = 12;
                createDbDialog.Controls.Add(chkCreateSchema);

                Bunifu.UI.WinForms.BunifuLabel lblCreateSchema = new Bunifu.UI.WinForms.BunifuLabel();
                lblCreateSchema.Text = "Create tables and schema after database creation";
                lblCreateSchema.Location = new Point(45, 253);
                lblCreateSchema.Size = new Size(300, 20);
                lblCreateSchema.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                createDbDialog.Controls.Add(lblCreateSchema);
                
                // Update script status message
                Bunifu.UI.WinForms.BunifuLabel lblScriptStatus = new Bunifu.UI.WinForms.BunifuLabel();
                lblScriptStatus.Location = new Point(20, 280);
                lblScriptStatus.Size = new Size(350, 20);
                lblScriptStatus.AutoEllipsis = true;
                createDbDialog.Controls.Add(lblScriptStatus);

                // Check for script file
                string specificPath = "SpaManagement.sql";
                
                // Check multiple possible locations for the SQL file
                string[] dialogPossiblePaths = new string[]
                {
                    specificPath,  // Current directory
                    Path.Combine(Application.StartupPath, "SpaManagement.sql"), // Application directory
                    ".\\SpaManagement.sql", // Explicit current directory
                    "..\\SpaManagement.sql" // Parent directory
                };
                
                foreach (string path in dialogPossiblePaths)
                {
                    if (File.Exists(path))
                    {
                        scriptPath = path;
                    scriptExists = true;
                        lblScriptStatus.Text = "SpaManagement.sql found";
                        lblScriptStatus.ForeColor = Color.Green;
                        break;
                    }
                }
                
                if (!scriptExists)
                {
                    lblScriptStatus.Text = "SpaManagement.sql not found in any of the expected locations";
                    lblScriptStatus.ForeColor = Color.Red;
                    
                    // Add a browse button when file is not found
                    Bunifu.UI.WinForms.BunifuButton.BunifuButton btnBrowse = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
                    btnBrowse.Text = "Browse...";
                    btnBrowse.Location = new Point(275, 275);
                    btnBrowse.Size = new Size(80, 30);
                    btnBrowse.BackColor = Color.Transparent;
                    btnBrowse.ForeColor = Color.White;
                    btnBrowse.OnIdleState.BorderColor = Color.DarkGoldenrod;
                    btnBrowse.OnIdleState.FillColor = Color.DarkGoldenrod;
                    btnBrowse.OnIdleState.BorderRadius = 15;
                    btnBrowse.onHoverState.BorderColor = Color.DarkGoldenrod;
                    btnBrowse.onHoverState.FillColor = Color.DarkGoldenrod;
                    btnBrowse.OnPressedState.BorderColor = Color.DarkGoldenrod;
                    btnBrowse.OnPressedState.FillColor = Color.DarkGoldenrod;
                    btnBrowse.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                    
                    btnBrowse.Click += (s, args) => {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            openFileDialog.Filter = "SQL Files (*.sql)|*.sql";
                            openFileDialog.Title = "Select SpaManagement.sql file";
                            
                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                try {
                                    // Copy the file to the application directory
                                    string fileName = Path.GetFileName(openFileDialog.FileName);
                                    string destFile = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                                    
                                    File.Copy(openFileDialog.FileName, destFile, true);
                                    
                                    scriptPath = destFile;
                                    scriptExists = true;
                                    lblScriptStatus.Text = "SpaManagement.sql found";
                                    lblScriptStatus.ForeColor = Color.Green;
                                    btnBrowse.Visible = false;
                                }
                                catch (Exception ex) {
                                    MessageBox.Show("Error copying file: " + ex.Message, "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    };
                    
                    createDbDialog.Controls.Add(btnBrowse);
                }
                
                // Event handler for checkbox
                chkSqlAuth.CheckedChanged += (s, args) => 
                {
                    lblUser.Enabled = chkSqlAuth.Checked;
                    txtUser.Enabled = chkSqlAuth.Checked;
                    lblPass.Enabled = chkSqlAuth.Checked;
                    txtPass.Enabled = chkSqlAuth.Checked;
                };

                // Buttons
                Bunifu.UI.WinForms.BunifuButton.BunifuButton btnCreate = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
                btnCreate.Text = "Create";
                btnCreate.Location = new Point(130, 310);
                btnCreate.Size = new Size(100, 30);
                btnCreate.BackColor = Color.Transparent;
                btnCreate.ForeColor = Color.White;
                btnCreate.OnIdleState.BorderColor = Color.DarkGoldenrod;
                btnCreate.OnIdleState.FillColor = Color.DarkGoldenrod;
                btnCreate.OnIdleState.BorderRadius = 15;
                btnCreate.onHoverState.BorderColor = Color.DarkGoldenrod;
                btnCreate.onHoverState.FillColor = Color.DarkGoldenrod;
                btnCreate.OnPressedState.BorderColor = Color.DarkGoldenrod;
                btnCreate.OnPressedState.FillColor = Color.DarkGoldenrod;
                btnCreate.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                createDbDialog.Controls.Add(btnCreate);

                Bunifu.UI.WinForms.BunifuButton.BunifuButton btnCancel = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
                btnCancel.Text = "Cancel";
                btnCancel.Location = new Point(240, 310);
                btnCancel.Size = new Size(100, 30);
                btnCancel.BackColor = Color.Transparent;
                btnCancel.ForeColor = Color.White;
                btnCancel.OnIdleState.BorderColor = Color.DarkGray;
                btnCancel.OnIdleState.FillColor = Color.DarkGray;
                btnCancel.OnIdleState.BorderRadius = 15;
                btnCancel.onHoverState.BorderColor = Color.DarkGray;
                btnCancel.onHoverState.FillColor = Color.DarkGray;
                btnCancel.OnPressedState.BorderColor = Color.DarkGray;
                btnCancel.OnPressedState.FillColor = Color.DarkGray;
                btnCancel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                createDbDialog.Controls.Add(btnCancel);

                // Set up dialog result handlers
                btnCreate.Click += (s, args) => {
                    createDbDialog.DialogResult = DialogResult.OK;
                    createDbDialog.Close();
                };
                
                btnCancel.Click += (s, args) => {
                    createDbDialog.DialogResult = DialogResult.Cancel;
                    createDbDialog.Close();
                };

                // Show dialog
                if (createDbDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string dbName = txtDbName.Text.Trim();
                        if (string.IsNullOrEmpty(dbName))
                        {
                            MessageBox.Show("Please enter a valid database name.", 
                                "Database Name Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Build connection string to master database
                        SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder();
                        masterBuilder.DataSource = txtServer.Text;
                        masterBuilder.InitialCatalog = "master";
                        
                        if (chkSqlAuth.Checked)
                        {
                            string username = txtUser.Text.Trim();
                            string password = txtPass.Text;
                            
                            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                            {
                                MessageBox.Show("Please enter both username and password for SQL authentication.", 
                                    "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            
                            masterBuilder.UserID = username;
                            masterBuilder.Password = password;
                        }
                        else
                        {
                            masterBuilder.IntegratedSecurity = true;
                        }
                        
                        masterBuilder.TrustServerCertificate = true;

                        // Create database
                        using (SqlConnection masterConn = new SqlConnection(masterBuilder.ConnectionString))
                        {
                            masterConn.Open();
                            
                            // Check if database already exists
                            string checkQuery = $"SELECT DB_ID('{dbName}')";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, masterConn))
                            {
                                object result = checkCmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    DialogResult overwriteResult = MessageBox.Show(
                                        $"Database '{dbName}' already exists. Do you want to use the existing database?",
                                        "Database Exists",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);
                                        
                                    if (overwriteResult == DialogResult.Yes)
                                    {
                                        // Update the database dropdown
                                        if (!txtDatabase.Items.Contains(dbName))
                                        {
                                            txtDatabase.Items.Add(dbName);
                                        }
                                        txtDatabase.SelectedItem = dbName;
                                        MessageBox.Show($"Using existing database '{dbName}'.", 
                                            "Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            
                                        // Check if schema creation is requested
                                        if (chkCreateSchema.Checked && scriptExists)
                                        {
                                            DialogResult createSchemaResult = MessageBox.Show(
                                                $"Do you want to recreate tables and schema in the existing database '{dbName}'?\n\nWARNING: This will delete all existing data!",
                                                "Create Schema",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Warning);
                                                
                                            if (createSchemaResult == DialogResult.Yes)
                                            {
                                                ExecuteDatabaseScript(masterBuilder.ConnectionString, dbName);
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            
                            // Create new database
                            string createDbQuery = $"CREATE DATABASE [{dbName}]";
                            try
                            {
                            using (SqlCommand createCmd = new SqlCommand(createDbQuery, masterConn))
                            {
                                createCmd.ExecuteNonQuery();
                            }
                            
                            MessageBox.Show($"Database '{dbName}' created successfully!", 
                                "Database Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            isNewDatabase = true;
                            }
                            catch (SqlException sqlEx)
                            {
                                // Error 1801 is "Database already exists"
                                if (sqlEx.Number == 1801)
                                {
                                    DialogResult useExistingResult = MessageBox.Show(
                                        $"Database '{dbName}' already exists. Would you like to use it and apply the schema?",
                                        "Database Exists",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);
                                    
                                    if (useExistingResult == DialogResult.Yes)
                                    {
                                        isNewDatabase = false;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    throw; // Re-throw if it's a different SQL error
                                }
                            }
                                
                            // Create schema (tables, etc) if requested and script exists
                            if (chkCreateSchema.Checked)
                            {
                                // Check again if the script exists at the path
                                if (File.Exists(scriptPath))
                                {
                                    ExecuteDatabaseScript(masterBuilder.ConnectionString, dbName);
                                }
                                else
                                {
                                    MessageBox.Show(
                                        "Database created successfully but schema was not applied because SpaManagement.sql was not found.",
                                        "Schema Not Applied",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                            }
                            
                            // Update the database dropdown
                            PopulateDatabaseDropdown();
                            
                            // Make sure the selected database is in the dropdown list
                            if (!txtDatabase.Items.Contains(dbName))
                            {
                                txtDatabase.Items.Add(dbName);
                            }
                            txtDatabase.SelectedItem = dbName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating database: {ex.Message}", 
                            "Database Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string GetDatabaseSchemaScript()
        {
            // Instead of defining the schema in code, we'll run the external SQL script file
            return "-- Using external SpaManagement.sql script instead";
        }

        private void ExecuteDatabaseScript(string masterConnectionString, string dbName)
        {
            try
            {
                // Get the current directory and display it for debugging
                string currentDirectory = Directory.GetCurrentDirectory();
                string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                // Try to locate the script with absolute path
                string absolutePath = Path.Combine(projectDirectory, "SpaManagement.sql");
                string scriptPath = null;
                
                // Check if debug file exists to display paths (for troubleshooting)
                string debugInfo = $"Current Directory: {currentDirectory}\nProject Directory: {projectDirectory}\nLooking for file at: {absolutePath}";
                Console.WriteLine(debugInfo);
                
                // Try different paths in priority order
                string[] possiblePaths = new string[] 
                {
                    "SpaManagement.sql",  // Current directory
                    absolutePath, // Absolute path
                    Path.Combine(Directory.GetCurrentDirectory(), "SpaManagement.sql"), // Current directory explicit
                    Path.Combine(Application.StartupPath, "SpaManagement.sql"), // Application directory
                    Path.GetFullPath("SpaManagement.sql"), // Resolved path
                    Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SpaManagement.sql"), // Executable directory
                    Path.Combine(Environment.CurrentDirectory, "SpaManagement.sql"), // Environment current directory
                };
                
                foreach (string path in possiblePaths)
                {
                    // Display check for debugging
                    Console.WriteLine($"Checking path: {path}, Exists: {File.Exists(path)}");
                    
                    if (File.Exists(path))
                    {
                        scriptPath = path;
                        break;
                    }
                }
                
                // If script not found in any location, prompt user to locate it
                if (scriptPath == null)
                {
                    DialogResult result = MessageBox.Show(
                        "SpaManagement.sql file not found. Do you want to browse for it?",
                        "File Not Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (result == DialogResult.Yes)
                    {
                        using (OpenFileDialog dlg = new OpenFileDialog())
                        {
                            dlg.Title = "Select SpaManagement.sql file";
                            dlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                            
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                scriptPath = dlg.FileName;
                            }
                            else
                            {
                                return; // User canceled the dialog
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = "Could not find SpaManagement.sql file. Please ensure the file exists in the project directory.";
                        MessageBox.Show(errorMessage, "Script Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                
                // Read the script content
                string scriptContent = File.ReadAllText(scriptPath);
                
                // Preprocess the script to remove any database creation statements
                string processedScript = PreprocessSqlScript(scriptContent, dbName);
                
                // Simpler approach - execute straight SQL commands to create tables
                using (SqlConnection connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    
                    // First, set the database context
                    using (SqlCommand useDbCommand = new SqlCommand($"USE [{dbName}]", connection))
                    {
                        useDbCommand.ExecuteNonQuery();
                    }
                    
                    try
                    {
                        // Create tables one by one to avoid complex parsing
                        // First, create the table structure
                        CreateBasicTableStructure(connection, dbName);
                        
                        // Then add indexes, constraints, etc.
                        ExecuteNonTableCommands(processedScript, connection);
                        
                        MessageBox.Show($"Database schema created successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Error creating schema: {ex.Message}\nError Number: {ex.Number}", 
                            "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing database script: {ex.Message}", 
                    "Script Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateBasicTableStructure(SqlConnection connection, string dbName)
        {
            // Basic table creation scripts
            string[] tableCreationScripts = {
                @"CREATE TABLE [dbo].[tbCard](
	[CardId] [varchar](255) NOT NULL,
	[Status] [varchar](20) NULL,
	[LastUsed] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
))",
                @"CREATE TABLE [dbo].[tbConsumable](
	[ConsumableId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [text] NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[Category] [varchar](50) NULL,
	[StockQuantity] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[ImagePath] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ConsumableId] ASC
))",
                @"CREATE TABLE [dbo].[tbCustomer](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[CardId] [varchar](255) NOT NULL,
	[IssuedTime] [datetime] NULL,
	[ReleasedTime] [datetime] NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
))",
                @"CREATE TABLE [dbo].[tbInvoice](
	[InvoiceId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[InvoiceDate] [datetime] NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC
))",
                @"CREATE TABLE [dbo].[tbOrder](
	[OrderId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[OrderTime] [datetime] NULL,
	[Notes] [text] NULL,
	[TotalAmount] [decimal](10, 2) NULL,
	[Discount] [decimal](10, 2) NULL,
	[FinalAmount] [decimal](10, 2) NULL,
	[Status] [varchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC
))",
                @"CREATE TABLE [dbo].[tbOrderItem](
	[OrderItemId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ItemType] [varchar](20) NOT NULL,
	[ItemId] [int] NOT NULL,
	[Quantity] [int] NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[TotalPrice] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderItemId] ASC
))",
                @"CREATE TABLE [dbo].[tbPayment](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[PaymentDate] [datetime] NULL,
	[PaymentMethod] [varchar](50) NOT NULL,
	[TransactionReference] [varchar](100) NULL,
	[Status] [varchar](20) NULL,
	[UserId] [int] NOT NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
))",
                @"CREATE TABLE [dbo].[tbService](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[ServiceName] [varchar](100) NOT NULL,
	[Description] [text] NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[ImagePath] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
))",
                @"CREATE TABLE [dbo].[tbUser](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
))"
            };
            
            // Create each table, ignore if already exists
            foreach (string tableScript in tableCreationScripts)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(tableScript, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // Ignore if table already exists (error 2714)
                    if (ex.Number != 2714)
                    {
                        throw; // Re-throw other errors
                    }
                }
            }
        }

        private void ExecuteNonTableCommands(string script, SqlConnection connection)
        {
            // Process stored procedures, defaults, indexes, etc.
            
            // First create the essential stored procedures directly
            CreateEssentialStoredProcedures(connection);
            
            // Extract all stored procedure creation blocks
            Regex spRegex = new Regex(@"CREATE\s+PROCEDURE\s+\[dbo\]\.\[([^\]]+)\](?:\s|.)*?END(?:\s+GO)?", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Extract ALTER TABLE statements for defaults and constraints
            Regex alterTableRegex = new Regex(@"ALTER\s+TABLE\s+\[dbo\]\.\[([^\]]+)\](?:\s|.)*?GO", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Extract index creation statements
            Regex indexRegex = new Regex(@"CREATE\s+(?:UNIQUE\s+)?(?:CLUSTERED|NONCLUSTERED)\s+INDEX\s+\[([^\]]+)\](?:\s|.)*?GO", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Find all stored procedures and execute them
            foreach (Match match in spRegex.Matches(script))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(match.Value.Replace("GO", ""), connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number != 2714) // Ignore if object already exists
                    {
                        Console.WriteLine($"Error executing stored procedure: {ex.Message}");
                    }
                }
            }
            
            // Execute ALTER TABLE statements for defaults and constraints
            foreach (Match match in alterTableRegex.Matches(script))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(match.Value.Replace("GO", ""), connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error executing ALTER TABLE: {ex.Message}");
                }
            }
            
            // Execute index creation statements
            foreach (Match match in indexRegex.Matches(script))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(match.Value.Replace("GO", ""), connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error creating index: {ex.Message}");
                }
            }
            
            // Add default values for columns
            AddDefaultColumnValues(connection);
        }

        private void CreateEssentialStoredProcedures(SqlConnection connection)
        {
            string[] essentialProcedures = {
                @"CREATE PROCEDURE [dbo].[sp_RegisterCard]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if card already exists
    IF EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId)
    BEGIN
        RAISERROR('This card is already registered in the system.', 16, 1);
        RETURN;
    END
    
    -- Register new card
    INSERT INTO tbCard (CardId, Status, CreatedDate)
    VALUES (@CardId, 'Available', GETDATE());
    
    SELECT @CardId AS CardId, 'Available' AS Status;
END",
                @"CREATE PROCEDURE [dbo].[sp_IssueCardToCustomer]
    @CardId VARCHAR(255),
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Check if card exists and is available
    IF NOT EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId AND Status = 'Available')
    BEGIN
        ROLLBACK;
        RAISERROR('Card not available or not registered.', 16, 1);
        RETURN;
    END
    
    -- Update card status
    UPDATE tbCard
    SET Status = 'InUse',
        LastUsed = GETDATE()
    WHERE CardId = @CardId;
    
    -- Create customer record
    INSERT INTO tbCustomer (CardId, IssuedTime, Notes)
    VALUES (@CardId, GETDATE(), @Notes);
    
    DECLARE @CustomerId INT = SCOPE_IDENTITY();
    
    COMMIT TRANSACTION;
    
    SELECT @CustomerId AS CustomerId, @CardId AS CardId;
END",
                @"CREATE PROCEDURE [dbo].[sp_CheckCardStatus]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CardId,
        c.Status,
        c.LastUsed,
        cust.CustomerId,
        cust.IssuedTime,
        o.OrderId,
        o.TotalAmount,
        o.FinalAmount
    FROM tbCard c
    LEFT JOIN tbCustomer cust ON c.CardId = cust.CardId AND cust.ReleasedTime IS NULL
    LEFT JOIN tbOrder o ON cust.CustomerId = o.CustomerId AND o.Status = 'Active'
    WHERE c.CardId = @CardId;
END",
                @"CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Username VARCHAR(50),
    @Password VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbUser (Username, Password)
    VALUES (@Username, @Password);
    
    SELECT SCOPE_IDENTITY() AS UserId;
END",
                @"CREATE PROCEDURE [dbo].[sp_AddOrderItem]
    @OrderId INT,
    @ItemType VARCHAR(20),
    @ItemId INT,
    @Quantity INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UnitPrice DECIMAL(10,2);
    DECLARE @TotalPrice DECIMAL(10,2);
    
    -- Get the price based on item type
    IF @ItemType = 'Service'
    BEGIN
        SELECT @UnitPrice = Price FROM tbService WHERE ServiceId = @ItemId;
    END
    ELSE IF @ItemType = 'Consumable'
    BEGIN
        SELECT @UnitPrice = Price FROM tbConsumable WHERE ConsumableId = @ItemId;
        
        -- Update stock quantity
        UPDATE tbConsumable SET 
            StockQuantity = StockQuantity - @Quantity,
            ModifiedDate = GETDATE()
        WHERE ConsumableId = @ItemId;
    END
    
    SET @TotalPrice = @UnitPrice * @Quantity;
    
    -- Add item to order
    INSERT INTO tbOrderItem (OrderId, ItemType, ItemId, Quantity, UnitPrice, TotalPrice)
    VALUES (@OrderId, @ItemType, @ItemId, @Quantity, @UnitPrice, @TotalPrice);
    
    -- Update order totals
    UPDATE tbOrder
    SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
        FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
    WHERE OrderId = @OrderId;
    
    SELECT SCOPE_IDENTITY() AS OrderItemId;
END",
                @"CREATE PROCEDURE [dbo].[sp_ApplyOrderDiscount]
    @OrderId INT,
    @DiscountAmount DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE tbOrder
    SET Discount = @DiscountAmount,
        FinalAmount = TotalAmount - @DiscountAmount
    WHERE OrderId = @OrderId;
    
    SELECT OrderId, TotalAmount, Discount, FinalAmount
    FROM tbOrder
    WHERE OrderId = @OrderId;
END",
                @"CREATE PROCEDURE [dbo].[sp_CompleteOrder]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Update order status
    UPDATE tbOrder
    SET Status = 'Completed'
    WHERE OrderId = @OrderId;
    
    -- Create invoice
    INSERT INTO tbInvoice (OrderId, TotalAmount)
    SELECT OrderId, FinalAmount
    FROM tbOrder
    WHERE OrderId = @OrderId;
    
    DECLARE @InvoiceId INT = SCOPE_IDENTITY();
    
    COMMIT TRANSACTION;
    
    -- Return the invoice details
    SELECT 
        i.InvoiceId,
        i.OrderId,
        i.InvoiceDate,
        i.TotalAmount,
        c.CardId AS CustomerCardId
    FROM tbInvoice i
    JOIN tbOrder o ON i.OrderId = o.OrderId
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    WHERE i.InvoiceId = @InvoiceId;
END",
                @"CREATE PROCEDURE [dbo].[sp_CreateOrder]
    @CustomerId INT,
    @UserId INT,
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbOrder (CustomerId, UserId, Notes)
    VALUES (@CustomerId, @UserId, @Notes);
    
    SELECT SCOPE_IDENTITY() AS OrderId;
END",
                @"CREATE PROCEDURE [dbo].[sp_CreateService]
    @ServiceName VARCHAR(100),
    @Description TEXT = NULL,
    @Price DECIMAL(10,2),
    @ImagePath VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbService (ServiceName, Description, Price, ImagePath)
    VALUES (@ServiceName, @Description, @Price, @ImagePath);
    
    SELECT SCOPE_IDENTITY() AS ServiceId;
END",
                @"CREATE PROCEDURE [dbo].[sp_GenerateDailySalesReport]
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Daily order summary
    SELECT 
        COUNT(OrderId) AS TotalOrders,
        SUM(TotalAmount) AS GrossSales,
        SUM(Discount) AS TotalDiscounts,
        SUM(FinalAmount) AS NetSales
    FROM tbOrder
    WHERE CONVERT(DATE, OrderTime) = @Date
    AND Status = 'Completed';
    
    -- Payment method breakdown
    SELECT 
        PaymentMethod,
        COUNT(PaymentId) AS PaymentCount,
        SUM(i.TotalAmount) AS TotalAmount
    FROM tbPayment p
    JOIN tbInvoice i ON p.InvoiceId = i.InvoiceId
    WHERE CONVERT(DATE, p.PaymentDate) = @Date
    GROUP BY PaymentMethod
    ORDER BY TotalAmount DESC;
    
    -- Top selling services
    SELECT 
        s.ServiceId,
        s.ServiceName,
        COUNT(oi.OrderItemId) AS TimesSold,
        SUM(oi.Quantity) AS TotalQuantity,
        SUM(oi.TotalPrice) AS TotalSales
    FROM tbOrderItem oi
    JOIN tbService s ON oi.ItemId = s.ServiceId
    JOIN tbOrder o ON oi.OrderId = o.OrderId
    WHERE oi.ItemType = 'Service'
    AND CONVERT(DATE, o.OrderTime) = @Date
    AND o.Status = 'Completed'
    GROUP BY s.ServiceId, s.ServiceName
    ORDER BY TotalSales DESC;
    
    -- Top selling consumables
    SELECT 
        c.ConsumableId,
        c.Name,
        COUNT(oi.OrderItemId) AS TimesSold,
        SUM(oi.Quantity) AS TotalQuantity,
        SUM(oi.TotalPrice) AS TotalSales
    FROM tbOrderItem oi
    JOIN tbConsumable c ON oi.ItemId = c.ConsumableId
    JOIN tbOrder o ON oi.OrderId = o.OrderId
    WHERE oi.ItemType = 'Consumable'
    AND CONVERT(DATE, o.OrderTime) = @Date
    AND o.Status = 'Completed'
    GROUP BY c.ConsumableId, c.Name
    ORDER BY TotalSales DESC;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetActiveOrders]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.OrderId,
        o.CustomerId,
        c.CardId AS CustomerCardId,
        o.UserId,
        u.Username AS UserName,
        o.OrderTime,
        o.TotalAmount,
        o.Discount,
        o.FinalAmount,
        o.Status
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    JOIN tbUser u ON o.UserId = u.UserId
    WHERE o.Status = 'Active'
    ORDER BY o.OrderTime DESC;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetAvailableCards]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CardId,
        LastUsed,
        CreatedDate
    FROM tbCard
    WHERE Status = 'Available'
    ORDER BY LastUsed DESC;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetAvailableConsumables]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ConsumableId,
        Name,
        Description,
        Price,
        Category,
        StockQuantity,
        CreatedDate,
        ModifiedDate
    FROM tbConsumable
    WHERE StockQuantity > 0
    ORDER BY Category, Name;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetAvailableServices]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ServiceId,
        ServiceName,
        Description,
        Price,
        ImagePath,
        CreatedDate,
        ModifiedDate
    FROM tbService
    ORDER BY ServiceName;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetOrderDetails]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get order header
    SELECT 
        o.OrderId,
        o.CustomerId,
        c.CardId AS CustomerCardId,
        o.UserId,
        u.Username AS UserName,
        o.OrderTime,
        o.TotalAmount,
        o.Discount,
        o.FinalAmount,
        o.Status,
        o.Notes
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    JOIN tbUser u ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId;
    
    -- Get order items
    SELECT 
        oi.OrderItemId,
        oi.OrderId,
        oi.ItemType,
        oi.ItemId,
        CASE 
            WHEN oi.ItemType = 'Service' THEN s.ServiceName
            WHEN oi.ItemType = 'Consumable' THEN c.Name
            ELSE 'Unknown'
        END AS ItemName,
        oi.Quantity,
        oi.UnitPrice,
        oi.TotalPrice
    FROM tbOrderItem oi
    LEFT JOIN tbService s ON oi.ItemType = 'Service' AND oi.ItemId = s.ServiceId
    LEFT JOIN tbConsumable c ON oi.ItemType = 'Consumable' AND oi.ItemId = c.ConsumableId
    WHERE oi.OrderId = @OrderId
    ORDER BY oi.OrderItemId;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetServiceById]
    @ServiceId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ServiceId,
        ServiceName, 
        Description,
        Price,
        ImagePath,
        CreatedDate,
        ModifiedDate
    FROM tbService
    WHERE ServiceId = @ServiceId;
END",
                @"CREATE PROCEDURE [dbo].[sp_ProcessPayment]
    @InvoiceId INT,
    @PaymentMethod VARCHAR(50),
    @TransactionReference VARCHAR(100) = NULL,
    @UserId INT,
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @TotalAmount DECIMAL(10,2);
    DECLARE @OrderId INT;
    DECLARE @CustomerId INT;
    DECLARE @CardId VARCHAR(255);
    
    -- Get total amount and order ID from invoice
    SELECT @TotalAmount = TotalAmount, @OrderId = OrderId
    FROM tbInvoice
    WHERE InvoiceId = @InvoiceId;
    
    -- Get customer ID and card ID from order
    SELECT @CustomerId = o.CustomerId, @CardId = c.CardId
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    WHERE o.OrderId = @OrderId;
    
    -- Create payment record
    INSERT INTO tbPayment (InvoiceId, PaymentMethod, TransactionReference, UserId, Notes)
    VALUES (@InvoiceId, @PaymentMethod, @TransactionReference, @UserId, @Notes);
    
    DECLARE @PaymentId INT = SCOPE_IDENTITY();
    
    -- Update customer record
    UPDATE tbCustomer
    SET ReleasedTime = GETDATE()
    WHERE CustomerId = @CustomerId;
    
    -- Release the card
    UPDATE tbCard
    SET Status = 'Available'
    WHERE CardId = @CardId;
    
    COMMIT TRANSACTION;
    
    SELECT @PaymentId AS PaymentId;
END",
                @"CREATE PROCEDURE [dbo].[sp_RegisterCardBatch]
    @Prefix VARCHAR(10),
    @StartNumber INT,
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @i INT = 0;
    DECLARE @CardId VARCHAR(255);
    
    WHILE @i < @Count
    BEGIN
        SET @CardId = @Prefix + RIGHT('00000' + CAST(@StartNumber + @i AS VARCHAR(10)), 5);
        
        -- Only insert if it doesn't already exist
        IF NOT EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId)
        BEGIN
            INSERT INTO tbCard (CardId, Status, CreatedDate)
            VALUES (@CardId, 'Available', GETDATE());
        END
        
        SET @i = @i + 1;
    END
    
    SELECT 'Registered ' + CAST(@Count AS VARCHAR(10)) + ' cards with prefix ' + @Prefix AS Result;
END",
                @"CREATE PROCEDURE [dbo].[sp_SetCardAsDamaged]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if card is in use
    IF EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId AND Status = 'InUse')
    BEGIN
        RAISERROR('Cannot mark card as damaged while it is in use.', 16, 1);
        RETURN;
    END
    
    UPDATE tbCard
    SET Status = 'Damaged'
    WHERE CardId = @CardId;
    
    SELECT CardId, Status FROM tbCard WHERE CardId = @CardId;
END",
                @"CREATE PROCEDURE [dbo].[sp_UpdateOrderItemQuantity]
    @OrderItemId INT,
    @Quantity INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OrderId INT;
    DECLARE @UnitPrice DECIMAL(10,2);
    
    -- Get order ID and unit price
    SELECT @OrderId = OrderId, @UnitPrice = UnitPrice 
    FROM tbOrderItem 
    WHERE OrderItemId = @OrderItemId;
    
    -- Update the order item
    UPDATE tbOrderItem
    SET Quantity = @Quantity,
        TotalPrice = @UnitPrice * @Quantity
    WHERE OrderItemId = @OrderItemId;
    
    -- Update the order totals
    UPDATE tbOrder
    SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
        FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
    WHERE OrderId = @OrderId;
END",
                @"CREATE PROCEDURE [dbo].[sp_UpdateService]
    @ServiceId INT,
    @ServiceName VARCHAR(100),
    @Description TEXT = NULL,
    @Price DECIMAL(10,2),
    @ImagePath VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE tbService
    SET ServiceName = @ServiceName,
        Description = @Description,
        Price = @Price,
        ImagePath = @ImagePath,
        ModifiedDate = GETDATE()
    WHERE ServiceId = @ServiceId;
    
    SELECT @ServiceId AS ServiceId;
END",
                @"CREATE PROCEDURE [dbo].[sp_GetAllCards]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if the table exists
    IF OBJECT_ID('dbo.tbCard', 'U') IS NOT NULL
    BEGIN
        -- Return all cards
        SELECT 
            CardId,
            Status,
            LastUsed,
            CreatedDate
        FROM tbCard
        ORDER BY Status, LastUsed DESC;
    END
    ELSE
    BEGIN
        -- Return empty result if table doesn't exist
        SELECT 
            '' AS CardId,
            '' AS Status,
            NULL AS LastUsed,
            NULL AS CreatedDate
        WHERE 1 = 0;
    END
END"
            };
            
            foreach (string procScript in essentialProcedures)
            {
                try
                {
                    // First try to drop the procedure if it exists
                    string procName = "";
                    Match match = Regex.Match(procScript, @"CREATE\s+PROCEDURE\s+\[dbo\]\.\[([^\]]+)\]", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        procName = match.Groups[1].Value;
                        string dropSql = $"IF OBJECT_ID('dbo.{procName}', 'P') IS NOT NULL DROP PROCEDURE [dbo].[{procName}]";
                        
                        using (SqlCommand dropCmd = new SqlCommand(dropSql, connection))
                        {
                            dropCmd.ExecuteNonQuery();
                        }
                    }
                    
                    // Now create the procedure
                    using (SqlCommand cmd = new SqlCommand(procScript, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Created stored procedure: {procName}");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error creating stored procedure: {ex.Message}", "Stored Procedure Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Console.WriteLine($"Error creating stored procedure: {ex.Message}");
                }
            }
        }

        private void AddDefaultColumnValues(SqlConnection connection)
        {
            string[] defaultCommands = {
                "ALTER TABLE [dbo].[tbCard] ADD DEFAULT ('Available') FOR [Status]",
                "ALTER TABLE [dbo].[tbCard] ADD DEFAULT (getdate()) FOR [CreatedDate]",
                "ALTER TABLE [dbo].[tbConsumable] ADD DEFAULT ((0)) FOR [StockQuantity]",
                "ALTER TABLE [dbo].[tbConsumable] ADD DEFAULT (getdate()) FOR [CreatedDate]",
                "ALTER TABLE [dbo].[tbConsumable] ADD DEFAULT (getdate()) FOR [ModifiedDate]",
                "ALTER TABLE [dbo].[tbCustomer] ADD DEFAULT (getdate()) FOR [IssuedTime]",
                "ALTER TABLE [dbo].[tbInvoice] ADD DEFAULT (getdate()) FOR [InvoiceDate]",
                "ALTER TABLE [dbo].[tbOrder] ADD DEFAULT (getdate()) FOR [OrderTime]",
                "ALTER TABLE [dbo].[tbOrder] ADD DEFAULT ((0)) FOR [TotalAmount]",
                "ALTER TABLE [dbo].[tbOrder] ADD DEFAULT ((0)) FOR [Discount]",
                "ALTER TABLE [dbo].[tbOrder] ADD DEFAULT ((0)) FOR [FinalAmount]",
                "ALTER TABLE [dbo].[tbOrder] ADD DEFAULT ('Active') FOR [Status]",
                "ALTER TABLE [dbo].[tbOrderItem] ADD DEFAULT ((1)) FOR [Quantity]",
                "ALTER TABLE [dbo].[tbPayment] ADD DEFAULT (getdate()) FOR [PaymentDate]",
                "ALTER TABLE [dbo].[tbPayment] ADD DEFAULT ('Completed') FOR [Status]",
                "ALTER TABLE [dbo].[tbService] ADD DEFAULT (getdate()) FOR [CreatedDate]",
                "ALTER TABLE [dbo].[tbService] ADD DEFAULT (getdate()) FOR [ModifiedDate]",
                "ALTER TABLE [dbo].[tbUser] ADD DEFAULT (getdate()) FOR [CreatedDate]",
                "ALTER TABLE [dbo].[tbUser] ADD DEFAULT (getdate()) FOR [ModifiedDate]"
            };
            
            foreach (string command in defaultCommands)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(command, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // Ignore if constraint already exists
                    Console.WriteLine($"Error adding default value: {ex.Message}");
                }
            }
        }

        private string PreprocessSqlScript(string script, string dbName)
        {
            // Remove comments to make sure we don't miss anything inside comments
            string noComments = RemoveSqlComments(script);
            
            // Replace variable with database name
            string withDbName = noComments.Replace("$(DatabaseName)", dbName);
            
            // Remove any CREATE DATABASE statements and related configurations
            string[] linesToRemove = new string[]
            {
                "CREATE DATABASE",
                "ALTER DATABASE",
                "USE [master]",
                "USE master",
                "EXEC [SpaManagement]",
                "CONTAINMENT =",
                "ON PRIMARY",
                "LOG ON",
                "WITH CATALOG_COLLATION",
                "SET COMPATIBILITY_LEVEL",
                "SET ANSI_NULL_DEFAULT",
                "SET ANSI_NULLS",
                "SET ANSI_PADDING",
                "SET ANSI_WARNINGS",
                "SET ARITHABORT",
                "SET AUTO_CLOSE",
                "SET AUTO_SHRINK",
                "SET AUTO_UPDATE_STATISTICS",
                "SET CURSOR_CLOSE_ON_COMMIT",
                "SET CURSOR_DEFAULT",
                "SET CONCAT_NULL_YIELDS_NULL",
                "SET NUMERIC_ROUNDABORT",
                "SET RECURSIVE_TRIGGERS",
                "SET DISABLE_BROKER",
                "SET AUTO_UPDATE_STATISTICS_ASYNC",
                "SET DATE_CORRELATION_OPTIMIZATION",
                "SET TRUSTWORTHY",
                "SET ALLOW_SNAPSHOT_ISOLATION",
                "SET PARAMETERIZATION",
                "SET READ_COMMITTED_SNAPSHOT",
                "SET HONOR_BROKER_PRIORITY",
                "SET RECOVERY",
                "SET MULTI_USER",
                "SET PAGE_VERIFY",
                "SET DB_CHAINING",
                "SET FILESTREAM",
                "SET TARGET_RECOVERY_TIME",
                "SET DELAYED_DURABILITY",
                "SET ACCELERATED_DATABASE_RECOVERY",
                "SET QUERY_STORE",
                "SET READ_WRITE"
            };
            
            StringBuilder processedScript = new StringBuilder();
            using (StringReader reader = new StringReader(withDbName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    bool skipLine = false;
                    
                    // Skip lines with USE [master] or similar
                    if (line.Trim().StartsWith("USE [master]", StringComparison.OrdinalIgnoreCase) || 
                        line.Trim().StartsWith("USE master", StringComparison.OrdinalIgnoreCase))
                    {
                        skipLine = true;
                    }
                    else
                    {
                        foreach (string removeText in linesToRemove)
                        {
                            if (line.Trim().StartsWith(removeText, StringComparison.OrdinalIgnoreCase))
                            {
                                skipLine = true;
                                break;
                            }
                        }
                    }
                    
                    if (!skipLine)
                    {
                        processedScript.AppendLine(line);
                    }
                }
            }
            
            return processedScript.ToString();
        }

        private string RemoveSqlComments(string script)
        {
            // Remove /* ... */ style comments
            script = Regex.Replace(script, @"/\*.*?\*/", "", RegexOptions.Singleline);
            
            // Remove -- style comments
            script = Regex.Replace(script, @"--(.*?)$", "", RegexOptions.Multiline);
            
            return script;
        }

        private void chkIntegratedSecurity_CheckedChanged(object sender, Bunifu.UI.WinForms.BunifuCheckBox.CheckedChangedEventArgs e)
        {
            bool useSqlAuth = !chkIntegratedSecurity.Checked;
            txtUsername.Enabled = useSqlAuth;
            txtPassword.Enabled = useSqlAuth;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string server = txtServer.Text.Trim();
                string database = txtDatabase.Text.Trim();
                bool integratedSecurity = chkIntegratedSecurity.Checked;
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
                {
                    MessageBox.Show("Please enter valid server and database names.", 
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!integratedSecurity && (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)))
                {
                    MessageBox.Show("Please enter valid username and password for SQL Server authentication.", 
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Build the connection string
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = server;
                builder.InitialCatalog = database;
                
                if (integratedSecurity)
                {
                    builder.IntegratedSecurity = true;
                }
                else
                {
                    builder.UserID = username;
                    builder.Password = password;
                }

                // Always disable encryption for local development
                builder.TrustServerCertificate = true;

                ConnectionString = builder.ConnectionString;

                // Test the connection
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    
                    // Check for missing stored procedure and create if needed
                    CreateMissingStoredProcedures(connection);
                    
                    // Store the connection string for later use
                    SqlConnectionManager.ConnectionString = ConnectionString;
                    
                    // Check if users exist and prompt to create admin user if needed
                    // Instead of setting DialogResult inside PromptForUserCreation, 
                    // capture its return value and only set this form's DialogResult if authentication succeeds
                    bool authenticationSuccessful = PromptForUserCreation(connection);
                    
                    if (authenticationSuccessful)
                    {
                        // Only close with OK result if user authentication was successful
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    }
                    // Otherwise just return to the form - DialogResult remains None
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool PromptForUserCreation(SqlConnection connection)
        {
            try
            {
                // Check if any users exist
                string checkUsersQuery = @"
                    SELECT COUNT(*) 
                    FROM sysobjects 
                    WHERE name='tbUser' AND xtype='U'";
                
                bool tableExists = false;
                int userCount = 0;
                
                using (SqlCommand cmd = new SqlCommand(checkUsersQuery, connection))
                {
                    tableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                
                // If table exists, count users
                if (tableExists)
                {
                    string countUsersQuery = "SELECT COUNT(*) FROM tbUser";
                    using (SqlCommand cmd = new SqlCommand(countUsersQuery, connection))
                    {
                        userCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                
                // Handle based on whether users exist
                if (userCount > 0) 
                {
                    // Users exist - show login form
                    using (LoginForm loginForm = new LoginForm())
                    {
                        DialogResult loginResult = loginForm.ShowDialog();
                        
                        if (loginResult != DialogResult.OK || !loginForm.LoginSuccessful)
                        {
                            MessageBox.Show("Login is required to use the application.",
                                "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                            
                        return true;
                        }
                    }
                
                    // No users exist - prompt to create users
                    DialogResult result = MessageBox.Show(
                        "No users exist in the database. You must create a user to continue.",
                        "Create User", 
                        MessageBoxButtons.OKCancel, 
                        MessageBoxIcon.Information);
                        
                if (result != DialogResult.OK)
                {
                    // User canceled user creation
                    return false;
                }
                        
                        // Show the User form
                        using (User userForm = new User())
                        {
                    userForm.ShowDialog();
                    
                    // Check if a user was created by querying the database again
                    string recheckUsersQuery = "SELECT COUNT(*) FROM tbUser";
                    using (SqlConnection recheckConn = new SqlConnection(ConnectionString))
                    {
                        recheckConn.Open();
                        using (SqlCommand cmd = new SqlCommand(recheckUsersQuery, recheckConn))
                        {
                            userCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    
                    if (userCount == 0)
                    {
                        MessageBox.Show("User creation is required to use the application.",
                            "User Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    
                                // Automatically show login form after user creation
                                using (LoginForm loginForm = new LoginForm())
                                {
                                    DialogResult loginResult = loginForm.ShowDialog();
                                    
                                    if (loginResult != DialogResult.OK || !loginForm.LoginSuccessful)
                                    {
                            MessageBox.Show("Login is required to use the application.",
                                            "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                        
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for users: {ex.Message}",
                    "User Check Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CreateMissingStoredProcedures(SqlConnection connection)
        {
            try
            {
                // First check if the stored procedure exists
                string checkProcedureQuery = @"
                    SELECT COUNT(*) 
                    FROM sys.procedures 
                    WHERE name = 'sp_GetAllCards'";
                    
                bool procedureExists = false;
                
                using (SqlCommand cmd = new SqlCommand(checkProcedureQuery, connection))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    procedureExists = (count > 0);
                }
                
                // If procedure doesn't exist, create it
                if (!procedureExists)
                {
                    string createProcedureQuery = @"
CREATE PROCEDURE [dbo].[sp_GetAllCards]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if the table exists
    IF OBJECT_ID('dbo.tbCard', 'U') IS NOT NULL
    BEGIN
        -- Return all cards
        SELECT 
            CardId,
            Status,
            LastUsed,
            CreatedDate
        FROM tbCard
        ORDER BY Status, LastUsed DESC;
    END
    ELSE
    BEGIN
        -- Return empty result if table doesn't exist
        SELECT 
            '' AS CardId,
            '' AS Status,
            NULL AS LastUsed,
            NULL AS CreatedDate
        WHERE 1 = 0;
    END
END";
                    
                    using (SqlCommand cmd = new SqlCommand(createProcedureQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                
                // Add more missing stored procedures here if needed
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the connection process
                Console.WriteLine($"Error creating stored procedures: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionDialog));
            Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges borderEdges1 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges();
            Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges borderEdges2 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties1 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties2 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties3 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties4 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties5 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties6 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties7 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            Bunifu.UI.WinForms.BunifuTextBox.StateProperties stateProperties8 = new Bunifu.UI.WinForms.BunifuTextBox.StateProperties();
            btnConnect = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
            btnCreateDatabase = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
            bunifuLabel1 = new Bunifu.UI.WinForms.BunifuLabel();
            txtPassword = new Bunifu.UI.WinForms.BunifuTextBox();
            txtUsername = new Bunifu.UI.WinForms.BunifuTextBox();
            txtServer = new Bunifu.UI.WinForms.BunifuDropdown();
            txtDatabase = new Bunifu.UI.WinForms.BunifuDropdown();
            chkIntegratedSecurity = new Bunifu.UI.WinForms.BunifuCheckBox();
            lblUseWinAuth = new Bunifu.UI.WinForms.BunifuLabel();
            bunifuFormDrag1 = new Bunifu.UI.WinForms.BunifuFormDrag();
            bunifuFormControlBox1 = new Bunifu.UI.WinForms.BunifuFormControlBox();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.AllowAnimations = true;
            btnConnect.AllowMouseEffects = true;
            btnConnect.AllowToggling = false;
            btnConnect.AnimationSpeed = 200;
            btnConnect.AutoGenerateColors = false;
            btnConnect.AutoRoundBorders = false;
            btnConnect.AutoSizeLeftIcon = true;
            btnConnect.AutoSizeRightIcon = true;
            btnConnect.BackColor = Color.Transparent;
            btnConnect.BackColor1 = Color.FromArgb(51, 122, 183);
            btnConnect.BackgroundImage = (Image)resources.GetObject("btnConnect.BackgroundImage");
            btnConnect.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnConnect.ButtonText = "Connect";
            btnConnect.ButtonTextMarginLeft = 0;
            btnConnect.ColorContrastOnClick = 45;
            btnConnect.ColorContrastOnHover = 45;
            borderEdges1.BottomLeft = true;
            borderEdges1.BottomRight = true;
            borderEdges1.TopLeft = true;
            borderEdges1.TopRight = true;
            btnConnect.CustomizableEdges = borderEdges1;
            btnConnect.DialogResult = DialogResult.None;
            btnConnect.DisabledBorderColor = Color.FromArgb(191, 191, 191);
            btnConnect.DisabledFillColor = Color.Empty;
            btnConnect.DisabledForecolor = Color.Empty;
            btnConnect.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton.ButtonStates.Pressed;
            btnConnect.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConnect.ForeColor = Color.White;
            btnConnect.IconLeft = null;
            btnConnect.IconLeftAlign = ContentAlignment.MiddleLeft;
            btnConnect.IconLeftCursor = Cursors.Default;
            btnConnect.IconLeftPadding = new Padding(11, 3, 3, 3);
            btnConnect.IconMarginLeft = 11;
            btnConnect.IconPadding = 10;
            btnConnect.IconRight = null;
            btnConnect.IconRightAlign = ContentAlignment.MiddleRight;
            btnConnect.IconRightCursor = Cursors.Default;
            btnConnect.IconRightPadding = new Padding(3, 3, 7, 3);
            btnConnect.IconSize = 25;
            btnConnect.IdleBorderColor = Color.Empty;
            btnConnect.IdleBorderRadius = 0;
            btnConnect.IdleBorderThickness = 0;
            btnConnect.IdleFillColor = Color.Empty;
            btnConnect.IdleIconLeftImage = null;
            btnConnect.IdleIconRightImage = null;
            btnConnect.IndicateFocus = false;
            btnConnect.Location = new Point(215, 308);
            btnConnect.Name = "btnConnect";
            btnConnect.OnDisabledState.BorderColor = Color.FromArgb(191, 191, 191);
            btnConnect.OnDisabledState.BorderRadius = 15;
            btnConnect.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnConnect.OnDisabledState.BorderThickness = 1;
            btnConnect.OnDisabledState.FillColor = Color.FromArgb(204, 204, 204);
            btnConnect.OnDisabledState.ForeColor = Color.FromArgb(168, 160, 168);
            btnConnect.OnDisabledState.IconLeftImage = null;
            btnConnect.OnDisabledState.IconRightImage = null;
            btnConnect.onHoverState.BorderColor = Color.DarkGoldenrod;
            btnConnect.onHoverState.BorderRadius = 15;
            btnConnect.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnConnect.onHoverState.BorderThickness = 1;
            btnConnect.onHoverState.FillColor = Color.DarkGoldenrod;
            btnConnect.onHoverState.ForeColor = Color.White;
            btnConnect.onHoverState.IconLeftImage = null;
            btnConnect.onHoverState.IconRightImage = null;
            btnConnect.OnIdleState.BorderColor = Color.DarkGoldenrod;
            btnConnect.OnIdleState.BorderRadius = 15;
            btnConnect.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnConnect.OnIdleState.BorderThickness = 1;
            btnConnect.OnIdleState.FillColor = Color.DarkGoldenrod;
            btnConnect.OnIdleState.ForeColor = Color.White;
            btnConnect.OnIdleState.IconLeftImage = null;
            btnConnect.OnIdleState.IconRightImage = null;
            btnConnect.OnPressedState.BorderColor = Color.DarkGoldenrod;
            btnConnect.OnPressedState.BorderRadius = 15;
            btnConnect.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnConnect.OnPressedState.BorderThickness = 1;
            btnConnect.OnPressedState.FillColor = Color.DarkGoldenrod;
            btnConnect.OnPressedState.ForeColor = Color.White;
            btnConnect.OnPressedState.IconLeftImage = null;
            btnConnect.OnPressedState.IconRightImage = null;
            btnConnect.Size = new Size(120, 30);
            btnConnect.TabIndex = 12;
            btnConnect.TextAlign = ContentAlignment.MiddleCenter;
            btnConnect.TextAlignment = HorizontalAlignment.Center;
            btnConnect.TextMarginLeft = 0;
            btnConnect.TextPadding = new Padding(0);
            btnConnect.UseDefaultRadiusAndThickness = true;
            // 
            // btnCreateDatabase
            // 
            btnCreateDatabase.AllowAnimations = true;
            btnCreateDatabase.AllowMouseEffects = true;
            btnCreateDatabase.AllowToggling = false;
            btnCreateDatabase.AnimationSpeed = 200;
            btnCreateDatabase.AutoGenerateColors = false;
            btnCreateDatabase.AutoRoundBorders = false;
            btnCreateDatabase.AutoSizeLeftIcon = true;
            btnCreateDatabase.AutoSizeRightIcon = true;
            btnCreateDatabase.BackColor = Color.Transparent;
            btnCreateDatabase.BackColor1 = Color.FromArgb(51, 122, 183);
            btnCreateDatabase.BackgroundImage = (Image)resources.GetObject("btnCreateDatabase.BackgroundImage");
            btnCreateDatabase.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnCreateDatabase.ButtonText = "Create Database";
            btnCreateDatabase.ButtonTextMarginLeft = 0;
            btnCreateDatabase.ColorContrastOnClick = 45;
            btnCreateDatabase.ColorContrastOnHover = 45;
            borderEdges2.BottomLeft = true;
            borderEdges2.BottomRight = true;
            borderEdges2.TopLeft = true;
            borderEdges2.TopRight = true;
            btnCreateDatabase.CustomizableEdges = borderEdges2;
            btnCreateDatabase.DialogResult = DialogResult.None;
            btnCreateDatabase.DisabledBorderColor = Color.FromArgb(191, 191, 191);
            btnCreateDatabase.DisabledFillColor = Color.Empty;
            btnCreateDatabase.DisabledForecolor = Color.Empty;
            btnCreateDatabase.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton.ButtonStates.Pressed;
            btnCreateDatabase.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCreateDatabase.ForeColor = Color.White;
            btnCreateDatabase.IconLeft = null;
            btnCreateDatabase.IconLeftAlign = ContentAlignment.MiddleLeft;
            btnCreateDatabase.IconLeftCursor = Cursors.Default;
            btnCreateDatabase.IconLeftPadding = new Padding(11, 3, 3, 3);
            btnCreateDatabase.IconMarginLeft = 11;
            btnCreateDatabase.IconPadding = 10;
            btnCreateDatabase.IconRight = null;
            btnCreateDatabase.IconRightAlign = ContentAlignment.MiddleRight;
            btnCreateDatabase.IconRightCursor = Cursors.Default;
            btnCreateDatabase.IconRightPadding = new Padding(3, 3, 7, 3);
            btnCreateDatabase.IconSize = 25;
            btnCreateDatabase.IdleBorderColor = Color.Empty;
            btnCreateDatabase.IdleBorderRadius = 0;
            btnCreateDatabase.IdleBorderThickness = 0;
            btnCreateDatabase.IdleFillColor = Color.Empty;
            btnCreateDatabase.IdleIconLeftImage = null;
            btnCreateDatabase.IdleIconRightImage = null;
            btnCreateDatabase.IndicateFocus = false;
            btnCreateDatabase.Location = new Point(60, 308);
            btnCreateDatabase.Name = "btnCreateDatabase";
            btnCreateDatabase.OnDisabledState.BorderColor = Color.FromArgb(191, 191, 191);
            btnCreateDatabase.OnDisabledState.BorderRadius = 15;
            btnCreateDatabase.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnCreateDatabase.OnDisabledState.BorderThickness = 1;
            btnCreateDatabase.OnDisabledState.FillColor = Color.FromArgb(204, 204, 204);
            btnCreateDatabase.OnDisabledState.ForeColor = Color.FromArgb(168, 160, 168);
            btnCreateDatabase.OnDisabledState.IconLeftImage = null;
            btnCreateDatabase.OnDisabledState.IconRightImage = null;
            btnCreateDatabase.onHoverState.BorderColor = Color.DarkGoldenrod;
            btnCreateDatabase.onHoverState.BorderRadius = 15;
            btnCreateDatabase.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnCreateDatabase.onHoverState.BorderThickness = 1;
            btnCreateDatabase.onHoverState.FillColor = Color.DarkGoldenrod;
            btnCreateDatabase.onHoverState.ForeColor = Color.White;
            btnCreateDatabase.onHoverState.IconLeftImage = null;
            btnCreateDatabase.onHoverState.IconRightImage = null;
            btnCreateDatabase.OnIdleState.BorderColor = Color.DarkGoldenrod;
            btnCreateDatabase.OnIdleState.BorderRadius = 15;
            btnCreateDatabase.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnCreateDatabase.OnIdleState.BorderThickness = 1;
            btnCreateDatabase.OnIdleState.FillColor = Color.DarkGoldenrod;
            btnCreateDatabase.OnIdleState.ForeColor = Color.White;
            btnCreateDatabase.OnIdleState.IconLeftImage = null;
            btnCreateDatabase.OnIdleState.IconRightImage = null;
            btnCreateDatabase.OnPressedState.BorderColor = Color.DarkGoldenrod;
            btnCreateDatabase.OnPressedState.BorderRadius = 15;
            btnCreateDatabase.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            btnCreateDatabase.OnPressedState.BorderThickness = 1;
            btnCreateDatabase.OnPressedState.FillColor = Color.DarkGoldenrod;
            btnCreateDatabase.OnPressedState.ForeColor = Color.White;
            btnCreateDatabase.OnPressedState.IconLeftImage = null;
            btnCreateDatabase.OnPressedState.IconRightImage = null;
            btnCreateDatabase.Size = new Size(120, 30);
            btnCreateDatabase.TabIndex = 13;
            btnCreateDatabase.TextAlign = ContentAlignment.MiddleCenter;
            btnCreateDatabase.TextAlignment = HorizontalAlignment.Center;
            btnCreateDatabase.TextMarginLeft = 0;
            btnCreateDatabase.TextPadding = new Padding(0);
            btnCreateDatabase.UseDefaultRadiusAndThickness = true;
            // 
            // bunifuLabel1
            // 
            bunifuLabel1.AllowParentOverrides = false;
            bunifuLabel1.AutoEllipsis = false;
            bunifuLabel1.CursorType = Cursors.Default;
            bunifuLabel1.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            bunifuLabel1.Location = new Point(93, 37);
            bunifuLabel1.Name = "bunifuLabel1";
            bunifuLabel1.RightToLeft = RightToLeft.No;
            bunifuLabel1.Size = new Size(212, 21);
            bunifuLabel1.TabIndex = 14;
            bunifuLabel1.Text = "Database Connection Setting";
            bunifuLabel1.TextAlignment = ContentAlignment.TopLeft;
            bunifuLabel1.TextFormat = Bunifu.UI.WinForms.BunifuLabel.TextFormattingOptions.Default;
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
            txtPassword.BorderColorActive = Color.DarkGoldenrod;
            txtPassword.BorderColorDisabled = Color.FromArgb(204, 204, 204);
            txtPassword.BorderColorHover = Color.DarkGoldenrod;
            txtPassword.BorderColorIdle = Color.Silver;
            txtPassword.BorderRadius = 15;
            txtPassword.BorderThickness = 1;
            txtPassword.CharacterCase = Bunifu.UI.WinForms.BunifuTextBox.CharacterCases.Normal;
            txtPassword.CharacterCasing = CharacterCasing.Normal;
            txtPassword.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPassword.DefaultText = "";
            txtPassword.Enabled = false;
            txtPassword.FillColor = Color.White;
            txtPassword.HideSelection = true;
            txtPassword.IconLeft = null;
            txtPassword.IconLeftCursor = Cursors.IBeam;
            txtPassword.IconPadding = 10;
            txtPassword.IconRight = null;
            txtPassword.IconRightCursor = Cursors.IBeam;
            txtPassword.Location = new Point(60, 246);
            txtPassword.MaxLength = 32767;
            txtPassword.MinimumSize = new Size(1, 1);
            txtPassword.Modified = false;
            txtPassword.Multiline = false;
            txtPassword.Name = "txtPassword";
            stateProperties1.BorderColor = Color.DarkGoldenrod;
            stateProperties1.FillColor = Color.Empty;
            stateProperties1.ForeColor = Color.Empty;
            stateProperties1.PlaceholderForeColor = Color.Empty;
            txtPassword.OnActiveState = stateProperties1;
            stateProperties2.BorderColor = Color.FromArgb(204, 204, 204);
            stateProperties2.FillColor = Color.FromArgb(240, 240, 240);
            stateProperties2.ForeColor = Color.FromArgb(109, 109, 109);
            stateProperties2.PlaceholderForeColor = Color.DarkGray;
            txtPassword.OnDisabledState = stateProperties2;
            stateProperties3.BorderColor = Color.DarkGoldenrod;
            stateProperties3.FillColor = Color.Empty;
            stateProperties3.ForeColor = Color.Empty;
            stateProperties3.PlaceholderForeColor = Color.Empty;
            txtPassword.OnHoverState = stateProperties3;
            stateProperties4.BorderColor = Color.Silver;
            stateProperties4.FillColor = Color.White;
            stateProperties4.ForeColor = Color.Empty;
            stateProperties4.PlaceholderForeColor = Color.Empty;
            txtPassword.OnIdleState = stateProperties4;
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
            txtPassword.Size = new Size(275, 38);
            txtPassword.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtPassword.TabIndex = 16;
            txtPassword.TextAlign = HorizontalAlignment.Left;
            txtPassword.TextMarginBottom = 0;
            txtPassword.TextMarginLeft = 3;
            txtPassword.TextMarginTop = 1;
            txtPassword.TextPlaceholder = "Enter password";
            txtPassword.UseSystemPasswordChar = false;
            txtPassword.WordWrap = true;
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
            txtUsername.BorderColorActive = Color.DarkGoldenrod;
            txtUsername.BorderColorDisabled = Color.FromArgb(204, 204, 204);
            txtUsername.BorderColorHover = Color.DarkGoldenrod;
            txtUsername.BorderColorIdle = Color.Silver;
            txtUsername.BorderRadius = 15;
            txtUsername.BorderThickness = 1;
            txtUsername.CharacterCase = Bunifu.UI.WinForms.BunifuTextBox.CharacterCases.Normal;
            txtUsername.CharacterCasing = CharacterCasing.Normal;
            txtUsername.DefaultFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtUsername.DefaultText = "";
            txtUsername.Enabled = false;
            txtUsername.FillColor = Color.White;
            txtUsername.HideSelection = true;
            txtUsername.IconLeft = null;
            txtUsername.IconLeftCursor = Cursors.IBeam;
            txtUsername.IconPadding = 10;
            txtUsername.IconRight = null;
            txtUsername.IconRightCursor = Cursors.IBeam;
            txtUsername.Location = new Point(60, 202);
            txtUsername.MaxLength = 32767;
            txtUsername.MinimumSize = new Size(1, 1);
            txtUsername.Modified = false;
            txtUsername.Multiline = false;
            txtUsername.Name = "txtUsername";
            stateProperties5.BorderColor = Color.DarkGoldenrod;
            stateProperties5.FillColor = Color.Empty;
            stateProperties5.ForeColor = Color.Empty;
            stateProperties5.PlaceholderForeColor = Color.Empty;
            txtUsername.OnActiveState = stateProperties5;
            stateProperties6.BorderColor = Color.FromArgb(204, 204, 204);
            stateProperties6.FillColor = Color.FromArgb(240, 240, 240);
            stateProperties6.ForeColor = Color.FromArgb(109, 109, 109);
            stateProperties6.PlaceholderForeColor = Color.DarkGray;
            txtUsername.OnDisabledState = stateProperties6;
            stateProperties7.BorderColor = Color.DarkGoldenrod;
            stateProperties7.FillColor = Color.Empty;
            stateProperties7.ForeColor = Color.Empty;
            stateProperties7.PlaceholderForeColor = Color.Empty;
            txtUsername.OnHoverState = stateProperties7;
            stateProperties8.BorderColor = Color.Silver;
            stateProperties8.FillColor = Color.White;
            stateProperties8.ForeColor = Color.Empty;
            stateProperties8.PlaceholderForeColor = Color.Empty;
            txtUsername.OnIdleState = stateProperties8;
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
            txtUsername.Size = new Size(275, 38);
            txtUsername.Style = Bunifu.UI.WinForms.BunifuTextBox._Style.Bunifu;
            txtUsername.TabIndex = 15;
            txtUsername.TextAlign = HorizontalAlignment.Left;
            txtUsername.TextMarginBottom = 0;
            txtUsername.TextMarginLeft = 3;
            txtUsername.TextMarginTop = 1;
            txtUsername.TextPlaceholder = "Enter username";
            txtUsername.UseSystemPasswordChar = false;
            txtUsername.WordWrap = true;
            // 
            // txtServer
            // 
            txtServer.BackColor = Color.Transparent;
            txtServer.BackgroundColor = Color.White;
            txtServer.BorderColor = Color.Silver;
            txtServer.BorderRadius = 15;
            txtServer.Color = Color.Silver;
            txtServer.Direction = Bunifu.UI.WinForms.BunifuDropdown.Directions.Down;
            txtServer.DisabledBackColor = Color.FromArgb(240, 240, 240);
            txtServer.DisabledBorderColor = Color.FromArgb(204, 204, 204);
            txtServer.DisabledColor = Color.FromArgb(240, 240, 240);
            txtServer.DisabledForeColor = Color.FromArgb(109, 109, 109);
            txtServer.DisabledIndicatorColor = Color.DarkGray;
            txtServer.DrawMode = DrawMode.OwnerDrawFixed;
            txtServer.DropdownBorderThickness = Bunifu.UI.WinForms.BunifuDropdown.BorderThickness.Thin;
            txtServer.DropDownStyle = ComboBoxStyle.DropDownList;
            txtServer.DropDownTextAlign = Bunifu.UI.WinForms.BunifuDropdown.TextAlign.Left;
            txtServer.FillDropDown = true;
            txtServer.FillIndicator = false;
            txtServer.FlatStyle = FlatStyle.Flat;
            txtServer.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtServer.ForeColor = Color.Black;
            txtServer.FormattingEnabled = true;
            txtServer.Icon = null;
            txtServer.IndicatorAlignment = Bunifu.UI.WinForms.BunifuDropdown.Indicator.Right;
            txtServer.IndicatorColor = Color.DarkGray;
            txtServer.IndicatorLocation = Bunifu.UI.WinForms.BunifuDropdown.Indicator.Right;
            txtServer.IndicatorThickness = 2;
            txtServer.IsDropdownOpened = false;
            txtServer.ItemBackColor = Color.White;
            txtServer.ItemBorderColor = Color.White;
            txtServer.ItemForeColor = Color.Black;
            txtServer.ItemHeight = 26;
            txtServer.ItemHighLightColor = Color.DarkGoldenrod;
            txtServer.ItemHighLightForeColor = Color.White;
            txtServer.ItemTopMargin = 3;
            txtServer.Location = new Point(60, 82);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(275, 32);
            txtServer.TabIndex = 17;
            txtServer.Text = null;
            txtServer.TextAlignment = Bunifu.UI.WinForms.BunifuDropdown.TextAlign.Left;
            txtServer.TextLeftMargin = 5;
            // 
            // txtDatabase
            // 
            txtDatabase.BackColor = Color.Transparent;
            txtDatabase.BackgroundColor = Color.White;
            txtDatabase.BorderColor = Color.Silver;
            txtDatabase.BorderRadius = 15;
            txtDatabase.Color = Color.Silver;
            txtDatabase.Direction = Bunifu.UI.WinForms.BunifuDropdown.Directions.Down;
            txtDatabase.DisabledBackColor = Color.FromArgb(240, 240, 240);
            txtDatabase.DisabledBorderColor = Color.FromArgb(204, 204, 204);
            txtDatabase.DisabledColor = Color.FromArgb(240, 240, 240);
            txtDatabase.DisabledForeColor = Color.FromArgb(109, 109, 109);
            txtDatabase.DisabledIndicatorColor = Color.DarkGray;
            txtDatabase.DrawMode = DrawMode.OwnerDrawFixed;
            txtDatabase.DropdownBorderThickness = Bunifu.UI.WinForms.BunifuDropdown.BorderThickness.Thin;
            txtDatabase.DropDownStyle = ComboBoxStyle.DropDownList;
            txtDatabase.DropDownTextAlign = Bunifu.UI.WinForms.BunifuDropdown.TextAlign.Left;
            txtDatabase.Enabled = false;
            txtDatabase.FillDropDown = true;
            txtDatabase.FillIndicator = false;
            txtDatabase.FlatStyle = FlatStyle.Flat;
            txtDatabase.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtDatabase.ForeColor = Color.Black;
            txtDatabase.FormattingEnabled = true;
            txtDatabase.Icon = null;
            txtDatabase.IndicatorAlignment = Bunifu.UI.WinForms.BunifuDropdown.Indicator.Right;
            txtDatabase.IndicatorColor = Color.DarkGray;
            txtDatabase.IndicatorLocation = Bunifu.UI.WinForms.BunifuDropdown.Indicator.Right;
            txtDatabase.IndicatorThickness = 2;
            txtDatabase.IsDropdownOpened = false;
            txtDatabase.ItemBackColor = Color.White;
            txtDatabase.ItemBorderColor = Color.DarkGoldenrod;
            txtDatabase.ItemForeColor = Color.Black;
            txtDatabase.ItemHeight = 26;
            txtDatabase.ItemHighLightColor = Color.DarkGoldenrod;
            txtDatabase.ItemHighLightForeColor = Color.White;
            txtDatabase.ItemTopMargin = 3;
            txtDatabase.Location = new Point(60, 120);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(275, 32);
            txtDatabase.TabIndex = 18;
            txtDatabase.Text = null;
            txtDatabase.TextAlignment = Bunifu.UI.WinForms.BunifuDropdown.TextAlign.Left;
            txtDatabase.TextLeftMargin = 5;
            // 
            // chkIntegratedSecurity
            // 
            chkIntegratedSecurity.AllowBindingControlAnimation = true;
            chkIntegratedSecurity.AllowBindingControlColorChanges = false;
            chkIntegratedSecurity.AllowBindingControlLocation = true;
            chkIntegratedSecurity.AllowCheckBoxAnimation = false;
            chkIntegratedSecurity.AllowCheckmarkAnimation = true;
            chkIntegratedSecurity.AllowOnHoverStates = true;
            chkIntegratedSecurity.AutoCheck = true;
            chkIntegratedSecurity.BackColor = Color.Transparent;
            chkIntegratedSecurity.BackgroundImage = (Image)resources.GetObject("chkIntegratedSecurity.BackgroundImage");
            chkIntegratedSecurity.BackgroundImageLayout = ImageLayout.Zoom;
            chkIntegratedSecurity.BindingControl = lblUseWinAuth;
            chkIntegratedSecurity.BindingControlPosition = Bunifu.UI.WinForms.BunifuCheckBox.BindingControlPositions.Right;
            chkIntegratedSecurity.BorderRadius = 12;
            chkIntegratedSecurity.Checked = true;
            chkIntegratedSecurity.CheckState = Bunifu.UI.WinForms.BunifuCheckBox.CheckStates.Checked;
            chkIntegratedSecurity.CustomCheckmarkImage = null;
            chkIntegratedSecurity.Location = new Point(69, 164);
            chkIntegratedSecurity.MinimumSize = new Size(17, 17);
            chkIntegratedSecurity.Name = "chkIntegratedSecurity";
            chkIntegratedSecurity.OnCheck.BorderColor = Color.DarkGoldenrod;
            chkIntegratedSecurity.OnCheck.BorderRadius = 12;
            chkIntegratedSecurity.OnCheck.BorderThickness = 2;
            chkIntegratedSecurity.OnCheck.CheckBoxColor = Color.DarkGoldenrod;
            chkIntegratedSecurity.OnCheck.CheckmarkColor = Color.White;
            chkIntegratedSecurity.OnCheck.CheckmarkThickness = 2;
            chkIntegratedSecurity.OnDisable.BorderColor = Color.LightGray;
            chkIntegratedSecurity.OnDisable.BorderRadius = 12;
            chkIntegratedSecurity.OnDisable.BorderThickness = 2;
            chkIntegratedSecurity.OnDisable.CheckBoxColor = Color.Transparent;
            chkIntegratedSecurity.OnDisable.CheckmarkColor = Color.LightGray;
            chkIntegratedSecurity.OnDisable.CheckmarkThickness = 2;
            chkIntegratedSecurity.OnHoverChecked.BorderColor = Color.FromArgb(105, 181, 255);
            chkIntegratedSecurity.OnHoverChecked.BorderRadius = 12;
            chkIntegratedSecurity.OnHoverChecked.BorderThickness = 2;
            chkIntegratedSecurity.OnHoverChecked.CheckBoxColor = Color.FromArgb(105, 181, 255);
            chkIntegratedSecurity.OnHoverChecked.CheckmarkColor = Color.White;
            chkIntegratedSecurity.OnHoverChecked.CheckmarkThickness = 2;
            chkIntegratedSecurity.OnHoverUnchecked.BorderColor = Color.FromArgb(105, 181, 255);
            chkIntegratedSecurity.OnHoverUnchecked.BorderRadius = 12;
            chkIntegratedSecurity.OnHoverUnchecked.BorderThickness = 1;
            chkIntegratedSecurity.OnHoverUnchecked.CheckBoxColor = Color.Transparent;
            chkIntegratedSecurity.OnUncheck.BorderColor = Color.DarkGray;
            chkIntegratedSecurity.OnUncheck.BorderRadius = 12;
            chkIntegratedSecurity.OnUncheck.BorderThickness = 1;
            chkIntegratedSecurity.OnUncheck.CheckBoxColor = Color.Transparent;
            chkIntegratedSecurity.Size = new Size(21, 21);
            chkIntegratedSecurity.Style = Bunifu.UI.WinForms.BunifuCheckBox.CheckBoxStyles.Bunifu;
            chkIntegratedSecurity.TabIndex = 19;
            chkIntegratedSecurity.ThreeState = false;
            chkIntegratedSecurity.ToolTipText = "";
            chkIntegratedSecurity.CheckedChanged += chkIntegratedSecurity_CheckedChanged;
            // 
            // lblUseWinAuth
            // 
            lblUseWinAuth.AccessibleRole = AccessibleRole.CheckButton;
            lblUseWinAuth.AllowParentOverrides = false;
            lblUseWinAuth.AutoEllipsis = false;
            lblUseWinAuth.CursorType = Cursors.Default;
            lblUseWinAuth.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUseWinAuth.Location = new Point(93, 168);
            lblUseWinAuth.Name = "lblUseWinAuth";
            lblUseWinAuth.RightToLeft = RightToLeft.No;
            lblUseWinAuth.Size = new Size(154, 15);
            lblUseWinAuth.TabIndex = 20;
            lblUseWinAuth.Text = "Use Windows Authentication";
            lblUseWinAuth.TextAlignment = ContentAlignment.TopLeft;
            lblUseWinAuth.TextFormat = Bunifu.UI.WinForms.BunifuLabel.TextFormattingOptions.Default;
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
            bunifuFormControlBox1.Location = new Point(365, 0);
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
            bunifuFormControlBox1.TabIndex = 21;
            // 
            // ConnectionDialog
            // 
            ClientSize = new Size(425, 350);
            Controls.Add(bunifuFormControlBox1);
            Controls.Add(lblUseWinAuth);
            Controls.Add(chkIntegratedSecurity);
            Controls.Add(txtDatabase);
            Controls.Add(txtServer);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(bunifuLabel1);
            Controls.Add(btnCreateDatabase);
            Controls.Add(btnConnect);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectionDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Database Connection Settings";
            ResumeLayout(false);
            PerformLayout();
        }
    }
} 