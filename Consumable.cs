using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Factory Pattern: Creates ConsumableModel objects
    public static class ConsumableFactory
    {
        // Factory Method: Creates a new ConsumableModel for insertion
        public static ConsumableModel CreateConsumable(string name, string description, decimal price, string category, int stockQuantity)
        {
            return new ConsumableModel
            {
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                StockQuantity = stockQuantity,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }

        // Factory Method: Creates a ConsumableModel with existing ID for update
        public static ConsumableModel CreateConsumable(int consumableId, string name, string description, decimal price,
                                                    string category, int stockQuantity, DateTime createdDate)
        {
            return new ConsumableModel
            {
                ConsumableId = consumableId,
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                StockQuantity = stockQuantity,
                CreatedDate = createdDate,
                ModifiedDate = DateTime.Now
            };
        }
    }

    // Repository Pattern: Data Access for Consumable
    public class ConsumableRepository
    {
        // Singleton Pattern: Using SqlConnectionManager singleton
        private readonly SqlConnectionManager _connectionManager;

        public ConsumableRepository()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        // Fetch all consumables from the database
        public DataTable GetAllConsumables()
        {
            string query = "SELECT * FROM tbConsumable";
            return _connectionManager.ExecuteQuery(query);
        }

        // Insert a new consumable
        public void InsertConsumable(string name, string description, decimal price, string category, int stockQuantity)
        {
            // Factory Pattern: Use factory to create the consumable
            ConsumableModel consumable = ConsumableFactory.CreateConsumable(name, description, price, category, stockQuantity);

            string query = "INSERT INTO tbConsumable (Name, Description, Price, Category, StockQuantity, CreatedDate, ModifiedDate) " +
                          "VALUES (@Name, @Description, @Price, @Category, @StockQuantity, @CreatedDate, @ModifiedDate)";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", consumable.Name),
                new SqlParameter("@Description", consumable.Description),
                new SqlParameter("@Price", consumable.Price),
                new SqlParameter("@Category", consumable.Category),
                new SqlParameter("@StockQuantity", consumable.StockQuantity),
                new SqlParameter("@CreatedDate", consumable.CreatedDate),
                new SqlParameter("@ModifiedDate", consumable.ModifiedDate)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Update an existing consumable
        public void UpdateConsumable(int consumableId, string name, string description, decimal price, string category, int stockQuantity)
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
                consumableId, name, description, price, category, stockQuantity, createdDate);

            string query = "UPDATE tbConsumable SET Name = @Name, Description = @Description, Price = @Price, Category = @Category, " +
                          "StockQuantity = @StockQuantity, ModifiedDate = @ModifiedDate WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", consumable.Name),
                new SqlParameter("@Description", consumable.Description),
                new SqlParameter("@Price", consumable.Price),
                new SqlParameter("@Category", consumable.Category),
                new SqlParameter("@StockQuantity", consumable.StockQuantity),
                new SqlParameter("@ModifiedDate", consumable.ModifiedDate),
                new SqlParameter("@ConsumableId", consumable.ConsumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Delete a consumable by ID
        public void DeleteConsumable(int consumableId)
        {
            string query = "DELETE FROM tbConsumable WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@ConsumableId", consumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }

    // MVC Pattern: View component
    public partial class Consumable : Form
    {
        private ConsumableRepository _repository;
        private DataTable _consumablesTable;

        public Consumable()
        {
            InitializeComponent();
            // Repository Pattern: Create repository instance
            _repository = new ConsumableRepository();
            LoadConsumables();
            WireUpEvents(); // Attach event handlers
        }

        // Load all consumables into the DataGridView
        private void LoadConsumables()
        {
            _consumablesTable = _repository.GetAllConsumables();
            dgvConsumable.DataSource = _consumablesTable;
        }

        // Wire up events (since not all may be set in designer)
        private void WireUpEvents()
        {
            btnNew.Click += btnNew_Click;
            btnInsert.Click += btnInsert_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;
            txtSearch.TextChanged += txtSearch_TextChanged;
            dgvConsumable.CellClick += dgvConsumable_CellClick;
        }

        // Clear all input fields
        private void btnNew_Click(object sender, EventArgs e)
        {
            txtID.Clear();
            txtName.Clear();
            txtDescription.Clear();
            txtPrice.Clear();
            txtCategory.Clear();
            txtQuantity.Clear();
            txtCreatedAt.Clear();
            txtModifiedAt.Clear();
            txtName.Focus();
        }

        // Insert a new consumable
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtPrice.Text))
                {
                    MessageBox.Show("Name and price are required.");
                    return;
                }

                string name = txtName.Text.Trim();
                string description = txtDescription.Text.Trim();
                decimal price = decimal.Parse(txtPrice.Text.Trim());
                string category = txtCategory.Text.Trim();
                int stockQuantity = int.Parse(txtQuantity.Text.Trim());

                if (price < 0 || stockQuantity < 0)
                {
                    MessageBox.Show("Price and quantity cannot be negative.");
                    return;
                }

                _repository.InsertConsumable(name, description, price, category, stockQuantity);
                LoadConsumables();
                btnNew_Click(sender, e); // Clear fields
                MessageBox.Show("Consumable inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting consumable: " + ex.Message);
            }
        }

        // Update the selected consumable
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate selection
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a consumable to update.");
                    return;
                }

                // Validate input
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtPrice.Text))
                {
                    MessageBox.Show("Name and price are required.");
                    return;
                }

                int consumableId = int.Parse(txtID.Text);
                string name = txtName.Text.Trim();
                string description = txtDescription.Text.Trim();
                decimal price = decimal.Parse(txtPrice.Text.Trim());
                string category = txtCategory.Text.Trim();
                int stockQuantity = int.Parse(txtQuantity.Text.Trim());

                if (price < 0 || stockQuantity < 0)
                {
                    MessageBox.Show("Price and quantity cannot be negative.");
                    return;
                }

                _repository.UpdateConsumable(consumableId, name, description, price, category, stockQuantity);
                LoadConsumables();
                btnNew_Click(sender, e); // Clear fields
                MessageBox.Show("Consumable updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating consumable: " + ex.Message);
            }
        }

        // Delete the selected consumable
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate selection
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a consumable to delete.");
                    return;
                }

                int consumableId = int.Parse(txtID.Text);

                // Confirm deletion
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this consumable?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _repository.DeleteConsumable(consumableId);
                    LoadConsumables();
                    btnNew_Click(sender, e); // Clear fields
                    MessageBox.Show("Consumable deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting consumable: " + ex.Message);
            }
        }

        // Clear fields (reuses New button logic)
        private void btnClear_Click(object sender, EventArgs e)
        {
            btnNew_Click(sender, e);
        }

        // Filter consumables based on search input
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchValue = txtSearch.Text.Trim().ToLower();
            if (_consumablesTable != null)
            {
                _consumablesTable.DefaultView.RowFilter = string.Format(
                    "LOWER(Name) LIKE '%{0}%' OR LOWER(Category) LIKE '%{0}%'",
                    searchValue);
            }
        }

        // Populate textboxes when a cell is clicked
        private void dgvConsumable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                DataGridViewRow row = dgvConsumable.Rows[e.RowIndex];
                txtID.Text = row.Cells["ConsumableId"].Value.ToString();
                txtName.Text = row.Cells["Name"].Value.ToString();
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? "";
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtCategory.Text = row.Cells["Category"].Value?.ToString() ?? "";
                txtQuantity.Text = row.Cells["StockQuantity"].Value.ToString();
                txtCreatedAt.Text = row.Cells["CreatedDate"].Value.ToString();
                txtModifiedAt.Text = row.Cells["ModifiedDate"].Value.ToString();
            }
        }
    }
}