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
    // Repository Pattern: Interface for the User data access
    public interface IUserRepository
    {
        List<UserModel> GetAll();
        DataTable Search(string searchText);
        UserModel GetById(int userId);
        int Insert(string username, string password);
        bool Update(int userId, string username, string password);
        bool Delete(int userId);
    }

    // Repository Pattern: Implementation for User data access
    public class UserRepository : IUserRepository
    {
        // Singleton Pattern: Uses the SqlConnectionManager singleton
        private readonly SqlConnectionManager _connectionManager;

        public UserRepository()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public List<UserModel> GetAll()
        {
            List<UserModel> users = new List<UserModel>();
            try
            {
                string query = "SELECT UserId, Username, Password, CreatedDate, ModifiedDate FROM tbUser";
                DataTable dataTable = _connectionManager.ExecuteQuery(query);

                foreach (DataRow row in dataTable.Rows)
                {
                    UserModel user = new UserModel
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        Password = row["Password"].ToString(),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        ModifiedDate = Convert.ToDateTime(row["ModifiedDate"])
                    };
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving users: " + ex.Message);
            }
            return users;
        }

        public DataTable Search(string searchText)
        {
            try
            {
                string query = "SELECT UserId, Username FROM tbUser WHERE Username LIKE @SearchText";
                SqlParameter parameter = new SqlParameter("@SearchText", "%" + searchText + "%");
                return _connectionManager.ExecuteQuery(query, parameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching users: " + ex.Message);
                return new DataTable();
            }
        }

        public UserModel GetById(int userId)
        {
            UserModel user = null;
            try
            {
                string query = "SELECT UserId, Username, Password, CreatedDate, ModifiedDate FROM tbUser WHERE UserId = @UserId";
                SqlParameter parameter = new SqlParameter("@UserId", userId);
                DataTable dataTable = _connectionManager.ExecuteQuery(query, parameter);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    user = new UserModel
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        Password = row["Password"].ToString(),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        ModifiedDate = Convert.ToDateTime(row["ModifiedDate"])
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user: " + ex.Message);
            }
            return user;
        }

        public int Insert(string username, string password)
        {
            try
            {
                // Security Enhancement: Consider hashing passwords before storage
                // password = HashPassword(password);

                string query = "EXEC sp_CreateUser @Username, @Password";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password)
                };

                // Execute the query and get the result
                object result = _connectionManager.ExecuteScalar(query, parameters);
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting user: " + ex.Message);
            }
            return -1;
        }

        public bool Update(int userId, string username, string password)
        {
            try
            {
                // Security Enhancement: Consider hashing passwords before storage
                // password = HashPassword(password);

                string query = "UPDATE tbUser SET Username = @Username, Password = @Password, ModifiedDate = GETDATE() WHERE UserId = @UserId";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password)
                };

                int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message);
                return false;
            }
        }

        public bool Delete(int userId)
        {
            try
            {
                string query = "DELETE FROM tbUser WHERE UserId = @UserId";
                SqlParameter parameter = new SqlParameter("@UserId", userId);

                int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameter);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting user: " + ex.Message);
                return false;
            }
        }

        // Optional: Password hashing method
        /*
        private string HashPassword(string password)
        {
            // Implementation of secure password hashing
            // For example, using BCrypt or other secure hashing algorithms
            return password; // Replace with actual hashing
        }
        */
    }

    // View component in MVC pattern
    public partial class User : Form
    {
        // Dependency Injection: Using the repository through an interface for better testability
        private readonly IUserRepository _repository;

        public User()
        {
            InitializeComponent();
            _repository = new UserRepository(); // Could be injected for testing
            LoadData();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Wire up button click events
            btnInsert.Click += BtnInsert_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvUser.CellClick += DgvUser_CellClick;
        }

        private void LoadData()
        {
            var users = _repository.GetAll();

            // Create DataTable for display
            DataTable dt = new DataTable();
            dt.Columns.Add("UserId", typeof(int));
            dt.Columns.Add("Username", typeof(string));

            foreach (var user in users)
            {
                dt.Rows.Add(user.UserId, user.Username);
            }

            dgvUser.DataSource = dt;

            // Hide password column for security
            if (dgvUser.Columns.Contains("Password"))
            {
                dgvUser.Columns["Password"].Visible = false;
            }
        }

        private void ClearFields()
        {
            txtID.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtID.ReadOnly = true;  // ID is auto-generated
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                dgvUser.DataSource = _repository.Search(searchText);
            }
            else
            {
                LoadData();
            }
        }

        private void DgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUser.Rows[e.RowIndex];
                int userId = Convert.ToInt32(row.Cells["UserId"].Value);

                UserModel user = _repository.GetById(userId);
                if (user != null)
                {
                    txtID.Text = user.UserId.ToString();
                    txtUsername.Text = user.Username;
                    txtPassword.Text = user.Password;
                }
            }
        }

        private void BtnInsert_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and password are required.");
                return;
            }

            int userId = _repository.Insert(username, password);
            if (userId > 0)
            {
                MessageBox.Show("User created successfully.");
                ClearFields();
                LoadData();
            }
            else
            {
                MessageBox.Show("Failed to create user.");
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Please select a user to update.");
                return;
            }

            int userId = Convert.ToInt32(txtID.Text);
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and password are required.");
                return;
            }

            bool success = _repository.Update(userId, username, password);
            if (success)
            {
                MessageBox.Show("User updated successfully.");
                ClearFields();
                LoadData();
            }
            else
            {
                MessageBox.Show("Failed to update user.");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Please select a user to delete.");
                return;
            }

            int userId = Convert.ToInt32(txtID.Text);

            // Confirm deletion
            DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                bool success = _repository.Delete(userId);
                if (success)
                {
                    MessageBox.Show("User deleted successfully.");
                    ClearFields();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Failed to delete user.");
                }
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtUsername.Focus();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
    }
}