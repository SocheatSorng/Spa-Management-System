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


        // Facade Pattern (Gang of Four) implementation
        // This class provides a simplified interface to the complex customer management subsystem
        private class CustomerFacade
        {
            // Subsystems
            private readonly CardSubsystem _cardSubsystem;
            private readonly CustomerSubsystem _customerSubsystem;

            public CustomerFacade()
            {
                _cardSubsystem = new CardSubsystem();
                _customerSubsystem = new CustomerSubsystem();
            }

            // Public facade methods that coordinate the subsystems

            // Card-related operations
            public DataTable GetAllCards()
            {
                return _cardSubsystem.GetAllCards();
            }

            public bool RegisterCard(string cardId)
            {
                return _cardSubsystem.RegisterCard(cardId);
            }

            public DataTable GetAvailableCards()
            {
                return _cardSubsystem.GetAvailableCards();
            }

            // Customer-related operations
            public List<CustomerModel> GetAllCustomers()
            {
                return _customerSubsystem.GetAll();
            }

            public CustomerModel GetCustomerByCardId(string cardId)
            {
                return _customerSubsystem.GetCustomerByCardId(cardId);
            }

            public CustomerModel GetCustomerById(int customerId)
            {
                return _customerSubsystem.GetById(customerId);
            }

            public DataTable SearchCustomers(string searchText)
            {
                return _customerSubsystem.Search(searchText);
            }

            // Direct database access for flexibility (used by existing code)
            public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
            {
                // We'll delegate this to either subsystem, since they both have _connectionManager
                return _cardSubsystem.ExecuteQuery(query, parameters);
            }

            // Combined operations (that require coordination between subsystems)
            public int IssueCardToCustomer(string cardId, string notes)
            {
                // Check if card exists and is available
                if (!_cardSubsystem.IsCardAvailable(cardId))
                {
                    MessageBox.Show("Card is not available or not registered.");
                    return -1;
                }

                // Issue card to customer
                int customerId = _customerSubsystem.IssueCard(cardId, notes);

                // If successful, update card status
                if (customerId > 0)
                {
                    _cardSubsystem.SetCardStatus(cardId, "InUse");
                }

                return customerId;
            }

            public bool ReleaseCustomerCard(int customerId)
            {
                string cardId = _customerSubsystem.GetCardIdByCustomerId(customerId);
                if (string.IsNullOrEmpty(cardId))
                {
                    return false;
                }

                bool released = _customerSubsystem.ReleaseCard(customerId);
                if (released)
                {
                    _cardSubsystem.SetCardStatus(cardId, "Available");
                }

                return released;
            }

            public bool UpdateCustomerNotes(int customerId, string notes)
            {
                return _customerSubsystem.UpdateNotes(customerId, notes);
            }
        }

        // Subsystem classes
        private class CardSubsystem
        {
            private readonly SqlConnectionManager _connectionManager;

            public CardSubsystem()
            {
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

            public bool IsCardAvailable(string cardId)
            {
                try
                {
                    string query = "SELECT Status FROM tbCard WHERE CardId = @CardId";
                    SqlParameter parameter = new SqlParameter("@CardId", cardId);

                    DataTable result = _connectionManager.ExecuteQuery(query, parameter);
                    if (result.Rows.Count > 0)
                    {
                        return result.Rows[0]["Status"].ToString() == "Available";
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error checking card status: " + ex.Message);
                    return false;
                }
            }

            public bool SetCardStatus(string cardId, string status)
            {
                try
                {
                    string query = "UPDATE tbCard SET Status = @Status, LastUsed = @LastUsed WHERE CardId = @CardId";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Status", status),
                        new SqlParameter("@LastUsed", DateTime.Now),
                        new SqlParameter("@CardId", cardId)
                    };

                    int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameters);
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating card status: " + ex.Message);
                    return false;
                }
            }

            // Direct database access for facade delegation
            public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
            {
                return _connectionManager.ExecuteQuery(query, parameters);
            }
        }

        private class CustomerSubsystem
        {
            private readonly SqlConnectionManager _connectionManager;

            public CustomerSubsystem()
            {
                _connectionManager = SqlConnectionManager.Instance;
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
                        customers.Add(CreateCustomerFromRow(row));
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
                try
                {
                    string query = "SELECT CustomerId, CardId, IssuedTime, ReleasedTime, Notes FROM tbCustomer WHERE CardId = @CardId AND ReleasedTime IS NULL";
                    SqlParameter parameter = new SqlParameter("@CardId", cardId);
                    DataTable dataTable = _connectionManager.ExecuteQuery(query, parameter);

                    if (dataTable.Rows.Count > 0)
                    {
                        return CreateCustomerFromRow(dataTable.Rows[0]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving customer: " + ex.Message);
                }
                return null;
            }

            public CustomerModel GetById(int customerId)
            {
                try
                {
                    string query = "SELECT CustomerId, CardId, IssuedTime, ReleasedTime, Notes FROM tbCustomer WHERE CustomerId = @CustomerId";
                    SqlParameter parameter = new SqlParameter("@CustomerId", customerId);
                    DataTable dataTable = _connectionManager.ExecuteQuery(query, parameter);

                    if (dataTable.Rows.Count > 0)
                    {
                        return CreateCustomerFromRow(dataTable.Rows[0]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving customer: " + ex.Message);
                }
                return null;
            }

            public int IssueCard(string cardId, string notes)
            {
                try
                {
                    string query = "INSERT INTO tbCustomer (CardId, IssuedTime, Notes) VALUES (@CardId, @IssuedTime, @Notes); SELECT SCOPE_IDENTITY()";
                    SqlParameter[] parameters = {
                        new SqlParameter("@CardId", cardId),
                        new SqlParameter("@IssuedTime", DateTime.Now),
                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes)
                    };

                    object result = _connectionManager.ExecuteScalar(query, parameters);
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error issuing card to customer: " + ex.Message);
                }
                return -1;
            }

            public bool ReleaseCard(int customerId)
            {
                try
                {
                    string query = "UPDATE tbCustomer SET ReleasedTime = @ReleasedTime WHERE CustomerId = @CustomerId";
                    SqlParameter[] parameters = {
                        new SqlParameter("@ReleasedTime", DateTime.Now),
                        new SqlParameter("@CustomerId", customerId)
                    };

                    int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameters);
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error releasing customer card: " + ex.Message);
                    return false;
                }
            }

            public bool UpdateNotes(int customerId, string notes)
            {
                try
                {
                    string query = "UPDATE tbCustomer SET Notes = @Notes WHERE CustomerId = @CustomerId";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes),
                        new SqlParameter("@CustomerId", customerId)
                    };

                    int rowsAffected = _connectionManager.ExecuteNonQuery(query, parameters);
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating customer notes: " + ex.Message);
                    return false;
                }
            }

            public string GetCardIdByCustomerId(int customerId)
            {
                try
                {
                    string query = "SELECT CardId FROM tbCustomer WHERE CustomerId = @CustomerId";
                    SqlParameter parameter = new SqlParameter("@CustomerId", customerId);

                    object result = _connectionManager.ExecuteScalar(query, parameter);
                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting customer's card: " + ex.Message);
                }
                return null;
            }

            private CustomerModel CreateCustomerFromRow(DataRow row)
            {
                return new CustomerModel
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

        private CustomerFacade _facade;

        public Customer()
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

            _facade = new CustomerFacade();
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
                DataTable cardStatus = _facade.ExecuteQuery(query, parameter);

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
                        bool success = _facade.RegisterCard(cardId);
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
                        int customerId = _facade.IssueCardToCustomer(cardId, notes);

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
                    CustomerModel customer = _facade.GetCustomerById(customerId);

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
            DataTable allCards = _facade.GetAllCards();
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
                dgvCustomer.DataSource = _facade.SearchCustomers(searchText);
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
                    CustomerModel customer = _facade.GetCustomerByCardId(cardId);
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
                bool success = _facade.RegisterCard(cardId);
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

            bool success = _facade.UpdateCustomerNotes(customerId, notes);
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

        // private void BtnDelete_Click(object sender, EventArgs e)
        // {
        //     string cardId = txtCustomerID.Text.Trim();

        //     if (string.IsNullOrEmpty(cardId))
        //     {
        //         MessageBox.Show("Please select a card to delete.");
        //         return;
        //     }

        //     try
        //     {
        //         // Check if the card is in use (assigned to a customer)
        //         string query = "EXEC sp_CheckCardStatus @CardId";
        //         SqlParameter parameter = new SqlParameter("@CardId", cardId);
        //         DataTable cardStatus = _facade.ExecuteQuery(query, parameter);

        //         if (cardStatus.Rows.Count > 0 && cardStatus.Rows[0]["Status"].ToString() == "InUse")
        //         {
        //             MessageBox.Show("Cannot delete card that is currently assigned to a customer. Please release the card first.",
        //                            "Card In Use", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //             return;
        //         }

        //         // Confirm deletion
        //         DialogResult result = MessageBox.Show("Are you sure you want to delete this card?",
        //                                             "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        //         if (result == DialogResult.Yes)
        //         {
        //             string deleteQuery = "DELETE FROM tbCard WHERE CardId = @CardId";
        //             SqlParameter deleteParam = new SqlParameter("@CardId", cardId);

        //             int rowsAffected = _facade.ExecuteQuery(deleteQuery, deleteParam).Rows.Count;
        //             if (rowsAffected > 0)
        //             {
        //                 MessageBox.Show("Card deleted successfully.");
        //                 ClearFields();
        //                 LoadData();
        //             }
        //             else
        //             {
        //                 MessageBox.Show("Failed to delete card.");
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         MessageBox.Show("Error deleting card: " + ex.Message);
        //     }
        // }


        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtNote.Focus();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnSetDamaged_Click(object sender, EventArgs e)
        {
            try
            {
                string cardId = txtCustomerID.Text.Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    MessageBox.Show("Please select a card or enter a card ID.");
                    return;
                }

                // Check if the card is in use (assigned to a customer)
                string query = "EXEC sp_CheckCardStatus @CardId";
                SqlParameter parameter = new SqlParameter("@CardId", cardId);
                DataTable cardStatus = _facade.ExecuteQuery(query, parameter);

                if (cardStatus.Rows.Count == 0)
                {
                    MessageBox.Show("Card not found in the system.");
                    return;
                }

                if (cardStatus.Rows.Count > 0 && cardStatus.Rows[0]["Status"].ToString() == "InUse")
                {
                    MessageBox.Show("Cannot mark a card as damaged while it is assigned to a customer. Please release the card first.", 
                                   "Card In Use", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Confirm marking as damaged
                DialogResult result = MessageBox.Show("Are you sure you want to mark this card as damaged? This will make it unusable.",
                                                    "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    // Update card status to Damaged
                    string updateQuery = "UPDATE tbCard SET Status = 'Damaged', LastUsed = @LastUsed WHERE CardId = @CardId";
                    SqlParameter[] updateParams = {
                        new SqlParameter("@LastUsed", DateTime.Now),
                        new SqlParameter("@CardId", cardId)
                    };
                    
                    _facade.ExecuteQuery(updateQuery, updateParams);
                    MessageBox.Show("Card marked as damaged successfully.");
                    ClearFields();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error marking card as damaged: " + ex.Message);
            }
        }
    }
}