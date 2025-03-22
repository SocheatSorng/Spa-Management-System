using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bunifu.UI.WinForms;
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
            btnExitProgram.Click += btnExitProgram_Clicked;
        }

        private void btnExitProgram_Clicked(object sender, EventArgs e)
        {
            Application.Exit();
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
            // Clear the order items container
            panOrderDetailOuter.Controls.Clear();
            panOrderDetailOuter.Controls.Add(scrollOrderDetail);

            if (_currentOrder == null || _currentOrderItems.Count == 0)
            {
                // No active order
                bunifuLabel27.Text = "No Active Order";
                bunifuLabel35.Text = "$0.00"; // Subtotal
                bunifuLabel36.Text = "$0.00"; // Discount
                bunifuLabel38.Text = "$0.00"; // Final Amount
                btnCheckout.Enabled = false;
                scrollOrderDetail.Visible = false;
                return;
            }

            // Update order header with order ID
            bunifuLabel27.Text = "Current Order #" + _currentOrder.OrderId;

            // Group identical items by combining ItemType, ItemId
            var groupedItems = _currentOrderItems
                .GroupBy(item => new { item.ItemType, item.ItemId })
                .Select(group => new
                {
                    ItemType = group.First().ItemType,
                    ItemId = group.First().ItemId,
                    ItemName = group.First().ItemName,
                    UnitPrice = group.First().UnitPrice,
                    Quantity = group.Sum(i => i.Quantity),
                    TotalPrice = group.Sum(i => i.TotalPrice),
                    OrderItemIds = group.Select(i => i.OrderItemId).ToList()
                })
                .ToList();

            // Dynamically create order item panels
            int yPos = 0;
            int itemHeight = 93; // Height from your panOrderDetailInner
            int panelWidth = panOrderDetailInner.Width;

            foreach (var item in groupedItems)
            {
                // Clone the panOrderDetailInner panel for this item
                Bunifu.UI.WinForms.BunifuPanel itemPanel = new Bunifu.UI.WinForms.BunifuPanel();
                itemPanel.Size = new System.Drawing.Size(panelWidth, itemHeight);
                itemPanel.Location = new System.Drawing.Point(3, yPos);
                itemPanel.BackgroundColor = Color.Transparent;
                itemPanel.BorderColor = Color.Transparent;
                itemPanel.BorderRadius = 3;
                itemPanel.BorderThickness = 1;
                itemPanel.ShowBorders = true;

                // Create image
                Bunifu.UI.WinForms.BunifuPictureBox itemImage = new Bunifu.UI.WinForms.BunifuPictureBox();
                itemImage.Size = new Size(60, 60);
                itemImage.Location = new Point(10, 15);
                itemImage.BorderRadius = 10;
                itemImage.BackColor = Color.LightBlue;

                // Create name label
                Bunifu.UI.WinForms.BunifuLabel nameLabel = new Bunifu.UI.WinForms.BunifuLabel();
                nameLabel.Text = "Service / Consumable's Name";
                nameLabel.AutoSize = false;
                nameLabel.Size = bunifuLabel28.Size;
                nameLabel.Location = bunifuLabel28.Location;
                nameLabel.Font = bunifuLabel28.Font;
                nameLabel.Text = item.ItemName;

                // Create price label
                Bunifu.UI.WinForms.BunifuLabel priceLabel = new Bunifu.UI.WinForms.BunifuLabel();
                priceLabel.Size = bunifuLabel29.Size;
                priceLabel.Location = bunifuLabel29.Location;
                priceLabel.Font = bunifuLabel29.Font;
                priceLabel.ForeColor = Color.DarkGoldenrod;
                priceLabel.Text = "$" + item.UnitPrice.ToString("0.00");

                // Clone the quantity label
                Bunifu.UI.WinForms.BunifuLabel qtyLabel = new Bunifu.UI.WinForms.BunifuLabel();
                qtyLabel.Size = txtItemAmount.Size;
                qtyLabel.Location = txtItemAmount.Location;
                qtyLabel.Font = txtItemAmount.Font;
                qtyLabel.TextFormat = txtItemAmount.TextFormat;
                qtyLabel.Text = item.Quantity.ToString();

                // Clone the plus button
                Bunifu.UI.WinForms.BunifuButton.BunifuIconButton plusBtn = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
                plusBtn.Size = btnPlusItem.Size;
                plusBtn.Location = btnPlusItem.Location;
                plusBtn.BackgroundColor = btnPlusItem.BackgroundColor;
                plusBtn.BorderColor = btnPlusItem.BorderColor;
                plusBtn.BorderRadius = btnPlusItem.BorderRadius;
                plusBtn.BorderThickness = btnPlusItem.BorderThickness;
                plusBtn.RoundBorders = true;
                plusBtn.Image = btnPlusItem.Image;
                plusBtn.Tag = item; // Store the item information
                plusBtn.Click += (sender, e) => IncreaseItemQuantity((dynamic)((Control)sender).Tag);

                // Clone the minus button
                Bunifu.UI.WinForms.BunifuButton.BunifuIconButton minusBtn = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
                minusBtn.Size = btnMinusItem.Size;
                minusBtn.Location = btnMinusItem.Location;
                minusBtn.BackgroundColor = btnMinusItem.BackgroundColor;
                minusBtn.BorderColor = btnMinusItem.BorderColor;
                minusBtn.BorderRadius = btnMinusItem.BorderRadius;
                minusBtn.BorderThickness = btnMinusItem.BorderThickness;
                minusBtn.RoundBorders = true;
                minusBtn.Image = btnMinusItem.Image;
                minusBtn.Tag = item; // Store the item information
                minusBtn.Click += (sender, e) => DecreaseItemQuantity((dynamic)((Control)sender).Tag);

                // Add all controls to the panel
                itemPanel.Controls.Add(itemImage);
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(priceLabel);
                itemPanel.Controls.Add(qtyLabel);
                itemPanel.Controls.Add(plusBtn);
                itemPanel.Controls.Add(minusBtn);

                // Add the panel to the container
                panOrderDetailOuter.Controls.Add(itemPanel);

                // Update Y position for next item
                yPos += itemHeight + 5;
            }

            // Configure scrollbar if needed
            if (groupedItems.Count > 0)
            {
                int contentHeight = yPos;
                int visibleHeight = panOrderDetailOuter.Height;

                scrollOrderDetail.Minimum = 0;

                if (contentHeight > visibleHeight)
                {
                    scrollOrderDetail.Maximum = contentHeight;
                    scrollOrderDetail.Value = 0;
                    scrollOrderDetail.Visible = true;

                    // Make sure scroll control is on top
                    scrollOrderDetail.BringToFront();
                }
                else
                {
                    scrollOrderDetail.Visible = false;
                }
            }
            else
            {
                scrollOrderDetail.Visible = false;
            }

            // Update order totals
            bunifuLabel35.Text = "$" + _currentOrder.TotalAmount.ToString("0.00"); // Subtotal
            bunifuLabel36.Text = "$" + _currentOrder.Discount.ToString("0.00");     // Discount
            bunifuLabel38.Text = "$" + _currentOrder.FinalAmount.ToString("0.00");  // Final amount

            // Enable checkout button
            btnCheckout.Enabled = true;
        }
        
        // Modified to handle the grouped items
        private void IncreaseItemQuantity(dynamic itemInfo)
        {
            try
            {
                // Get the first order item ID from the group
                int orderItemId = itemInfo.OrderItemIds[0];

                // Execute stored procedure to add 1 to quantity
                string query = @"
            UPDATE tbOrderItem
            SET Quantity = Quantity + 1,
                TotalPrice = UnitPrice * (Quantity + 1)
            WHERE OrderItemId = @OrderItemId;
            
            -- Update order totals
            DECLARE @OrderId INT;
            SELECT @OrderId = OrderId FROM tbOrderItem WHERE OrderItemId = @OrderItemId;
            
            UPDATE tbOrder
            SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
                FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
            WHERE OrderId = @OrderId;";

                SqlParameter param = new SqlParameter("@OrderItemId", orderItemId);
                _connectionManager.ExecuteNonQuery(query, param);

                // Refresh order data
                if (_currentOrder != null)
                {
                    _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                    _currentOrder = GetActiveOrderForCustomer(_currentOrder.CustomerId);
                    UpdateOrderDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating item quantity: " + ex.Message);
            }
        }
        // Modified to handle the grouped items
        private void DecreaseItemQuantity(dynamic itemInfo)
        {
            try
            {
                // Get the first order item ID from the group
                int orderItemId = itemInfo.OrderItemIds[0];
                int currentQuantity = itemInfo.Quantity;

                if (currentQuantity <= 1)
                {
                    // Ask for confirmation before removing item
                    DialogResult result = MessageBox.Show(
                        "Remove this item from the order?",
                        "Confirm Removal",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Remove the order item
                        string deleteQuery = @"
                    DECLARE @OrderId INT;
                    SELECT @OrderId = OrderId FROM tbOrderItem WHERE OrderItemId = @OrderItemId;
                    
                    DELETE FROM tbOrderItem WHERE OrderItemId = @OrderItemId;
                    
                    UPDATE tbOrder
                    SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
                        FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
                    WHERE OrderId = @OrderId;";

                        SqlParameter param = new SqlParameter("@OrderItemId", orderItemId);
                        _connectionManager.ExecuteNonQuery(deleteQuery, param);
                    }
                    else
                    {
                        return; // Cancel if user says no
                    }
                }
                else
                {
                    // Decrease quantity by 1
                    string updateQuery = @"
                UPDATE tbOrderItem
                SET Quantity = Quantity - 1,
                    TotalPrice = UnitPrice * (Quantity - 1)
                WHERE OrderItemId = @OrderItemId;
                
                -- Update order totals
                DECLARE @OrderId INT;
                SELECT @OrderId = OrderId FROM tbOrderItem WHERE OrderItemId = @OrderItemId;
                
                UPDATE tbOrder
                SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
                    FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
                WHERE OrderId = @OrderId;";

                    SqlParameter param = new SqlParameter("@OrderItemId", orderItemId);
                    _connectionManager.ExecuteNonQuery(updateQuery, param);
                }

                // Refresh order data
                if (_currentOrder != null)
                {
                    _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                    _currentOrder = GetActiveOrderForCustomer(_currentOrder.CustomerId);
                    UpdateOrderDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating item quantity: " + ex.Message);
            }
        }
        private void ClearCurrentOrder()
        {
            _currentCustomer = null;
            _currentOrder = null;
            _currentOrderItems = new List<OrderItemModel>();

            // Clear and hide the order panel items
            panOrderDetailOuter.Controls.Clear();
            scrollOrderDetail.Visible = false;

            UpdateOrderDisplay();
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

        // Modify your AddServiceToOrder method to check for existing items
        private void AddServiceToOrder(ServiceModel service)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please scan a customer card first.");
                return;
            }

            try
            {
                // Check if this service already exists in the order
                var existingItem = _currentOrderItems.FirstOrDefault(
                    item => item.ItemType == "Service" && item.ItemId == service.ServiceId);

                if (existingItem != null)
                {
                    // Service already in order, increase quantity by 1
                    IncreaseItemQuantity(new
                    {
                        OrderItemIds = new List<int> { existingItem.OrderItemId },
                        Quantity = existingItem.Quantity
                    });
                    return;
                }

                // Add new service to order
                string query = "EXEC sp_AddOrderItem @OrderId, @ItemType, @ItemId, @Quantity";
                SqlParameter[] parameters = {
            new SqlParameter("@OrderId", _currentOrder.OrderId),
            new SqlParameter("@ItemType", "Service"),
            new SqlParameter("@ItemId", service.ServiceId),
            new SqlParameter("@Quantity", 1)
        };

                _connectionManager.ExecuteNonQuery(query, parameters);

                // Refresh order data
                _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                _currentOrder = GetActiveOrderForCustomer(_currentOrder.CustomerId);
                UpdateOrderDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding service to order: " + ex.Message);
            }
        }

        // Similarly modify AddConsumableToOrder to check for existing items
        private void AddConsumableToOrder(ConsumableModel consumable)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please scan a customer card first.");
                return;
            }

            try
            {
                // Check if this consumable already exists in the order
                var existingItem = _currentOrderItems.FirstOrDefault(
                    item => item.ItemType == "Consumable" && item.ItemId == consumable.ConsumableId);

                if (existingItem != null)
                {
                    // Consumable already in order, increase quantity by 1
                    IncreaseItemQuantity(new
                    {
                        OrderItemIds = new List<int> { existingItem.OrderItemId },
                        Quantity = existingItem.Quantity
                    });
                    return;
                }

                // Add new consumable to order
                string query = "EXEC sp_AddOrderItem @OrderId, @ItemType, @ItemId, @Quantity";
                SqlParameter[] parameters = {
            new SqlParameter("@OrderId", _currentOrder.OrderId),
            new SqlParameter("@ItemType", "Consumable"),
            new SqlParameter("@ItemId", consumable.ConsumableId),
            new SqlParameter("@Quantity", 1)
        };

                _connectionManager.ExecuteNonQuery(query, parameters);

                // Refresh order data
                _currentOrderItems = GetOrderItems(_currentOrder.OrderId);
                _currentOrder = GetActiveOrderForCustomer(_currentOrder.CustomerId);
                UpdateOrderDisplay();
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

        private void scrollOrderDetail_Scroll(object sender, Bunifu.UI.WinForms.BunifuVScrollBar.ScrollEventArgs e)
        {
            // Bunifu scrollbars provide the Value directly in the event args
            panOrderDetailOuter.AutoScrollPosition = new Point(0, e.Value);
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