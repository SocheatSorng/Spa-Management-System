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

        private void Dashboard_Load(object sender, EventArgs e)
        {
            // Add a status strip to show database connection information
            StatusStrip statusStrip = new StatusStrip();
            ToolStripStatusLabel lblConnection = new ToolStripStatusLabel();
            
            string server = SqlConnectionManager.Instance.GetConnectedServerName();
            string database = SqlConnectionManager.Instance.GetConnectedDatabaseName();
            lblConnection.Text = $"Connected to: {server} / {database}";
            
            statusStrip.Items.Add(lblConnection);
            this.Controls.Add(statusStrip);
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
            btnStatistic.Click += btnStatistic_Click;
            btnInvoice.Click += btnInvoice_Click;
            btnSetting.Click += btnSetting_Click;
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
            // Ask for confirmation
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?", 
                "Confirm Logout", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                // Log the user out
                SqlConnectionManager.SetAuthenticated(null, false);
                
                // Close the current form
                this.Hide();
                
                // Show the login form
                using (LoginForm loginForm = new LoginForm())
                {
                    DialogResult loginResult = loginForm.ShowDialog();
                    
                    // If login was unsuccessful or cancelled, exit the application
                    if (loginResult != DialogResult.OK || !loginForm.LoginSuccessful)
                    {
                        this.Close();
                        Application.Exit();
                    }
                    else
                    {
                        // Login successful, show dashboard again
                        this.Show();
                    }
                }
            }
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
            // Check if popup is already open and close it if clicking the button again
            if (this.Controls.OfType<Panel>().Any(p => p.Name == "popupMenuPanel"))
            {
                ClosePopupMenu();
                return;
            }

            // Create a custom popup panel (using Panel instead of Form)
            Panel popupMenuPanel = new Panel();
            popupMenuPanel.Name = "popupMenuPanel";
            popupMenuPanel.Size = new Size(200, 270); // Increased height to accommodate all buttons
            popupMenuPanel.BorderStyle = BorderStyle.FixedSingle;
            popupMenuPanel.BackColor = Color.White;

            // Add shadow effect
            popupMenuPanel.Paint += (s, args) =>
            {
                ControlPaint.DrawBorder(args.Graphics, popupMenuPanel.ClientRectangle,
                    Color.LightGray, 1, ButtonBorderStyle.Solid,
                    Color.LightGray, 1, ButtonBorderStyle.Solid,
                    Color.Gray, 1, ButtonBorderStyle.Solid,
                    Color.Gray, 1, ButtonBorderStyle.Solid);
            };

            // Calculate position to show next to the btnAllForm
            Point btnScreenLocation = btnAllForm.PointToScreen(new Point(0, 0));
            Point formScreenLocation = this.PointToScreen(new Point(0, 0));
            Point relativeLoc = new Point(
                btnScreenLocation.X - formScreenLocation.X + btnAllForm.Width,
                btnScreenLocation.Y - formScreenLocation.Y
            );
            popupMenuPanel.Location = relativeLoc;

            // Create buttons with consistent styling - add all original buttons
            // Customer button
            CreateMenuButton(popupMenuPanel, "Card Management", 10, 10, (s, args) =>
            {
                ClosePopupMenu();
                Customer customerForm = new Customer();
                customerForm.Show();
            });

            // Service button
            CreateMenuButton(popupMenuPanel, "Service", 10, 50, (s, args) =>
            {
                ClosePopupMenu();
                Service serviceForm = new Service();
                serviceForm.Show();
            });

            // Consumable button
            CreateMenuButton(popupMenuPanel, "Foods/Drinks", 10, 90, (s, args) =>
            {
                ClosePopupMenu();
                Consumable consumableForm = new Consumable();
                consumableForm.Show();
            });

            // Invoice button
            CreateMenuButton(popupMenuPanel, "Invoice", 10, 130, (s, args) =>
            {
                ClosePopupMenu();
                Invoice invoiceForm = new Invoice();
                invoiceForm.Show();
            });

            // Order button
            CreateMenuButton(popupMenuPanel, "Order", 10, 170, (s, args) =>
            {
                ClosePopupMenu();
                Order orderForm = new Order();
                orderForm.Show();
            });

            // User button
            CreateMenuButton(popupMenuPanel, "User", 10, 210, (s, args) =>
            {
                ClosePopupMenu();
                User userForm = new User();
                userForm.Show();
            });

            // Add close button at the top-right
            Bunifu.UI.WinForms.BunifuButton.BunifuIconButton closeButton = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
            closeButton.Size = new Size(24, 24);
            closeButton.Location = new Point(popupMenuPanel.Width - 27, 3);
            closeButton.BackgroundColor = Color.Transparent;
            closeButton.BorderColor = Color.Transparent;
            closeButton.BorderRadius = 12;
            closeButton.RoundBorders = true;
            closeButton.Click += (s, args) => ClosePopupMenu();
            popupMenuPanel.Controls.Add(closeButton);

            // Add the panel to the form
            this.Controls.Add(popupMenuPanel);
            popupMenuPanel.BringToFront();

            // Add event to close popup when user clicks elsewhere
            EventHandler docClickHandler = null;
            docClickHandler = (s, args) =>
            {
                // Check if click was outside the popup
                if (s is Control control && !IsChildOf(control, popupMenuPanel) && control != btnAllForm)
                {
                    ClosePopupMenu();
                    this.Click -= docClickHandler;
                }
            };

            this.Click += docClickHandler;

            // Also close popup if form loses focus
            this.Deactivate += (s, args) => ClosePopupMenu();
        }

        // Helper method to create styled menu buttons
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton2 CreateMenuButton(Panel panel, string text, int x, int y, EventHandler clickHandler)
        {
            Bunifu.UI.WinForms.BunifuButton.BunifuButton2 button = new Bunifu.UI.WinForms.BunifuButton.BunifuButton2();
            button.Text = text;
            button.Size = new Size(180, 40);
            button.Location = new Point(x, y);

            // Styling the button
            button.BackColor = Color.Transparent;
            button.ForeColor = Color.Black;
            button.Font = new Font("Century Gothic", 10, FontStyle.Regular);

            // Set initial background color to a light gray that blends with your theme
            button.IdleFillColor = Color.FromArgb(245, 245, 245);
            button.IdleBorderColor = Color.FromArgb(230, 230, 230);
            button.IdleBorderRadius = 10;
            button.IdleBorderThickness = 1;

            // Hover effect
            button.onHoverState.FillColor = Color.FromArgb(60, 120, 188);
            button.onHoverState.ForeColor = Color.White;
            button.onHoverState.BorderColor = Color.FromArgb(60, 120, 188);

            // Active state (when clicked)
            button.OnPressedState.FillColor = Color.FromArgb(40, 96, 144);
            button.OnPressedState.ForeColor = Color.White;
            button.OnPressedState.BorderColor = Color.FromArgb(40, 96, 144);

            button.Click += clickHandler;
            panel.Controls.Add(button);

            return button;
        }

        // Helper method to check if a control is child of another
        private bool IsChildOf(Control child, Control parent)
        {
            if (child == parent) return true;

            foreach (Control c in parent.Controls)
            {
                if (c == child) return true;
                if (IsChildOf(child, c)) return true;
            }

            return false;
        }

        // Method to close the popup menu
        private void ClosePopupMenu()
        {
            foreach (Control c in this.Controls.OfType<Panel>().Where(p => p.Name == "popupMenuPanel").ToList())
            {
                this.Controls.Remove(c);
                c.Dispose();
            }
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

        private void btnInvoice_Click(object sender, EventArgs e)
        {
            Invoice invoiceForm = new Invoice();
            invoiceForm.Show();
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            Payment paymentForm = new Payment();
            paymentForm.Show();
        }

        private void btnStatistic_Click(object sender, EventArgs e)
        {
            // Check if a statistics form is already open
            Form existingForm = Application.OpenForms.OfType<Statistic>().FirstOrDefault();
            
            if (existingForm != null)
            {
                // If a form is already open, bring it to front
                existingForm.BringToFront();
                existingForm.Focus();
            }
            else
            {
                // If no form is open, create a new one
                Statistic statisticForm = new Statistic();
                statisticForm.Show();
            }
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            // Create a settings popup dialog
            using (Form settingsForm = new Form())
            {
                settingsForm.Text = "Application Settings";
                settingsForm.Size = new Size(450, 400);
                settingsForm.StartPosition = FormStartPosition.CenterParent;
                settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                settingsForm.MaximizeBox = false;
                settingsForm.MinimizeBox = false;
                
                // Add a panel with title
                Panel titlePanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = Color.FromArgb(0, 122, 204)
                };
                
                Label titleLabel = new Label
                {
                    Text = "System Settings",
                    ForeColor = Color.White,
                    Font = new Font("Century Gothic", 16, FontStyle.Bold),
                    AutoSize = true
                };
                titleLabel.Location = new Point(10, (titlePanel.Height - titleLabel.Height) / 2);
                titlePanel.Controls.Add(titleLabel);
                
                // Create a TabControl for settings categories
                TabControl tabControl = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Padding = new Point(20, 10),
                    Font = new Font("Century Gothic", 10)
                };
                
                // Add General Settings tab
                TabPage generalTab = new TabPage("General");
                generalTab.Padding = new Padding(10);
                
                // Add Database Settings tab
                TabPage databaseTab = new TabPage("Database");
                databaseTab.Padding = new Padding(10);
                
                // Add Appearance Settings tab but mark it as under development
                TabPage appearanceTab = new TabPage("Appearance (Coming Soon)");
                appearanceTab.Padding = new Padding(10);
                
                // Create "Under Development" message for appearance tab
                Label lblUnderDev = new Label
                {
                    Text = "This feature is currently under development.\nCheck back in a future update!",
                    Location = new Point(50, 100),
                    Size = new Size(300, 50),
                    Font = new Font("Century Gothic", 10),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                appearanceTab.Controls.Add(lblUnderDev);
                
                // Add content to General tab
                CheckBox chkAutoRefresh = new CheckBox
                {
                    Text = "Auto-refresh products list",
                    Location = new Point(20, 30),
                    Checked = _autoRefreshEnabled,
                    Width = 250
                };
                
                Label lblRefreshInterval = new Label
                {
                    Text = "Refresh interval (minutes):",
                    Location = new Point(20, 70),
                    Width = 200
                };
                
                NumericUpDown numRefreshInterval = new NumericUpDown
                {
                    Location = new Point(220, 68),
                    Width = 80,
                    Minimum = 1,
                    Maximum = 60,
                    Value = _autoRefreshInterval
                };
                
                // Add content to Database tab
                Label lblConnection = new Label
                {
                    Text = "Current Connection:",
                    Location = new Point(20, 30),
                    Width = 150
                };
                
                string server = SqlConnectionManager.Instance.GetConnectedServerName();
                string database = SqlConnectionManager.Instance.GetConnectedDatabaseName();
                
                Label lblConnectionValue = new Label
                {
                    Text = $"{server} / {database}",
                    Location = new Point(180, 30),
                    Width = 200,
                    Font = new Font("Century Gothic", 10, FontStyle.Bold)
                };
                
                Button btnChangeConnection = new Button
                {
                    Text = "Change Connection",
                    Location = new Point(20, 70),
                    Width = 150,
                    Height = 30
                };
                
                // Add click handler for change connection button
                btnChangeConnection.Click += (s, args) =>
                {
                    settingsForm.Hide();
                    
                    // Show connection dialog
                    using (ConnectionDialog connectionDialog = new ConnectionDialog())
                    {
                        if (connectionDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Update connection
                            SqlConnectionManager.ConnectionString = connectionDialog.ConnectionString;
                            
                            // Update the label
                            string newServer = SqlConnectionManager.Instance.GetConnectedServerName();
                            string newDb = SqlConnectionManager.Instance.GetConnectedDatabaseName();
                            lblConnectionValue.Text = $"{newServer} / {newDb}";
                            
                            MessageBox.Show("Connection updated successfully.", "Connection Changed", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    
                    settingsForm.Show();
                };
                
                // Add content to Appearance tab
                Label lblTheme = new Label
                {
                    Text = "Theme:",
                    Location = new Point(20, 30),
                    Width = 100
                };
                
                ComboBox cmbTheme = new ComboBox
                {
                    Location = new Point(130, 27),
                    Width = 150,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbTheme.Items.AddRange(new string[] { "Default", "Light", "Dark", "Blue" });
                cmbTheme.SelectedIndex = 0;
                
                // Add theme preview panel
                Panel themePreviewPanel = new Panel
                {
                    Location = new Point(20, 70),
                    Size = new Size(330, 100),
                    BorderStyle = BorderStyle.FixedSingle
                };
                
                // Add theme preview elements
                Button previewButton = new Button
                {
                    Text = "Sample Button",
                    Location = new Point(20, 20),
                    Size = new Size(120, 30)
                };
                
                Label previewLabel = new Label
                {
                    Text = "Sample Text",
                    Location = new Point(20, 60),
                    AutoSize = true
                };
                
                themePreviewPanel.Controls.AddRange(new Control[] { previewButton, previewLabel });
                
                // Add theme change handler
                cmbTheme.SelectedIndexChanged += (s, args) =>
                {
                    UpdateThemePreview(cmbTheme.SelectedItem.ToString(), themePreviewPanel, previewButton, previewLabel);
                };
                
                // Initialize preview with default theme
                UpdateThemePreview("Default", themePreviewPanel, previewButton, previewLabel);
                
                // Add current user info at the bottom
                string currentUser = SqlConnectionManager.CurrentUser ?? "Not logged in";
                Label lblUserInfo = new Label
                {
                    Text = $"Current User: {currentUser}",
                    Location = new Point(20, 280),
                    Width = 400,
                    ForeColor = Color.Gray,
                    Font = new Font("Century Gothic", 9)
                };
                
                // Add only a Close button
                Button btnClose = new Button
                {
                    Text = "Close",
                    Location = new Point(170, 320),
                    Width = 100,
                    Height = 30,
                    DialogResult = DialogResult.Cancel
                };
                
                // Add a tip label about using Enter to save
                Label lblTip = new Label
                {
                    Text = "Press Enter to save changes",
                    Location = new Point(20, 320),
                    Width = 150,
                    ForeColor = Color.Gray,
                    Font = new Font("Century Gothic", 8, FontStyle.Italic)
                };
                
                // Add controls to tabs
                generalTab.Controls.AddRange(new Control[] { chkAutoRefresh, lblRefreshInterval, numRefreshInterval });
                databaseTab.Controls.AddRange(new Control[] { lblConnection, lblConnectionValue, btnChangeConnection });
                appearanceTab.Controls.AddRange(new Control[] { lblTheme, cmbTheme, themePreviewPanel });
                
                // Add tabs to TabControl
                tabControl.Controls.AddRange(new Control[] { generalTab, databaseTab, appearanceTab });
                
                // Add event handler to prevent selecting the Appearance tab
                tabControl.Selecting += (s, args) =>
                {
                    if (args.TabPageIndex == 2) // Appearance tab index
                    {
                        args.Cancel = true;
                        MessageBox.Show("The Appearance settings are coming in a future update!", 
                            "Feature Under Development", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);
                    }
                };
                
                // Add controls to form
                settingsForm.Controls.AddRange(new Control[] { tabControl, lblUserInfo, btnClose, lblTip });
                settingsForm.Controls.Add(titlePanel);
                
                // Handle the KeyDown event to save settings when Enter is pressed
                settingsForm.KeyPreview = true;
                settingsForm.KeyDown += (s, args) =>
                {
                    if (args.KeyCode == Keys.Enter)
                    {
                        // Save settings
                        string selectedTheme = cmbTheme.SelectedItem.ToString();
                        ApplyTheme(selectedTheme);
                        
                        // Save auto-refresh settings
                        _autoRefreshEnabled = chkAutoRefresh.Checked;
                        _autoRefreshInterval = (int)numRefreshInterval.Value;
                        
                        // Update the auto-refresh timer
                        UpdateAutoRefreshTimer();
                        
                        MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        args.Handled = true;
                    }
                };
                
                // Also handle double-click on theme combobox to apply theme immediately
                cmbTheme.DoubleClick += (s, args) =>
                {
                    if (cmbTheme.SelectedItem != null)
                    {
                        string selectedTheme = cmbTheme.SelectedItem.ToString();
                        ApplyTheme(selectedTheme);
                        MessageBox.Show($"{selectedTheme} theme applied!", "Theme Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };
                
                // Show the settings dialog
                DialogResult result = settingsForm.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    // Save settings
                    string selectedTheme = cmbTheme.SelectedItem.ToString();
                    ApplyTheme(selectedTheme);
                    
                    // Save auto-refresh settings
                    _autoRefreshEnabled = chkAutoRefresh.Checked;
                    _autoRefreshInterval = (int)numRefreshInterval.Value;
                    
                    // Update the auto-refresh timer
                    UpdateAutoRefreshTimer();
                    
                    MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void UpdateThemePreview(string themeName, Panel previewPanel, Button previewButton, Label previewLabel)
        {
            switch (themeName)
            {
                case "Light":
                    previewPanel.BackColor = Color.White;
                    previewButton.BackColor = Color.WhiteSmoke;
                    previewButton.ForeColor = Color.Black;
                    previewLabel.ForeColor = Color.Black;
                    break;
                    
                case "Dark":
                    previewPanel.BackColor = Color.FromArgb(50, 50, 50);
                    previewButton.BackColor = Color.FromArgb(80, 80, 80);
                    previewButton.ForeColor = Color.White;
                    previewLabel.ForeColor = Color.White;
                    break;
                    
                case "Blue":
                    previewPanel.BackColor = Color.FromArgb(230, 240, 250);
                    previewButton.BackColor = Color.FromArgb(0, 122, 204);
                    previewButton.ForeColor = Color.White;
                    previewLabel.ForeColor = Color.FromArgb(0, 80, 150);
                    break;
                    
                default: // Default theme
                    previewPanel.BackColor = SystemColors.Control;
                    previewButton.BackColor = SystemColors.ButtonFace;
                    previewButton.ForeColor = SystemColors.ControlText;
                    previewLabel.ForeColor = SystemColors.ControlText;
                    break;
            }
        }
        
        private void ApplyTheme(string themeName)
        {
            // Create color schemes based on the selected theme
            Color backgroundColor;
            Color panelColor;
            Color buttonColor;
            Color textColor;
            Color accentColor;
            Color headerColor;
            Color sidebarColor;
            Color cardColor;
            Color borderColor;
            Color orderPanelColor;
            
            switch (themeName)
            {
                case "Light":
                    backgroundColor = Color.White;
                    panelColor = Color.WhiteSmoke;
                    buttonColor = Color.FromArgb(240, 240, 240);
                    textColor = Color.Black;
                    accentColor = Color.DarkGoldenrod;
                    headerColor = Color.White;
                    sidebarColor = Color.White;
                    cardColor = Color.White;
                    borderColor = Color.FromArgb(230, 230, 230);
                    orderPanelColor = Color.White;
                    break;
                    
                case "Dark":
                    backgroundColor = Color.FromArgb(40, 40, 40);
                    panelColor = Color.FromArgb(60, 60, 60);
                    buttonColor = Color.FromArgb(70, 70, 70);
                    textColor = Color.White;
                    accentColor = Color.Gold;
                    headerColor = Color.FromArgb(50, 50, 50);
                    sidebarColor = Color.FromArgb(30, 30, 30);
                    cardColor = Color.FromArgb(55, 55, 55);
                    borderColor = Color.FromArgb(80, 80, 80);
                    orderPanelColor = Color.FromArgb(50, 50, 50);
                    break;
                    
                case "Blue":
                    backgroundColor = Color.FromArgb(230, 240, 250);
                    panelColor = Color.White;
                    buttonColor = Color.FromArgb(0, 122, 204);
                    textColor = Color.FromArgb(0, 80, 150);
                    accentColor = Color.FromArgb(0, 122, 204);
                    headerColor = Color.FromArgb(0, 122, 204);
                    sidebarColor = Color.FromArgb(240, 245, 255);
                    cardColor = Color.White;
                    borderColor = Color.FromArgb(200, 220, 240);
                    orderPanelColor = Color.White;
                    break;
                    
                default: // Default theme
                    backgroundColor = SystemColors.Control;
                    panelColor = Color.White;
                    buttonColor = SystemColors.ButtonFace;
                    textColor = SystemColors.ControlText;
                    accentColor = Color.DarkGoldenrod;
                    headerColor = Color.White;
                    sidebarColor = Color.White;
                    cardColor = Color.White;
                    borderColor = Color.LightGray;
                    orderPanelColor = Color.White;
                    return; // If default theme, don't change anything
            }
            
            // Apply theme to main form
            this.BackColor = backgroundColor;
            
            // Log the background color being applied
            Console.WriteLine($"Applying theme: {themeName}, Background color: {backgroundColor}");
            
            // Apply theme to all panels in the main form
            ApplyColorToControlsOfType<Panel>(this, panelColor, textColor);
            ApplyColorToControlsOfType<Bunifu.UI.WinForms.BunifuPanel>(this, panelColor, textColor);
            
            // Special handling for main display panel
            panDisplayItem.BackColor = backgroundColor;
            
            // Direct access to specific panels that might need special handling
            try
            {
                // Try to find the right-side order panel by name or location
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel || ctrl is Bunifu.UI.WinForms.BunifuPanel)
                    {
                        // If this is the right-side order panel
                        if (ctrl.Location.X > this.Width - 500 && ctrl.Height > 400)
                        {
                            Console.WriteLine($"Found right panel: {ctrl.Name}, setting to {orderPanelColor}");
                            ctrl.BackColor = orderPanelColor;
                            
                            // Apply to all child controls
                            foreach (Control childCtrl in ctrl.Controls)
                            {
                                if (childCtrl is Label || childCtrl is Bunifu.UI.WinForms.BunifuLabel)
                                {
                                    childCtrl.ForeColor = textColor;
                                }
                                else if (childCtrl is Panel || childCtrl is Bunifu.UI.WinForms.BunifuPanel)
                                {
                                    childCtrl.BackColor = orderPanelColor;
                                }
                                else if (childCtrl is TextBox)
                                {
                                    if (themeName == "Dark")
                                    {
                                        childCtrl.BackColor = Color.FromArgb(70, 70, 70);
                                        childCtrl.ForeColor = Color.White;
                                    }
                                }
                                else if (childCtrl is Button)
                                {
                                    childCtrl.BackColor = buttonColor;
                                    childCtrl.ForeColor = themeName == "Dark" ? Color.White : textColor;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying theme to order panel: {ex.Message}");
            }
            
            // Apply theme to all product item panels by direct access
            try
            {
                foreach (Control ctrl in panDisplayItem.Controls)
                {
                    if (ctrl is Bunifu.UI.WinForms.BunifuPanel itemPanel)
                    {
                        Console.WriteLine($"Applying to item panel: {cardColor}");
                        itemPanel.BackgroundColor = cardColor;
                        itemPanel.BorderColor = borderColor;
                        
                        // Apply theme to all controls within each product panel
                        foreach (Control innerCtrl in itemPanel.Controls)
                        {
                            if (innerCtrl is Bunifu.UI.WinForms.BunifuLabel label)
                            {
                                if (label.Text.StartsWith("$"))
                                {
                                    // Price labels keep their accent color
                                    label.ForeColor = accentColor;
                                }
                                else
                                {
                                    label.ForeColor = textColor;
                                }
                            }
                            else if (innerCtrl is Label labelStd)
                            {
                                if (labelStd.Text.StartsWith("$"))
                                {
                                    labelStd.ForeColor = accentColor;
                                }
                                else
                                {
                                    labelStd.ForeColor = textColor;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying theme to product panels: {ex.Message}");
            }
            
            // Apply to buttons
            ApplyColorToControlsOfType<Button>(this, buttonColor, themeName == "Blue" || themeName == "Dark" ? Color.White : textColor);
            
            // Apply to Bunifu buttons if any
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType().Name.Contains("BunifuButton"))
                {
                    ctrl.BackColor = buttonColor;
                    ctrl.ForeColor = themeName == "Blue" || themeName == "Dark" ? Color.White : textColor;
                }
            }
            
            // Apply to navigation buttons specifically
            btnDashboard.BackColor = buttonColor;
            btnAllForm.BackColor = buttonColor;
            btnStatistic.BackColor = buttonColor;
            btnInvoice.BackColor = buttonColor;
            btnSetting.BackColor = buttonColor;
            btnLogout.BackColor = buttonColor;
            btnExitProgram.BackColor = buttonColor;
            
            // Apply to order panel - ensure all controls in the order panel get themed
            if (bunifuPanel3 != null)
            {
                Console.WriteLine($"Applying to bunifuPanel3: {orderPanelColor}");
                bunifuPanel3.BackgroundColor = orderPanelColor;
                
                // Apply theme to all controls in the order panel
                ApplyColorToControlsOfType<Bunifu.UI.WinForms.BunifuLabel>(bunifuPanel3, 
                    Color.Transparent, themeName == "Dark" ? Color.White : textColor);
                
                // Special handling for price labels in the order panel
                if (bunifuLabel35 != null) bunifuLabel35.ForeColor = accentColor; // Subtotal
                if (bunifuLabel36 != null) bunifuLabel36.ForeColor = accentColor; // Discount
                if (bunifuLabel38 != null) bunifuLabel38.ForeColor = accentColor; // Total
            }
            
            // Apply to the order details container
            if (panOrderDetailOuter != null)
            {
                Console.WriteLine($"Applying to panOrderDetailOuter: {(themeName == "Dark" ? Color.FromArgb(45, 45, 45) : backgroundColor)}");
                panOrderDetailOuter.BackColor = themeName == "Dark" ? Color.FromArgb(45, 45, 45) : backgroundColor;
                
                // Apply theme to all order item panels
                foreach (Control ctrl in panOrderDetailOuter.Controls)
                {
                    if (ctrl is Bunifu.UI.WinForms.BunifuPanel orderItemPanel)
                    {
                        orderItemPanel.BackgroundColor = themeName == "Dark" ? Color.FromArgb(55, 55, 55) : cardColor;
                        orderItemPanel.BorderColor = borderColor;
                        
                        // Theme all controls within the order item panel
                        foreach (Control innerCtrl in orderItemPanel.Controls)
                        {
                            if (innerCtrl is Bunifu.UI.WinForms.BunifuLabel label)
                            {
                                if (label.Text.StartsWith("$"))
                                {
                                    label.ForeColor = accentColor;
                                }
                                else
                                {
                                    label.ForeColor = textColor;
                                }
                            }
                        }
                    }
                }
            }
            
            // Direct approach - find all panels that match certain size/location criteria that could be item panels
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Bunifu.UI.WinForms.BunifuPanel panel)
                {
                    // Product cards are typically around 280x280
                    if (panel.Width >= 250 && panel.Width <= 300 && panel.Height >= 250 && panel.Height <= 300)
                    {
                        Console.WriteLine($"Found product card panel: {panel.Name}");
                        panel.BackgroundColor = cardColor;
                        panel.BorderColor = borderColor;
                        
                        // Theme child elements
                        foreach (Control innerCtrl in panel.Controls)
                        {
                            if (innerCtrl is Label || innerCtrl is Bunifu.UI.WinForms.BunifuLabel)
                            {
                                if (innerCtrl.Text.StartsWith("$"))
                                {
                                    innerCtrl.ForeColor = accentColor;
                                }
                                else
                                {
                                    innerCtrl.ForeColor = textColor;
                                }
                            }
                        }
                    }
                    // Order panel is typically wider
                    else if (panel.Width > 400)
                    {
                        Console.WriteLine($"Found order panel: {panel.Name}");
                        panel.BackgroundColor = orderPanelColor;
                    }
                }
            }
            
            // Apply to category buttons with special handling to preserve the active category
            btnServices.BackColor = panelColor;
            btnFoods.BackColor = panelColor;
            btnDrinks.BackColor = panelColor;
            btnServices.ForeColor = textColor;
            btnFoods.ForeColor = textColor;
            btnDrinks.ForeColor = textColor;
            
            // Reset and apply the active category style with the new theme colors
            SetCategoryButtonStyles(_currentCategory);
            
            // Apply to labels throughout the application
            ApplyColorToControlsOfType<Label>(this, Color.Transparent, textColor);
            ApplyColorToControlsOfType<Bunifu.UI.WinForms.BunifuLabel>(this, Color.Transparent, textColor);
            
            // Apply to sidebar
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.Name == "SidePanel" || ctrl.Name.Contains("sidebar") || ctrl.Name.Contains("side_panel"))
                {
                    ctrl.BackColor = sidebarColor;
                    // Apply to all controls in sidebar
                    foreach (Control sidebarCtrl in ctrl.Controls)
                    {
                        if (sidebarCtrl is Button)
                        {
                            sidebarCtrl.BackColor = sidebarColor;
                            sidebarCtrl.ForeColor = textColor;
                        }
                    }
                }
            }
            
            // Apply to TextBoxes
            ApplyColorToControlsOfType<TextBox>(this, 
                themeName == "Dark" ? Color.FromArgb(70, 70, 70) : Color.White, 
                textColor);
            
            // Apply to Bunifu TextBoxes if any
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType().Name.Contains("BunifuTextBox"))
                {
                    if (themeName == "Dark")
                    {
                        ctrl.BackColor = Color.FromArgb(70, 70, 70);
                        ctrl.ForeColor = Color.White;
                    }
                    else
                    {
                        ctrl.BackColor = Color.White;
                        ctrl.ForeColor = textColor;
                    }
                }
            }
            
            // Force a complete redraw
            foreach (Control control in this.Controls)
            {
                control.Invalidate();
                control.Update();
            }
            this.Invalidate(true);
            this.Update();
            Application.DoEvents();
        }
        
        // Helper method to apply colors to all controls of a specific type
        private void ApplyColorToControlsOfType<T>(Control container, Color backColor, Color foreColor) where T : Control
        {
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is T)
                {
                    ctrl.BackColor = backColor;
                    ctrl.ForeColor = foreColor;
                }
                
                // Recursively apply to child controls
                if (ctrl.Controls.Count > 0)
                {
                    ApplyColorToControlsOfType<T>(ctrl, backColor, foreColor);
                }
            }
        }

        // Auto-refresh variables
        private bool _autoRefreshEnabled = false;
        private int _autoRefreshInterval = 5;
        private System.Threading.Timer _autoRefreshTimer;

        private void UpdateAutoRefreshTimer()
        {
            // Dispose of existing timer if any
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Dispose();
                _autoRefreshTimer = null;
            }

            // Create a new timer if auto-refresh is enabled
            if (_autoRefreshEnabled)
            {
                // Convert minutes to milliseconds
                int intervalMs = _autoRefreshInterval * 60 * 1000;
                
                // Create a new timer that triggers the refresh
                _autoRefreshTimer = new System.Threading.Timer(AutoRefreshCallback, null, intervalMs, intervalMs);
                
                Console.WriteLine($"Auto-refresh timer started with interval: {_autoRefreshInterval} minutes");
            }
            else
            {
                Console.WriteLine("Auto-refresh disabled");
            }
        }

        private void AutoRefreshCallback(object state)
        {
            // We need to invoke the UI thread to refresh the product list
            try
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    Console.WriteLine("Auto-refresh triggered");
                    try
                    {
                        // Call the refresh method
                        BtnRefresh_Click(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in auto-refresh: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking auto-refresh: {ex.Message}");
            }
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up the auto-refresh timer when the form is closing
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Dispose();
                _autoRefreshTimer = null;
            }
        }
    }

}