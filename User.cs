using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public partial class User : Form
    {
        // Using Template Method Pattern for data access
        private readonly UserDataAccess _dataAccess;
        
        // Using Command Pattern for operations
        private readonly CommandInvoker _commandInvoker;
        
        // Using State Pattern for form states
        private FormStateContext _stateContext;

        // Additional variables for DataGridView operations
        private DataTable _usersTable;
        private bool _isLoading = false;

        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public User()
        {
            InitializeComponent();

            // Setup DataGridView general properties
            SetupDataGridView();

            // Add an event handler for grid layout
            dgvUser.DataBindingComplete += DgvUser_DataBindingComplete;

            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            // Skip database operations during design time
            if (!IsDesignMode)
            {
                try
                {
                    // Initialize data access and patterns
                    _dataAccess = new UserDataAccess();
                    _commandInvoker = new CommandInvoker();
                    _stateContext = new FormStateContext(
                        this,
                        EnableControls,
                        DisableControls,
                        ClearFields
                    );
                    
                    LoadData();
                    WireUpEvents();
                }
                catch (Exception ex)
                {
                    // Log error instead of showing message box
                    Console.WriteLine("Initialization error: " + ex.Message);
                }
            }
        }
        
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        // Configure the DataGridView general properties
        private void SetupDataGridView()
        {
            try
            {
                // Disable auto-sizing to prevent column width changes
                dgvUser.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Set selection mode to full row select
                dgvUser.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                
                // Disable multiple selection
                dgvUser.MultiSelect = false;
                
                // Prevent default Windows styling to enable our custom styling
                dgvUser.EnableHeadersVisualStyles = false;
                
                // Set column header style (orange/yellow like Consumable)
                dgvUser.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(255, 153, 0),  // Orange header
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.False
                };
                
                // Configure header height to match Consumable
                dgvUser.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvUser.ColumnHeadersHeight = 40;
                
                // Setup cell and row appearance
                dgvUser.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),  // Light orange selection
                    SelectionForeColor = Color.Black
                };
                
                // Row alternating color - similar to Consumable
                dgvUser.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 242, 204),  // Light yellow alternate rows
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),
                    SelectionForeColor = Color.Black
                };
                
                // Setting other appearance properties
                dgvUser.BackgroundColor = Color.White;
                dgvUser.BorderStyle = BorderStyle.None;
                dgvUser.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvUser.RowTemplate.Height = 30;  // Taller rows like Consumable
                dgvUser.RowHeadersVisible = false;
                dgvUser.GridColor = Color.FromArgb(255, 226, 173);  // Light orange grid lines
                
                // Other settings
                dgvUser.AllowUserToResizeColumns = false;
                dgvUser.AllowUserToResizeRows = false;
                dgvUser.ReadOnly = false;
                dgvUser.AutoGenerateColumns = false;
                dgvUser.StandardTab = true;
                
                // Configure columns
                ConfigureDataGridViewColumns();
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error setting up DataGridView: " + ex.Message);
            }
        }

        // Configure DataGridView columns
        private void ConfigureDataGridViewColumns()
        {
            if (IsDesignMode) return;

            try
            {
                // Clear existing columns to control the order
                dgvUser.Columns.Clear();
                
                // Disable AutoSizeColumnsMode to ensure our width settings are respected
                dgvUser.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Create ID column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "UserId",
                    HeaderText = "ID",
                    DataPropertyName = "UserId",
                    Width = 70,
                    MinimumWidth = 70,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvUser.Columns.Add(idColumn);

                // Create Username column - SECOND position
                DataGridViewTextBoxColumn usernameColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Username",
                    HeaderText = "Username",
                    DataPropertyName = "Username",
                    Width = 200,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvUser.Columns.Add(usernameColumn);

                // Create Password column - THIRD position
                DataGridViewTextBoxColumn passwordColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Password",
                    HeaderText = "Password",
                    DataPropertyName = "Password",
                    Width = 200,
                    ReadOnly = false,
                    Visible = false, // Hide password for security
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvUser.Columns.Add(passwordColumn);

                // Create Created Date column - FOURTH position
                DataGridViewTextBoxColumn createdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "CreatedDate",
                    HeaderText = "Created",
                    DataPropertyName = "CreatedDate",
                    Width = 150,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt"
                    }
                };
                dgvUser.Columns.Add(createdColumn);

                // Create Modified Date column - FIFTH position
                DataGridViewTextBoxColumn modifiedColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ModifiedDate",
                    HeaderText = "Modified",
                    DataPropertyName = "ModifiedDate",
                    Width = 150,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt"
                    }
                };
                dgvUser.Columns.Add(modifiedColumn);
                
                // Apply consistent header styling
                foreach (DataGridViewColumn col in dgvUser.Columns)
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    col.HeaderCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                
                // Disable column resizing by users
                dgvUser.AllowUserToResizeColumns = false;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        // Helper method to hide specific columns
        private void HideColumns()
        {
            // Check if columns exist first
            if (dgvUser.Columns.Count == 0) return;

            try
            {
                // Hide columns that shouldn't be visible for security
                string[] columnsToHide = { "Password" };
                
                foreach (string colName in columnsToHide)
                {
                    if (dgvUser.Columns.Contains(colName))
                    {
                        dgvUser.Columns[colName].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error hiding columns: " + ex.Message);
            }
        }

        // Wire up events (since not all may be set in designer)
        private void WireUpEvents()
        {
            if (IsDesignMode) return;      
            
            // Wire up button click events
            btnInsert.Click += BtnInsert_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            
            // Make the delete button visible only when a row is selected
            btnDelete.Visible = false;
            
            // Attach search event for proper method
            if (txtSearch != null)
            {
                // Remove any existing handlers first to avoid duplicates
                txtSearch.TextChanged -= TxtSearch_TextChanged;
                // Add our handler to the correct event
                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
            
            // Add events for DataGridView
            dgvUser.CellClick -= DgvUser_CellClick; // Remove first to avoid duplicates
            dgvUser.CellClick += DgvUser_CellClick;
            
            // Add events for direct editing in the grid
            dgvUser.CellEndEdit += DgvUser_CellEndEdit;
            dgvUser.UserAddedRow += DgvUser_UserAddedRow;
            dgvUser.UserDeletingRow += DgvUser_UserDeletingRow;
        }

        #region Form State Management
        
        private void EnableControls()
        {
            // Enable input controls
            txtUsername.Enabled = true;
            txtPassword.Enabled = true;
            
            // Set button states
            btnInsert.Enabled = true;
            btnDelete.Enabled = true;
            btnNew.Enabled = true;
            btnClear.Enabled = true;
        }
        
        private void DisableControls()
        {
            // Disable input controls
            txtUsername.Enabled = false;
            txtPassword.Enabled = false;
            
            // Set button states
            btnInsert.Enabled = false;
            btnDelete.Enabled = false;
            btnNew.Enabled = true;
            btnClear.Enabled = true;
        }
        
        #endregion

        private void LoadData()
        {
            if (IsDesignMode) return;

            try
            {
                // Store the currently selected user ID if any
                int? selectedUserId = null;
                if (dgvUser.CurrentRow != null && !dgvUser.CurrentRow.IsNewRow)
                {
                    var idCell = dgvUser.CurrentRow.Cells["UserId"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedUserId = Convert.ToInt32(idCell.Value);
                    }
                }

                // Set loading flag to prevent reentrant calls
                _isLoading = true;
                
                // Prevent auto-generation of columns 
                dgvUser.AutoGenerateColumns = false;
                
                // Disable AutoSizeColumnsMode to prevent auto-resizing
                dgvUser.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Temporarily suspend layout and events
                dgvUser.SuspendLayout();
                
                // Get data from data access layer
                _usersTable = _dataAccess.GetAll();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvUser.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
                dgvUser.DataSource = _usersTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_usersTable != null && _usersTable.DefaultView != null)
                    {
                        _usersTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvUser.Columns["UserId"] != null)
                    dgvUser.Columns["UserId"].Width = 37;
                    
                if (dgvUser.Columns["Username"] != null)
                    dgvUser.Columns["Username"].Width = 150;
                    
                if (dgvUser.Columns["CreatedDate"] != null)
                    dgvUser.Columns["CreatedDate"].Width = 150;
                    
                if (dgvUser.Columns["ModifiedDate"] != null)
                    dgvUser.Columns["ModifiedDate"].Width = 150;
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvUser.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvUser.Refresh();
                
                // Initially hide the delete button
                btnDelete.Visible = false;
                
                // Restore selection if possible
                if (selectedUserId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvUser.Rows)
                    {
                        var cell = row.Cells["UserId"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedUserId.Value)
                        {
                            row.Selected = true;
                            dgvUser.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error loading users: " + ex.Message);
            }
            finally
            {
                // Reset loading flag
                _isLoading = false;
            }
        }

        private void ClearFields()
        {
            txtID.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtID.ReadOnly = true;  // ID is auto-generated
            
            // Hide delete button when fields are cleared
            btnDelete.Visible = false;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchValue = txtSearch.Text.Trim().ToLower();
                
                // Check if we have data to filter
                if (_usersTable == null || _usersTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _usersTable.DefaultView.RowFilter = string.Empty;
                    return;
                }

                // Build filter with exact string matching for better accuracy
                string filter = string.Format(
                    "Convert(UserId, 'System.String') LIKE '%{0}%' OR Username LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _usersTable.DefaultView.RowFilter = filter;
                
                // Hide delete button when search results change
                btnDelete.Visible = false;
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_usersTable != null && _usersTable.DefaultView != null)
                {
                    _usersTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }

        private void DgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Check if this is the new row (last row with no data)
                bool isNewEmptyRow = e.RowIndex == dgvUser.Rows.Count - 1 && 
                                    dgvUser.Rows[e.RowIndex].IsNewRow;
                
                // Only show delete button if not clicking on the empty new row
                btnDelete.Visible = !isNewEmptyRow;
                
                // If we have a valid row index and it's not the new empty row
                if (e.RowIndex < dgvUser.Rows.Count && !isNewEmptyRow)
                {
                    DataGridViewRow row = dgvUser.Rows[e.RowIndex];
                    int userId = Convert.ToInt32(row.Cells["UserId"].Value);

                    try
                    {
                        UserModel user = _dataAccess.GetById(userId);
                        if (user != null)
                        {
                            txtID.Text = user.UserId.ToString();
                            txtUsername.Text = user.Username;
                            txtPassword.Text = user.Password;
                            
                            // Enable controls for editing
                            _stateContext.Edit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving user details: {ex.Message}");
                    }
                }
                else
                {
                    ClearFields(); // Clear fields if clicking empty row
                }
            }
        }

        private void DgvUser_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Apply alternating row colors
            foreach (DataGridViewRow row in dgvUser.Rows)
            {
                if (row.Index % 2 == 0)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 242, 204); // Light yellow
                }
            }
            
            // Ensure the correct column widths are applied
            if (dgvUser.Columns["UserId"] != null)
                dgvUser.Columns["UserId"].Width = 37;
                
            if (dgvUser.Columns["Username"] != null)
                dgvUser.Columns["Username"].Width = 150;
                
            if (dgvUser.Columns["CreatedDate"] != null)
                dgvUser.Columns["CreatedDate"].Width = 150;
                
            if (dgvUser.Columns["ModifiedDate"] != null)
                dgvUser.Columns["ModifiedDate"].Width = 150;
            
            // Ensure password column stays hidden
            HideColumns();
            
            // Disable column auto-sizing one more time to be sure
            dgvUser.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Ensure header styling is maintained
            dgvUser.EnableHeadersVisualStyles = false;
            dgvUser.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 153, 0);
            dgvUser.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }
        
        private void DgvUser_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["UserId"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int userId = Convert.ToInt32(idCell.Value);
                    string username = e.Row.Cells["Username"].Value?.ToString() ?? "this user";
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete user '{username}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _dataAccess.Delete(userId);
                        // No need to reload as the row is already being removed
                    }
                    else
                    {
                        // Cancel deletion if user says no
                        e.Cancel = true;
                    }
                }
                else
                {
                    // If there's no valid ID, just let the default behavior happen
                    // This is likely a new row that hasn't been saved yet
                    Console.WriteLine("No valid ID found for deleting row.");
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing a message box
                Console.WriteLine("Error deleting record: " + ex.Message);
                e.Cancel = true;
            }
        }

        private void DgvUser_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                // Auto-select the new row for editing
                e.Row.Selected = true;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine($"Error in UserAddedRow: {ex.Message}");
            }
        }

        private void DgvUser_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;
            
            try
            {
                // Don't immediately reload, which could cause reentrant issues
                // Instead, save the changes to the database without reloading
                
                DataGridViewRow row = dgvUser.Rows[e.RowIndex];
                
                // Check if this is a new row
                bool isNewRow = row.IsNewRow;
                
                if (!isNewRow)
                {
                    // Get values from the row, with proper null handling
                    int userId = 0;
                    if (row.Cells["UserId"].Value != null && row.Cells["UserId"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["UserId"].Value.ToString(), out userId);
                    }
                    
                    // Get username with null check
                    string username = "";
                    if (row.Cells["Username"].Value != null && row.Cells["Username"].Value != DBNull.Value)
                    {
                        username = row.Cells["Username"].Value.ToString();
                    }
                    
                    // Get password with null check - it's hidden but still in the data
                    string password = "";
                    if (row.Cells["Password"].Value != null && row.Cells["Password"].Value != DBNull.Value)
                    {
                        password = row.Cells["Password"].Value.ToString();
                    }
                    else if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("Password"))
                    {
                        password = drv.Row["Password"] == DBNull.Value ? "" : drv.Row["Password"].ToString();
                    }
                    
                    // If it's a valid record with username and password
                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        // Create user model
                        UserModel user = new UserModel
                        {
                            UserId = userId,
                            Username = username,
                            Password = password,
                            ModifiedDate = DateTime.Now
                        };
                        
                        if (userId > 0)
                        {
                            // Update existing record
                            _dataAccess.Update(user);
                        }
                        else
                        {
                            // Insert new record
                            _dataAccess.InsertAndGetId(user);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadData();
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error saving changes: " + ex.Message);
            }
        }

        private void BtnInsert_Click(object sender, EventArgs e)
        {
            // Using Command pattern for insert operation
            _commandInvoker.SetCommand(new InsertUserCommand(this, _dataAccess, GetUserFromForm));
            _commandInvoker.ExecuteCommand();
            
            // Refresh data and clear form
            LoadData();
            ClearFields();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Using Command pattern for update operation
            _commandInvoker.SetCommand(new UpdateUserCommand(this, _dataAccess, GetUserFromForm));
            _commandInvoker.ExecuteCommand();
            
            // Refresh data and clear form
            LoadData();
            ClearFields();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Using Command pattern for delete operation
            _commandInvoker.SetCommand(new DeleteUserCommand(this, _dataAccess, GetSelectedUserId));
            _commandInvoker.ExecuteCommand();
            
            // Refresh data and clear form
            LoadData();
            ClearFields();
            
            // Hide delete button after deletion
            btnDelete.Visible = false;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            // Set form to new state
            _stateContext.New();
            ClearFields();
            txtUsername.Focus();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        
        // Helper method to get user from form inputs
        private UserModel GetUserFromForm()
        {
            return new UserModel
            {
                UserId = string.IsNullOrEmpty(txtID.Text) ? 0 : Convert.ToInt32(txtID.Text),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim()
            };
        }
        
        // Helper method to get the selected user ID
        private int GetSelectedUserId()
        {
            return string.IsNullOrEmpty(txtID.Text) ? 0 : Convert.ToInt32(txtID.Text);
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}