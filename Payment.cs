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
    // Payment form - UI component
    public partial class Payment : Form
    {
        private readonly PaymentContext _paymentContext;
        private DataTable _paymentsTable;
        private bool _isLoading = false;
        
        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public Payment()
        {
            InitializeComponent();
            
            // Setup DataGridView general properties
            SetupDataGridView();
            
            // Configure the DataGridView columns
            ConfigureDataGridViewColumns();
            
            // Add an event handler for grid layout
            dgvPayment.DataBindingComplete += DgvPayment_DataBindingComplete;

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
            // Strategy Pattern: Start with the database implementation by default
            _paymentContext = new PaymentContext(new DatabasePaymentStrategy());
            LoadPayments();
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

        // Load all payments into the DataGridView
        private void LoadPayments()
        {
            if (IsDesignMode) return;

            try
            {
                // Store the currently selected payment ID if any
                int? selectedPaymentId = null;
                if (dgvPayment.CurrentRow != null && !dgvPayment.CurrentRow.IsNewRow)
                {
                    var idCell = dgvPayment.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedPaymentId = Convert.ToInt32(idCell.Value);
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
            _paymentsTable = _paymentContext.GetAllPayments();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvPayment.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
            dgvPayment.DataSource = _paymentsTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_paymentsTable != null && _paymentsTable.DefaultView != null)
                    {
                        _paymentsTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvPayment.Columns["ID"] != null)
                    dgvPayment.Columns["ID"].Width = 50;
                    
                if (dgvPayment.Columns["InvoiceID"] != null)
                    dgvPayment.Columns["InvoiceID"].Width = 70;
                    
                if (dgvPayment.Columns["UserID"] != null)
                    dgvPayment.Columns["UserID"].Width = 60;
                    
                if (dgvPayment.Columns["PaymentDate"] != null)
                    dgvPayment.Columns["PaymentDate"].Width = 150;
                    
                if (dgvPayment.Columns["PaymentMethod"] != null)
                    dgvPayment.Columns["PaymentMethod"].Width = 100;
                    
                if (dgvPayment.Columns["Status"] != null)
                    dgvPayment.Columns["Status"].Width = 80;
                    
                if (dgvPayment.Columns["Notes"] != null)
                    dgvPayment.Columns["Notes"].Width = 300;
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvPayment.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvPayment.Refresh();
                
                // Restore selection if possible
                if (selectedPaymentId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvPayment.Rows)
                    {
                        var cell = row.Cells["ID"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedPaymentId.Value)
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
                Console.WriteLine("Error loading payments: " + ex.Message);
            }
            finally
            {
                // Reset loading flag
                _isLoading = false;
            }
        }
        
        // Helper method to hide specific columns if needed
        private void HideColumns()
        {
            // Check if columns exist first
            if (dgvPayment.Columns.Count == 0) return;

            try
            {
                // Hide database columns that shouldn't be visible (if any)
                string[] columnsToHide = { "InvoiceID", "UserID" };
                
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
            dgvPayment.CellEndEdit += DgvPayment_CellEndEdit;
            dgvPayment.UserAddedRow += DgvPayment_UserAddedRow;
            dgvPayment.UserDeletingRow += DgvPayment_UserDeletingRow;
            
            // Add exit button handler
            btnExitProgram.Click += btnExitProgram_Click;
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
                
                // Set default cell style for all cells to be center-aligned
                dgvPayment.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // Set column header style - bright orange like the other forms
                dgvPayment.EnableHeadersVisualStyles = false;
                dgvPayment.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(255, 153, 0),  // Bright orange header
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.False
                };
                
                // Configure header height
                dgvPayment.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvPayment.ColumnHeadersHeight = 40;
                
                // Setup cell and row appearance
                dgvPayment.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(255, 204, 102),  // Light orange selection
                    SelectionForeColor = Color.Black,
                    Font = new Font("Segoe UI", 9)
                };
                
                // Row alternating color
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
                dgvPayment.GridColor = Color.FromArgb(255, 226, 173);  // Light orange grid lines
                dgvPayment.RowHeadersVisible = false;
                dgvPayment.RowTemplate.Height = 30;  // Taller rows
                
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

        // Delete the selected payment
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row
                if (dgvPayment.CurrentRow != null && !dgvPayment.CurrentRow.IsNewRow)
                {
                    // Get the ID of the record to delete
                    var idCell = dgvPayment.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int paymentId = Convert.ToInt32(idCell.Value);

                        // Show confirmation dialog with the payment ID
                        DialogResult result = MessageBox.Show(
                            $"Are you sure you want to delete Payment #{paymentId}?",
                            "Confirm Delete", 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            // Store the index of the current row to maintain position if possible
                            int currentRowIndex = dgvPayment.CurrentRow.Index;
                            
                            // Delete the payment with the specific ID
                            _paymentContext.DeletePayment(paymentId);
                            
                            // Refresh the data
                            LoadPayments();
                            
                            // Try to select a row at the same position if available
                            if (currentRowIndex < dgvPayment.Rows.Count)
                            {
                                dgvPayment.CurrentCell = dgvPayment.Rows[currentRowIndex].Cells[0];
                                dgvPayment.Rows[currentRowIndex].Selected = true;
                            }
                            else if (dgvPayment.Rows.Count > 0)
                            {
                                // Select the last row if we deleted the last one
                                dgvPayment.CurrentCell = dgvPayment.Rows[dgvPayment.Rows.Count - 1].Cells[0];
                                dgvPayment.Rows[dgvPayment.Rows.Count - 1].Selected = true;
                            }
                        }
                    }
                    else
                    {
                        // Log instead of showing message
                        Console.WriteLine("No valid ID found for payment.");
                    }
                }
                else
                {
                    // Log instead of showing message
                    Console.WriteLine("No payment selected for deletion.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting payment: " + ex.Message);
            }
        }

        // Filter payments based on search input
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchValue = txtSearch.Text.Trim().ToLower();
                
                // Check if we have data to filter
                if (_paymentsTable == null || _paymentsTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _paymentsTable.DefaultView.RowFilter = string.Empty;
                    return;
                }

                // Build filter with exact string matching for better accuracy
                string filter = string.Format(
                    "Convert(PaymentId, 'System.String') LIKE '%{0}%' OR Convert(InvoiceId, 'System.String') LIKE '%{0}%' OR " +
                    "Convert(UserId, 'System.String') LIKE '%{0}%' OR PaymentMethod LIKE '%{0}%' OR Status LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _paymentsTable.DefaultView.RowFilter = filter;
                
                // Debug output
                Console.WriteLine($"Search value: '{searchValue}', Filter: '{filter}', Results: {_paymentsTable.DefaultView.Count}");
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_paymentsTable != null && _paymentsTable.DefaultView != null)
                {
                    _paymentsTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }
        
        private void DgvPayment_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["ID"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int paymentId = Convert.ToInt32(idCell.Value);
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete Payment #{paymentId}?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _paymentContext.DeletePayment(paymentId);
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

        private void DgvPayment_UserAddedRow(object sender, DataGridViewRowEventArgs e)
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

        private void DgvPayment_CellEndEdit(object sender, DataGridViewCellEventArgs e)
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
                    int paymentId = 0;
                    if (row.Cells["ID"].Value != null && row.Cells["ID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["ID"].Value.ToString(), out paymentId);
                    }
                    
                    // Get invoice ID with null check
                    int invoiceId = 0;
                    if (row.Cells["InvoiceID"].Value != null && row.Cells["InvoiceID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["InvoiceID"].Value.ToString(), out invoiceId);
                    }
                    
                    // Get user ID with null check
                    int userId = 0;
                    if (row.Cells["UserID"].Value != null && row.Cells["UserID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["UserID"].Value.ToString(), out userId);
                    }
                    
                    // Get payment date with null check
                    DateTime paymentDate = DateTime.Now;
                    if (row.Cells["PaymentDate"].Value != null && row.Cells["PaymentDate"].Value != DBNull.Value)
                    {
                        DateTime.TryParse(row.Cells["PaymentDate"].Value.ToString(), out paymentDate);
                    }
                    
                    // Get payment method with null check
                    string paymentMethod = "";
                    if (row.Cells["PaymentMethod"].Value != null && row.Cells["PaymentMethod"].Value != DBNull.Value)
                    {
                        paymentMethod = row.Cells["PaymentMethod"].Value.ToString();
                    }
                    
                    // Get transaction reference with null check
                    string transactionReference = "";
                    if (row.Cells["TransactionReference"].Value != null && row.Cells["TransactionReference"].Value != DBNull.Value)
                    {
                        transactionReference = row.Cells["TransactionReference"].Value.ToString();
                    }
                    
                    // Get status with null check
                    string status = "";
                    if (row.Cells["Status"].Value != null && row.Cells["Status"].Value != DBNull.Value)
                    {
                        status = row.Cells["Status"].Value.ToString();
                    }
                    
                    // Get notes with null check
                    string notes = "";
                    if (row.Cells["Notes"].Value != null && row.Cells["Notes"].Value != DBNull.Value)
                    {
                        notes = row.Cells["Notes"].Value.ToString();
                    }
                    
                    // If it's a valid record with invoiceId and userId
                    if (invoiceId > 0 && userId > 0 && !string.IsNullOrWhiteSpace(paymentMethod) && !string.IsNullOrWhiteSpace(status))
                    {
                        // Create payment model
                        PaymentModel payment = new PaymentModel
                        {
                            PaymentId = paymentId,
                            InvoiceId = invoiceId,
                            UserId = userId,
                            PaymentDate = paymentDate,
                            PaymentMethod = paymentMethod,
                            TransactionReference = transactionReference,
                            Status = status,
                            Notes = notes
                        };
                        
                        if (paymentId > 0)
                        {
                            // Update existing record
                            _paymentContext.UpdatePayment(payment);
                        }
                        else
                        {
                            // Insert new record
                            _paymentContext.InsertPayment(payment);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadPayments();
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

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Configure the DataGridView to use custom columns
        private void ConfigureDataGridViewColumns()
        {
            if (IsDesignMode) return;

            try
            {
                // Clear existing columns to control the order
                dgvPayment.Columns.Clear();
                
                // Create ID column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ID",
                    HeaderText = "ID",
                    DataPropertyName = "PaymentId",
                    Width = 50,
                    MinimumWidth = 50,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(idColumn);

                // Create InvoiceID column - SECOND position
                DataGridViewTextBoxColumn invoiceIdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "InvoiceID",
                    HeaderText = "Invoice ID",
                    DataPropertyName = "InvoiceId",
                    Width = 70,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(invoiceIdColumn);

                // Create UserID column - THIRD position
                DataGridViewTextBoxColumn userIdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "UserID",
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

                // Create PaymentDate column - FOURTH position
                DataGridViewTextBoxColumn dateColumn = new DataGridViewTextBoxColumn
                {
                    Name = "PaymentDate",
                    HeaderText = "Payment Date",
                    DataPropertyName = "PaymentDate",
                    Width = 150,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvPayment.Columns.Add(dateColumn);

                // Create PaymentMethod column - FIFTH position
                DataGridViewTextBoxColumn methodColumn = new DataGridViewTextBoxColumn
                {
                    Name = "PaymentMethod",
                    HeaderText = "Method",
                    DataPropertyName = "PaymentMethod",
                    Width = 100,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvPayment.Columns.Add(methodColumn);

                // Create TransactionReference column - SIXTH position
                DataGridViewTextBoxColumn refColumn = new DataGridViewTextBoxColumn
                {
                    Name = "TransactionReference",
                    HeaderText = "Reference #",
                    DataPropertyName = "TransactionReference",
                    Width = 120,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                dgvPayment.Columns.Add(refColumn);

                // Create Status column - SEVENTH position
                DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    Width = 80,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvPayment.Columns.Add(statusColumn);

                // Create Notes column - EIGHTH position
                DataGridViewTextBoxColumn notesColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Notes",
                    HeaderText = "Notes",
                    DataPropertyName = "Notes",
                    Width = 300,
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
        
        private void DgvPayment_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
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
            if (dgvPayment.Columns["ID"] != null)
                dgvPayment.Columns["ID"].Width = 50;
                
            if (dgvPayment.Columns["InvoiceID"] != null)
                dgvPayment.Columns["InvoiceID"].Width = 70;
                
            if (dgvPayment.Columns["UserID"] != null)
                dgvPayment.Columns["UserID"].Width = 60;
                
            if (dgvPayment.Columns["PaymentDate"] != null)
                dgvPayment.Columns["PaymentDate"].Width = 150;
                
            if (dgvPayment.Columns["PaymentMethod"] != null)
                dgvPayment.Columns["PaymentMethod"].Width = 100;
                
            if (dgvPayment.Columns["TransactionReference"] != null)
                dgvPayment.Columns["TransactionReference"].Width = 166;
                
            if (dgvPayment.Columns["Status"] != null)
                dgvPayment.Columns["Status"].Width = 80;
                
            if (dgvPayment.Columns["Notes"] != null)
                dgvPayment.Columns["Notes"].Width = 300;
            
            // Ensure visibility settings are maintained
            HideColumns();
            
            // Disable column auto-sizing one more time to be sure
            dgvPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Ensure header styling is maintained
            dgvPayment.EnableHeadersVisualStyles = false;
            dgvPayment.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 153, 0);
            dgvPayment.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }
    }

    // Strategy Pattern: Context class that uses a strategy
    public class PaymentContext
    {
        private IPaymentDataStrategy _strategy;

        public PaymentContext(IPaymentDataStrategy strategy)
        {
            _strategy = strategy;
        }

        // Strategy Pattern: Allows changing strategies at runtime
        public void SetStrategy(IPaymentDataStrategy strategy)
        {
            _strategy = strategy;
        }

        // Delegate method calls to the current strategy
        public DataTable GetAllPayments()
        {
            return _strategy.GetAllPayments();
        }

        public void InsertPayment(PaymentModel payment)
        {
            _strategy.InsertPayment(payment);
        }

        public void UpdatePayment(PaymentModel payment)
        {
            _strategy.UpdatePayment(payment);
        }

        public void DeletePayment(int paymentId)
        {
            _strategy.DeletePayment(paymentId);
        }
    }

    // Strategy Pattern: Interface defining operations for all payment data strategies
    public interface IPaymentDataStrategy
    {
        DataTable GetAllPayments();
        void InsertPayment(PaymentModel payment);
        void UpdatePayment(PaymentModel payment);
        void DeletePayment(int paymentId);
    }

    // Strategy Pattern: Concrete strategy for database operations
    public class DatabasePaymentStrategy : IPaymentDataStrategy
    {
        // Singleton Pattern: Using SqlConnectionManager singleton
        private readonly SqlConnectionManager _connectionManager;

        public DatabasePaymentStrategy()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllPayments()
        {
            string query = "SELECT PaymentId, InvoiceId, UserId, PaymentDate, PaymentMethod, TransactionReference, Status, Notes FROM tbPayment";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertPayment(PaymentModel payment)
        {
            string query = "INSERT INTO tbPayment (InvoiceId, UserId, PaymentDate, PaymentMethod, TransactionReference, Status, Notes) " +
                           "VALUES (@InvoiceId, @UserId, @PaymentDate, @PaymentMethod, @TransactionReference, @Status, @Notes)";
            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceId", payment.InvoiceId),
                new SqlParameter("@UserId", payment.UserId),
                new SqlParameter("@PaymentDate", payment.PaymentDate),
                new SqlParameter("@PaymentMethod", payment.PaymentMethod),
                new SqlParameter("@TransactionReference", payment.TransactionReference ?? (object)DBNull.Value),
                new SqlParameter("@Status", payment.Status),
                new SqlParameter("@Notes", payment.Notes ?? (object)DBNull.Value)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdatePayment(PaymentModel payment)
        {
            string query = "UPDATE tbPayment SET InvoiceId = @InvoiceId, UserId = @UserId, PaymentDate = @PaymentDate, " +
                           "PaymentMethod = @PaymentMethod, TransactionReference = @TransactionReference, Status = @Status, Notes = @Notes " +
                           "WHERE PaymentId = @PaymentId";
            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceId", payment.InvoiceId),
                new SqlParameter("@UserId", payment.UserId),
                new SqlParameter("@PaymentDate", payment.PaymentDate),
                new SqlParameter("@PaymentMethod", payment.PaymentMethod),
                new SqlParameter("@TransactionReference", payment.TransactionReference ?? (object)DBNull.Value),
                new SqlParameter("@Status", payment.Status),
                new SqlParameter("@Notes", payment.Notes ?? (object)DBNull.Value),
                new SqlParameter("@PaymentId", payment.PaymentId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeletePayment(int paymentId)
        {
            string query = "DELETE FROM tbPayment WHERE PaymentId = @PaymentId";
            SqlParameter[] parameters = {
                new SqlParameter("@PaymentId", paymentId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }
}