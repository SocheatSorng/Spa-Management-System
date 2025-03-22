using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public partial class Dashboard : Form, IOrderObserver
    {
        // Singleton database connection
        private readonly SqlConnectionManager _connectionManager;

        // Current customer and order information
        private CustomerModel _currentCustomer;
        private OrderModel _currentOrder;
        private List<OrderItemModel> _currentOrderItems;

        // Service and consumable items for display
        private List<ServiceModel> _services;
        private List<ConsumableModel> _consumables;

        // Track which category is currently selected
        private string _currentCategory = "Services";

        public Dashboard()
        {
            InitializeComponent();

            // Initialize connection and data
            _connectionManager = SqlConnectionManager.Instance;
            _currentOrderItems = new List<OrderItemModel>();

            // Load data
            LoadServices();
            LoadConsumables();

            // Set up event handlers
            SetupEventHandlers();

            // Initial UI state
            ShowServices(); // Default view is services
            ClearCurrentOrder();
        }

        #region Data Loading Methods

        private void LoadServices()
        {
            try
            {
                string query = "SELECT * FROM tbService";
                DataTable servicesTable = _connectionManager.ExecuteQuery(query);

                _services = new List<ServiceModel>();
                foreach (DataRow row in servicesTable.Rows)
                {
                    _services.Add(new ServiceModel
                    {
                        ServiceId = Convert.ToInt32(row["ServiceId"]),
                        ServiceName = row["ServiceName"].ToString(),
                        Description = row["Description"]?.ToString(),
                        Price = Convert.ToDecimal(row["Price"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading services: " + ex.Message);
            }
        }

        private void LoadConsumables()
        {
            try
            {
                string query = "SELECT * FROM tbConsumable WHERE StockQuantity > 0";
                DataTable consumablesTable = _connectionManager.ExecuteQuery(query);

                _consumables = new List<ConsumableModel>();
                foreach (DataRow row in consumablesTable.Rows)
                {
                    _consumables.Add(new ConsumableModel
                    {
                        ConsumableId = Convert.ToInt32(row["ConsumableId"]),
                        Name = row["Name"].ToString(),
                        Description = row["Description"]?.ToString(),
                        Price = Convert.ToDecimal(row["Price"]),
                        Category = row["Category"]?.ToString(),
                        StockQuantity = Convert.ToInt32(row["StockQuantity"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading consumables: " + ex.Message);
            }
        }

        private CustomerModel GetCustomerByCardId(string cardId)
        {
            try
            {
                string query = "EXEC sp_CheckCardStatus @CardId";
                SqlParameter param = new SqlParameter("@CardId", cardId);
                DataTable result = _connectionManager.ExecuteQuery(query, param);

                if (result.Rows.Count > 0 && result.Rows[0]["Status"].ToString() == "InUse")
                {
                    // Card is in use by a customer
                    int customerId = Convert.ToInt32(result.Rows[0]["CustomerId"]);

                    // Get full customer information
                    string customerQuery = "SELECT * FROM tbCustomer WHERE CustomerId = @CustomerId";
                    SqlParameter customerParam = new SqlParameter("@CustomerId", customerId);
                    DataTable customerResult = _connectionManager.ExecuteQuery(customerQuery, customerParam);

                    if (customerResult.Rows.Count > 0)
                    {
                        DataRow row = customerResult.Rows[0];
                        return new CustomerModel
                        {
                            CustomerId = customerId,
                            CardId = cardId,
                            IssuedTime = Convert.ToDateTime(row["IssuedTime"]),
                            Notes = row["Notes"]?.ToString()
                        };
                    }
                }

                return null; // Card not in use or customer not found
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking card status: " + ex.Message);
                return null;
            }
        }

        private OrderModel GetActiveOrderForCustomer(int customerId)
        {
            try
            {
                string query = "SELECT * FROM tbOrder WHERE CustomerId = @CustomerId AND Status = 'Active'";
                SqlParameter param = new SqlParameter("@CustomerId", customerId);
                DataTable result = _connectionManager.ExecuteQuery(query, param);

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    return new OrderModel
                    {
                        OrderId = Convert.ToInt32(row["OrderId"]),
                        CustomerId = customerId,
                        UserId = Convert.ToInt32(row["UserId"]),
                        OrderTime = Convert.ToDateTime(row["OrderTime"]),
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        Discount = Convert.ToDecimal(row["Discount"]),
                        FinalAmount = Convert.ToDecimal(row["FinalAmount"]),
                        Notes = row["Notes"]?.ToString(),
                        Status = row["Status"].ToString()
                    };
                }

                return null; // No active order found
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting active order: " + ex.Message);
                return null;
            }
        }

        private List<OrderItemModel> GetOrderItems(int orderId)
        {
            try
            {
                string query = @"
                    SELECT oi.*, 
                    CASE 
                        WHEN oi.ItemType = 'Service' THEN s.ServiceName
                        WHEN oi.ItemType = 'Consumable' THEN c.Name
                    END AS ItemName
                    FROM tbOrderItem oi
                    LEFT JOIN tbService s ON oi.ItemType = 'Service' AND oi.ItemId = s.ServiceId
                    LEFT JOIN tbConsumable c ON oi.ItemType = 'Consumable' AND oi.ItemId = c.ConsumableId
                    WHERE oi.OrderId = @OrderId";

                SqlParameter param = new SqlParameter("@OrderId", orderId);
                DataTable result = _connectionManager.ExecuteQuery(query, param);

                List<OrderItemModel> items = new List<OrderItemModel>();
                foreach (DataRow row in result.Rows)
                {
                    items.Add(new OrderItemModel
                    {
                        OrderItemId = Convert.ToInt32(row["OrderItemId"]),
                        OrderId = orderId,
                        ItemType = row["ItemType"].ToString(),
                        ItemId = Convert.ToInt32(row["ItemId"]),
                        ItemName = row["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(row["Quantity"]),
                        UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                        TotalPrice = Convert.ToDecimal(row["TotalPrice"])
                    });
                }

                return items;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting order items: " + ex.Message);
                return new List<OrderItemModel>();
            }
        }

        #endregion

        #region Event Handlers

        private void SetupEventHandlers()
        {
            // Navigation buttons
            btnDashboard.Click += (s, e) => { /* Toggle home view */ };
            btnStatistic.Click += (s, e) => { /* Open statistics view */ };
            btnInvoice.Click += (s, e) => { /* Open invoices view */ };
            btnSetting.Click += (s, e) => { /* Open settings view */ };
            btnLogout.Click += (s, e) => { /* Logout */ };

            // Category buttons
            btnServices.Click += (s, e) => { _currentCategory = "Services"; ShowServices(); };
            btnFoods.Click += (s, e) => { _currentCategory = "Foods"; ShowConsumables("Foods"); };
            btnDrinks.Click += (s, e) => { _currentCategory = "Drinks"; ShowConsumables("Drinks"); };

            // Search box
            txtSearch.TextChanged += (s, e) => FilterItems(txtSearch.Text);

            // Customer ID input
            txtCustomerID.KeyDown += TxtCustomerID_KeyDown;

            // Checkout button
            btnCheckout.Click += BtnCheckout_Click;

            // Filter button
            btnFilter.Click += (s, e) => { /* Open filter options */ };

            // Exit button
            //bunifuIconButton1.Click += bunifuIconButton1_Click;
        }

        private void TxtCustomerID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Process customer ID input
                string cardId = txtCustomerID.Text.Trim();
                ProcessCardId(cardId);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ServicePanel_Click(object sender, EventArgs e)
        {
            if (sender is Bunifu.UI.WinForms.BunifuPanel panel && panel.Tag is ServiceModel service)
            {
                AddServiceToOrder(service);
            }
        }

        private void ConsumablePanel_Click(object sender, EventArgs e)
        {
            if (sender is Bunifu.UI.WinForms.BunifuPanel panel && panel.Tag is ConsumableModel consumable)
            {
                AddConsumableToOrder(consumable);
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_currentOrder != null && _currentOrderItems.Count > 0)
            {
                // Complete order and generate invoice
                try
                {
                    string query = "EXEC sp_CompleteOrder @OrderId";
                    SqlParameter param = new SqlParameter("@OrderId", _currentOrder.OrderId);
                    DataTable result = _connectionManager.ExecuteQuery(query, param);

                    if (result.Rows.Count > 0)
                    {
                        int invoiceId = Convert.ToInt32(result.Rows[0]["InvoiceId"]);

                        // Open payment form or process payment
                        ProcessPayment(invoiceId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error completing order: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No items in current order to checkout.");
            }
        }

        #endregion

        #region UI Methods

        private void ShowServices()
        {
            ClearItemPanels();

            // Get panels to populate
            Bunifu.UI.WinForms.BunifuPanel[] panels = new Bunifu.UI.WinForms.BunifuPanel[] {
                bunifuPanel3, bunifuPanel4, bunifuPanel5,
                bunifuPanel6, bunifuPanel7, bunifuPanel8
            };

            // Store panel label mappings for easier updating
            var panelLabels = new Dictionary<Bunifu.UI.WinForms.BunifuPanel, (Bunifu.UI.WinForms.BunifuLabel Name, Bunifu.UI.WinForms.BunifuLabel Desc, Bunifu.UI.WinForms.BunifuLabel Price)>
            {
                { bunifuPanel3, (bunifuLabel4, bunifuLabel5, bunifuLabel6) },
                { bunifuPanel4, (bunifuLabel10, bunifuLabel8, bunifuLabel7) },
                { bunifuPanel5, (bunifuLabel14, bunifuLabel12, bunifuLabel11) },
                { bunifuPanel6, (bunifuLabel18, bunifuLabel16, bunifuLabel15) },
                { bunifuPanel7, (bunifuLabel22, bunifuLabel20, bunifuLabel19) },
                { bunifuPanel8, (bunifuLabel26, bunifuLabel24, bunifuLabel23) }
            };

            int panelIndex = 0;
            foreach (var service in _services)
            {
                if (panelIndex >= panels.Length) break;

                Bunifu.UI.WinForms.BunifuPanel panel = panels[panelIndex];
                panel.Tag = service; // Store service object in Tag

                // Get labels for this panel
                var labels = panelLabels[panel];

                // Update labels
                labels.Name.Text = service.ServiceName;
                labels.Desc.Text = service.Description ?? "";
                labels.Price.Text = "$" + service.Price.ToString("0.00");

                // Add click event handler
                panel.Click += ServicePanel_Click;
                panelIndex++;
            }
        }

        private void ShowConsumables(string category)
        {
            ClearItemPanels();

            // Get panels to populate
            Bunifu.UI.WinForms.BunifuPanel[] panels = new Bunifu.UI.WinForms.BunifuPanel[] {
                bunifuPanel3, bunifuPanel4, bunifuPanel5,
                bunifuPanel6, bunifuPanel7, bunifuPanel8
            };

            // Store panel label mappings
            var panelLabels = new Dictionary<Bunifu.UI.WinForms.BunifuPanel, (Bunifu.UI.WinForms.BunifuLabel Name, Bunifu.UI.WinForms.BunifuLabel Desc, Bunifu.UI.WinForms.BunifuLabel Price)>
            {
                { bunifuPanel3, (bunifuLabel4, bunifuLabel5, bunifuLabel6) },
                { bunifuPanel4, (bunifuLabel10, bunifuLabel8, bunifuLabel7) },
                { bunifuPanel5, (bunifuLabel14, bunifuLabel12, bunifuLabel11) },
                { bunifuPanel6, (bunifuLabel18, bunifuLabel16, bunifuLabel15) },
                { bunifuPanel7, (bunifuLabel22, bunifuLabel20, bunifuLabel19) },
                { bunifuPanel8, (bunifuLabel26, bunifuLabel24, bunifuLabel23) }
            };

            // Filter consumables by category
            var filteredConsumables = _consumables
                .Where(c => c.Category == category && c.StockQuantity > 0)
                .ToList();

            int panelIndex = 0;
            foreach (var consumable in filteredConsumables)
            {
                if (panelIndex >= panels.Length) break;

                Bunifu.UI.WinForms.BunifuPanel panel = panels[panelIndex];
                panel.Tag = consumable; // Store consumable object in Tag

                // Get labels for this panel
                var labels = panelLabels[panel];

                // Update labels
                labels.Name.Text = consumable.Name;
                labels.Desc.Text = consumable.Description ?? "";
                labels.Price.Text = "$" + consumable.Price.ToString("0.00");

                // Add click event handler
                panel.Click += ConsumablePanel_Click;
                panelIndex++;
            }
        }

        private void ClearItemPanels()
        {
            // Get all panels
            Bunifu.UI.WinForms.BunifuPanel[] panels = new Bunifu.UI.WinForms.BunifuPanel[] {
                bunifuPanel3, bunifuPanel4, bunifuPanel5,
                bunifuPanel6, bunifuPanel7, bunifuPanel8
            };

            // Remove event handlers and clear any stored data
            foreach (Bunifu.UI.WinForms.BunifuPanel panel in panels)
            {
                panel.Click -= ServicePanel_Click;
                panel.Click -= ConsumablePanel_Click;
                panel.Tag = null;
            }
        }

        private void UpdateOrderDisplay()
        {
            // Clear current order items display
            bunifuPanel9.Controls.Clear();

            // Add back the fixed elements
            bunifuPanel9.Controls.Add(bunifuLabel34);
            bunifuPanel9.Controls.Add(bunifuLabel35);
            bunifuPanel9.Controls.Add(bunifuLabel36);
            bunifuPanel9.Controls.Add(bunifuLabel37);
            bunifuPanel9.Controls.Add(bunifuLabel38);
            bunifuPanel9.Controls.Add(bunifuLabel40);
            bunifuPanel9.Controls.Add(bunifuSeparator1);

            if (_currentOrder == null || _currentOrderItems.Count == 0)
            {
                // No active order
                bunifuLabel27.Text = "No Active Order";
                bunifuLabel35.Text = "$0.00"; // Subtotal
                bunifuLabel36.Text = "$0.00"; // Discount
                bunifuLabel38.Text = "$0.00"; // Final Amount
                btnCheckout.Enabled = false;
                return;
            }

            // Update order header
            bunifuLabel27.Text = "Current Order #" + _currentOrder.OrderId;

            // Update order items - this would need custom implementation since we need to create controls dynamically
            // Here we update the sample items already in the right panel
            if (_currentOrderItems.Count > 0 && _currentOrderItems.Count <= 3)
            {
                // We have predefined item displays in the UI for up to 3 items
                if (_currentOrderItems.Count >= 1)
                {
                    bunifuLabel28.Text = _currentOrderItems[0].ItemName;
                    bunifuLabel29.Text = "$" + _currentOrderItems[0].TotalPrice.ToString("0.00");
                    bunifuPictureBox3.Visible = true;
                    bunifuLabel28.Visible = true;
                    bunifuLabel29.Visible = true;
                }

                if (_currentOrderItems.Count >= 2)
                {
                    bunifuLabel31.Text = _currentOrderItems[1].ItemName;
                    bunifuLabel30.Text = "$" + _currentOrderItems[1].TotalPrice.ToString("0.00");
                    bunifuPictureBox4.Visible = true;
                    bunifuLabel31.Visible = true;
                    bunifuLabel30.Visible = true;
                }

                if (_currentOrderItems.Count >= 3)
                {
                    bunifuLabel33.Text = _currentOrderItems[2].ItemName;
                    bunifuLabel32.Text = "$" + _currentOrderItems[2].TotalPrice.ToString("0.00");
                    bunifuPictureBox5.Visible = true;
                    bunifuLabel33.Visible = true;
                    bunifuLabel32.Visible = true;
                }
            }
            else if (_currentOrderItems.Count > 3)
            {
                // If more than 3 items, show the first 2 and a summary of the rest
                bunifuLabel28.Text = _currentOrderItems[0].ItemName;
                bunifuLabel29.Text = "$" + _currentOrderItems[0].TotalPrice.ToString("0.00");
                bunifuPictureBox3.Visible = true;
                bunifuLabel28.Visible = true;
                bunifuLabel29.Visible = true;

                bunifuLabel31.Text = _currentOrderItems[1].ItemName;
                bunifuLabel30.Text = "$" + _currentOrderItems[1].TotalPrice.ToString("0.00");
                bunifuPictureBox4.Visible = true;
                bunifuLabel31.Visible = true;
                bunifuLabel30.Visible = true;

                bunifuLabel33.Text = $"+ {_currentOrderItems.Count - 2} more items";
                bunifuLabel32.Text = "";
                bunifuPictureBox5.Visible = true;
                bunifuLabel33.Visible = true;
                bunifuLabel32.Visible = false;
            }

            // Update order totals
            bunifuLabel35.Text = "$" + _currentOrder.TotalAmount.ToString("0.00"); // Subtotal
            bunifuLabel36.Text = "$" + _currentOrder.Discount.ToString("0.00"); // Discount
            bunifuLabel38.Text = "$" + _currentOrder.FinalAmount.ToString("0.00"); // Final amount

            // Enable checkout button
            btnCheckout.Enabled = true;
        }

        private void FilterItems(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all items based on current category
                if (_currentCategory == "Services")
                    ShowServices();
                else
                    ShowConsumables(_currentCategory);

                return;
            }

            // Filter based on current category and search text
            if (_currentCategory == "Services")
            {
                var filteredServices = _services
                    .Where(s => s.ServiceName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               (s.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

                // Use a modified ShowServices method that takes a filtered list
                ShowFilteredServices(filteredServices);
            }
            else
            {
                var filteredConsumables = _consumables
                    .Where(c => c.Category == _currentCategory &&
                               (c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               (c.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)))
                    .ToList();

                // Use a modified ShowConsumables method that takes a filtered list
                ShowFilteredConsumables(filteredConsumables);
            }
        }

        private void ShowFilteredServices(List<ServiceModel> filteredServices)
        {
            ClearItemPanels();

            // Get panels to populate
            Bunifu.UI.WinForms.BunifuPanel[] panels = new Bunifu.UI.WinForms.BunifuPanel[] {
                bunifuPanel3, bunifuPanel4, bunifuPanel5,
                bunifuPanel6, bunifuPanel7, bunifuPanel8
            };

            // Store panel label mappings
            var panelLabels = new Dictionary<Bunifu.UI.WinForms.BunifuPanel, (Bunifu.UI.WinForms.BunifuLabel Name, Bunifu.UI.WinForms.BunifuLabel Desc, Bunifu.UI.WinForms.BunifuLabel Price)>
            {
                { bunifuPanel3, (bunifuLabel4, bunifuLabel5, bunifuLabel6) },
                { bunifuPanel4, (bunifuLabel10, bunifuLabel8, bunifuLabel7) },
                { bunifuPanel5, (bunifuLabel14, bunifuLabel12, bunifuLabel11) },
                { bunifuPanel6, (bunifuLabel18, bunifuLabel16, bunifuLabel15) },
                { bunifuPanel7, (bunifuLabel22, bunifuLabel20, bunifuLabel19) },
                { bunifuPanel8, (bunifuLabel26, bunifuLabel24, bunifuLabel23) }
            };

            int panelIndex = 0;
            foreach (var service in filteredServices)
            {
                if (panelIndex >= panels.Length) break;

                Bunifu.UI.WinForms.BunifuPanel panel = panels[panelIndex];
                panel.Tag = service; // Store service object in Tag

                // Get labels for this panel
                var labels = panelLabels[panel];

                // Update labels
                labels.Name.Text = service.ServiceName;
                labels.Desc.Text = service.Description ?? "";
                labels.Price.Text = "$" + service.Price.ToString("0.00");

                // Add click event handler
                panel.Click += ServicePanel_Click;
                panelIndex++;
            }
        }

        private void ShowFilteredConsumables(List<ConsumableModel> filteredConsumables)
        {
            ClearItemPanels();

            // Get panels to populate
            Bunifu.UI.WinForms.BunifuPanel[] panels = new Bunifu.UI.WinForms.BunifuPanel[] {
                bunifuPanel3, bunifuPanel4, bunifuPanel5,
                bunifuPanel6, bunifuPanel7, bunifuPanel8
            };

            // Store panel label mappings
            var panelLabels = new Dictionary<Bunifu.UI.WinForms.BunifuPanel, (Bunifu.UI.WinForms.BunifuLabel Name, Bunifu.UI.WinForms.BunifuLabel Desc, Bunifu.UI.WinForms.BunifuLabel Price)>
            {
                { bunifuPanel3, (bunifuLabel4, bunifuLabel5, bunifuLabel6) },
                { bunifuPanel4, (bunifuLabel10, bunifuLabel8, bunifuLabel7) },
                { bunifuPanel5, (bunifuLabel14, bunifuLabel12, bunifuLabel11) },
                { bunifuPanel6, (bunifuLabel18, bunifuLabel16, bunifuLabel15) },
                { bunifuPanel7, (bunifuLabel22, bunifuLabel20, bunifuLabel19) },
                { bunifuPanel8, (bunifuLabel26, bunifuLabel24, bunifuLabel23) }
            };

            int panelIndex = 0;
            foreach (var consumable in filteredConsumables)
            {
                if (panelIndex >= panels.Length) break;

                Bunifu.UI.WinForms.BunifuPanel panel = panels[panelIndex];
                panel.Tag = consumable; // Store consumable object in Tag

                // Get labels for this panel
                var labels = panelLabels[panel];

                // Update labels
                labels.Name.Text = consumable.Name;
                labels.Desc.Text = consumable.Description ?? "";
                labels.Price.Text = "$" + consumable.Price.ToString("0.00");

                // Add click event handler
                panel.Click += ConsumablePanel_Click;
                panelIndex++;
            }
        }

        private void ClearCurrentOrder()
        {
            _currentCustomer = null;
            _currentOrder = null;
            _currentOrderItems = new List<OrderItemModel>();

            // Hide all order item displays
            bunifuPictureBox3.Visible = false;
            bunifuLabel28.Visible = false;
            bunifuLabel29.Visible = false;

            bunifuPictureBox4.Visible = false;
            bunifuLabel31.Visible = false;
            bunifuLabel30.Visible = false;

            bunifuPictureBox5.Visible = false;
            bunifuLabel33.Visible = false;
            bunifuLabel32.Visible = false;

            UpdateOrderDisplay();
        }

        #endregion

        #region Business Logic Methods

        private void ProcessCardId(string cardId)
        {
            // Check if card is valid and in use
            CustomerModel customer = GetCustomerByCardId(cardId);

            if (customer != null)
            {
                // Card is in use, get active order if exists
                _currentCustomer = customer;
                _currentOrder = GetActiveOrderForCustomer(customer.CustomerId);

                if (_currentOrder != null)
                {
                    // Active order exists, load items
                    _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                    UpdateOrderDisplay();
                }
                else
                {
                    // No active order, create new one
                    CreateNewOrder(customer.CustomerId);
                }
            }
            else
            {
                // Card not in use, ask if user wants to issue it
                DialogResult result = MessageBox.Show(
                    "This card is not currently issued to a customer. Would you like to issue it now?",
                    "Issue Card",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    IssueCardToCustomer(cardId);
                }
            }
        }

        private void IssueCardToCustomer(string cardId)
        {
            try
            {
                // Get customer notes (you'll need to add a dialog for this)
                string notes = "New customer";

                // Issue card to customer
                string query = "EXEC sp_IssueCardToCustomer @CardId, @Notes";
                SqlParameter[] parameters = {
                    new SqlParameter("@CardId", cardId),
                    new SqlParameter("@Notes", notes)
                };

                DataTable result = _connectionManager.ExecuteQuery(query, parameters);
                if (result.Rows.Count > 0)
                {
                    int customerId = Convert.ToInt32(result.Rows[0]["CustomerId"]);

                    // Set current customer
                    _currentCustomer = new CustomerModel
                    {
                        CustomerId = customerId,
                        CardId = cardId,
                        IssuedTime = DateTime.Now,
                        Notes = notes
                    };

                    // Create new order
                    CreateNewOrder(customerId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error issuing card: " + ex.Message);
            }
        }

        private void CreateNewOrder(int customerId)
        {
            try
            {
                // Get current user ID (would come from login)
                int userId = 1; // Default user ID for now

                // Create new order
                string query = "EXEC sp_CreateOrder @CustomerId, @UserId, @Notes";
                SqlParameter[] parameters = {
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Notes", DBNull.Value)
                };

                DataTable result = _connectionManager.ExecuteQuery(query, parameters);
                if (result.Rows.Count > 0)
                {
                    int orderId = Convert.ToInt32(result.Rows[0]["OrderId"]);

                    // Set current order
                    _currentOrder = new OrderModel
                    {
                        OrderId = orderId,
                        CustomerId = customerId,
                        UserId = userId,
                        OrderTime = DateTime.Now,
                        TotalAmount = 0,
                        Discount = 0,
                        FinalAmount = 0,
                        Status = "Active"
                    };

                    // Clear order items
                    _currentOrderItems = new List<OrderItemModel>();
                    UpdateOrderDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating order: " + ex.Message);
            }
        }

        private void AddServiceToOrder(ServiceModel service)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please scan a customer card first.");
                return;
            }

            try
            {
                // Add service to order
                string query = "EXEC sp_AddOrderItem @OrderId, @ItemType, @ItemId, @Quantity";
                SqlParameter[] parameters = {
                    new SqlParameter("@OrderId", _currentOrder.OrderId),
                    new SqlParameter("@ItemType", "Service"),
                    new SqlParameter("@ItemId", service.ServiceId),
                    new SqlParameter("@Quantity", 1)
                };

                DataTable result = _connectionManager.ExecuteQuery(query, parameters);
                if (result.Rows.Count > 0)
                {
                    // Get updated order details
                    string orderQuery = "SELECT * FROM tbOrder WHERE OrderId = @OrderId";
                    SqlParameter orderParam = new SqlParameter("@OrderId", _currentOrder.OrderId);
                    DataTable orderResult = _connectionManager.ExecuteQuery(orderQuery, orderParam);

                    if (orderResult.Rows.Count > 0)
                    {
                        DataRow row = orderResult.Rows[0];
                        _currentOrder.TotalAmount = Convert.ToDecimal(row["TotalAmount"]);
                        _currentOrder.Discount = Convert.ToDecimal(row["Discount"]);
                        _currentOrder.FinalAmount = Convert.ToDecimal(row["FinalAmount"]);
                    }

                    // Add to current order items
                    OrderItemModel newItem = new OrderItemModel
                    {
                        OrderId = _currentOrder.OrderId,
                        ItemType = "Service",
                        ItemId = service.ServiceId,
                        ItemName = service.ServiceName,
                        Quantity = 1,
                        UnitPrice = service.Price,
                        TotalPrice = service.Price
                    };

                    _currentOrderItems.Add(newItem);
                    UpdateOrderDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding service to order: " + ex.Message);
            }
        }

        private void AddConsumableToOrder(ConsumableModel consumable)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please scan a customer card first.");
                return;
            }

            try
            {
                // Add consumable to order
                string query = "EXEC sp_AddOrderItem @OrderId, @ItemType, @ItemId, @Quantity";
                SqlParameter[] parameters = {
                    new SqlParameter("@OrderId", _currentOrder.OrderId),
                    new SqlParameter("@ItemType", "Consumable"),
                    new SqlParameter("@ItemId", consumable.ConsumableId),
                    new SqlParameter("@Quantity", 1)
                };

                DataTable result = _connectionManager.ExecuteQuery(query, parameters);
                if (result.Rows.Count > 0)
                {
                    // Get updated order details
                    string orderQuery = "SELECT * FROM tbOrder WHERE OrderId = @OrderId";
                    SqlParameter orderParam = new SqlParameter("@OrderId", _currentOrder.OrderId);
                    DataTable orderResult = _connectionManager.ExecuteQuery(orderQuery, orderParam);

                    if (orderResult.Rows.Count > 0)
                    {
                        DataRow row = orderResult.Rows[0];
                        _currentOrder.TotalAmount = Convert.ToDecimal(row["TotalAmount"]);
                        _currentOrder.Discount = Convert.ToDecimal(row["Discount"]);
                        _currentOrder.FinalAmount = Convert.ToDecimal(row["FinalAmount"]);
                    }

                    // Add to current order items
                    OrderItemModel newItem = new OrderItemModel
                    {
                        OrderId = _currentOrder.OrderId,
                        ItemType = "Consumable",
                        ItemId = consumable.ConsumableId,
                        ItemName = consumable.Name,
                        Quantity = 1,
                        UnitPrice = consumable.Price,
                        TotalPrice = consumable.Price
                    };

                    _currentOrderItems.Add(newItem);
                    UpdateOrderDisplay();

                    // Refresh consumables if current category
                    if (_currentCategory != "Services")
                    {
                        LoadConsumables();
                        ShowConsumables(_currentCategory);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding consumable to order: " + ex.Message);
            }
        }

        private void ProcessPayment(int invoiceId)
        {
            try
            {
                // First, display the invoice details to the user
                string query = "SELECT i.InvoiceId, i.TotalAmount, c.CardId, o.CustomerId " +
                              "FROM tbInvoice i " +
                              "JOIN tbOrder o ON i.OrderId = o.OrderId " +
                              "JOIN tbCustomer c ON o.CustomerId = c.CustomerId " +
                              "WHERE i.InvoiceId = @InvoiceId";

                SqlParameter param = new SqlParameter("@InvoiceId", invoiceId);
                DataTable invoiceResult = _connectionManager.ExecuteQuery(query, param);

                if (invoiceResult.Rows.Count > 0)
                {
                    decimal totalAmount = Convert.ToDecimal(invoiceResult.Rows[0]["TotalAmount"]);

                    // Show payment confirmation dialog
                    DialogResult result = MessageBox.Show(
                        $"Total amount to pay: ${totalAmount}\nConfirm payment?",
                        "Payment Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Only process payment after confirmation
                        int userId = 1; // Default user ID
                        string paymentMethod = "Cash";

                        string paymentQuery = "EXEC sp_ProcessPayment @InvoiceId, @PaymentMethod, @TransactionReference, @UserId, @Notes";
                        SqlParameter[] parameters = {
                    new SqlParameter("@InvoiceId", invoiceId),
                    new SqlParameter("@PaymentMethod", paymentMethod),
                    new SqlParameter("@TransactionReference", DBNull.Value),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Notes", "Payment processed from Dashboard")
                };

                        DataTable paymentResult = _connectionManager.ExecuteQuery(paymentQuery, parameters);
                        if (paymentResult.Rows.Count > 0)
                        {
                            MessageBox.Show("Payment processed successfully. Card has been released.");
                            ClearCurrentOrder();
                        }
                    }
                    else
                    {
                        // Cancel payment process - the order remains active
                        MessageBox.Show("Payment cancelled. The order remains active.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing payment: " + ex.Message);
            }
        }

        #endregion

        // Implement IOrderObserver interface
        public void OnOrderUpdated()
        {
            if (_currentOrder != null)
            {
                // Refresh order and order items
                _currentOrder = GetActiveOrderForCustomer(_currentOrder.CustomerId);
                if (_currentOrder != null)
                {
                    _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                }
                else
                {
                    _currentOrderItems = new List<OrderItemModel>();
                }

                UpdateOrderDisplay();
            }
        }
    }

    // Model classes if not already defined elsewhere
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CardId { get; set; }
        public DateTime IssuedTime { get; set; }
        public DateTime? ReleasedTime { get; set; }
        public string Notes { get; set; }
    }

    public class OrderItemModel
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public string ItemType { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}