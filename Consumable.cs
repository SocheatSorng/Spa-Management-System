using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Bunifu.UI.WinForms;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Spa_Management_System
{
    // MVC Pattern: View component
    public partial class Consumable : Form
    {

        private readonly ConsumableContext _consumableContext;
        private DataTable _consumablesTable;
        private string _selectedImagePath;
        private bool _isLoading = false;

        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public Consumable()
        {
            InitializeComponent();

            // Configure the DataGridView columns
            ConfigureDataGridViewColumns();
            
            // Add an event handler for grid layout
            dgvConsumable.DataBindingComplete += DgvConsumable_DataBindingComplete;
            
            // Add an event handler for deletion from Bunifu button (keep Delete button)
            if (!IsDesignMode && btnDelete != null)
            {
                btnDelete.Click += btnDelete_Click;
                // Initially hide the delete button
                btnDelete.Visible = false;
            }
            
            // Fix the TextChanged event for BunifuTextBox
            if (!IsDesignMode && txtSearch != null)
            {
                // Use OnTextChange event instead of TextChanged to fix the error
                txtSearch.TextChange += txtSearch_TextChanged;
            }

            // Add this code for form dragging

            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) =>
            {
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
                    // Strategy Pattern: Create context with database strategy
                    _consumableContext = new ConsumableContext(new DatabaseConsumableStrategy());
                    LoadConsumables();
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


        // Load all consumables into the DataGridView
        private void LoadConsumables()
        {
            if (IsDesignMode) return;

            try
            {
                // Store the currently selected consumable ID if any
                int? selectedConsumableId = null;
                if (dgvConsumable.CurrentRow != null && !dgvConsumable.CurrentRow.IsNewRow)
                {
                    var idCell = dgvConsumable.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedConsumableId = Convert.ToInt32(idCell.Value);
                    }
                }

                // Set loading flag to prevent reentrant calls
                _isLoading = true;
                
                // Prevent auto-generation of columns 
                dgvConsumable.AutoGenerateColumns = false;
                
                // Disable AutoSizeColumnsMode to prevent auto-resizing
                dgvConsumable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Temporarily suspend layout and events
                dgvConsumable.SuspendLayout();
                
                // Load the data
            _consumablesTable = _consumableContext.GetAllConsumables();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvConsumable.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
            dgvConsumable.DataSource = _consumablesTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_consumablesTable != null && _consumablesTable.DefaultView != null)
                    {
                        _consumablesTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvConsumable.Columns["ID"] != null)
                    dgvConsumable.Columns["ID"].Width = 30;
                    
                if (dgvConsumable.Columns["Name"] != null)
                    dgvConsumable.Columns["Name"].Width = 100;
                    
                if (dgvConsumable.Columns["Price"] != null)
                    dgvConsumable.Columns["Price"].Width = 50;
                    
                if (dgvConsumable.Columns["Category"] != null)
                    dgvConsumable.Columns["Category"].Width = 100;
                
                if (dgvConsumable.Columns["Created"] != null)
                    dgvConsumable.Columns["Created"].Width = 160;
                    
                if (dgvConsumable.Columns["Modified"] != null)
                    dgvConsumable.Columns["Modified"].Width = 160;
                
                // Style the ComboBox column
                if (dgvConsumable.Columns["Category"] is DataGridViewComboBoxColumn categoryColumn)
                {
                    // Make sure dropdown button is always visible
                    categoryColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                }
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvConsumable.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvConsumable.Refresh();
                
                // Restore selection if possible
                if (selectedConsumableId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvConsumable.Rows)
                    {
                        var cell = row.Cells["ID"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedConsumableId.Value)
                        {
                            row.Selected = true;
                            dgvConsumable.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error loading consumables: " + ex.Message);
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
            if (dgvConsumable.Columns.Count == 0) return;

            try
            {
                // Hide database columns that shouldn't be visible
                string[] columnsToHide = { "ConsumableId", "Description", "ImagePath", "StockQuantity", "CreatedDate", "ModifiedDate" };
                
                foreach (string colName in columnsToHide)
                {
                    if (dgvConsumable.Columns.Contains(colName))
                    {
                        dgvConsumable.Columns[colName].Visible = false;
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

            // Remove unused button handlers, only keep Delete
            btnDelete.Click += btnDelete_Click;
            
            // Attach search event using the proper method for Bunifu controls
            if (txtSearch != null)
            {
                // Remove any existing handlers first to avoid duplicates
                txtSearch.TextChange -= txtSearch_TextChanged;
                // Add our handler to the correct event
                txtSearch.TextChange += txtSearch_TextChanged;
            }
            
            dgvConsumable.CellClick += dgvConsumable_CellClick;
            btnSelectPicture.Click += btnSelectPicture_Click;
            
            // Add events for direct editing in the grid
            dgvConsumable.CellEndEdit += DgvConsumable_CellEndEdit;
            dgvConsumable.UserAddedRow += DgvConsumable_UserAddedRow;
            dgvConsumable.UserDeletingRow += DgvConsumable_UserDeletingRow;
        }

        private void Consumable_Load(object sender, EventArgs e)
        {
            // Disable column auto-sizing
            dgvConsumable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvConsumable.AllowUserToResizeColumns = false;
            
            // Hide unused buttons
            if (btnNew != null) btnNew.Visible = false;
            if (btnInsert != null) btnInsert.Visible = false;
            if (btnUpdate != null) btnUpdate.Visible = false;
            if (btnClear != null) btnClear.Visible = false;
            
            // Hide delete button initially
            if (btnDelete != null) btnDelete.Visible = false;
            
            // Set search box to appropriate width
            if (txtSearch != null)
            {
                txtSearch.Width = 200; // Set fixed width instead of increasing
            }
            
            // Ensure columns are properly configured
            if (!IsDesignMode)
            {
                // Set up columns to correctly bind to data
                ConfigureDataGridViewColumns();
                
                // Force the columns to have the specified widths
                if (dgvConsumable.Columns["ID"] != null)
                    dgvConsumable.Columns["ID"].Width = 30;
                    
                if (dgvConsumable.Columns["Name"] != null)
                    dgvConsumable.Columns["Name"].Width = 100;
                    
                if (dgvConsumable.Columns["Price"] != null)
                    dgvConsumable.Columns["Price"].Width = 50;
                    
                if (dgvConsumable.Columns["Category"] != null)
                    dgvConsumable.Columns["Category"].Width = 100;
            }
        }

        // Delete the selected consumable
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row
                if (dgvConsumable.CurrentRow != null && !dgvConsumable.CurrentRow.IsNewRow)
                {
                    // Get the ID of the record to delete
                    var idCell = dgvConsumable.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int consumableId = Convert.ToInt32(idCell.Value);
                        string consumableName = dgvConsumable.CurrentRow.Cells["Name"].Value?.ToString() ?? "this consumable";

                        // Show confirmation dialog with the name of the consumable
                DialogResult result = MessageBox.Show(
                            $"Are you sure you want to delete '{consumableName}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                            // Store the index of the current row to maintain position if possible
                            int currentRowIndex = dgvConsumable.CurrentRow.Index;
                            
                            // Delete the consumable with the specific ID
                    _consumableContext.DeleteConsumable(consumableId);
                            
                            // Refresh the data
                    LoadConsumables();
                            
                            // Try to select a row at the same position if available
                            if (currentRowIndex < dgvConsumable.Rows.Count)
                            {
                                dgvConsumable.CurrentCell = dgvConsumable.Rows[currentRowIndex].Cells[0];
                                dgvConsumable.Rows[currentRowIndex].Selected = true;
                            }
                            else if (dgvConsumable.Rows.Count > 0)
                            {
                                // Select the last row if we deleted the last one
                                dgvConsumable.CurrentCell = dgvConsumable.Rows[dgvConsumable.Rows.Count - 1].Cells[0];
                                dgvConsumable.Rows[dgvConsumable.Rows.Count - 1].Selected = true;
                            }
                            
                            // Hide the delete button after successful deletion
                            btnDelete.Visible = false;
                        }
                    }
                    else
                    {
                        // Changed from message box to avoid dialog
                        Console.WriteLine("No valid ID found for consumable.");
                    }
                }
                else
                {
                    // Changed from message box to avoid dialog
                    Console.WriteLine("No consumable selected for deletion.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting consumable: " + ex.Message);
            }
        }

        // Filter consumables based on search input - works with both regular TextChanged and Bunifu's TextChange
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text from the Bunifu TextBox
                string searchValue = string.Empty;
                
                if (sender is Bunifu.UI.WinForms.BunifuTextBox bunifuTextBox)
                {
                    searchValue = bunifuTextBox.Text.Trim().ToLower();
                }
                else if (sender is TextBox textBox)
                {
                    searchValue = textBox.Text.Trim().ToLower();
                }
                
                // Check if we have data to filter
                if (_consumablesTable == null || _consumablesTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _consumablesTable.DefaultView.RowFilter = string.Empty;
                    return;
                }
                
                // Build filter with exact string matching for better accuracy
                // The LIKE operator is case-insensitive in SQL syntax
                string filter = string.Format(
                    "Name LIKE '%{0}%' OR Category LIKE '%{0}%' OR CONVERT(Price, 'System.String') LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _consumablesTable.DefaultView.RowFilter = filter;
                
                // Debug output
                Console.WriteLine($"Search value: '{searchValue}', Filter: '{filter}', Results: {_consumablesTable.DefaultView.Count}");
                
                // Hide the delete button since selection may have changed
                btnDelete.Visible = false;
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_consumablesTable != null && _consumablesTable.DefaultView != null)
                {
                    _consumablesTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }

        // Populate textboxes when a cell is clicked
        private void dgvConsumable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Check if this is the new row (last row with no data)
                bool isNewEmptyRow = e.RowIndex == dgvConsumable.Rows.Count - 1 && 
                                    dgvConsumable.Rows[e.RowIndex].IsNewRow;
                
                // Only show delete button if not clicking on the empty new row
                btnDelete.Visible = !isNewEmptyRow;
                
                // If we have a valid row index and it's not the new empty row
                if (e.RowIndex < dgvConsumable.Rows.Count && !isNewEmptyRow)
                {
                    // Get the ID of the selected consumable
                DataGridViewRow row = dgvConsumable.Rows[e.RowIndex];
                
                try
                {
                    // Get the ID value
                    object idValue = null;
                    if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "ID"))
                        idValue = row.Cells["ID"].Value;
                    else if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "ConsumableId"))
                        idValue = row.Cells["ConsumableId"].Value;
                                      
                    // Get Quantity
                    object quantityValue = null;
                    if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Quantity"))
                        quantityValue = row.Cells["Quantity"].Value;
                    else if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "StockQuantity"))
                        quantityValue = row.Cells["StockQuantity"].Value;
                    
                    // Get Created date
                    object createdValue = null;
                    if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Created"))
                        createdValue = row.Cells["Created"].Value;
                    else if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "CreatedDate"))
                        createdValue = row.Cells["CreatedDate"].Value;
                    
                    // Get Modified date
                    object modifiedValue = null;
                    if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Modified"))
                        modifiedValue = row.Cells["Modified"].Value;
                    else if (row.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "ModifiedDate"))
                        modifiedValue = row.Cells["ModifiedDate"].Value;
                    
                    // Handle image path - this is hidden but still in the data
                    if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("ImagePath"))
                        _selectedImagePath = drv.Row["ImagePath"] == DBNull.Value ? null : drv.Row["ImagePath"].ToString();
                    else
                        _selectedImagePath = null;

                        // Display image with proper stretching
                        DisplayImage(_selectedImagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling cell click: {ex.Message}");
                    }
                }
                else
                {
                    // Clear image when clicking on the new empty row
                    picConsumable.Image = null;
                    _selectedImagePath = null;
                }
            }
        }

        private void btnSelectPicture_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Consumable Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Get the selected file path
                        string sourceFilePath = openFileDialog.FileName;

                        // Create directory if it doesn't exist
                        string destinationFolder = Path.Combine(Application.StartupPath, "Images", "Consumables");
                        if (!Directory.Exists(destinationFolder))
                        {
                            Directory.CreateDirectory(destinationFolder);
                        }

                        // Generate unique filename based on timestamp
                        string fileName = "consumable_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(sourceFilePath);
                        string destinationPath = Path.Combine(destinationFolder, fileName);

                        // Copy the file to our application's images folder
                        File.Copy(sourceFilePath, destinationPath, true);

                        // Store the relative path in the database
                        _selectedImagePath = Path.Combine("Images", "Consumables", fileName);

                        // Display the image using streams to avoid file locking
                        DisplayImage(_selectedImagePath);
                        
                        // Update the currently selected row with the new image path
                        UpdateSelectedRowImage();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error processing image: " + ex.Message);
                        _selectedImagePath = null;
                    }
                }
            }
        }

        // Helper method to display an image with proper stretching
        private void DisplayImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    picConsumable.Image = null;
                    return;
                }
                
                string fullPath = Path.Combine(Application.StartupPath, imagePath);
                if (File.Exists(fullPath))
                {
                    // Dispose of existing image if there is one
                    if (picConsumable.Image != null)
                    {
                        Image oldImage = picConsumable.Image;
                        picConsumable.Image = null;
                        oldImage.Dispose();
                    }
                    
                    // Load the image from file
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        // Create a new image from the stream
                        picConsumable.Image = Image.FromStream(stream);
                        
                        // Set the PictureBox to stretch the image
                        picConsumable.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    picConsumable.Image = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying image: {ex.Message}");
                picConsumable.Image = null;
            }
        }
        
        // Helper method to update the selected row's image path
        private void UpdateSelectedRowImage()
        {
            if (dgvConsumable.CurrentRow != null && !dgvConsumable.CurrentRow.IsNewRow && _selectedImagePath != null)
            {
                try
                {
                    // Get the ID of the selected consumable
                    var idCell = dgvConsumable.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int consumableId = Convert.ToInt32(idCell.Value);
                        
                        // Get the current data from the row
                        string name = dgvConsumable.CurrentRow.Cells["Name"].Value?.ToString() ?? "";
                        
                        decimal price = 0;
                        if (dgvConsumable.CurrentRow.Cells["Price"].Value != null)
                        {
                            decimal.TryParse(dgvConsumable.CurrentRow.Cells["Price"].Value.ToString(), out price);
                        }
                        
                        string category = dgvConsumable.CurrentRow.Cells["Category"].Value?.ToString() ?? "Foods";
                        
                        int stockQuantity = 0;
                        if (dgvConsumable.CurrentRow.Cells["Quantity"].Value != null)
                        {
                            int.TryParse(dgvConsumable.CurrentRow.Cells["Quantity"].Value.ToString(), out stockQuantity);
                        }
                        
                        // Get description from data source if available
                        string description = "";
                        if (dgvConsumable.CurrentRow.DataBoundItem is DataRowView drv && 
                            drv.Row.Table.Columns.Contains("Description"))
                        {
                            description = drv.Row["Description"] == DBNull.Value ? 
                                          "" : drv.Row["Description"].ToString();
                        }
                        
                        // Update the consumable with the new image path
                        _consumableContext.UpdateConsumable(
                            consumableId, name, description, price, category, 
                            stockQuantity, _selectedImagePath);
                            
                        // Reload the data to reflect changes
                        LoadConsumables();
                        
                        // Re-select the row after reload
                        foreach (DataGridViewRow row in dgvConsumable.Rows)
                        {
                            if (row.Cells["ID"].Value != null && 
                                Convert.ToInt32(row.Cells["ID"].Value) == consumableId)
                            {
                                row.Selected = true;
                                dgvConsumable.CurrentCell = row.Cells[0];
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating image: {ex.Message}");
                }
            }
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

        // Configure the DataGridView to use our custom columns
        private void ConfigureDataGridViewColumns()
        {
            if (IsDesignMode) return;

            try
            {
                // Clear existing columns to control the order
                dgvConsumable.Columns.Clear();
                
                // Disable AutoSizeColumnsMode to ensure our width settings are respected
                dgvConsumable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Create ID column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ID",
                    HeaderText = "ID",
                    DataPropertyName = "ConsumableId",
                    Width = 30,
                    MinimumWidth = 30,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvConsumable.Columns.Add(idColumn);

                // Create Name column - SECOND position
                DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Name",
                    HeaderText = "Name",
                    DataPropertyName = "Name",
                    Width = 100,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvConsumable.Columns.Add(nameColumn);

                // Create Price column - THIRD position
                DataGridViewTextBoxColumn priceColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Price",
                    HeaderText = "Price",
                    DataPropertyName = "Price",
                    Width = 50,
                    MinimumWidth = 50,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvConsumable.Columns.Add(priceColumn);

                // Create Category column - FOURTH position
                // Create a ComboBox column for Category with predefined values
                DataGridViewComboBoxColumn categoryColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Category",
                    HeaderText = "Category",
                    DataPropertyName = "Category",
                    Width = 100,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Popup,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                    DisplayMember = "Text",
                    ValueMember = "Value",
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        SelectionBackColor = Color.FromArgb(255, 201, 102)
                    }
                };
                
                // Add predefined category items
                categoryColumn.Items.AddRange(new[] { "Foods", "Drinks", "Skincare", "Massage", "Other" });
                
                // Set drawing mode to ensure the dropdown arrow shows properly
                dgvConsumable.EnableHeadersVisualStyles = false;
                
                dgvConsumable.Columns.Add(categoryColumn);

                // Create Quantity column - FIFTH position
                DataGridViewTextBoxColumn quantityColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Quantity",
                    HeaderText = "Quantity",
                    DataPropertyName = "StockQuantity",
                    Width = 80,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvConsumable.Columns.Add(quantityColumn);

                // Create Created Date column - SIXTH position
                DataGridViewTextBoxColumn createdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Created",
                    HeaderText = "Created",
                    DataPropertyName = "CreatedDate",
                    Width = 160, // Wider to fit the full date/time
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt"
                    }
                };
                dgvConsumable.Columns.Add(createdColumn);

                // Create Modified Date column - SEVENTH position
                DataGridViewTextBoxColumn modifiedColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Modified",
                    HeaderText = "Modified",
                    DataPropertyName = "ModifiedDate",
                    Width = 160, // Wider to fit the full date/time
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt"
                    }
                };
                dgvConsumable.Columns.Add(modifiedColumn);
                
                // Make sure first column (ID) is visible
                if (dgvConsumable.Columns["ID"] != null)
                {
                    dgvConsumable.Columns["ID"].Visible = true;
                }
                
                // Disable column resizing by users
                dgvConsumable.AllowUserToResizeColumns = false;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        private void dgvConsumable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DgvConsumable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Apply alternating row colors
            foreach (DataGridViewRow row in dgvConsumable.Rows)
            {
                if (row.Index % 2 == 0)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 237, 204);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 232, 191);
                }
            }
            
            // Ensure the correct column widths are applied
            if (dgvConsumable.Columns["ID"] != null)
                dgvConsumable.Columns["ID"].Width = 30;
                
            if (dgvConsumable.Columns["Name"] != null)
                dgvConsumable.Columns["Name"].Width = 100;
                
            if (dgvConsumable.Columns["Price"] != null)
                dgvConsumable.Columns["Price"].Width = 50;
                
            if (dgvConsumable.Columns["Category"] != null)
                dgvConsumable.Columns["Category"].Width = 100;
                
            if (dgvConsumable.Columns["Created"] != null)
                dgvConsumable.Columns["Created"].Width = 160;
                
            if (dgvConsumable.Columns["Modified"] != null)
                dgvConsumable.Columns["Modified"].Width = 160;
            
            // Ensure Category column is properly styled
            if (dgvConsumable.Columns["Category"] is DataGridViewComboBoxColumn categoryCol)
            {
                categoryCol.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                categoryCol.FlatStyle = FlatStyle.Popup;
                
                // Apply cell templates to make dropdown arrow always visible
                foreach (DataGridViewRow row in dgvConsumable.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Category"] is DataGridViewComboBoxCell cell)
                    {
                        cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                    }
                }
            }
            
            // Hide delete button when no row is selected
            btnDelete.Visible = false;
            
            // Disable column auto-sizing one more time to be sure
            dgvConsumable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        private void DgvConsumable_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["ID"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int consumableId = Convert.ToInt32(idCell.Value);
                    string consumableName = e.Row.Cells["Name"].Value?.ToString() ?? "this consumable";
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete '{consumableName}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _consumableContext.DeleteConsumable(consumableId);
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

        private void DgvConsumable_UserAddedRow(object sender, DataGridViewRowEventArgs e)
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

        private void DgvConsumable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;
            
            try
            {
                // Don't immediately reload, which could cause reentrant issues
                // Instead, save the changes to the database without reloading
                
                DataGridViewRow row = dgvConsumable.Rows[e.RowIndex];
                
                // Check if this is a new row
                bool isNewRow = row.IsNewRow;
                
                if (!isNewRow)
                {
                    // Get values from the row, with proper null handling
                    int consumableId = 0;
                    if (row.Cells["ID"].Value != null && row.Cells["ID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["ID"].Value.ToString(), out consumableId);
                    }
                    
                    // Get name with null check
                    string name = "";
                    if (row.Cells["Name"].Value != null && row.Cells["Name"].Value != DBNull.Value)
                    {
                        name = row.Cells["Name"].Value.ToString();
                    }
                    
                    // Description is hidden, use empty string as default
                    string description = "";
                    
                    // Handle DataRowView safely
                    if (row.DataBoundItem is DataRowView drv)
                    {
                        if (drv.Row.Table.Columns.Contains("Description") && 
                            drv.Row["Description"] != DBNull.Value)
                        {
                            description = drv.Row["Description"].ToString();
                        }
                    }
                    
                    // Get price with null check
                    decimal price = 0;
                    if (row.Cells["Price"].Value != null && row.Cells["Price"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["Price"].Value.ToString(), out price);
                    }
                    
                    // Get category with null check
                    string category = "Foods"; // Default value
                    if (row.Cells["Category"].Value != null && row.Cells["Category"].Value != DBNull.Value)
                    {
                        category = row.Cells["Category"].Value.ToString();
                    }
                    
                    // Get quantity with null check
                    int stockQuantity = 0;
                    if (row.Cells["Quantity"].Value != null && row.Cells["Quantity"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["Quantity"].Value.ToString(), out stockQuantity);
                    }
                    
                    // Handle image path safely
                    string imagePath = null;
                    if (row.DataBoundItem is DataRowView dataRowView)
                    {
                        if (dataRowView.Row.Table.Columns.Contains("ImagePath") && 
                            dataRowView.Row["ImagePath"] != DBNull.Value)
                        {
                            imagePath = dataRowView.Row["ImagePath"].ToString();
                        }
                    }
                    
                    // If it's a valid record with name 
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (consumableId > 0)
                        {
                            // Update existing record
                            _consumableContext.UpdateConsumable(consumableId, name, description, price, category, stockQuantity, imagePath);
                        }
                        else
                        {
                            // Insert new record
                            _consumableContext.InsertConsumable(name, description, price, category, stockQuantity, imagePath);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadConsumables();
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

        // Initialize search functionality
        private void InitializeSearch()
        {
            try
            {
                // Remove any existing handlers to prevent duplicates
                txtSearch.TextChange -= txtSearch_TextChanged;
                
                // Add the handler
                txtSearch.TextChange += txtSearch_TextChanged;
                
                // Clear any existing text
                txtSearch.Clear();
                
                // Set placeholder text
                txtSearch.PlaceholderText = "Search...";
                
                // Initialize with empty filter
                if (_consumablesTable != null && _consumablesTable.DefaultView != null)
                {
                    _consumablesTable.DefaultView.RowFilter = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing search: " + ex.Message);
            }
        }
    }

    // Strategy Pattern: Context class that uses a strategy
    public class ConsumableContext
    {
        private IConsumableDataStrategy _strategy;

        public ConsumableContext(IConsumableDataStrategy strategy)
        {
            _strategy = strategy;
        }

        // Strategy Pattern: Allows changing strategies at runtime
        public void SetStrategy(IConsumableDataStrategy strategy)
        {
            _strategy = strategy;
        }

        // Delegate method calls to the current strategy
        public DataTable GetAllConsumables()
        {
            return _strategy.GetAllConsumables();
        }

        public void InsertConsumable(string name, string description, decimal price, string category, int stockQuantity, string imagePath = null)
        {
            _strategy.InsertConsumable(name, description, price, category, stockQuantity, imagePath);
        }

        public void UpdateConsumable(int consumableId, string name, string description, decimal price, string category, int stockQuantity, string imagePath = null)
        {
            _strategy.UpdateConsumable(consumableId, name, description, price, category, stockQuantity, imagePath);
        }

        public void DeleteConsumable(int consumableId)
        {
            _strategy.DeleteConsumable(consumableId);
        }
    }

    // Strategy Pattern: Interface defining operations for all consumable data strategies
    public interface IConsumableDataStrategy
    {
        DataTable GetAllConsumables();
        void InsertConsumable(string name, string description, decimal price, string category, int stockQuantity, string imagePath = null);
        void UpdateConsumable(int consumableId, string name, string description, decimal price, string category, int stockQuantity, string imagePath = null);
        void DeleteConsumable(int consumableId);
    }

    // Strategy Pattern: Concrete strategy for database operations
    public class DatabaseConsumableStrategy : IConsumableDataStrategy
    {
        // Singleton Pattern: Using SqlConnectionManager singleton
        private readonly SqlConnectionManager _connectionManager;

        public DatabaseConsumableStrategy()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllConsumables()
        {
            string query = "SELECT * FROM tbConsumable";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertConsumable(string name, string description, decimal price, string category, int stockQuantity, string imagePath = null)
        {
            // Factory Pattern: Use factory to create the consumable
            ConsumableModel consumable = ConsumableFactory.CreateConsumable(name, description, price, category, stockQuantity, imagePath);

            string query = "INSERT INTO tbConsumable (Name, Description, Price, Category, StockQuantity, ImagePath, CreatedDate, ModifiedDate) " +
                          "VALUES (@Name, @Description, @Price, @Category, @StockQuantity, @ImagePath, @CreatedDate, @ModifiedDate)";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", consumable.Name),
                new SqlParameter("@Description", (object)consumable.Description ?? DBNull.Value),
                new SqlParameter("@Price", consumable.Price),
                new SqlParameter("@Category", consumable.Category),
                new SqlParameter("@StockQuantity", consumable.StockQuantity),
                new SqlParameter("@ImagePath", (object)consumable.ImagePath ?? DBNull.Value),
                new SqlParameter("@CreatedDate", consumable.CreatedDate),
                new SqlParameter("@ModifiedDate", consumable.ModifiedDate)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdateConsumable(int consumableId, string name, string description, decimal price, string category, int stockQuantity, string imagePath = null)
        {
            // Get the original creation date
            DateTime createdDate = DateTime.Now; // Default value
            string getCreatedDateQuery = "SELECT CreatedDate FROM tbConsumable WHERE ConsumableId = @ConsumableId";
            SqlParameter param = new SqlParameter("@ConsumableId", consumableId);
            DataTable result = _connectionManager.ExecuteQuery(getCreatedDateQuery, param);
            if (result.Rows.Count > 0)
            {
                createdDate = Convert.ToDateTime(result.Rows[0]["CreatedDate"]);
            }

            // Factory Pattern: Use factory to create the updated consumable
            ConsumableModel consumable = ConsumableFactory.CreateConsumable(
                consumableId, name, description, price, category, stockQuantity, createdDate, imagePath);

            string query = "UPDATE tbConsumable SET Name = @Name, Description = @Description, Price = @Price, Category = @Category, " +
                          "StockQuantity = @StockQuantity, ImagePath = @ImagePath, ModifiedDate = @ModifiedDate WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", consumable.Name),
                new SqlParameter("@Description", (object)consumable.Description ?? DBNull.Value),
                new SqlParameter("@Price", consumable.Price),
                new SqlParameter("@Category", consumable.Category),
                new SqlParameter("@StockQuantity", consumable.StockQuantity),
                new SqlParameter("@ImagePath", (object)consumable.ImagePath ?? DBNull.Value),
                new SqlParameter("@ModifiedDate", consumable.ModifiedDate),
                new SqlParameter("@ConsumableId", consumable.ConsumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeleteConsumable(int consumableId)
        {
            try
        {
            string query = "DELETE FROM tbConsumable WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@ConsumableId", consumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine($"Error in database deletion: {ex.Message}");
                throw; // Rethrow to let caller handle it
            }
        }
    }

    // Factory Pattern: Creates ConsumableModel objects
    public static class ConsumableFactory
    {
        // Factory Method: Creates a new ConsumableModel for insertion
        public static ConsumableModel CreateConsumable(string name, string description, decimal price, string category, int stockQuantity, string imagePath = null)
        {
            return new ConsumableModel
            {
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                StockQuantity = stockQuantity,
                ImagePath = imagePath,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }

        // Factory Method: Creates a ConsumableModel with existing ID for update
        public static ConsumableModel CreateConsumable(int consumableId, string name, string description, decimal price,
                                                    string category, int stockQuantity, DateTime createdDate, string imagePath = null)
        {
            return new ConsumableModel
            {
                ConsumableId = consumableId,
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                StockQuantity = stockQuantity,
                ImagePath = imagePath,
                CreatedDate = createdDate,
                ModifiedDate = DateTime.Now
            };
        }
    }
}