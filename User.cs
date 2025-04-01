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

        public User()
        {
            InitializeComponent();

            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

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
            SetupEventHandlers();
        }
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        #region Form State Management
        
        private void EnableControls()
        {
            // Enable input controls
            txtUsername.Enabled = true;
            txtPassword.Enabled = true;
            
            // Set button states
            btnInsert.Enabled = true;
            btnUpdate.Enabled = true;
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
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnNew.Enabled = true;
            btnClear.Enabled = true;
        }
        
        #endregion

        private void SetupEventHandlers()
        {
            // Wire up button click events
            btnInsert.Click += BtnInsert_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearchbun_TextChanged;
            dgvUser.CellClick += DgvUser_CellClick;
        }

        private void LoadData()
        {
            try
            {
                DataTable usersTable = _dataAccess.GetAll();

                // Create DataTable for display
                DataTable displayTable = new DataTable();
                displayTable.Columns.Add("UserId", typeof(int));
                displayTable.Columns.Add("Username", typeof(string));

                foreach (DataRow row in usersTable.Rows)
                {
                    displayTable.Rows.Add(
                        Convert.ToInt32(row["UserId"]),
                        row["Username"].ToString()
                    );
                }

                dgvUser.DataSource = displayTable;

                // Hide password column for security
                if (dgvUser.Columns.Contains("Password"))
                {
                    dgvUser.Columns["Password"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtID.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtSearch.Clear();
            txtID.ReadOnly = true;  // ID is auto-generated
        }

        private void TxtSearchbun_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    dgvUser.DataSource = _dataAccess.Search(searchText);
                }
                else
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvUser.Rows.Count - 1) // Exclude last empty row
            {
                DataGridViewRow row = dgvUser.Rows[e.RowIndex];
                int userId = Convert.ToInt32(row.Cells["UserId"].Value);

                try
                {
                    Spa_Management_System.UserModel user = _dataAccess.GetById(userId);
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
                    MessageBox.Show($"Error retrieving user details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                ClearFields(); // Clear fields if clicking empty row
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
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            // Set form to new state
            _stateContext.New();
            txtUsername.Focus();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        
        // Helper method to get user from form inputs
        private Spa_Management_System.UserModel GetUserFromForm()
        {
            return new Spa_Management_System.UserModel
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