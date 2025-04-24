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
    // Interface for Observer Pattern
    public interface IInvoiceObserver
    {
        void OnInvoiceUpdated();
    }

    public partial class Invoice : Form, IInvoiceObserver
    {
        private readonly InvoiceManager _invoiceManager;
        private DataTable _invoicesTable;
        private bool _isLoading = false;
        
        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public Invoice()
        {
            InitializeComponent();
            
            // Setup DataGridView general properties
            SetupDataGridView();
            
            // Configure the DataGridView columns
            ConfigureDataGridViewColumns();
            
            // Add an event handler for grid layout
            dgvInvoice.DataBindingComplete += DgvInvoice_DataBindingComplete;

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
                       _invoiceManager = new InvoiceManager();
            _invoiceManager.AddObserver(this); // Register form as an observer
            LoadInvoices();
                    WireUpEvents(); // Attach event handlers
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

        // Load all invoices into the DataGridView
        private void LoadInvoices()
        {
            if (IsDesignMode) return;

            try
            {
                // Store the currently selected invoice ID if any
                int? selectedInvoiceId = null;
                if (dgvInvoice.CurrentRow != null && !dgvInvoice.CurrentRow.IsNewRow)
                {
                    var idCell = dgvInvoice.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedInvoiceId = Convert.ToInt32(idCell.Value);
                    }
                }

                // Set loading flag to prevent reentrant calls
                _isLoading = true;
                
                // Prevent auto-generation of columns 
                dgvInvoice.AutoGenerateColumns = false;
                
                // Disable AutoSizeColumnsMode to prevent auto-resizing
                dgvInvoice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Temporarily suspend layout and events
                dgvInvoice.SuspendLayout();
                
                // Load the data
            _invoicesTable = _invoiceManager.GetAllInvoices();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvInvoice.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
            dgvInvoice.DataSource = _invoicesTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_invoicesTable != null && _invoicesTable.DefaultView != null)
                    {
                        _invoicesTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvInvoice.Columns["ID"] != null)
                    dgvInvoice.Columns["ID"].Width = 50;
                    
                if (dgvInvoice.Columns["OrderID"] != null)
                {
                    dgvInvoice.Columns["OrderID"].Width = 70;
                    dgvInvoice.Columns["OrderID"].Visible = false;
                }
                    
                if (dgvInvoice.Columns["TotalAmount"] != null)
                    dgvInvoice.Columns["TotalAmount"].Width = 120;
                    
                if (dgvInvoice.Columns["InvoiceDate"] != null)
                    dgvInvoice.Columns["InvoiceDate"].Width = 150;
                
                if (dgvInvoice.Columns["Notes"] != null)
                    dgvInvoice.Columns["Notes"].Width = 546;
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvInvoice.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvInvoice.Refresh();
                
                // Restore selection if possible
                if (selectedInvoiceId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvInvoice.Rows)
                    {
                        var cell = row.Cells["ID"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedInvoiceId.Value)
                        {
                            row.Selected = true;
                            dgvInvoice.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error loading invoices: " + ex.Message);
            }
            finally
            {
                // Reset loading flag
                _isLoading = false;
            }
        }
        
        // Helper method to hide specific columns
        private void HideColumns()
        {
            // Check if columns exist first
            if (dgvInvoice.Columns.Count == 0) return;

            try
            {
                // Hide database columns that shouldn't be visible (if any)
                string[] columnsToHide = { };
                
                foreach (string colName in columnsToHide)
                {
                    if (dgvInvoice.Columns.Contains(colName))
                    {
                        dgvInvoice.Columns[colName].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        // Wire up events (since not all may be set in designer)
        private void WireUpEvents()
        {
            if (IsDesignMode) return;      
            
            // Attach search event using the proper method
            if (txtSearch != null)
            {
                // Remove any existing handlers first to avoid duplicates
                txtSearch.TextChanged -= TxtSearch_TextChanged;
                // Add our handler to the correct event
                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
            
            // Add events for direct editing in the grid
            dgvInvoice.CellEndEdit += DgvInvoice_CellEndEdit;
            dgvInvoice.UserAddedRow += DgvInvoice_UserAddedRow;
            dgvInvoice.UserDeletingRow += DgvInvoice_UserDeletingRow;
            
            // Add exit button handler
            btnExitProgram.Click += btnExitProgram_Click;
        }
        
        // Configure the DataGridView general properties
        private void SetupDataGridView()
        {
            try
            {
                // Disable auto-sizing to prevent column width changes
                dgvInvoice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Set selection mode to full row select
                dgvInvoice.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                
                // Disable multiple selection
                dgvInvoice.MultiSelect = false;
                
                // Prevent default Windows styling to enable our custom styling
                dgvInvoice.EnableHeadersVisualStyles = false;
                
                // Set column header style (orange/yellow like Consumable)
                dgvInvoice.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(255, 153, 0),  // Orange header like in Consumable
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.False
                };
                
                // Configure header height to match Consumable
                dgvInvoice.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvInvoice.ColumnHeadersHeight = 40;
                
                // Setup cell and row appearance
                dgvInvoice.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),  // Light orange selection
                    SelectionForeColor = Color.Black
                };
                
                // Row alternating color - similar to Consumable
                dgvInvoice.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 242, 204),  // Light yellow alternate rows
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),
                    SelectionForeColor = Color.Black
                };
                
                // Setting other appearance properties
                dgvInvoice.BackgroundColor = Color.White;
                dgvInvoice.BorderStyle = BorderStyle.None;
                dgvInvoice.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvInvoice.RowTemplate.Height = 30;  // Taller rows like Consumable
                dgvInvoice.RowHeadersVisible = false;
                dgvInvoice.GridColor = Color.FromArgb(255, 226, 173);  // Light orange grid lines
                
                // Other settings
                dgvInvoice.AllowUserToResizeColumns = false;
                dgvInvoice.AllowUserToResizeRows = false;
                dgvInvoice.ReadOnly = false;
                dgvInvoice.AutoGenerateColumns = false;
                dgvInvoice.StandardTab = true;
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
                dgvInvoice.Columns.Clear();
                
                // Disable AutoSizeColumnsMode to ensure our width settings are respected
                dgvInvoice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Create ID column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ID",
                    HeaderText = "ID",
                    DataPropertyName = "InvoiceId",
                    Width = 50,
                    MinimumWidth = 50,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvInvoice.Columns.Add(idColumn);

                // Create OrderID column - SECOND position (but hidden)
                DataGridViewTextBoxColumn orderIdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "OrderID",
                    HeaderText = "Order ID",
                    DataPropertyName = "OrderId",
                    Width = 70,
                    ReadOnly = false,
                    Visible = false, // Hide this column
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvInvoice.Columns.Add(orderIdColumn);

                // Create TotalAmount column - THIRD position (moved before InvoiceDate)
                DataGridViewTextBoxColumn amountColumn = new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Total Amount",
                    DataPropertyName = "TotalAmount",
                    Width = 120,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2", // Keep the currency format
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvInvoice.Columns.Add(amountColumn);

                // Create InvoiceDate column - FOURTH position
                DataGridViewTextBoxColumn dateColumn = new DataGridViewTextBoxColumn
                {
                    Name = "InvoiceDate",
                    HeaderText = "Invoice Date",
                    DataPropertyName = "InvoiceDate",
                    Width = 150,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt", // Keep the date format
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvInvoice.Columns.Add(dateColumn);

                // Create Notes column - FIFTH position (expanded width)
                DataGridViewTextBoxColumn notesColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Notes",
                    HeaderText = "Notes",
                    DataPropertyName = "Notes",
                    Width = 546, 
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvInvoice.Columns.Add(notesColumn);
                
                // Make sure first column (ID) is visible
                if (dgvInvoice.Columns["ID"] != null)
                {
                    dgvInvoice.Columns["ID"].Visible = true;
                }
                
                // Disable column resizing by users
                dgvInvoice.AllowUserToResizeColumns = false;
                
                // Apply consistent header styling
                foreach (DataGridViewColumn col in dgvInvoice.Columns)
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    col.HeaderCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        // Filter invoices based on search input
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchValue = txtSearch.Text.Trim().ToLower();
                
                // Check if we have data to filter
                if (_invoicesTable == null || _invoicesTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _invoicesTable.DefaultView.RowFilter = string.Empty;
                    return;
                }

                // Build filter with exact string matching for better accuracy
                string filter = string.Format(
                    "Convert(InvoiceId, 'System.String') LIKE '%{0}%' OR Convert(OrderId, 'System.String') LIKE '%{0}%' OR " +
                    "Convert(TotalAmount, 'System.String') LIKE '%{0}%' OR Convert(InvoiceDate, 'System.String') LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _invoicesTable.DefaultView.RowFilter = filter;
                
                // Debug output
                Console.WriteLine($"Search value: '{searchValue}', Filter: '{filter}', Results: {_invoicesTable.DefaultView.Count}");
                
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_invoicesTable != null && _invoicesTable.DefaultView != null)
                {
                    _invoicesTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }
        
        private void DgvInvoice_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Apply alternating row colors
            foreach (DataGridViewRow row in dgvInvoice.Rows)
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
            if (dgvInvoice.Columns["ID"] != null)
                dgvInvoice.Columns["ID"].Width = 50;
                
            if (dgvInvoice.Columns["OrderID"] != null)
            {
                dgvInvoice.Columns["OrderID"].Width = 70;
                dgvInvoice.Columns["OrderID"].Visible = false;
            }
            
            if (dgvInvoice.Columns["TotalAmount"] != null)
                dgvInvoice.Columns["TotalAmount"].Width = 120;
                
            if (dgvInvoice.Columns["InvoiceDate"] != null)
                dgvInvoice.Columns["InvoiceDate"].Width = 150;
                
            if (dgvInvoice.Columns["Notes"] != null)
                dgvInvoice.Columns["Notes"].Width = 546;
            
            // Disable column auto-sizing one more time to be sure
            dgvInvoice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Ensure header styling is maintained
            dgvInvoice.EnableHeadersVisualStyles = false;
            dgvInvoice.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 153, 0);
            dgvInvoice.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void DgvInvoice_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["ID"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int invoiceId = Convert.ToInt32(idCell.Value);
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete Invoice #{invoiceId}?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _invoiceManager.DeleteInvoice(invoiceId);
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

        private void DgvInvoice_UserAddedRow(object sender, DataGridViewRowEventArgs e)
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

        private void DgvInvoice_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;
            
            try
            {
                // Don't immediately reload, which could cause reentrant issues
                // Instead, save the changes to the database without reloading
                
                DataGridViewRow row = dgvInvoice.Rows[e.RowIndex];
                
                // Check if this is a new row
                bool isNewRow = row.IsNewRow;
                
                if (!isNewRow)
                {
                    // Get values from the row, with proper null handling
                    int invoiceId = 0;
                    if (row.Cells["ID"].Value != null && row.Cells["ID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["ID"].Value.ToString(), out invoiceId);
                    }
                    
                    // Get OrderId with null check
                    int orderId = 0;
                    if (row.Cells["OrderID"].Value != null && row.Cells["OrderID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["OrderID"].Value.ToString(), out orderId);
                    }
                    
                    // Get InvoiceDate with null check
                    DateTime invoiceDate = DateTime.Now;
                    if (row.Cells["InvoiceDate"].Value != null && row.Cells["InvoiceDate"].Value != DBNull.Value)
                    {
                        DateTime.TryParse(row.Cells["InvoiceDate"].Value.ToString(), out invoiceDate);
                    }
                    
                    // Get TotalAmount with null check
                    decimal totalAmount = 0;
                    if (row.Cells["TotalAmount"].Value != null && row.Cells["TotalAmount"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["TotalAmount"].Value.ToString(), out totalAmount);
                    }
                    
                    // Get Notes with null check
                    string notes = "";
                    if (row.Cells["Notes"].Value != null && row.Cells["Notes"].Value != DBNull.Value)
                    {
                        notes = row.Cells["Notes"].Value.ToString();
                    }
                    
                    // If it's a valid record with orderId
                    if (orderId > 0)
                    {
                        // Create invoice model
                        InvoiceModel invoice = new InvoiceModel
                        {
                            InvoiceId = invoiceId,
                            OrderId = orderId,
                            InvoiceDate = invoiceDate,
                            TotalAmount = totalAmount,
                            Notes = notes
                        };
                        
                        if (invoiceId > 0)
                        {
                            // Update existing record
                            _invoiceManager.UpdateInvoice(invoice);
                        }
                        else
                        {
                            // Insert new record
                            _invoiceManager.InsertInvoice(invoice);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadInvoices();
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

        // Observer Pattern implementation
        public void OnInvoiceUpdated()
        {
            LoadInvoices(); // Refresh the DataGridView when notified
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Empty event handler from designer, can be removed if not needed
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Delete the selected invoice
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row
                if (dgvInvoice.CurrentRow != null && !dgvInvoice.CurrentRow.IsNewRow)
                {
                    // Get the ID of the record to delete
                    var idCell = dgvInvoice.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int invoiceId = Convert.ToInt32(idCell.Value);

                        // Show confirmation dialog
                        DialogResult result = MessageBox.Show(
                            $"Are you sure you want to delete Invoice #{invoiceId}?",
                            "Confirm Delete", 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            // Store the index of the current row to maintain position if possible
                            int currentRowIndex = dgvInvoice.CurrentRow.Index;
                            
                            // Delete the invoice with the specific ID
                            _invoiceManager.DeleteInvoice(invoiceId);
                            
                            // Refresh the data
                            LoadInvoices();
                            
                            // Try to select a row at the same position if available
                            if (currentRowIndex < dgvInvoice.Rows.Count)
                            {
                                dgvInvoice.CurrentCell = dgvInvoice.Rows[currentRowIndex].Cells[0];
                                dgvInvoice.Rows[currentRowIndex].Selected = true;
                            }
                            else if (dgvInvoice.Rows.Count > 0)
                            {
                                // Select the last row if we deleted the last one
                                dgvInvoice.CurrentCell = dgvInvoice.Rows[dgvInvoice.Rows.Count - 1].Cells[0];
                                dgvInvoice.Rows[dgvInvoice.Rows.Count - 1].Selected = true;
                            }
                        }
                    }
                    else
                    {
                        // Log instead of showing message
                        Console.WriteLine("No valid ID found for invoice.");
                    }
                }
                else
                {
                    // Log instead of showing message
                    Console.WriteLine("No invoice selected for deletion.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting invoice: " + ex.Message);
            }
        }
    }

    // Manager class implementing Observer Pattern for business logic
    public class InvoiceManager
    {
        private readonly IInvoiceStrategy _strategy;
        private readonly List<IInvoiceObserver> _observers;

        public InvoiceManager(IInvoiceStrategy strategy = null)
        {
            _strategy = strategy ?? new SqlServerInvoiceStrategy();
            _observers = new List<IInvoiceObserver>();
        }

        // Observer Pattern: Methods to register and notify observers
        public void AddObserver(IInvoiceObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IInvoiceObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnInvoiceUpdated();
            }
        }

        // Change strategy at runtime if needed
        public IInvoiceStrategy Strategy { get; set; }

        public DataTable GetAllInvoices()
        {
            return _strategy.GetAllInvoices();
        }

        public void InsertInvoice(InvoiceModel invoice)
        {
            _strategy.InsertInvoice(invoice);
            NotifyObservers();
        }

        public void UpdateInvoice(InvoiceModel invoice)
        {
            _strategy.UpdateInvoice(invoice);
            NotifyObservers();
        }

        public void DeleteInvoice(int invoiceId)
        {
            _strategy.DeleteInvoice(invoiceId);
            NotifyObservers();
        }
    }
}