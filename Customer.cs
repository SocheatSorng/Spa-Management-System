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
using Newtonsoft.Json.Linq;

namespace Spa_Management_System
{
    public delegate void CardScannedEventHandler(object sender, string cardId);
    public partial class Customer : Form
    {


        // Repository Pattern for Customer data access
        private class CustomerRepository
        {
            private readonly SqlConnectionManager _connectionManager;
 
            public CustomerRepository()
            {
                // Get singleton instance of connection manager
                _connectionManager = SqlConnectionManager.Instance;
            }

            public DataTable GetAllCards()
            {
                try
                {
                    string query = "EXEC sp_GetAllCards";
                    return _connectionManager.ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving cards: " + ex.Message);
                    return new DataTable();
                }
            }
            public bool RegisterCard(string cardId)
            {
                try
                {
                    // Using the sp_RegisterCard stored procedure
                    string query = "EXEC sp_RegisterCard @CardId";
                    SqlParameter parameter = new SqlParameter("@CardId", cardId);

                    DataTable result = _connectionManager.ExecuteQuery(query, parameter);
                    return result.Rows.Count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error registering card: " + ex.Message);
                    return false;
                }
            }
            public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
            {
                return _connectionManager.ExecuteQuery(query, parameters);
            }
            public List<CustomerModel> GetAll()
            {
                List<CustomerModel> customers = new List<CustomerModel>();
                try
                {
                    string query = "SELECT CustomerId, CardId, IssuedTime, ReleasedTime, Notes FROM tbCustomer";
                    DataTable dataTable = _connectionManager.ExecuteQuery(query);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        CustomerModel customer = new CustomerModel
                        {
                            CustomerId = Convert.ToInt32(row["CustomerId"]),
                            CardId = row["CardId"].ToString(),
                            IssuedTime = Convert.ToDateTime(row["IssuedTime"]),
                            ReleasedTime = row["ReleasedTime"] == DBNull.Value ?
                                          (DateTime?)null : Convert.ToDateTime(row["ReleasedTime"]),
                            Notes = row["Notes"] == DBNull.Value ?
                                   string.Empty : row["Notes"].ToString()
                        };
                        customers.Add(customer);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving customers: " + ex.Message);
                }
                return customers;
            }

            public DataTable Search(string searchText)
            {
                try
                {
                    string query = "SELECT CustomerId, CardId, IssuedTime FROM tbCustomer WHERE CardId LIKE @SearchText";
                    SqlParameter parameter = new SqlParameter("@SearchText", "%" + searchText + "%");
                    return _connectionManager.ExecuteQuery(query, parameter);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching customers: " + ex.Message);
                    return new DataTable();
                }
            }
            public CustomerModel GetCustomerByCardId(string cardId)
            {
                CustomerModel customer = null;
                try
                {
                    string query = "EXEC sp_GetCustomerByCardId @CardId";
                    SqlParameter parameter = new SqlParameter("@CardId", cardId);
                    DataTable dataTable = _connectionManager.ExecuteQuery(query, parameter);

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow row = dataTable.Rows[0];
                        customer = new CustomerModel
                        {
                            CustomerId = Convert.ToInt32(row["CustomerId"]),
                            CardId = row["CardId"].ToString(),
                            IssuedTime = Convert.ToDateTime(row["IssuedTime"]),
                            ReleasedTime = row["ReleasedTime"] == DBNull.Value ?
                                         (DateTime?)null : Convert.ToDateTime(row["ReleasedTime"]),
                            Notes = row["Notes"] == DBNull.Value ?
                                  string.Empty : row["Notes"].ToString()
                        };
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving customer: " + ex.Message);
                }
                return customer;
            }

            public CustomerModel GetById(int customerId)
            {
                CustomerModel customer = null;
                try
                {
                    string query = "SELECT CustomerId, CardId, IssuedTime, ReleasedTime, Notes FROM tbCustomer WHERE CustomerId = @CustomerId";
                    SqlParameter parameter = new SqlParameter("@CustomerId", customerId);
                    DataTable dataTable = _connectionManager.ExecuteQuery(query, parameter);

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow row = dataTable.Rows[0];
                        customer = new CustomerModel
                        {
                            CustomerId = Convert.ToInt32(row["CustomerId"]),
                            CardId = row["CardId"].ToString(),
                            IssuedTime = Convert.ToDateTime(row["IssuedTime"]),
                            ReleasedTime = row["ReleasedTime"] == DBNull.Value ?
                                          (DateTime?)null : Convert.ToDateTime(row["ReleasedTime"]),
                            Notes = row["Notes"] == DBNull.Value ?
                                   string.Empty : row["Notes"].ToString()
                        };
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving customer: " + ex.Message);
                }
                return customer;
            }

