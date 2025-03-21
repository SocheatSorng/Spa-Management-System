using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Factory Pattern: Creates ServiceModel objects
    public static class ServiceFactory
    {
        // Factory Method: Creates a new ServiceModel for insertion
        public static ServiceModel CreateService(string serviceName, string description, decimal price)
        {
            return new ServiceModel
            {
                ServiceName = serviceName,
                Description = description,
                Price = price,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }

        // Factory Method: Creates a ServiceModel with existing ID for update
        public static ServiceModel CreateService(int serviceId, string serviceName, string description, decimal price, DateTime createdDate, DateTime modifiedDate)
        {
            return new ServiceModel
            {
                ServiceId = serviceId,
                ServiceName = serviceName,
                Description = description,
                Price = price,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            };
        }
    }

    // DAO Pattern: Data Access Object for Service
    public class ServiceDAO
    {
        // Singleton Pattern: Using SqlConnectionManager singleton
        private readonly SqlConnectionManager _connectionManager;

        public ServiceDAO()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllServices()
        {
            string query = "SELECT ServiceId, ServiceName, Description, Price, CreatedDate, ModifiedDate FROM tbService";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertService(ServiceModel service)
        {
            string query = "INSERT INTO tbService (ServiceName, Description, Price, CreatedDate, ModifiedDate) " +
                           "VALUES (@ServiceName, @Description, @Price, @CreatedDate, @ModifiedDate)";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceName", service.ServiceName),
                new SqlParameter("@Description", service.Description),
                new SqlParameter("@Price", service.Price),
                new SqlParameter("@CreatedDate", service.CreatedDate),
                new SqlParameter("@ModifiedDate", service.ModifiedDate)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdateService(ServiceModel service)
        {
            string query = "UPDATE tbService SET ServiceName = @ServiceName, Description = @Description, Price = @Price, " +
                           "ModifiedDate = @ModifiedDate WHERE ServiceId = @ServiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceName", service.ServiceName),
                new SqlParameter("@Description", service.Description),
                new SqlParameter("@Price", service.Price),
                new SqlParameter("@ModifiedDate", DateTime.Now),
                new SqlParameter("@ServiceId", service.ServiceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeleteService(int serviceId)
        {
            string query = "DELETE FROM tbService WHERE ServiceId = @ServiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceId", serviceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }

    // View component in MVC pattern
    public partial class Service : Form
    {
        private ServiceDAO _dao;
        private DataTable _servicesTable;

        public Service()
        {
            InitializeComponent();
            _dao = new ServiceDAO();
            LoadServices();
            WireUpEvents(); // Attach event handlers
        }

        // Load all services into the DataGridView
        private void LoadServices()
        {
            _servicesTable = _dao.GetAllServices();
            dgvService.DataSource = _servicesTable;
        }

        // Wire up events
        private void WireUpEvents()
        {
            btnInsert.Click += BtnInsert_Click; // Insert
            btnUpdate.Click += BtnUpdate_Click; // Update
            btnDelete.Click += BtnDelete_Click; // Delete
            btnNew.Click += BtnNew_Click; // New
            btnClear.Click += BtnClear_Click; // Clear
            txtSearch.TextChanged += TxtSearch_TextChanged; // Search
            dgvService.CellClick += DgvService_CellClick; // Cell click to populate fields
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtID.Clear(); // ID
            txtName.Clear(); // Name
            txtDescription.Clear(); // Description
            txtPrice.Clear(); // Price
            txtCreatedAt.Clear(); // CreatedDate
            txtModifiedAt.Clear(); // ModifiedDate
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

                // Factory Pattern: Use Factory to create a new ServiceModel
                ServiceModel newService = ServiceFactory.CreateService(serviceName, description, price);
                _dao.InsertService(newService);
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

                // Factory Pattern: Use Factory to create an updated ServiceModel
                ServiceModel updatedService = ServiceFactory.CreateService(serviceId, serviceName, description, price, createdDate, DateTime.Now);
                _dao.UpdateService(updatedService);
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
                DialogResult result = MessageBox.Show("Are you sure you want to delete this service?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _dao.DeleteService(serviceId);
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
            }
        }
    }
}