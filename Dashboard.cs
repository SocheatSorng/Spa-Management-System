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

        private bool isSliderDragging = false;
        private System.Threading.Timer filterDelayTimer;

        // Track which category is currently selected
        private string _currentCategory = "Services";

        public Dashboard()
        {
            InitializeComponent();

            // Hide fixed panels
            bunifuPanel3.Visible = false;

            // Enable the built-in vertical scrollbar of panOrderDetailOuter
            panOrderDetailOuter.AutoScroll = true;

            // Initialize connection and data
            _connectionManager = SqlConnectionManager.Instance;
            _currentOrderItems = new List<OrderItemModel>();

            // Load data
            LoadServices();
            LoadConsumables();

            // Set up event handlers
            SetupEventHandlers();

            // Set default category
            _currentCategory = "Services";
            SetCategoryButtonStyles("Services");

            // Initial UI state
            ShowServicesResponsive(); // Default view is services
            ClearCurrentOrder();

            this.Controls.Add(sliderPriceFilter);

            // Add the price range label
            this.Controls.Add(lblPriceRange);
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
                        Price = Convert.ToDecimal(row["Price"]),
                        ImagePath = row["ImagePath"]?.ToString() // Add this line
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
                        StockQuantity = Convert.ToInt32(row["StockQuantity"]),
                        ImagePath = row["ImagePath"]?.ToString() // Add this line
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
                        TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m,
                        Discount = row["Discount"] != DBNull.Value ? Convert.ToDecimal(row["Discount"]) : 0m,
                        FinalAmount = row["FinalAmount"] != DBNull.Value ? Convert.ToDecimal(row["FinalAmount"]) : 0m,
                        Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null,
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

        private void SetCategoryButtonStyles(string activeCategory)
        {
            // Reset all buttons to default style
            btnServices.BackColor = Color.White;
            btnServices.ForeColor = Color.Black;

            btnFoods.BackColor = Color.White;
            btnFoods.ForeColor = Color.Black;

            btnDrinks.BackColor = Color.White;
            btnDrinks.ForeColor = Color.Black;

            // Highlight the active button
            switch (activeCategory)
            {
                case "Services":
                    btnServices.BackColor = Color.DarkGoldenrod;
                    btnServices.ForeColor = Color.White;
                    break;
                case "Foods":
                    btnFoods.BackColor = Color.DarkGoldenrod;
                    btnFoods.ForeColor = Color.White;
                    break;
                case "Drinks":
                    btnDrinks.BackColor = Color.DarkGoldenrod;
                    btnDrinks.ForeColor = Color.White;
                    break;
            }
        }

        #endregion

        #region Event Handlers

        private void SetupEventHandlers()
        {
            // Navigation buttons
            btnDashboard.Click += (s, e) => { /* Toggle home view */ };
            btnAllForm.Click += BtnAllForm_Click;
            btnStatistic.Click += (s, e) => { /* Open statistics view */ };
            btnInvoice.Click += (s, e) => { /* Open invoices view */ };
            btnSetting.Click += (s, e) => { /* Open settings view */ };
            btnLogout.Click += (s, e) => { /* Logout */ };

            SetupCategoryButtons();
            // Search box
            txtSearch.TextChanged += (s, e) => FilterItems(txtSearch.Text);

            // Customer ID input
            txtCustomerID.KeyDown += TxtCustomerID_KeyDown;

            // Checkout button
            btnCheckout.Click += BtnCheckout_Click;


            // Exit button
            btnExitProgram.Click += btnExitProgram_Clicked;

            sliderPriceFilter.ValueChanged += SliderPriceFilter_ValueChanged;
            // Add Refresh button event handler
            btnRefresh.Click += BtnRefresh_Click;
        }
        private void SetupCategoryButtons()
        {
            // Services button
            btnServices.Click += (s, e) =>
            {
                _currentCategory = "Services";
                SetCategoryButtonStyles("Services");
                FilterItems(txtSearch.Text); // Apply current filter
            };

            // Foods button
            btnFoods.Click += (s, e) =>
            {
                _currentCategory = "Foods";
                SetCategoryButtonStyles("Foods");
                FilterItems(txtSearch.Text); // Apply current filter
            };

            // Drinks button
            btnDrinks.Click += (s, e) =>
            {
                _currentCategory = "Drinks";
                SetCategoryButtonStyles("Drinks");
                FilterItems(txtSearch.Text); // Apply current filter
            };
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

                // Check if the field is empty - this is the new feature
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    // Clear the current order and customer
                    ClearCurrentOrder();
                    MessageBox.Show("Customer cleared. Ready for new scan.");
                }
                else
                {
                    // Process card ID as normal
                    ProcessCardId(cardId);
                }

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

        private void BtnAllForm_Click(object sender, EventArgs e)
        {
            // Create a custom popup form
            Form popupForm = new Form();
            popupForm.Size = new Size(200, 300); // Increased height to accommodate new button
            popupForm.FormBorderStyle = FormBorderStyle.None;
            popupForm.BackColor = Color.White;
            popupForm.ShowInTaskbar = false;
            popupForm.StartPosition = FormStartPosition.Manual;
            popupForm.TopMost = true;

            // Calculate position to show next to the btnAllForm
            Point btnLocation = btnAllForm.PointToScreen(new Point(0, 0));
            popupForm.Location = new Point(btnLocation.X + btnAllForm.Width, btnLocation.Y);

            // Create CardRegistration button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnCardRegistration = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnCardRegistration.Text = "Card Registration";
            btnCardRegistration.Size = new Size(180, 40);
            btnCardRegistration.Location = new Point(10, 10);
            btnCardRegistration.Click += (s, args) =>
            {
                CardRegistration cardRegistrationForm = new CardRegistration();
                cardRegistrationForm.Show();
                popupForm.Close();
            };

            // Create Consumable button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnConsumable = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnConsumable.Text = "Consumable";
            btnConsumable.Size = new Size(180, 40);
            btnConsumable.Location = new Point(10, 10);
            btnConsumable.Click += (s, args) => {
                Consumable consumableForm = new Consumable();
                consumableForm.Show();
                popupForm.Close();
            };

            // Create Customer button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnCustomer = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnCustomer.Text = "Customer";
            btnCustomer.Size = new Size(180, 40);
            btnCustomer.Location = new Point(10, 50);
            btnCustomer.Click += (s, args) => {
                Customer customerForm = new Customer();
                customerForm.Show();
                popupForm.Close();
            };

            // Create Invoice button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnInvoice = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnInvoice.Text = "Invoice";
            btnInvoice.Size = new Size(180, 40);
            btnInvoice.Location = new Point(10, 90);
            btnInvoice.Click += (s, args) =>
            {
                Invoice invoiceForm = new Invoice();
                invoiceForm.Show();
                popupForm.Close();
            };

            // Create Order button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnOrder = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnOrder.Text = "Order";
            btnOrder.Size = new Size(180, 40);
            btnOrder.Location = new Point(10, 130);
            btnOrder.Click += (s, args) =>
            {
                Order orderForm = new Order();
                orderForm.Show();
                popupForm.Close();
            };

            // Create Payment button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnPayment = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnPayment.Text = "Payment";
            btnPayment.Size = new Size(180, 40);
            btnPayment.Location = new Point(10, 170);
            btnPayment.Click += (s, args) =>
            {
                Payment paymentForm = new Payment();
                paymentForm.Show();
                popupForm.Close();
            };

            // Create Service button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnService = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnService.Text = "Service";
            btnService.Size = new Size(180, 40);
            btnService.Location = new Point(10, 210);
            btnService.Click += (s, args) => {
                Service serviceForm = new Service();
                serviceForm.Show();
                popupForm.Close();
            };

            // Create User button
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 btnUser = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            btnUser.Text = "User";
            btnUser.Size = new Size(180, 40);
            btnUser.Location = new Point(10, 250);
            btnUser.Click += (s, args) => {
                User userForm = new User();
                userForm.Show();
                popupForm.Close();
            };

            // Add buttons to the popup form
            popupForm.Controls.Add(btnCardRegistration);
            popupForm.Controls.Add(btnConsumable);
            popupForm.Controls.Add(btnCustomer);
            popupForm.Controls.Add(btnInvoice);
            popupForm.Controls.Add(btnOrder);
            popupForm.Controls.Add(btnPayment);
            popupForm.Controls.Add(btnService);
            popupForm.Controls.Add(btnUser);


            // Show the popup form
            popupForm.Show();
        }
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Show loading cursor
            Cursor = Cursors.WaitCursor;

            try
            {
                // Reload all data from database
                LoadServices();
                LoadConsumables();

                // Refresh UI based on current category and filters
                string currentSearchText = txtSearch.Text;

                // Clear search to ensure we show all items after refresh
                txtSearch.Clear();

                // Apply category
                switch (_currentCategory)
                {
                    case "Services":
                        ShowServicesResponsive();
                        break;
                    case "Foods":
                    case "Drinks":
                        ShowConsumablesResponsive(_currentCategory);
                        break;
                }

                // If there was a search filter, reapply it
                if (!string.IsNullOrEmpty(currentSearchText))
                {
                    txtSearch.Text = currentSearchText;
                    // FilterItems will be called by the TextChanged event
                }

                MessageBox.Show("Product data refreshed successfully!", "Refresh Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message, "Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore cursor
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region UI Methods
        private void ShowServicesResponsive()
        {
            // Clear the container panel
            panDisplayItem.Controls.Clear();

            int itemWidth = 280;
            int itemHeight = 280;
            int padding = 28;
            int containerWidth = panDisplayItem.Width;

            // Calculate how many items can fit per row
            int itemsPerRow = (containerWidth - padding) / (itemWidth + padding);
            if (itemsPerRow < 1) itemsPerRow = 1;

            int xPos = padding;
            int yPos = padding;
            int itemCount = 0;

            foreach (var service in _services)
            {
                // Create a new panel for this service
                Bunifu.UI.WinForms.BunifuPanel itemPanel = new Bunifu.UI.WinForms.BunifuPanel();
                itemPanel.Size = new Size(itemWidth, itemHeight);
                itemPanel.BackgroundColor = Color.White;
                itemPanel.BorderColor = Color.White;
                itemPanel.BorderRadius = 15;
                itemPanel.BorderThickness = 1;
                itemPanel.ShowBorders = true;
                itemPanel.Tag = service; // Store service object in Tag

                // Calculate position
                itemPanel.Location = new Point(xPos, yPos);

                // Use standard PictureBox instead of BunifuPictureBox
                PictureBox imageBox = CloneItemDisplay();

                // Load and set the image
                Image serviceImage = LoadImageSafely(service.ImagePath);
                if (serviceImage != null)
                {
                    imageBox.Image = serviceImage;
                }

                // Create name label
                Bunifu.UI.WinForms.BunifuLabel nameLabel = new Bunifu.UI.WinForms.BunifuLabel();
                nameLabel.Text = service.ServiceName;
                nameLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                nameLabel.Size = new Size(itemWidth - 20, 20);
                nameLabel.Location = new Point(10, 170);

                // Create description label
                Bunifu.UI.WinForms.BunifuLabel descLabel = new Bunifu.UI.WinForms.BunifuLabel();
                descLabel.Text = service.Description ?? "";
                descLabel.Font = new Font("Century Gothic", 10, FontStyle.Regular);
                descLabel.ForeColor = SystemColors.ActiveBorder;
                descLabel.Size = new Size(itemWidth - 20, 40);
                descLabel.Location = new Point(10, 205);

                // Create price label
                Bunifu.UI.WinForms.BunifuLabel priceLabel = new Bunifu.UI.WinForms.BunifuLabel();
                priceLabel.Text = "$" + service.Price.ToString("0.00");
                priceLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                priceLabel.ForeColor = Color.DarkGoldenrod;
                priceLabel.Size = new Size(itemWidth - 20, 20);
                priceLabel.Location = new Point(10, 250);

                // Add controls to panel
                itemPanel.Controls.Add(imageBox);
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(descLabel);
                itemPanel.Controls.Add(priceLabel);

                // Add click event
                itemPanel.Click += ServicePanel_Click;
                imageBox.Click += (s, e) => ServicePanel_Click(itemPanel, e);

                // Add panel to container
                panDisplayItem.Controls.Add(itemPanel);

                // Update position for next item
                itemCount++;
                if (itemCount % itemsPerRow == 0)
                {
                    // New row
                    xPos = padding;
                    yPos += itemHeight + padding;
                }
                else
                {
                    // Next column
                    xPos += itemWidth + padding;
                }
            }
        }

        private void ShowConsumablesResponsive(string category)
        {
            // Clear the container panel
            panDisplayItem.Controls.Clear();

            int itemWidth = 280;
            int itemHeight = 280;
            int padding = 28;
            int containerWidth = panDisplayItem.Width;

            // Calculate how many items can fit per row
            int itemsPerRow = (containerWidth - padding) / (itemWidth + padding);
            if (itemsPerRow < 1) itemsPerRow = 1;

            int xPos = padding;
            int yPos = padding;
            int itemCount = 0;

            // Filter consumables by category
            var filteredConsumables = _consumables
                .Where(c =>
                {
                    if (c.Category == null) return false;
                    return c.Category == category && c.StockQuantity > 0;
                })
                .ToList();

            // Debug output
            Console.WriteLine($"Found {filteredConsumables.Count} consumables in category {category}");

            foreach (var consumable in filteredConsumables)
            {
                // Debug image path
                Console.WriteLine($"Consumable: {consumable.Name}, Path: {consumable.ImagePath}");

                // Create a new panel for this consumable
                Bunifu.UI.WinForms.BunifuPanel itemPanel = new Bunifu.UI.WinForms.BunifuPanel();
                itemPanel.Size = new Size(itemWidth, itemHeight);
                itemPanel.BackgroundColor = Color.White;
                itemPanel.BorderColor = Color.White;
                itemPanel.BorderRadius = 15;
                itemPanel.BorderThickness = 1;
                itemPanel.ShowBorders = true;
                itemPanel.Tag = consumable; // Store consumable object in Tag

                // Calculate position
                itemPanel.Location = new Point(xPos, yPos);

                // Use standard PictureBox instead of BunifuPictureBox
                PictureBox imageBox = CloneItemDisplay();

                // Load and set the image
                Image consumableImage = LoadImageSafely(consumable.ImagePath);
                if (consumableImage != null)
                {
                    imageBox.Image = consumableImage;
                    Console.WriteLine($"Successfully loaded image for {consumable.Name}");
                }
                else
                {
                    Console.WriteLine($"Failed to load image for {consumable.Name}");
                }

                // Create name label
                Bunifu.UI.WinForms.BunifuLabel nameLabel = new Bunifu.UI.WinForms.BunifuLabel();
                nameLabel.Text = consumable.Name;
                nameLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                nameLabel.Size = new Size(itemWidth - 20, 20);
                nameLabel.Location = new Point(10, 170);

                // Create description label
                Bunifu.UI.WinForms.BunifuLabel descLabel = new Bunifu.UI.WinForms.BunifuLabel();
                descLabel.Text = consumable.Description ?? "";
                descLabel.Font = new Font("Century Gothic", 10, FontStyle.Regular);
                descLabel.ForeColor = SystemColors.ActiveBorder;
                descLabel.Size = new Size(itemWidth - 20, 40);
                descLabel.Location = new Point(10, 205);

                // Create price label
                Bunifu.UI.WinForms.BunifuLabel priceLabel = new Bunifu.UI.WinForms.BunifuLabel();
                priceLabel.Text = "$" + consumable.Price.ToString("0.00");
                priceLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                priceLabel.ForeColor = Color.DarkGoldenrod;
                priceLabel.Size = new Size(itemWidth - 20, 20);
                priceLabel.Location = new Point(10, 250);

                // Add controls to panel - only add each control once!
                itemPanel.Controls.Add(imageBox);
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(descLabel);
                itemPanel.Controls.Add(priceLabel);

                // Add click event
                itemPanel.Click += ConsumablePanel_Click;
                imageBox.Click += (s, e) => ConsumablePanel_Click(itemPanel, e);

                // Add panel to container
                panDisplayItem.Controls.Add(itemPanel);

                // Update position for next item
                itemCount++;
                if (itemCount % itemsPerRow == 0)
                {
                    // New row
                    xPos = padding;
                    yPos += itemHeight + padding;
                }
                else
                {
                    // Next column
                    xPos += itemWidth + padding;
                }
            }
        }
        private void ClearItemPanels()
        {
            // Clear all dynamically created panels from the container
            if (panDisplayItem != null && panDisplayItem.Controls.Count > 0)
            {
                // Remove event handlers from each panel before clearing
                foreach (Control control in panDisplayItem.Controls)
                {
                    if (control is Bunifu.UI.WinForms.BunifuPanel panel)
                    {
                        panel.Click -= ServicePanel_Click;
                        panel.Click -= ConsumablePanel_Click;
                        panel.Tag = null;
                    }
                }

                // Clear all controls
                panDisplayItem.Controls.Clear();
            }
        }

        private void UpdateOrderDisplay()
        {
            // Clear the order items container
            panOrderDetailOuter.Controls.Clear();

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
                if (item.ItemType == "Service")
                {
                    var service = _services.FirstOrDefault(s => s.ServiceId == item.ItemId);  // Changed from s.ItemId
                    if (service != null && !string.IsNullOrEmpty(service.ImagePath))
                    {
                        Image img = LoadImageSafely(service.ImagePath);
                        if (img != null)
                            itemImage.Image = img;
                    }
                }
                else if (item.ItemType == "Consumable")
                {
                    var consumable = _consumables.FirstOrDefault(c => c.ConsumableId == item.ItemId);
                    if (consumable != null && !string.IsNullOrEmpty(consumable.ImagePath))
                    {
                        Image img = LoadImageSafely(consumable.ImagePath);
                        if (img != null)
                            itemImage.Image = img;
                    }
                }
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

                if (contentHeight > visibleHeight)
                {
                }
                else
                {
                }
            }
            else
            {
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
                    
                    -- Check if there are any items left
                    IF EXISTS (SELECT 1 FROM tbOrderItem WHERE OrderId = @OrderId)
                    BEGIN
                        -- Items still exist, update totals
                        UPDATE tbOrder
                        SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
                            FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
                        WHERE OrderId = @OrderId;
                    END
                    ELSE
                    BEGIN
                        -- No items left, set totals to zero
                        UPDATE tbOrder
                        SET TotalAmount = 0,
                            FinalAmount = 0
                        WHERE OrderId = @OrderId;
                    END";

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

            // Reset all UI elements
            bunifuLabel27.Text = "No Active Order";
            bunifuLabel35.Text = "$0.00"; // Subtotal
            bunifuLabel36.Text = "$0.00"; // Discount
            bunifuLabel38.Text = "$0.00"; // Final Amount
            btnCheckout.Enabled = false;
            txtCardStatus.Text = "Available"; // Also reset the card status

            // Clear customer ID (optional, depends on your UX preference)
            // txtCustomerID.Clear();

            UpdateOrderDisplay();
        }

        private void FilterItems(string searchText)
        {
            // Get the price range based on slider position value
            decimal minPrice = 0;
            decimal maxPrice = decimal.MaxValue;

            switch (sliderPriceFilter.Value)
            {
                case 0: // All Prices
                    minPrice = 0;
                    maxPrice = decimal.MaxValue;
                    break;
                case 1: // Under $5
                    minPrice = 0;
                    maxPrice = 5;
                    break;
                case 2: // Under $10 (changed from $5-$10)
                    minPrice = 0;
                    maxPrice = 10;
                    break;
                case 3: // Under $20 (changed from $10-$20)
                    minPrice = 0;
                    maxPrice = 20;
                    break;
                case 4: // Under $50 (changed from $20-$50)
                    minPrice = 0;
                    maxPrice = 50;
                    break;
                case 5: // $50+
                    minPrice = 50;
                    maxPrice = decimal.MaxValue;
                    break;
            }

            // Update the price range label to match these new ranges
            UpdatePriceRangeLabel();

            // Rest of your existing filtering logic with these new price boundaries...
            if (string.IsNullOrWhiteSpace(searchText) && sliderPriceFilter.Value == 0)
            {
                if (_currentCategory == "Services")
                    ShowServicesResponsive();
                else
                    ShowConsumablesResponsive(_currentCategory);
                return;
            }

            // Filter based on current category, search text, and price range
            if (_currentCategory == "Services")
            {
                var filteredServices = _services
                    .Where(s => (string.IsNullOrWhiteSpace(searchText) ||
                                s.ServiceName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                (s.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                                s.Price >= minPrice && (maxPrice == decimal.MaxValue || s.Price <= maxPrice))
                    .ToList();

                ShowFilteredServicesResponsive(filteredServices);
            }
            else
            {
                var filteredConsumables = _consumables
            .Where(c =>
                c.Category == _currentCategory &&
                (string.IsNullOrWhiteSpace(searchText) ||
                 c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                 (c.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                c.Price >= minPrice && (maxPrice == decimal.MaxValue || c.Price <= maxPrice) &&
                c.StockQuantity > 0)
            .ToList();

                ShowFilteredConsumablesResponsive(filteredConsumables);
            }
        }
        private void ShowFilteredServicesResponsive(List<ServiceModel> filteredServices)
        {
            // Clear the container panel
            panDisplayItem.Controls.Clear();

            int itemWidth = 280;
            int itemHeight = 280;
            int padding = 10;
            int containerWidth = panDisplayItem.Width;

            // Calculate how many items can fit per row
            int itemsPerRow = (containerWidth - padding) / (itemWidth + padding);
            if (itemsPerRow < 1) itemsPerRow = 1;

            int xPos = padding;
            int yPos = padding;
            int itemCount = 0;

            foreach (var service in filteredServices)
            {
                // Create a new panel for this service
                Bunifu.UI.WinForms.BunifuPanel itemPanel = new Bunifu.UI.WinForms.BunifuPanel();
                itemPanel.Size = new Size(itemWidth, itemHeight);
                itemPanel.BackgroundColor = Color.White;
                itemPanel.BorderColor = Color.White;
                itemPanel.BorderRadius = 15;
                itemPanel.BorderThickness = 1;
                itemPanel.ShowBorders = true;
                itemPanel.Tag = service; // Store service object in Tag

                // Calculate position
                itemPanel.Location = new Point(xPos, yPos);

                // Create name label
                Bunifu.UI.WinForms.BunifuLabel nameLabel = new Bunifu.UI.WinForms.BunifuLabel();
                nameLabel.Text = service.ServiceName;
                nameLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                nameLabel.Size = new Size(itemWidth - 20, 20);
                nameLabel.Location = new Point(10, 170);

                // Create description label
                Bunifu.UI.WinForms.BunifuLabel descLabel = new Bunifu.UI.WinForms.BunifuLabel();
                descLabel.Text = service.Description ?? "";
                descLabel.Font = new Font("Century Gothic", 10, FontStyle.Regular);
                descLabel.ForeColor = SystemColors.ActiveBorder;
                descLabel.Size = new Size(itemWidth - 20, 40);
                descLabel.Location = new Point(10, 205);

                // Create price label
                Bunifu.UI.WinForms.BunifuLabel priceLabel = new Bunifu.UI.WinForms.BunifuLabel();
                priceLabel.Text = "$" + service.Price.ToString("0.00");
                priceLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                priceLabel.ForeColor = Color.DarkGoldenrod;
                priceLabel.Size = new Size(itemWidth - 20, 20);
                priceLabel.Location = new Point(10, 250);

                // Add labels to panel
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(descLabel);
                itemPanel.Controls.Add(priceLabel);

                // Add click event
                itemPanel.Click += ServicePanel_Click;

                // Add panel to container
                panDisplayItem.Controls.Add(itemPanel);

                // Update position for next item
                itemCount++;
                if (itemCount % itemsPerRow == 0)
                {
                    // New row
                    xPos = padding;
                    yPos += itemHeight + padding;
                }
                else
                {
                    // Next column
                    xPos += itemWidth + padding;
                }
            }
        }

        private void ShowFilteredConsumablesResponsive(List<ConsumableModel> filteredConsumables)
        {
            // Clear the container panel
            panDisplayItem.Controls.Clear();

            int itemWidth = 280;
            int itemHeight = 280;
            int padding = 10;
            int containerWidth = panDisplayItem.Width;

            // Calculate how many items can fit per row
            int itemsPerRow = (containerWidth - padding) / (itemWidth + padding);
            if (itemsPerRow < 1) itemsPerRow = 1;

            int xPos = padding;
            int yPos = padding;
            int itemCount = 0;

            foreach (var consumable in filteredConsumables)
            {
                // Create a new panel for this consumable
                Bunifu.UI.WinForms.BunifuPanel itemPanel = new Bunifu.UI.WinForms.BunifuPanel();
                itemPanel.Size = new Size(itemWidth, itemHeight);
                itemPanel.BackgroundColor = Color.White;
                itemPanel.BorderColor = Color.White;
                itemPanel.BorderRadius = 15;
                itemPanel.BorderThickness = 1;
                itemPanel.ShowBorders = true;
                itemPanel.Tag = consumable; // Store consumable object in Tag

                // Calculate position
                itemPanel.Location = new Point(xPos, yPos);

                // Create name label
                Bunifu.UI.WinForms.BunifuLabel nameLabel = new Bunifu.UI.WinForms.BunifuLabel();
                nameLabel.Text = consumable.Name;
                nameLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                nameLabel.Size = new Size(itemWidth - 20, 20);
                nameLabel.Location = new Point(10, 170);

                // Create description label
                Bunifu.UI.WinForms.BunifuLabel descLabel = new Bunifu.UI.WinForms.BunifuLabel();
                descLabel.Text = consumable.Description ?? "";
                descLabel.Font = new Font("Century Gothic", 10, FontStyle.Regular);
                descLabel.ForeColor = SystemColors.ActiveBorder;
                descLabel.Size = new Size(itemWidth - 20, 40);
                descLabel.Location = new Point(10, 205);

                // Create price label
                Bunifu.UI.WinForms.BunifuLabel priceLabel = new Bunifu.UI.WinForms.BunifuLabel();
                priceLabel.Text = "$" + consumable.Price.ToString("0.00");
                priceLabel.Font = new Font("Century Gothic", 12, FontStyle.Bold);
                priceLabel.ForeColor = Color.DarkGoldenrod;
                priceLabel.Size = new Size(itemWidth - 20, 20);
                priceLabel.Location = new Point(10, 250);

                // Add labels to panel
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(descLabel);
                itemPanel.Controls.Add(priceLabel);

                // Add click event
                itemPanel.Click += ConsumablePanel_Click;

                // Add panel to container
                panDisplayItem.Controls.Add(itemPanel);

                // Update position for next item
                itemCount++;
                if (itemCount % itemsPerRow == 0)
                {
                    // New row
                    xPos = padding;
                    yPos += itemHeight + padding;
                }
                else
                {
                    // Next column
                    xPos += itemWidth + padding;
                }
            }
        }
        #endregion

        #region Business Logic Methods

        private void UpdateCardStatus()
        {
            if (_currentCustomer != null)
            {
                bool hasUnpaidOrders = CheckForUnpaidOrders(_currentCustomer.CustomerId);
                txtCardStatus.Text = hasUnpaidOrders ? "Active" : "Available";
            }
            else
            {
                txtCardStatus.Text = "Available";
            }
        }

        private bool CheckForUnpaidOrders(int customerId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbOrder WHERE CustomerId = @CustomerId AND Status = 'Active'";
                SqlParameter param = new SqlParameter("@CustomerId", customerId);
                int activeOrderCount = (int)_connectionManager.ExecuteScalar(query, param);

                return activeOrderCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for unpaid orders: " + ex.Message);
                return false;
            }
        }

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

            // Update card status after processing card ID
            UpdateCardStatus();
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
                // First, get the invoice details to pre-fill the payment form
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
                    string cardId = invoiceResult.Rows[0]["CardId"].ToString();
                    int customerId = Convert.ToInt32(invoiceResult.Rows[0]["CustomerId"]);

                    // Create a quick payment dialog form
                    using (Form paymentDialog = new Form())
                    {
                        // Configure the dialog
                        paymentDialog.Text = "Process Payment";
                        paymentDialog.Size = new Size(400, 500);
                        paymentDialog.StartPosition = FormStartPosition.CenterParent;
                        paymentDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                        paymentDialog.MaximizeBox = false;
                        paymentDialog.MinimizeBox = false;

                        // Add controls to display invoice information
                        Label lblTitleInvoice = new Label { Text = "Invoice ID:", Location = new Point(20, 20), Width = 100 };
                        Label lblInvoiceValue = new Label { Text = invoiceId.ToString(), Location = new Point(130, 20), Width = 200, Font = new Font(lblTitleInvoice.Font, FontStyle.Bold) };

                        Label lblTitleAmount = new Label { Text = "Total Amount:", Location = new Point(20, 50), Width = 100 };
                        Label lblAmountValue = new Label { Text = "$" + totalAmount.ToString("0.00"), Location = new Point(130, 50), Width = 200, Font = new Font(lblTitleAmount.Font, FontStyle.Bold) };

                        Label lblTitleCard = new Label { Text = "Card ID:", Location = new Point(20, 80), Width = 100 };
                        Label lblCardValue = new Label { Text = cardId, Location = new Point(130, 80), Width = 200 };

                        // Add payment method selection
                        Label lblPayMethod = new Label { Text = "Payment Method:", Location = new Point(20, 120), Width = 120 };
                        ComboBox cmbPayMethod = new ComboBox
                        {
                            Location = new Point(140, 120),
                            Width = 200,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        cmbPayMethod.Items.AddRange(new string[] { "Cash", "Credit Card", "Debit Card", "Mobile Payment" });
                        cmbPayMethod.SelectedIndex = 0;

                        // Transaction reference field
                        Label lblTransRef = new Label { Text = "Transaction Ref:", Location = new Point(20, 160), Width = 120 };
                        TextBox txtTransRef = new TextBox { Location = new Point(140, 160), Width = 200 };

                        // Notes field
                        Label lblNotes = new Label { Text = "Notes:", Location = new Point(20, 200), Width = 100 };
                        TextBox txtNotes = new TextBox
                        {
                            Location = new Point(20, 230),
                            Width = 340,
                            Height = 100,
                            Multiline = true,
                            Text = $"Payment for Invoice #{invoiceId}, Card: {cardId}"
                        };

                        // Add buttons
                        Button btnProcess = new Button
                        {
                            Text = "Process Payment",
                            Location = new Point(120, 350),
                            Width = 150,
                            DialogResult = DialogResult.OK
                        };

                        Button btnCancel = new Button
                        {
                            Text = "Cancel",
                            Location = new Point(280, 350),
                            Width = 80,
                            DialogResult = DialogResult.Cancel
                        };

                        // Add controls to form
                        paymentDialog.Controls.AddRange(new Control[] {
                    lblTitleInvoice, lblInvoiceValue,
                    lblTitleAmount, lblAmountValue,
                    lblTitleCard, lblCardValue,
                    lblPayMethod, cmbPayMethod,
                    lblTransRef, txtTransRef,
                    lblNotes, txtNotes,
                    btnProcess, btnCancel
                });

                        // Show the dialog
                        DialogResult result = paymentDialog.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            // Process the payment
                            int userId = 1; // Default user ID
                            string paymentMethod = cmbPayMethod.SelectedItem.ToString();
                            string transactionRef = txtTransRef.Text.Trim();
                            string notes = txtNotes.Text.Trim();

                            // Use stored procedure to process payment
                            string paymentQuery = "EXEC sp_ProcessPayment @InvoiceId, @PaymentMethod, @TransactionReference, @UserId, @Notes";
                            SqlParameter[] parameters = {
                        new SqlParameter("@InvoiceId", invoiceId),
                        new SqlParameter("@PaymentMethod", paymentMethod),
                        new SqlParameter("@TransactionReference", string.IsNullOrEmpty(transactionRef) ? DBNull.Value : (object)transactionRef),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes)
                    };

                            _connectionManager.ExecuteNonQuery(paymentQuery, parameters);

                            // Clear current order and show success message
                            ClearCurrentOrder();
                            MessageBox.Show("Payment processed successfully. Card has been released.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing payment: " + ex.Message);
            }
        }
        private Image LoadImageSafely(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return null;

                // Log the original path for debugging
                Console.WriteLine($"Original image path: {imagePath}");

                // Clean the path (remove backslashes, forward slashes at beginning)
                imagePath = imagePath.TrimStart('\\', '/').Replace('\\', '/');

                // Try multiple path variations
                string[] pathVariations = new string[]
                {
            // Direct application path join
            Path.Combine(Application.StartupPath, imagePath),
            
            // Without "Images" folder prefix if it exists in the path
            Path.Combine(Application.StartupPath,
                imagePath.StartsWith("Images/") ? imagePath : "Images/" + imagePath),
                
            // Just the filename in the Images/Consumables folder
            Path.Combine(Application.StartupPath, "Images", "Consumables",
                Path.GetFileName(imagePath)),
                
            // Just the filename in the Images/Services folder
            Path.Combine(Application.StartupPath, "Images", "Services",
                Path.GetFileName(imagePath))
                };

                // Try each path variation
                foreach (string path in pathVariations)
                {
                    Console.WriteLine($"Trying path: {path}");

                    if (File.Exists(path))
                    {
                        Console.WriteLine($"Image found at: {path}");
                        using (var stream = new MemoryStream(File.ReadAllBytes(path)))
                        {
                            return Image.FromStream(stream);
                        }
                    }
                }

                Console.WriteLine("Image not found in any of the tried locations");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return null;
            }
        }
        private void EnsureImageDirectoriesExist()
        {
            string baseDir = Application.StartupPath;
            Directory.CreateDirectory(Path.Combine(baseDir, "Images", "Services"));
            Directory.CreateDirectory(Path.Combine(baseDir, "Images", "Consumables"));
        }
        private PictureBox CloneItemDisplay()
        {
            // Create a new standard PictureBox with the properties you specified
            PictureBox clone = new PictureBox();

            // Set the properties to match your template
            clone.Location = new Point(4, 3);
            clone.Size = new Size(274, 161);
            clone.TabIndex = 12;
            clone.TabStop = false;
            clone.SizeMode = PictureBoxSizeMode.StretchImage; // Set this to maintain stretch appearance

            return clone;
        }
        // Add event handler for slider value changes


        private void SliderPriceFilter_ValueChanged(object sender, EventArgs e)
        {
            // Update the price range label immediately for responsive feedback
            UpdatePriceRangeLabel();

            // Cancel any pending timer
            filterDelayTimer?.Dispose();

            // Set a timer to apply the filter after a short delay (250ms)
            filterDelayTimer = new System.Threading.Timer(_ =>
            {
                // Need to invoke back to UI thread
                this.Invoke((MethodInvoker)delegate
                {
                    FilterItems(txtSearch.Text);
                });
            }, null, 250, Timeout.Infinite);
        }
        private void UpdatePriceRangeLabel()
        {
            switch (sliderPriceFilter.Value)
            {
                case 0: lblPriceRange.Text = "All Prices"; break;
                case 1: lblPriceRange.Text = "Under $5"; break;
                case 2: lblPriceRange.Text = "Under $10"; break;
                case 3: lblPriceRange.Text = "Under $20"; break;
                case 4: lblPriceRange.Text = "Under $50"; break;
                case 5: lblPriceRange.Text = "$50+"; break;
            }
        }

        private void SetupPriceSliderLabels()
        {
            // Create labels for each price range position
            string[] priceLabels = { "All", "< $5", "< $10", "< $20", "< $50", "$50+" };

            // Calculate spacing
            float segmentWidth = (float)sliderPriceFilter.Width / 5;

            // Create and position labels
            for (int i = 0; i < priceLabels.Length; i++)
            {
                Bunifu.UI.WinForms.BunifuLabel tickLabel = new Bunifu.UI.WinForms.BunifuLabel();
                tickLabel.Text = priceLabels[i];
                tickLabel.AutoSize = true;
                tickLabel.Font = new Font("Century Gothic", 8F, FontStyle.Regular);
                tickLabel.ForeColor = Color.Gray;

                // Position below the slider
                float xPos = sliderPriceFilter.Location.X + (i * segmentWidth);
                if (i == priceLabels.Length - 1) // Last label
                    xPos = sliderPriceFilter.Location.X + sliderPriceFilter.Width - 20;

                tickLabel.Location = new Point((int)xPos, sliderPriceFilter.Location.Y + sliderPriceFilter.Height + 2);

                this.Controls.Add(tickLabel);
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

}