﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public partial class Service : Form
    {
        private IServiceComponent _serviceComponent;
        private DataTable _servicesTable;
        private string _selectedImagePath;

        public Service()
        {
            InitializeComponent();
            
            // Add the same dragging capability to the top panel
            bunifuPanel2.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
                       
            _serviceComponent = ServiceComponentFactory.CreateServiceComponent();
            LoadServices();
            WireUpEvents(); // Attach event handlers
        }
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        // Load all services into the DataGridView
        private void LoadServices()
        {
            _servicesTable = _serviceComponent.GetAllServices();
            dgvService.DataSource = _servicesTable;
        }
        private void ClearFields()
        {
            txtID.Clear(); // ID
            txtName.Clear(); // Name
            txtDescription.Clear(); // Description
            txtPrice.Clear(); // Price
            txtCreatedAt.Clear(); // CreatedDate
            txtModifiedAt.Clear(); // ModifiedDate
            picService.Image = null; // Clear image
            _selectedImagePath = null; // Clear selected image path
        }

        // Clear all input fields
        private void WireUpEvents()
        {
            btnInsert.Click += BtnInsert_Click; // Insert
            btnUpdate.Click += BtnUpdate_Click; // Update
            btnDelete.Click += BtnDelete_Click; // Delete
            btnNew.Click += BtnNew_Click; // New
            btnClear.Click += BtnClear_Click; // Clear
            txtSearch.TextChanged += TxtSearch_TextChanged; // Search
            dgvService.CellClick += DgvService_CellClick; // Cell click to populate fields
            btnSelectPicture.Click += BtnSelectPicture_Click; // Select image
        }
        // Insert button click
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                string serviceName = txtName.Text.Trim();
                string description = txtDescription.Text.Trim();
                decimal price = decimal.Parse(txtPrice.Text.Trim());

                if (string.IsNullOrEmpty(serviceName) || price < 0)
                {
                    MessageBox.Show("Service name and a valid price are required.");
                    return;
                }

                // Create a new ServiceModel
                ServiceModel newService = new ServiceModel
                {
                    ServiceName = serviceName,
                    Description = description,
                    Price = price,
                    ImagePath = _selectedImagePath,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _serviceComponent.InsertService(newService);
                LoadServices();
                ClearFields();
                MessageBox.Show("Service inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting service: " + ex.Message);
            }
        }

        // Update button click
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a service to update.");
                    return;
                }

                int serviceId = int.Parse(txtID.Text);
                string serviceName = txtName.Text.Trim();
                string description = txtDescription.Text.Trim();
                decimal price = decimal.Parse(txtPrice.Text.Trim());
                DateTime createdDate = DateTime.Parse(txtCreatedAt.Text); // Preserve original CreatedDate

                if (string.IsNullOrEmpty(serviceName) || price < 0)
                {
                    MessageBox.Show("Service name and a valid price are required.");
                    return;
                }

                // Create updated ServiceModel
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

                _serviceComponent.UpdateService(updatedService);
                LoadServices();
                ClearFields();
                MessageBox.Show("Service updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating service: " + ex.Message);
            }
        }

        // Delete button click
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a service to delete.");
                    return;
                }

                int serviceId = int.Parse(txtID.Text);

                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this service?", 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _serviceComponent.DeleteService(serviceId);
                    LoadServices();
                    ClearFields();
                    MessageBox.Show("Service deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting service: " + ex.Message);
            }
        }

        // New button click (clear fields and focus on name)
        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtName.Focus();
        }

        // Clear button click (reuse New logic)
        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        // Search functionality
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchValue = txtSearch.Text.Trim().ToLower();
            if (_servicesTable != null)
            {
                _servicesTable.DefaultView.RowFilter = string.Format("ServiceName LIKE '%{0}%'", searchValue);
            }
        }

        // Populate textboxes when a cell is clicked
        private void DgvService_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                DataGridViewRow row = dgvService.Rows[e.RowIndex];
                txtID.Text = row.Cells["ServiceId"].Value.ToString(); // ID
                txtName.Text = row.Cells["ServiceName"].Value.ToString(); // Name
                txtDescription.Text = row.Cells["Description"].Value.ToString(); // Description
                txtPrice.Text = row.Cells["Price"].Value.ToString(); // Price
                txtCreatedAt.Text = row.Cells["CreatedDate"].Value.ToString(); // CreatedDate
                txtModifiedAt.Text = row.Cells["ModifiedDate"].Value.ToString(); // ModifiedDate

                // Handle image path
                _selectedImagePath = row.Cells["ImagePath"].Value == DBNull.Value ?
                    null : row.Cells["ImagePath"].Value.ToString();

                // Display image if available
                if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                {
                    try
                    {
                        picService.Image = Image.FromFile(_selectedImagePath);
                        picService.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch
                    {
                        picService.Image = null;
                    }
                }
                else
                {
                    picService.Image = null;
                }
            }
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }
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

                        // Display the image
                        picService.Image = Image.FromFile(destinationPath);
                        picService.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error processing image: " + ex.Message);
                        _selectedImagePath = null;
                    }
                }
            }
        }

    }
}