            public DataTable GetAvailableCards()
            {
                try
                {
                    string query = "EXEC sp_GetAvailableCards";
                    return _connectionManager.ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting available cards: " + ex.Message);
                    return new DataTable();
                }
            }

            public int IssueCardToCustomer(string cardId, string notes)
            {
                try
                {
                    string query = "EXEC sp_IssueCardToCustomer @CardId, @Notes";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CardId", cardId),
                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes)
                    };

                    // Execute the query and get the result
                    DataTable result = _connectionManager.ExecuteQuery(query, parameters);
                    if (result.Rows.Count > 0)
                    {
                        return Convert.ToInt32(result.Rows[0]["CustomerId"]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error issuing card to customer: " + ex.Message);
                }
                return -1;
            }

            public bool UpdateCustomer(int customerId, string notes)
            {
                try
                {
                    string query = "UPDATE tbCustomer SET Notes = @Notes WHERE CustomerId = @CustomerId";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CustomerId", customerId),
                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes)
                    };

                    int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameters);
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating customer: " + ex.Message);
                    return false;
                }
            }

            public bool ReleaseCard(int customerId)
            {
                try
                {
                    // First get the CardId associated with this customer
                    string getCardQuery = "SELECT CardId FROM tbCustomer WHERE CustomerId = @CustomerId";
                    SqlParameter getCardParam = new SqlParameter("@CustomerId", customerId);
                    DataTable cardResult = _connectionManager.ExecuteQuery(getCardQuery, getCardParam);

                    if (cardResult.Rows.Count == 0)
                    {
                        MessageBox.Show("Customer not found.");
                        return false;
                    }

                    string cardId = cardResult.Rows[0]["CardId"].ToString();

                    // Begin transaction
                    using (SqlConnection connection = _connectionManager.CreateConnection())
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            // Update customer record to mark as released
                            string updateCustomerQuery = "UPDATE tbCustomer SET ReleasedTime = GETDATE() WHERE CustomerId = @CustomerId";
                            SqlCommand updateCustomerCmd = new SqlCommand(updateCustomerQuery, connection, transaction);
                            updateCustomerCmd.Parameters.AddWithValue("@CustomerId", customerId);
                            updateCustomerCmd.ExecuteNonQuery();

                            // Update card status to Available
                            string updateCardQuery = "UPDATE tbCard SET Status = 'Available' WHERE CardId = @CardId";
                            SqlCommand updateCardCmd = new SqlCommand(updateCardQuery, connection, transaction);
                            updateCardCmd.Parameters.AddWithValue("@CardId", cardId);
                            updateCardCmd.ExecuteNonQuery();

                            // Commit transaction
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Error releasing card: " + ex.Message);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error releasing card: " + ex.Message);
                    return false;
                }
            }
        }

        private CustomerRepository _repository;

        public Customer()
        {
            InitializeComponent();

            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            _repository = new CustomerRepository();
            LoadData();
            SetupEventHandlers();
        }
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public event CardScannedEventHandler CardScanned;

        private void SetupEventHandlers()
        {
            // Wire up button click events
            btnInsert.Click += BtnInsert_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvCustomer.CellClick += DgvCustomer_CellClick;
            // Add these new event handlers for card scanning
            txtCustomerID.KeyDown += TxtCustomerID_KeyDown;
            // Subscribe to our custom event
            CardScanned += Customer_CardScanned;
            // Add exit button handler to close only this form
            btnExitProgram.Click += (s, e) => this.Close();
        }
        private void TxtCustomerID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                string cardId = txtCustomerID.Text.Trim();
                if (!string.IsNullOrEmpty(cardId))
                {
                    // Trigger the card scanned event
                    CardScanned?.Invoke(this, cardId);
                }
            }
        }
        private void Customer_CardScanned(object sender, string cardId)
        {
            try
            {
                // Check if card exists and get its status
                string query = "EXEC sp_CheckCardStatus @CardId";
                SqlParameter parameter = new SqlParameter("@CardId", cardId);
                DataTable cardStatus = _repository.ExecuteQuery(query, parameter);

                if (cardStatus.Rows.Count == 0)
                {
                    // Ask if the user wants to register this new card
                    DialogResult result = MessageBox.Show(
                        $"Card {cardId} is not registered in the system. Would you like to register it now?",
                        "Register New Card",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Directly call the register method
                        bool success = _repository.RegisterCard(cardId);
                        if (success)
                        {
                            MessageBox.Show($"Card {cardId} registered successfully.");
                            ClearFields();
                            LoadData();
                        }
                    }
                    return;
                }

                // Get the status of the card
                string status = cardStatus.Rows[0]["Status"].ToString();

                if (status == "Available")
                {
                    // If available, ask to issue to new customer
                    DialogResult result = MessageBox.Show(
                        $"Card {cardId} is available. Do you want to issue it to a new customer?",
                        "Issue Card",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        string notes = txtNote.Text.Trim();
                        int customerId = _repository.IssueCardToCustomer(cardId, notes);

                        if (customerId > 0)
                        {
                            MessageBox.Show($"Card {cardId} issued to new customer successfully.");
                            ClearFields();
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to issue card.");
                        }
                    }
                }
                else if (status == "InUse")
                {
                    // If in use, load customer details
                    int customerId = Convert.ToInt32(cardStatus.Rows[0]["CustomerId"]);
                    CustomerModel customer = _repository.GetById(customerId);

                    if (customer != null)
                    {
                        txtID.Text = customer.CustomerId.ToString();
                        txtCustomerID.Text = customer.CardId;
                        txtIssuedTime.Text = customer.IssuedTime.ToString("yyyy-MM-dd HH:mm:ss");
                        txtNote.Text = customer.Notes;
                        MessageBox.Show($"Card {cardId} is currently in use by customer ID {customerId}.");
                    }
                }
                else if (status == "Damaged")
                {
                    MessageBox.Show($"Card {cardId} is marked as damaged and cannot be used.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing card: " + ex.Message);
            }
        }

        private void LoadData()
        {
            // Get all cards, not just customers with assigned cards
            DataTable allCards = _repository.GetAllCards();
            dgvCustomer.DataSource = allCards;

            // Rename the columns for better display
            if (dgvCustomer.Columns.Count > 0)
            {
                dgvCustomer.Columns["CardId"].HeaderText = "Card ID";
                dgvCustomer.Columns["Status"].HeaderText = "Status";
                dgvCustomer.Columns["LastUsed"].HeaderText = "Last Used";
                dgvCustomer.Columns["CreatedDate"].HeaderText = "Created Date";
            }
        }
        private void ClearFields()
        {
            txtID.Clear();
            txtCustomerID.Clear();
            txtIssuedTime.Clear();
            txtNote.Clear();
            txtID.ReadOnly = true;  // ID is auto-generated
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                dgvCustomer.DataSource = _repository.Search(searchText);
            }
            else
            {
                LoadData();
            }
        }

        private void DgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomer.Rows[e.RowIndex];

                // Now we're working with card data, not customer data
                string cardId = row.Cells["CardId"].Value.ToString();
                string status = row.Cells["Status"].Value.ToString();

                // Clear fields and show card info
                txtID.Clear(); // No customer ID for a card that's not assigned
                txtCustomerID.Text = cardId;
                txtIssuedTime.Clear();
                txtNote.Clear();

                // If the card is in use, try to get the associated customer
                if (status == "InUse")
                {
                    CustomerModel customer = _repository.GetCustomerByCardId(cardId);
                    if (customer != null)
                    {
                        txtID.Text = customer.CustomerId.ToString();
                        txtIssuedTime.Text = customer.IssuedTime.ToString("yyyy-MM-dd HH:mm:ss");
                        txtNote.Text = customer.Notes;
                    }
                }
            }
        }

        private void BtnInsert_Click(object sender, EventArgs e)
        {
            // This button registers a new card in the system
            string cardId = txtCustomerID.Text.Trim();

            if (string.IsNullOrEmpty(cardId))
            {
                MessageBox.Show("Please enter a card ID to register.");
                return;
            }

            try
            {
                // Directly register the card without first checking if it exists
                bool success = _repository.RegisterCard(cardId);
                if (success)
                {
                    MessageBox.Show($"Card {cardId} registered successfully.");
                    ClearFields();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Failed to register card.");
                }
            }
            catch (Exception ex)
            {
                // Handle specific error for existing card
                if (ex.Message.Contains("already registered"))
                {
                    MessageBox.Show("This card is already registered in the system.");
                }
                else
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Please select a customer to update.");
                return;
            }

            int customerId = Convert.ToInt32(txtID.Text);
            string notes = txtNote.Text.Trim();

            bool success = _repository.UpdateCustomer(customerId, notes);
            if (success)
            {
                MessageBox.Show("Customer updated successfully.");
                ClearFields();
                LoadData();
            }
            else
            {
                MessageBox.Show("Failed to update customer.");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Please select a customer to release card.");
                return;
            }

            int customerId = Convert.ToInt32(txtID.Text);

            // Confirm release
            DialogResult result = MessageBox.Show("Are you sure you want to release this card?",
                                                 "Confirm Release", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                bool success = _repository.ReleaseCard(customerId);
                if (success)
                {
                    MessageBox.Show("Card released successfully.");
                    ClearFields();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Failed to release card.");
                }
            }
        }
       

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtNote.Focus();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
    }
}