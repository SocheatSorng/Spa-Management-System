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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.Load += Login_Load;
            btnLogin.Click += btnLogin_Click;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Set up any initial UI states
            txtPassword.PasswordChar = '•';

            // Optional: Check database connection on form load
            try
            {
                var test = SqlConnectionManager.Instance.ExecuteScalar("SELECT 1");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Database connection failed. Please check your connection settings.\n\nError: {ex.Message}",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Basic validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // First verify if the stored procedure exists
                object procExists = SqlConnectionManager.Instance.ExecuteScalar(
                    "SELECT COUNT(*) FROM sys.procedures WHERE name = 'sp_AuthenticateUser'");

                if (Convert.ToInt32(procExists) == 0)
                {
                    // Create the stored procedure if it doesn't exist
                    string createProc = @"
                        CREATE PROCEDURE sp_AuthenticateUser
                            @Username VARCHAR(50),
                            @Password VARCHAR(255)
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            
                            -- Check if user exists with provided credentials
                            SELECT UserId, Username
                            FROM tbUser
                            WHERE Username = @Username AND Password = @Password;
                        END";

                    SqlConnectionManager.Instance.ExecuteNonQuery(createProc);
                }

                // Create parameters for the stored procedure
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password)
                };

                // Call the stored procedure
                DataTable result = SqlConnectionManager.Instance.ExecuteQuery(
                    "EXEC sp_AuthenticateUser @Username, @Password",
                    parameters);

                // Check if login is successful
                if (result != null && result.Rows.Count > 0)
                {
                    int userId = Convert.ToInt32(result.Rows[0]["UserId"]);

                    MessageBox.Show($"Login successful! Welcome {result.Rows[0]["Username"]}",
                        "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Open the dashboard form
                    Dashboard dashboardForm = new Dashboard();
                    this.Hide();
                    dashboardForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    // Check if user exists to give more specific error
                    SqlParameter[] userParam = new SqlParameter[] { new SqlParameter("@Username", username) };
                    var userExists = SqlConnectionManager.Instance.ExecuteScalar(
                        "SELECT COUNT(*) FROM tbUser WHERE Username = @Username", userParam);

                    if (Convert.ToInt32(userExists) > 0)
                    {
                        MessageBox.Show("Invalid password for this username.",
                            "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Username does not exist in the system.",
                            "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"Database error ({sqlEx.Number}): {sqlEx.Message}";

                if (sqlEx.Number == 4060) // Cannot open database
                    errorMsg = "Cannot connect to the database. Please check your connection settings.";
                else if (sqlEx.Number == 208) // Invalid object name
                    errorMsg = "Database table not found. Please ensure the database is set up correctly.";

                MessageBox.Show(errorMsg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nPlease contact your system administrator.",
                    "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // If you want to add a button to show/hide password
        private void togglePasswordVisibility_Click(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = (txtPassword.PasswordChar == '\0') ? '•' : '\0';
            // Update button text/icon as needed
        }

        // Add this as a helper method if needed
        private bool DoesDatabaseHaveUsers()
        {
            try
            {
                var count = SqlConnectionManager.Instance.ExecuteScalar("SELECT COUNT(*) FROM tbUser");
                return Convert.ToInt32(count) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}