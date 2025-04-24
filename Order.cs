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
    // Observer Pattern: Interface for observers to receive notifications about order updates
    public interface IOrderObserver
    {
        void OnOrderUpdated(); // Called when an order is inserted, updated, or deleted
    }

    public partial class Order : Form, IOrderObserver
    {
        private readonly OrderManager _orderManager;
        private DataTable _ordersTable;
        private bool _isLoading = false;

        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public Order()
        {
            InitializeComponent();

            // Setup DataGridView general properties
            SetupDataGridView();
            
            // Configure the DataGridView columns
            ConfigureDataGridViewColumns();
            
            // Add an event handler for grid layout
            dgvPayment.DataBindingComplete += DgvOrder_DataBindingComplete;

            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) => {
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
                    _orderManager = new OrderManager();
                    _orderManager.AddObserver(this); // Observer Pattern: Register form as an observer
                    LoadOrders();
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


        // Load all orders into the DataGridView
        private void LoadOrders()
        {
            if (IsDesignMode) return;

            try
            {
                // Store the currently selected order ID if any
                int? selectedOrderId = null;
                if (dgvPayment.CurrentRow != null && !dgvPayment.CurrentRow.IsNewRow)
                {
                    var idCell = dgvPayment.CurrentRow.Cells["OrderId"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedOrderId = Convert.ToInt32(idCell.Value);
                    }
                }

                // Set loading flag to prevent reentrant calls
                _isLoading = true;
                
                // Prevent auto-generation of columns 
                dgvPayment.AutoGenerateColumns = false;
                
                // Disable AutoSizeColumnsMode to prevent auto-resizing
                dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Temporarily suspend layout and events
                dgvPayment.SuspendLayout();
                
                // Load the data
                _ordersTable = _orderManager.GetAllOrders();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvPayment.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
                dgvPayment.DataSource = _ordersTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_ordersTable != null && _ordersTable.DefaultView != null)
                    {
                        _ordersTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvPayment.Columns["OrderId"] != null)
                    dgvPayment.Columns["OrderId"].Width = 60;
                    
                if (dgvPayment.Columns["CustomerId"] != null)
                    dgvPayment.Columns["CustomerId"].Width = 80;
                    
                if (dgvPayment.Columns["UserId"] != null)
                    dgvPayment.Columns["UserId"].Width = 60;
                    
                if (dgvPayment.Columns["OrderTime"] != null)
                    dgvPayment.Columns["OrderTime"].Width = 150;
                
                if (dgvPayment.Columns["TotalAmount"] != null)
                    dgvPayment.Columns["TotalAmount"].Width = 100;
                    
                if (dgvPayment.Columns["Status"] != null)
                    dgvPayment.Columns["Status"].Width = 100;
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvPayment.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvPayment.Refresh();
                
                // Restore selection if possible
                if (selectedOrderId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvPayment.Rows)
                    {
                        var cell = row.Cells["OrderId"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedOrderId.Value)
                        {
                            row.Selected = true;
                            dgvPayment.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error loading orders: " + ex.Message);
            }
            finally
            {
                // Reset loading flag
                _isLoading = false;
            }
        }

        // Wire up events
        private void WireUpEvents()
        {
            if (IsDesignMode) return;
            
            // Attach search event for proper method
            if (txtSearch != null)
            {
                // Remove any existing handlers first to avoid duplicates
                txtSearch.TextChanged -= TxtSearch_TextChanged;
                // Add our handler to the correct event
                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
            
            // Add events for direct editing in the grid
            dgvPayment.CellEndEdit += DgvOrder_CellEndEdit;
            dgvPayment.UserAddedRow += DgvOrder_UserAddedRow;
            dgvPayment.UserDeletingRow += DgvOrder_UserDeletingRow;
        }

        // Search functionality
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchValue = txtSearch.Text.Trim().ToLower();
                
                // Check if we have data to filter
                if (_ordersTable == null || _ordersTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _ordersTable.DefaultView.RowFilter = string.Empty;
                    return;
                }

                // Build filter with exact string matching for better accuracy
                string filter = string.Format(
                    "Convert(OrderId, 'System.String') LIKE '%{0}%' OR Convert(CustomerId, 'System.String') LIKE '%{0}%' OR " +
                    "Convert(UserId, 'System.String') LIKE '%{0}%' OR Status LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _ordersTable.DefaultView.RowFilter = filter;
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_ordersTable != null && _ordersTable.DefaultView != null)
                {
                    _ordersTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }

        // Handle DataGridView binding complete event
        private void DgvOrder_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Apply alternating row colors
            foreach (DataGridViewRow row in dgvPayment.Rows)
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
            if (dgvPayment.Columns["OrderId"] != null)
                dgvPayment.Columns["OrderId"].Width = 60;
                
            if (dgvPayment.Columns["CustomerId"] != null)
                dgvPayment.Columns["CustomerId"].Width = 80;
                
            if (dgvPayment.Columns["UserId"] != null)
                dgvPayment.Columns["UserId"].Width = 60;
                
            if (dgvPayment.Columns["OrderTime"] != null)
                dgvPayment.Columns["OrderTime"].Width = 150;
                
            if (dgvPayment.Columns["TotalAmount"] != null)
                dgvPayment.Columns["TotalAmount"].Width = 100;
                
            if (dgvPayment.Columns["Discount"] != null)
                dgvPayment.Columns["Discount"].Width = 80;
                
            if (dgvPayment.Columns["FinalAmount"] != null)
                dgvPayment.Columns["FinalAmount"].Width = 100;
                
            if (dgvPayment.Columns["Status"] != null)
                dgvPayment.Columns["Status"].Width = 100;
                
            if (dgvPayment.Columns["Notes"] != null)
                dgvPayment.Columns["Notes"].Width = 256;
            
            // Ensure Status column is properly styled if it's a ComboBox
            if (dgvPayment.Columns["Status"] is DataGridViewComboBoxColumn statusCol)
            {
                statusCol.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                statusCol.FlatStyle = FlatStyle.Popup;
            }
            
            // Hide any columns that shouldn't be visible
            HideColumns();
            
            // Disable column auto-sizing one more time to be sure
            dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Ensure header styling is maintained
            dgvPayment.EnableHeadersVisualStyles = false;
            dgvPayment.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 153, 0);
            dgvPayment.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        // Observer Pattern: Implementation of IOrderObserver interface
        public void OnOrderUpdated()
        {
            LoadOrders(); // Refresh the DataGridView when notified
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Configure the DataGridView general properties
        private void SetupDataGridView()
        {
            try
            {
                // Disable auto-sizing to prevent column width changes
                dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Set selection mode to full row select
                dgvPayment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                
                // Disable multiple selection
                dgvPayment.MultiSelect = false;
                
                // Prevent default Windows styling to enable our custom styling
                dgvPayment.EnableHeadersVisualStyles = false;
                
                // Set column header style (orange/yellow like Consumable)
                dgvPayment.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(255, 153, 0),  // Orange header
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.False
                };
                
                // Configure header height to match Consumable
                dgvPayment.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvPayment.ColumnHeadersHeight = 40;
                
                // Setup cell and row appearance
                dgvPayment.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),  // Light orange selection
                    SelectionForeColor = Color.Black
                };
                
                // Row alternating color - similar to Consumable
                dgvPayment.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 242, 204),  // Light yellow alternate rows
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),
                    SelectionForeColor = Color.Black
                };
                
                // Setting other appearance properties
                dgvPayment.BackgroundColor = Color.White;
                dgvPayment.BorderStyle = BorderStyle.None;
                dgvPayment.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvPayment.RowTemplate.Height = 30;  // Taller rows like Consumable
                dgvPayment.RowHeadersVisible = false;
                dgvPayment.GridColor = Color.FromArgb(255, 226, 173);  // Light orange grid lines
                
                // Other settings
                dgvPayment.AllowUserToResizeColumns = false;
                dgvPayment.AllowUserToResizeRows = false;
                dgvPayment.ReadOnly = false;
                dgvPayment.AutoGenerateColumns = false;
                dgvPayment.StandardTab = true;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error setting up DataGridView: " + ex.Message);
            }
        }

        // Configure the DataGridView to use custom columns
        private void ConfigureDataGridViewColumns()
        {
            if (IsDesignMode) return;

            try
            {
                // Clear existing columns to control the order
                dgvPayment.Columns.Clear();
                
                // Disable AutoSizeColumnsMode to ensure our width settings are respected
                dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Create OrderId column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "OrderId",
                    HeaderText = "ID",
                    DataPropertyName = "OrderId",
                    Width = 60,
                    MinimumWidth = 60,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(idColumn);

                // Create CustomerId column - SECOND position
                DataGridViewTextBoxColumn customerIdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "CustomerId",
                    HeaderText = "Customer ID",
                    DataPropertyName = "CustomerId",
                    Width = 80,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(customerIdColumn);

                // Create UserId column - THIRD position
                DataGridViewTextBoxColumn userIdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "UserId",
                    HeaderText = "User ID",
                    DataPropertyName = "UserId",
                    Width = 60,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(userIdColumn);

                // Create OrderTime column - FOURTH position
                DataGridViewTextBoxColumn timeColumn = new DataGridViewTextBoxColumn
                {
                    Name = "OrderTime",
                    HeaderText = "Order Time",
                    DataPropertyName = "OrderTime",
                    Width = 150,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy hh:mm tt",
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvPayment.Columns.Add(timeColumn);

                // Create TotalAmount column - FIFTH position
                DataGridViewTextBoxColumn totalColumn = new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Total Amount",
                    DataPropertyName = "TotalAmount",
                    Width = 100,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvPayment.Columns.Add(totalColumn);

                // Create Discount column - SIXTH position
                DataGridViewTextBoxColumn discountColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Discount",
                    HeaderText = "Discount",
                    DataPropertyName = "Discount",
                    Width = 80,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvPayment.Columns.Add(discountColumn);

                // Create FinalAmount column - SEVENTH position
                DataGridViewTextBoxColumn finalColumn = new DataGridViewTextBoxColumn
                {
                    Name = "FinalAmount",
                    HeaderText = "Final Amount",
                    DataPropertyName = "FinalAmount",
                    Width = 100,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvPayment.Columns.Add(finalColumn);

                // Create Status column - EIGHTH position
                DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    Width = 100,
                    ReadOnly = false,
                    FlatStyle = FlatStyle.Popup,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                
                // Add status options
                statusColumn.Items.AddRange(new[] { "New", "Processing", "Completed", "Cancelled" });
                dgvPayment.Columns.Add(statusColumn);

                // Create Notes column - NINTH position
                DataGridViewTextBoxColumn notesColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Notes",
                    HeaderText = "Notes",
                    DataPropertyName = "Notes",
                    Width = 150,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvPayment.Columns.Add(notesColumn);
                
                // Apply consistent header styling
                foreach (DataGridViewColumn col in dgvPayment.Columns)
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    col.HeaderCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                
                // Disable column resizing by users
                dgvPayment.AllowUserToResizeColumns = false;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        // Helper method to hide specific columns if needed
        private void HideColumns()
        {
            // Check if columns exist first
            if (dgvPayment.Columns.Count == 0) return;

            try
            {
                // Hide columns that shouldn't be visible
                string[] columnsToHide = { "CustomerId", "UserId" };
                
                foreach (string colName in columnsToHide)
                {
                    if (dgvPayment.Columns.Contains(colName))
                    {
                        dgvPayment.Columns[colName].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error hiding columns: " + ex.Message);
            }
        }

        // Handle user deleting a row directly in the grid
        private void DgvOrder_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["OrderId"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int orderId = Convert.ToInt32(idCell.Value);
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete Order #{orderId}?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _orderManager.DeleteOrder(orderId);
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

        // Handle user adding a new row
        private void DgvOrder_UserAddedRow(object sender, DataGridViewRowEventArgs e)
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

        // Handle cell editing completion
        private void DgvOrder_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;
            
            try
            {
                // Don't immediately reload, which could cause reentrant issues
                // Instead, save the changes to the database without reloading
                
                DataGridViewRow row = dgvPayment.Rows[e.RowIndex];
                
                // Check if this is a new row
                bool isNewRow = row.IsNewRow;
                
                if (!isNewRow)
                {
                    // Get values from the row, with proper null handling
                    int orderId = 0;
                    if (row.Cells["OrderId"].Value != null && row.Cells["OrderId"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["OrderId"].Value.ToString(), out orderId);
                    }
                    
                    int customerId = 0;
                    if (row.Cells["CustomerId"].Value != null && row.Cells["CustomerId"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["CustomerId"].Value.ToString(), out customerId);
                    }
                    
                    int userId = 0;
                    if (row.Cells["UserId"].Value != null && row.Cells["UserId"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["UserId"].Value.ToString(), out userId);
                    }
                    
                    // Get order time
                    DateTime orderTime = DateTime.Now;
                    if (row.Cells["OrderTime"].Value != null && row.Cells["OrderTime"].Value != DBNull.Value)
                    {
                        DateTime.TryParse(row.Cells["OrderTime"].Value.ToString(), out orderTime);
                    }
                    
                    // Get total amount
                    decimal totalAmount = 0;
                    if (row.Cells["TotalAmount"].Value != null && row.Cells["TotalAmount"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["TotalAmount"].Value.ToString(), out totalAmount);
                    }
                    
                    // Get discount
                    decimal discount = 0;
                    if (row.Cells["Discount"].Value != null && row.Cells["Discount"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["Discount"].Value.ToString(), out discount);
                    }
                    
                    // Get final amount
                    decimal finalAmount = totalAmount - discount;
                    if (row.Cells["FinalAmount"].Value != null && row.Cells["FinalAmount"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["FinalAmount"].Value.ToString(), out finalAmount);
                    }
                    
                    // Get status
                    string status = "New";
                    if (row.Cells["Status"].Value != null && row.Cells["Status"].Value != DBNull.Value)
                    {
                        status = row.Cells["Status"].Value.ToString();
                    }
                    
                    // Get notes
                    string notes = "";
                    if (row.Cells["Notes"].Value != null && row.Cells["Notes"].Value != DBNull.Value)
                    {
                        notes = row.Cells["Notes"].Value.ToString();
                    }
                    
                    // If it's a valid record with customer ID and user ID
                    if (customerId > 0 && userId > 0)
                    {
                        // Create order model
                        OrderModel order = new OrderModel
                        {
                            OrderId = orderId,
                            CustomerId = customerId,
                            UserId = userId,
                            OrderTime = orderTime,
                            TotalAmount = totalAmount,
                            Discount = discount,
                            FinalAmount = finalAmount,
                            Notes = notes,
                            Status = status
                        };
                        
                        if (orderId > 0)
                        {
                            // Update existing record
                            _orderManager.UpdateOrder(order);
                        }
                        else
                        {
                            // Insert new record
                            _orderManager.InsertOrder(order);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadOrders();
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
    }

    // Manager class implementing Observer Pattern for business logic
    public class OrderManager
    {
        private readonly OrderDataAccess _dataAccess;
        private readonly List<IOrderObserver> _observers;

        public OrderManager()
        {
            _dataAccess = new OrderDataAccess();
            _observers = new List<IOrderObserver>();
        }

        // Observer Pattern: Methods to register and notify observers
        public void AddObserver(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                // Using explicit cast to avoid ambiguity errors
                ((IOrderObserver)observer).OnOrderUpdated();
            }
        }

        public DataTable GetAllOrders()
        {
            return _dataAccess.GetAll();
        }

        public void InsertOrder(OrderModel order)
        {
            _dataAccess.Insert(order);
            NotifyObservers();
        }

        public void UpdateOrder(OrderModel order)
        {
            _dataAccess.Update(order);
            NotifyObservers();
        }

        public void DeleteOrder(int orderId)
        {
            _dataAccess.Delete(orderId);
            NotifyObservers();
        }
    }
}