using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Spa_Management_System
{
    public partial class Service : Form
    {
        private IServiceComponent _serviceComponent;
        private DataTable _servicesTable;
        private string _selectedImagePath;
        private bool _isLoading = false;

        // Check if the form is in design mode
        private bool IsDesignMode => System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        public Service()
        {
            InitializeComponent();
            
            // Setup DataGridView general properties
            SetupDataGridView();
            
            // Configure the DataGridView columns
            ConfigureDataGridViewColumns();
            
            // Add an event handler for grid layout
            dgvService.DataBindingComplete += DgvService_DataBindingComplete;
            
            // Hide the delete button initially
            if (!IsDesignMode && btnDelete != null)
            {
                btnDelete.Click += BtnDelete_Click;
                btnDelete.Visible = false;
            }
            
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
            _serviceComponent = ServiceComponentFactory.CreateServiceComponent();
            LoadServices();
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

        // Load all services into the DataGridView
        private void LoadServices()
        {
            if (IsDesignMode) return;

            try
            {
                // Set loading flag to prevent reentrant calls
                _isLoading = true;
                
                // Store the currently selected service ID if any
                int? selectedServiceId = null;
                if (dgvService.CurrentRow != null && !dgvService.CurrentRow.IsNewRow)
                {
                    var idCell = dgvService.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        selectedServiceId = Convert.ToInt32(idCell.Value);
                    }
                }
                
                // Prevent auto-generation of columns 
                dgvService.AutoGenerateColumns = false;
                
                // Disable AutoSizeColumnsMode to prevent auto-resizing
                dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Temporarily suspend layout and events
                dgvService.SuspendLayout();
                
                // Load the data
            _servicesTable = _serviceComponent.GetAllServices();
                
                // Set DataSource to null first to avoid reentrant errors
                dgvService.DataSource = null;
                
                // Make sure columns are configured before setting DataSource
                ConfigureDataGridViewColumns();
                
                // Apply the data
            dgvService.DataSource = _servicesTable;
                
                // Clear the search box and reset any filters
                if (txtSearch != null)
                {
                    txtSearch.Clear();
                    if (_servicesTable != null && _servicesTable.DefaultView != null)
                    {
                        _servicesTable.DefaultView.RowFilter = string.Empty;
                    }
                }
                
                // Explicitly force column widths after data binding
                if (dgvService.Columns["ID"] != null)
                    dgvService.Columns["ID"].Width = 30;
                    
                if (dgvService.Columns["Name"] != null)
                    dgvService.Columns["Name"].Width = 130;
                    
                if (dgvService.Columns["Price"] != null)
                    dgvService.Columns["Price"].Width = 50;
                
                if (dgvService.Columns["Created"] != null)
                    dgvService.Columns["Created"].Width = 160;
                    
                if (dgvService.Columns["Modified"] != null)
                    dgvService.Columns["Modified"].Width = 160;
                
                // Hide any columns that shouldn't be visible
                HideColumns();
                
                // Resume layout
                dgvService.ResumeLayout();
                
                // Refresh to ensure proper display
                dgvService.Refresh();
                
                // Restore selection if possible
                if (selectedServiceId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvService.Rows)
                    {
                        var cell = row.Cells["ID"];
                        if (cell != null && cell.Value != null && cell.Value != DBNull.Value &&
                            Convert.ToInt32(cell.Value) == selectedServiceId.Value)
                        {
                            row.Selected = true;
                            dgvService.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error loading services: " + ex.Message);
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
            if (dgvService.Columns.Count == 0) return;

            try
            {
                // Hide database columns that shouldn't be visible
                string[] columnsToHide = { "Description", "ImagePath" };
                
                foreach (string colName in columnsToHide)
                {
                    if (dgvService.Columns.Contains(colName))
                    {
                        dgvService.Columns[colName].Visible = false;
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

            // Set up button handlers
            btnDelete.Click += BtnDelete_Click;
            
            // Attach search event using the proper method
            if (txtSearch != null)
            {
                // Remove any existing handlers first to avoid duplicates
                txtSearch.TextChanged -= TxtSearch_TextChanged;
                // Add our handler
                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
            
            dgvService.CellClick += DgvService_CellClick;
            btnSelectPicture.Click += BtnSelectPicture_Click;
            
            // Add events for direct editing in the grid
            dgvService.CellEndEdit += DgvService_CellEndEdit;
            dgvService.UserAddedRow += DgvService_UserAddedRow;
            dgvService.UserDeletingRow += DgvService_UserDeletingRow;
            
            // Add exit button handler
            btnExitProgram.Click += btnExitProgram_Click;
        }

        private void Service_Load(object sender, EventArgs e)
        {
            // Disable column auto-sizing
            dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvService.AllowUserToResizeColumns = false;
            
            // Hide delete button initially
            if (btnDelete != null) btnDelete.Visible = false;
            
            // Set search box to appropriate width
            if (txtSearch != null)
            {
                txtSearch.Width = 200; // Set fixed width
            }
            
            // Hide related labels if they exist
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Label)
                {
                    ctrl.Visible = false;
                }
            }
            
            // Make sure DataGridView takes more space
            if (dgvService != null)
            {
                dgvService.Height += 150; // Make it taller
                
                // Ensure columns are properly configured
                if (!IsDesignMode)
                {
                    // Set up columns to correctly bind to data
                    ConfigureDataGridViewColumns();
                    
                    // Force the columns to have the specified widths
                    if (dgvService.Columns["ID"] != null)
                        dgvService.Columns["ID"].Width = 30;
                        
                    if (dgvService.Columns["Name"] != null)
                        dgvService.Columns["Name"].Width = 130;
                        
                    if (dgvService.Columns["Price"] != null)
                        dgvService.Columns["Price"].Width = 50;
                }
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
                dgvService.Columns.Clear();
                
                // Disable AutoSizeColumnsMode to ensure our width settings are respected
                dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Set default cell style for all cells to be center-aligned
                dgvService.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // Set default column header style to be center-aligned
                dgvService.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // Create ID column - FIRST position
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ID",
                    HeaderText = "ID",
                    DataPropertyName = "ServiceId",
                    // Width = 30,
                    // MinimumWidth = 30,
                    ReadOnly = true,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvService.Columns.Add(idColumn);

                // Create Name column - SECOND position
                DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Name",
                    HeaderText = "Name",
                    DataPropertyName = "ServiceName",
                    // Width = 130,
                    ReadOnly = false,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvService.Columns.Add(nameColumn);

                // Create Price column - THIRD position
                DataGridViewTextBoxColumn priceColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Price",
                    HeaderText = "Price",
                    DataPropertyName = "Price",
                    // Width = 50,
                    // MinimumWidth = 50,
                    ReadOnly = false,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvService.Columns.Add(priceColumn);

                // Create Description column - FOURTH position
                DataGridViewTextBoxColumn descColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Description",
                    HeaderText = "Description",
                    DataPropertyName = "Description",
                    // Width = 200,
                    ReadOnly = false,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                };
                dgvService.Columns.Add(descColumn);

                // Create Created Date column - FIFTH position
                DataGridViewTextBoxColumn createdColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Created",
                    HeaderText = "Created",
                    DataPropertyName = "CreatedDate",
                    // Width = 160, 
                    ReadOnly = true,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvService.Columns.Add(createdColumn);

                // Create Modified Date column - SIXTH position
                DataGridViewTextBoxColumn modifiedColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Modified",
                    HeaderText = "Modified",
                    DataPropertyName = "ModifiedDate",
                    // Width = 160,
                    ReadOnly = true,
                    // AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "MM/dd/yyyy h:mm tt",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                dgvService.Columns.Add(modifiedColumn);
                
                // Create hidden ImagePath column
                DataGridViewTextBoxColumn imagePathColumn = new DataGridViewTextBoxColumn
                {
                    Name = "ImagePath",
                    HeaderText = "ImagePath",
                    DataPropertyName = "ImagePath",
                    Visible = false
                };
                dgvService.Columns.Add(imagePathColumn);
                
                // Make sure first column (ID) is visible
                if (dgvService.Columns["ID"] != null)
                {
                    dgvService.Columns["ID"].Visible = true;
                }
                
                // Disable column resizing by users
                dgvService.AllowUserToResizeColumns = false;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error configuring columns: " + ex.Message);
            }
        }

        // Filter services based on search input
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search text
                string searchValue = txtSearch.Text.Trim().ToLower();
                
                // Check if we have data to filter
                if (_servicesTable == null || _servicesTable.DefaultView == null)
                    return;
                
                // If search is empty, show all records
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    _servicesTable.DefaultView.RowFilter = string.Empty;
                    return;
                }

                // Build filter with exact string matching for better accuracy
                string filter = string.Format(
                    "ServiceName LIKE '%{0}%' OR Description LIKE '%{0}%' OR CONVERT(Price, 'System.String') LIKE '%{0}%'",
                    searchValue.Replace("'", "''") // Escape any single quotes
                );
                
                // Apply the filter
                _servicesTable.DefaultView.RowFilter = filter;
                
                // Debug output
                Console.WriteLine($"Search value: '{searchValue}', Filter: '{filter}', Results: {_servicesTable.DefaultView.Count}");
                
                // Hide the delete button since selection may have changed
                btnDelete.Visible = false;
            }
            catch (Exception ex)
            {
                // Log error but don't show message box as it can be disruptive during typing
                Console.WriteLine("Search error: " + ex.Message);
                
                // If there's an error in the filter expression, just clear the filter
                if (_servicesTable != null && _servicesTable.DefaultView != null)
                {
                    _servicesTable.DefaultView.RowFilter = string.Empty;
                }
            }
        }
        
        // Populate fields when a cell is clicked
        private void DgvService_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Check if this is the new row (last row with no data)
                bool isNewEmptyRow = e.RowIndex == dgvService.Rows.Count - 1 && 
                                    dgvService.Rows[e.RowIndex].IsNewRow;
                
                // Only show delete button if not clicking on the empty new row
                btnDelete.Visible = !isNewEmptyRow;
                
                // If we have a valid row index and it's not the new empty row
                if (e.RowIndex < dgvService.Rows.Count && !isNewEmptyRow)
                {
                    // Get the selected service
                    DataGridViewRow row = dgvService.Rows[e.RowIndex];
                    
                    try
                    {
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
                    picService.Image = null;
                    _selectedImagePath = null;
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
                    picService.Image = null;
                    return;
                }

                string fullPath = Path.Combine(Application.StartupPath, imagePath);
                if (File.Exists(fullPath))
                {
                    // Dispose of existing image if there is one
                    if (picService.Image != null)
                    {
                        Image oldImage = picService.Image;
                        picService.Image = null;
                        oldImage.Dispose();
                    }
                    
                    // Load the image from file
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        // Create a new image from the stream
                        picService.Image = Image.FromStream(stream);
                        
                        // Set the PictureBox to stretch the image
                        picService.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    picService.Image = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying image: {ex.Message}");
                picService.Image = null;
            }
        }

        // Delete the selected service
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row
                if (dgvService.CurrentRow != null && !dgvService.CurrentRow.IsNewRow)
                {
                    // Get the ID of the record to delete
                    var idCell = dgvService.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int serviceId = Convert.ToInt32(idCell.Value);
                        string serviceName = dgvService.CurrentRow.Cells["Name"].Value?.ToString() ?? "this service";

                        // Show confirmation dialog with the name of the service
                DialogResult result = MessageBox.Show(
                            $"Are you sure you want to delete '{serviceName}'?",
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                            // Store the index of the current row to maintain position if possible
                            int currentRowIndex = dgvService.CurrentRow.Index;
                            
                            // Delete the service with the specific ID
                    _serviceComponent.DeleteService(serviceId);
                            
                            // Refresh the data
                    LoadServices();
                            
                            // Try to select a row at the same position if available
                            if (currentRowIndex < dgvService.Rows.Count)
                            {
                                dgvService.CurrentCell = dgvService.Rows[currentRowIndex].Cells[0];
                                dgvService.Rows[currentRowIndex].Selected = true;
                            }
                            else if (dgvService.Rows.Count > 0)
                            {
                                // Select the last row if we deleted the last one
                                dgvService.CurrentCell = dgvService.Rows[dgvService.Rows.Count - 1].Cells[0];
                                dgvService.Rows[dgvService.Rows.Count - 1].Selected = true;
                            }
                            
                            // Hide the delete button after successful deletion
                            btnDelete.Visible = false;
                        }
                    }
                    else
                    {
                        // Log instead of showing message
                        Console.WriteLine("No valid ID found for service.");
                    }
                }
                else
                {
                    // Log instead of showing message
                    Console.WriteLine("No service selected for deletion.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting service: " + ex.Message);
            }
        }

        private void DgvService_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Apply alternating row colors
            foreach (DataGridViewRow row in dgvService.Rows)
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
            if (dgvService.Columns["ID"] != null)
                dgvService.Columns["ID"].Width = 50;
                
            if (dgvService.Columns["Name"] != null)
                dgvService.Columns["Name"].Width = 130;
                
            if (dgvService.Columns["Price"] != null)
                dgvService.Columns["Price"].Width = 100;
                
            if (dgvService.Columns["Description"] != null)
                dgvService.Columns["Description"].Width = 200;
            
            if (dgvService.Columns["Created"] != null)
                dgvService.Columns["Created"].Width = 195;
                
            if (dgvService.Columns["Modified"] != null)
                dgvService.Columns["Modified"].Width = 195;
            
            // Hide delete button when no row is selected
            btnDelete.Visible = false;
            
            // Disable column auto-sizing one more time to be sure
            dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Ensure header styling is maintained
            dgvService.EnableHeadersVisualStyles = false;
            dgvService.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 153, 0);
            dgvService.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void DgvService_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Get the ID of the row being deleted
                var idCell = e.Row.Cells["ID"];
                
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int serviceId = Convert.ToInt32(idCell.Value);
                    string serviceName = e.Row.Cells["Name"].Value?.ToString() ?? "this service";
                    
                    // Confirm deletion
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete '{serviceName}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _serviceComponent.DeleteService(serviceId);
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

        private void DgvService_UserAddedRow(object sender, DataGridViewRowEventArgs e)
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

        private void DgvService_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;
            
            try
            {
                // Don't immediately reload, which could cause reentrant issues
                // Instead, save the changes to the database without reloading
                
                DataGridViewRow row = dgvService.Rows[e.RowIndex];
                
                // Check if this is a new row
                bool isNewRow = row.IsNewRow;
                
                if (!isNewRow)
                {
                    // Get values from the row, with proper null handling
                    int serviceId = 0;
                    if (row.Cells["ID"].Value != null && row.Cells["ID"].Value != DBNull.Value)
                    {
                        int.TryParse(row.Cells["ID"].Value.ToString(), out serviceId);
                    }
                    
                    // Get name with null check
                    string serviceName = "";
                    if (row.Cells["Name"].Value != null && row.Cells["Name"].Value != DBNull.Value)
                    {
                        serviceName = row.Cells["Name"].Value.ToString();
                    }
                    
                    // Get description with null check
                    string description = "";
                    if (row.Cells["Description"].Value != null && row.Cells["Description"].Value != DBNull.Value)
                    {
                        description = row.Cells["Description"].Value.ToString();
                    }
                    
                    // Get price with null check
                    decimal price = 0;
                    if (row.Cells["Price"].Value != null && row.Cells["Price"].Value != DBNull.Value)
                    {
                        decimal.TryParse(row.Cells["Price"].Value.ToString(), out price);
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
                    
                    // Get created date with null check for existing records
                    DateTime createdDate = DateTime.Now;
                    if (serviceId > 0 && row.Cells["Created"].Value != null && row.Cells["Created"].Value != DBNull.Value)
                    {
                        DateTime.TryParse(row.Cells["Created"].Value.ToString(), out createdDate);
                    }
                    
                    // If it's a valid record with name 
                    if (!string.IsNullOrWhiteSpace(serviceName))
                    {
                        // Create a service model
                        ServiceModel service = new ServiceModel
                        {
                            ServiceId = serviceId,
                            ServiceName = serviceName,
                            Description = description,
                            Price = price,
                            ImagePath = imagePath,
                            CreatedDate = createdDate,
                            ModifiedDate = DateTime.Now
                        };
                        
                        if (serviceId > 0)
                        {
                            // Update existing record
                            _serviceComponent.UpdateService(service);
                }
                else
                {
                            // Insert new record
                            _serviceComponent.InsertService(service);
                            
                            // Only for new records, we need to reload to get the ID
                            BeginInvoke(new Action(() => {
                                // Use BeginInvoke to avoid reentrant calls
                                LoadServices();
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
        
        // Handle image selection and update
        private void BtnSelectPicture_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Service Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Get the selected file path
                        string sourceFilePath = openFileDialog.FileName;

                        // Create directory if it doesn't exist
                        string destinationFolder = Path.Combine(Application.StartupPath, "Images", "Services");
                            if (!Directory.Exists(destinationFolder))
                            {
                                Directory.CreateDirectory(destinationFolder);
                        }

                        // Generate unique filename based on timestamp
                        string fileName = "service_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(sourceFilePath);
                        string destinationPath = Path.Combine(destinationFolder, fileName);

                        // Copy the file to our application's images folder
                        File.Copy(sourceFilePath, destinationPath, true);

                        // Store the relative path in the database
                        _selectedImagePath = Path.Combine("Images", "Services", fileName);

                        // Display the image using streams to avoid file locking
                        DisplayImage(_selectedImagePath);
                        
                        // Update the currently selected row with the new image path
                        UpdateSelectedRowImage();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing image: " + ex.Message);
                        _selectedImagePath = null;
                    }
                }
            }
        }

        // Helper method to update the selected row's image path
        private void UpdateSelectedRowImage()
        {
            if (dgvService.CurrentRow != null && !dgvService.CurrentRow.IsNewRow && _selectedImagePath != null)
            {
                try
                {
                    // Get the ID of the selected service
                    var idCell = dgvService.CurrentRow.Cells["ID"];
                    if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                    {
                        int serviceId = Convert.ToInt32(idCell.Value);
                        
                        // Get the current data from the row
                        string serviceName = dgvService.CurrentRow.Cells["Name"].Value?.ToString() ?? "";
                        
                        decimal price = 0;
                        if (dgvService.CurrentRow.Cells["Price"].Value != null)
                        {
                            decimal.TryParse(dgvService.CurrentRow.Cells["Price"].Value.ToString(), out price);
                        }
                        
                        string description = "";
                        if (dgvService.CurrentRow.Cells["Description"].Value != null)
                        {
                            description = dgvService.CurrentRow.Cells["Description"].Value.ToString();
                        }
                        
                        // Get created date for existing record
                        DateTime createdDate = DateTime.Now;
                        if (dgvService.CurrentRow.Cells["Created"].Value != null)
                        {
                            DateTime.TryParse(dgvService.CurrentRow.Cells["Created"].Value.ToString(), out createdDate);
                        }
                        
                        // Create updated service model
                        ServiceModel updatedService = new ServiceModel
                        {
                            ServiceId = serviceId,
                            ServiceName = serviceName,
                            Description = description,
                            Price = price,
                            ImagePath = _selectedImagePath,
                            CreatedDate = createdDate,
                            ModifiedDate = DateTime.Now
                        };
                        
                        // Update the service
                        _serviceComponent.UpdateService(updatedService);
                        
                        // Reload the data to reflect changes
                        LoadServices();
                        
                        // Re-select the row after reload
                        foreach (DataGridViewRow row in dgvService.Rows)
                        {
                            if (row.Cells["ID"].Value != null && 
                                Convert.ToInt32(row.Cells["ID"].Value) == serviceId)
                            {
                                row.Selected = true;
                                dgvService.CurrentCell = row.Cells[0];
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

        // Configure the DataGridView general properties
        private void SetupDataGridView()
        {
            try
            {
                // Disable auto-sizing to prevent column width changes
                dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Set selection mode to full row select
                dgvService.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                
                // Disable multiple selection
                dgvService.MultiSelect = false;
                
                // Set default cell style for all cells to be center-aligned
                dgvService.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // Set column header style - bright orange like Consumable and Invoice
                dgvService.EnableHeadersVisualStyles = false;
                dgvService.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(255, 153, 0),  // Bright orange header
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.False
                };
                
                // Configure header height
                dgvService.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvService.ColumnHeadersHeight = 40;
                
                // Cell display configuration - same as before but adjusted slightly
                dgvService.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 242, 204);  // Light yellow alternate rows
                dgvService.DefaultCellStyle.BackColor = Color.White;
                dgvService.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                dgvService.DefaultCellStyle.ForeColor = Color.Black;
                dgvService.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 204, 102);  // Light orange selection
                dgvService.DefaultCellStyle.SelectionForeColor = Color.Black;
                
                // Setting other appearance properties
                dgvService.BackgroundColor = Color.White;
                dgvService.BorderStyle = BorderStyle.None;
                dgvService.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvService.GridColor = Color.FromArgb(255, 226, 173);  // Light orange grid lines
                dgvService.RowHeadersVisible = false;
                dgvService.RowTemplate.Height = 30;
                dgvService.StandardTab = true;
                
                // Additional settings
                dgvService.AllowUserToResizeColumns = false;
                dgvService.AllowUserToResizeRows = false;
                dgvService.ReadOnly = false;
                dgvService.AutoGenerateColumns = false;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box
                Console.WriteLine("Error setting up DataGridView: " + ex.Message);
            }
        }
    }
}