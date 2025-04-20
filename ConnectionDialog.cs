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
            string scriptPath = Path.Combine(Application.StartupPath, "SpaManagement.sql");
            bool scriptExists = File.Exists(scriptPath);

            // Create a dialog for database creation
            using (Form createDbDialog = new Form())
            {
                createDbDialog.Text = "Create New Database";
                createDbDialog.Size = new System.Drawing.Size(400, 330); // Increased height from 280 to 330
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

                // Schema creation option
                CheckBox chkCreateSchema = new CheckBox { 
                    Text = "Create tables and schema after database creation", 
                    Location = new System.Drawing.Point(20, 160),
                    Width = 350, 
                    Checked = scriptExists,
                    Enabled = scriptExists
                };
                
                // Update the script file message to be more helpful and add a Browse button
                Label lblScriptNote = null;
                Button btnBrowseScript = null;

                if (!scriptExists)
                {
                    lblScriptNote = new Label { 
                        Text = "SpaManagement.sql not found in application directory.",
                        Location = new System.Drawing.Point(40, 190),
                        Width = 350,
                        ForeColor = System.Drawing.Color.Red
                    };
                    
                    // Add a second label with more detailed instructions
                    Label lblScriptInstructions = new Label {
                        Text = "Copy SpaManagement.sql to the application folder or browse to select it:",
                        Location = new System.Drawing.Point(40, 210),
                        Width = 350,
                        ForeColor = System.Drawing.Color.Black
                    };
                    
                    // Add a Browse button to locate the script file
                    btnBrowseScript = new Button {
                        Text = "Browse...",
                        Location = new System.Drawing.Point(150, 230),
                        Width = 100
                    };
                    
                    // Handle browse button click
                    btnBrowseScript.Click += (s, args) => {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            openFileDialog.Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*";
                            openFileDialog.Title = "Select SpaManagement.sql File";
                            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            
                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                string selectedFilePath = openFileDialog.FileName;
                                
                                try
                                {
                                    // Copy the file to the application directory
                                    string targetPath = Path.Combine(Application.StartupPath, "SpaManagement.sql");
                                    File.Copy(selectedFilePath, targetPath, true);
                                    
                                    MessageBox.Show(
                                        $"SpaManagement.sql copied to application directory successfully!\n\nPath: {targetPath}",
                                        "File Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    
                                    // Update the UI
                                    lblScriptNote.Text = "SpaManagement.sql found and ready to use!";
                                    lblScriptNote.ForeColor = System.Drawing.Color.Green;
                                    lblScriptInstructions.Visible = false;
                                    btnBrowseScript.Visible = false;
                                    chkCreateSchema.Enabled = true;
                                    chkCreateSchema.Checked = true;
                                    
                                    // Update our flag
                                    scriptExists = true;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(
                                        $"Error copying file: {ex.Message}\n\nPlease try copying the file manually to: {Application.StartupPath}",
                                        "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    };
                    
                    // Add the new controls to the form
                    createDbDialog.Controls.Add(lblScriptInstructions);
                    createDbDialog.Controls.Add(btnBrowseScript);
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
                Button btnCreate = new Button { Text = "Create", Location = new System.Drawing.Point(150, 270), Width = 100, DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(260, 270), Width = 100, DialogResult = DialogResult.Cancel };

                // Add controls to form
                createDbDialog.Controls.AddRange(new Control[] { 
                    lblDbName, txtDbName, chkSqlAuth, 
                    lblUser, txtUser, lblPass, txtPass,
                    chkCreateSchema, btnCreate, btnCancel 
                });
                
                if (lblScriptNote != null)
                {
                    createDbDialog.Controls.Add(lblScriptNote);
                }

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
                            using (SqlCommand createCmd = new SqlCommand(createDbQuery, masterConn))
                            {
                                createCmd.ExecuteNonQuery();
                            }
                            
                            MessageBox.Show($"Database '{dbName}' created successfully!", 
                                "Database Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            isNewDatabase = true;
                                
                            // Create schema (tables, etc) if requested and script exists
                            if (chkCreateSchema.Checked && scriptExists)
                            {
                                ExecuteDatabaseScript(masterBuilder.ConnectionString, dbName);
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

        private void ExecuteDatabaseScript(string connectionString, string dbName)
        {
            try
            {
                // Find the SpaManagement.sql file in the application directory
                string scriptPath = Path.Combine(Application.StartupPath, "SpaManagement.sql");
                
                // Check if the script file exists
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show($"Could not find the database script file at: {scriptPath}\n\nPlease make sure SpaManagement.sql is in the application directory.",
                        "Script File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Read the script content
                string scriptContent = File.ReadAllText(scriptPath);
                
                // First, properly handle database creation
                // We need to completely remove/skip any CREATE DATABASE statements and related commands
                StringBuilder processedScript = new StringBuilder();
                bool inCreateDatabaseBlock = false;
                
                // More thorough handling of database creation
                Regex createDbRegex = new Regex(@"CREATE\s+DATABASE\s+\[?\w+\]?|USE\s+\[?master\]?", RegexOptions.IgnoreCase);
                Regex useDbRegex = new Regex(@"USE\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
                
                // Split on GO to process each batch
                string[] batches = Regex.Split(scriptContent, @"^\s*GO\s*$", RegexOptions.Multiline);
                
                foreach (string batch in batches)
                {
                    string trimmedBatch = batch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedBatch))
                        continue;
                        
                    // Check if this batch contains CREATE DATABASE or USE [master]
                    if (createDbRegex.IsMatch(trimmedBatch))
                    {
                        // Skip database creation batches entirely
                        continue;
                    }
                    
                    // Replace any USE [SpaManagement] with USE [our_new_db_name]
                    string modifiedBatch = useDbRegex.Replace(trimmedBatch, match => {
                        string dbNameInScript = match.Groups[1].Value;
                        // Only modify if it's SpaManagement, leave other USE statements intact
                        if (string.Equals(dbNameInScript, "SpaManagement", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(dbNameInScript, "[SpaManagement]", StringComparison.OrdinalIgnoreCase))
                        {
                            return $"USE [{dbName}]";
                        }
                        return match.Value;
                    });
                    
                    // Add the batch to our processed script
                    processedScript.AppendLine(modifiedBatch);
                    processedScript.AppendLine("GO");
                }
                
                // Now use the processed script content
                string finalScript = processedScript.ToString();
                
                // Build connection string to the newly created database 
                SqlConnectionStringBuilder dbBuilder = new SqlConnectionStringBuilder(connectionString);
                dbBuilder.InitialCatalog = dbName;
                
                using (SqlConnection dbConnection = new SqlConnection(dbBuilder.ConnectionString))
                {
                    dbConnection.Open();
                    Cursor = Cursors.WaitCursor;
                    
                    // Show progress dialog
                    using (Form progressDialog = new Form())
                    {
                        progressDialog.Text = "Creating Database Schema";
                        progressDialog.Size = new Size(400, 100);
                        progressDialog.StartPosition = FormStartPosition.CenterScreen;
                        progressDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                        progressDialog.ControlBox = false;
                        
                        Label lblStatus = new Label
                        {
                            Text = "Creating database schema...",
                            AutoSize = true,
                            Location = new Point(20, 20)
                        };
                        
                        ProgressBar progressBar = new ProgressBar
                        {
                            Style = ProgressBarStyle.Marquee,
                            Location = new Point(20, 40),
                            Size = new Size(350, 20)
                        };
                        
                        progressDialog.Controls.Add(lblStatus);
                        progressDialog.Controls.Add(progressBar);
                        
                        // Show the dialog without blocking
                        progressDialog.Show(this);
                        Application.DoEvents();
                        
                        try
                        {
                            // Split by GO again for execution
                            batches = Regex.Split(finalScript, @"^\s*GO\s*$", RegexOptions.Multiline);
                            
                            int totalBatches = batches.Length;
                            int completedBatches = 0;
                            int errorCount = 0;
                            
                            foreach (string batch in batches)
                            {
                                string trimmedBatch = batch.Trim();
                                if (string.IsNullOrWhiteSpace(trimmedBatch))
                                    continue;
                                    
                                // Update status
                                lblStatus.Text = $"Executing batch {++completedBatches} of {totalBatches}...";
                                Application.DoEvents();
                                
                                try
                                {
                                    using (SqlCommand command = new SqlCommand(trimmedBatch, dbConnection))
                                    {
                                        command.CommandTimeout = 60; // Set longer timeout
                                        command.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    
                                    // Log the error but continue with other batches
                                    Console.WriteLine($"Error executing batch: {ex.Message}");
                                    Console.WriteLine($"Batch content: {trimmedBatch}");
                                    
                                    // Only show a maximum of 3 error messages to avoid dialog spam
                                    if (errorCount <= 3)
                                    {
                                        // Update dialog to show error
                                        lblStatus.Text = $"Error: {ex.Message}";
                                        lblStatus.ForeColor = System.Drawing.Color.Red;
                                        Application.DoEvents();
                                        System.Threading.Thread.Sleep(2000); // Pause briefly so user can see the error
                                        
                                        // Reset status for next batch
                                        lblStatus.Text = $"Continuing with batch {completedBatches + 1} of {totalBatches}...";
                                        lblStatus.ForeColor = System.Drawing.SystemColors.ControlText;
                                        Application.DoEvents();
                                    }
                                }
                            }
                            
                            // Close the progress dialog
                            progressDialog.Close();
                            
                            // Show final result
                            if (errorCount > 0)
                            {
                                MessageBox.Show($"Database schema created with {errorCount} errors. Some features may not work correctly.",
                                    "Schema Created with Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Database schema created successfully!",
                                    "Schema Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            progressDialog.Close();
                            MessageBox.Show($"Error executing script: {ex.Message}",
                                "Script Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            Cursor = Cursors.Default;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing database script: {ex.Message}",
                    "Script Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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