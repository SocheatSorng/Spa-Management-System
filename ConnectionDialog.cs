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
        
        // Declare variables as ComboBox directly
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.ComboBox txtServer;
        private System.Windows.Forms.CheckBox chkIntegratedSecurity;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.ComboBox txtDatabase;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnConnect;

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
        }

        private async void ConnectionDialog_Load(object sender, EventArgs e)
        {
            // Center the form on screen
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Add instructions label
            Label lblInstructions = new Label();
            lblInstructions.AutoSize = true;
            lblInstructions.Location = new System.Drawing.Point(20, 95);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new System.Drawing.Size(350, 17);
            lblInstructions.Text = "Configure your SQL Server connection.";
            lblInstructions.ForeColor = System.Drawing.Color.Navy;
            this.Controls.Add(lblInstructions);
            
            // Update the form text
            this.Text = "Database Connection Settings";

            // Initialize default server text but allow editing
            txtServer.Text = "localhost";
            txtServer.Enabled = true; // Always enable the server field
            btnConnect.Enabled = true; // Always enable the connect button

            // Add create database button
            Button btnCreateDatabase = new Button();
            btnCreateDatabase.Text = "Create Database";
            btnCreateDatabase.Location = new System.Drawing.Point(20, 245);
            btnCreateDatabase.Size = new System.Drawing.Size(120, 30);
            btnCreateDatabase.Click += btnCreateDatabase_Click;
            this.Controls.Add(btnCreateDatabase);

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
                createDbDialog.Size = new System.Drawing.Size(400, 330);
                createDbDialog.StartPosition = FormStartPosition.CenterParent;
                createDbDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                createDbDialog.MaximizeBox = false;
                createDbDialog.MinimizeBox = false;

                // Create controls
                Label lblDbName = new Label { Text = "Database Name:", Location = new System.Drawing.Point(20, 20), Width = 120 };
                TextBox txtDbName = new TextBox { Location = new System.Drawing.Point(150, 20), Width = 200, Text = "SpaManagement" };

                // For SQL Authentication
                CheckBox chkSqlAuth = new CheckBox { Text = "Use SQL Server Authentication", Location = new System.Drawing.Point(20, 50), Width = 220 };
                Label lblUser = new Label { Text = "Username:", Location = new System.Drawing.Point(40, 80), Width = 100, Enabled = false };
                TextBox txtUser = new TextBox { Location = new System.Drawing.Point(150, 80), Width = 200, Enabled = false };
                Label lblPass = new Label { Text = "Password:", Location = new System.Drawing.Point(40, 110), Width = 100, Enabled = false };
                TextBox txtPass = new TextBox { Location = new System.Drawing.Point(150, 110), Width = 200, PasswordChar = '•', Enabled = false };

                // Schema creation option - always checked but shows status
                CheckBox chkCreateSchema = new CheckBox { 
                    Text = "Create tables and schema after database creation", 
                    Location = new System.Drawing.Point(20, 160),
                    Width = 350, 
                    Checked = true,
                    Enabled = true
                };
                
                // Update script status message
                Label lblScriptStatus = new Label { 
                    Location = new System.Drawing.Point(20, 190),
                    Width = 350,
                    AutoSize = false,
                    Height = 40
                };
                
                // Check specifically for the path mentioned in the image
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
                    lblScriptStatus.ForeColor = System.Drawing.Color.Green;
                        break;
                }
                }
                
                if (!scriptExists)
                {
                    lblScriptStatus.Text = "SpaManagement.sql not found in any of the expected locations";
                    lblScriptStatus.ForeColor = System.Drawing.Color.Red;
                    
                    // Add a browse button when file is not found
                    Button btnBrowse = new Button {
                        Text = "Browse...",
                        Location = new System.Drawing.Point(275, 190),
                        Size = new System.Drawing.Size(75, 23)
                    };
                    
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
                                    lblScriptStatus.ForeColor = System.Drawing.Color.Green;
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
                Button btnCreate = new Button { Text = "Create", Location = new System.Drawing.Point(150, 230), Width = 100, DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(260, 230), Width = 100, DialogResult = DialogResult.Cancel };

                // Add controls to form
                createDbDialog.Controls.AddRange(new Control[] { 
                    lblDbName, txtDbName, chkSqlAuth, 
                    lblUser, txtUser, lblPass, txtPass,
                    chkCreateSchema, lblScriptStatus,
                    btnCreate, btnCancel 
                });

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

        private void chkIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
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
                    
                    // Check if users exist and prompt to create admin user if needed
                    PromptForUserCreation(connection);
                    
                    // No success message, just close the dialog with OK result
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void PromptForUserCreation(SqlConnection connection)
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
                
                // Store the connection string
                SqlConnectionManager.ConnectionString = ConnectionString;
                
                // Handle based on whether users exist
                if (userCount > 0) 
                {
                    // Users exist - force login by closing this dialog and showing login form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    
                    // Show login form
                    using (LoginForm loginForm = new LoginForm())
                    {
                        DialogResult loginResult = loginForm.ShowDialog();
                        
                        if (loginResult != DialogResult.OK || !loginForm.LoginSuccessful)
                        {
                            MessageBox.Show("Login is required to use the application. The application will now close.",
                                "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            
                            // Force application to exit if login fails
                            Application.Exit();
                        }
                    }
                }
                else if (userCount == 0 || isNewDatabase)
                {
                    // No users exist - prompt to create users
                    DialogResult result = MessageBox.Show(
                        "No users exist in the database. You must create a user to continue.",
                        "Create User", 
                        MessageBoxButtons.OKCancel, 
                        MessageBoxIcon.Information);
                        
                    if (result == DialogResult.OK)
                    {
                        // Close this dialog
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        
                        // Show the User form
                        using (User userForm = new User())
                        {
                            DialogResult userFormResult = userForm.ShowDialog();
                            
                            // Check if a user was created
                            if (userFormResult != DialogResult.OK)
                            {
                                MessageBox.Show("User creation is required to use the application. The application will now close.",
                                    "User Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Application.Exit();
                            }
                            else
                            {
                                // Automatically show login form after user creation
                                using (LoginForm loginForm = new LoginForm())
                                {
                                    DialogResult loginResult = loginForm.ShowDialog();
                                    
                                    if (loginResult != DialogResult.OK || !loginForm.LoginSuccessful)
                                    {
                                        MessageBox.Show("Login is required to use the application. The application will now close.",
                                            "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        Application.Exit();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // User canceled - exit application
                        MessageBox.Show("User creation is required to use the application. The application will now close.",
                            "User Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for users: {ex.Message}",
                    "User Check Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void InitializeComponent()
        {
            this.lblServer = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.ComboBox();
            this.chkIntegratedSecurity = new System.Windows.Forms.CheckBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.ComboBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(20, 25);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(93, 17);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Server name:";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(150, 22);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(250, 24);
            this.txtServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.txtServer.TabIndex = 1;
            // 
            // chkIntegratedSecurity
            // 
            this.chkIntegratedSecurity.AutoSize = true;
            this.chkIntegratedSecurity.Checked = true;
            this.chkIntegratedSecurity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIntegratedSecurity.Location = new System.Drawing.Point(150, 120);
            this.chkIntegratedSecurity.Name = "chkIntegratedSecurity";
            this.chkIntegratedSecurity.Size = new System.Drawing.Size(203, 21);
            this.chkIntegratedSecurity.TabIndex = 5;
            this.chkIntegratedSecurity.Text = "Use Windows Authentication";
            this.chkIntegratedSecurity.UseVisualStyleBackColor = true;
            this.chkIntegratedSecurity.CheckedChanged += new System.EventHandler(this.chkIntegratedSecurity_CheckedChanged);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(20, 65);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(75, 17);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database:";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(150, 62);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(250, 24);
            this.txtDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtDatabase.TabIndex = 3;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(20, 165);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(77, 17);
            this.lblUsername.TabIndex = 6;
            this.lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            this.txtUsername.Enabled = false;
            this.txtUsername.Location = new System.Drawing.Point(150, 162);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(250, 22);
            this.txtUsername.TabIndex = 7;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(20, 205);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(73, 17);
            this.lblPassword.TabIndex = 8;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(150, 202);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.Size = new System.Drawing.Size(250, 22);
            this.txtPassword.TabIndex = 9;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(152, 245);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(150, 30);
            this.btnConnect.TabIndex = 10;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // ConnectionDialog
            // 
            this.AcceptButton = this.btnConnect;
            this.ClientSize = new System.Drawing.Size(424, 301);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.chkIntegratedSecurity);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Connection Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
} 