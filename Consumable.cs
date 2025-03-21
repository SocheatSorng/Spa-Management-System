using System;
using System.Data;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public partial class Consumable : Form
    {
        private ConsumableRepository _repository;
        private DataTable _consumablesTable;

        public Consumable()
        {
            InitializeComponent();
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
        }

        // Insert a new consumable
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtName.Text;
                string description = txtDescription.Text;
                decimal price = decimal.Parse(txtPrice.Text);
                string category = txtCategory.Text;
                int stockQuantity = int.Parse(txtQuantity.Text);

                _repository.InsertConsumable(name, description, price, category, stockQuantity);
                LoadConsumables();
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
                int consumableId = int.Parse(txtID.Text);
                string name = txtName.Text;
                string description = txtDescription.Text;
                decimal price = decimal.Parse(txtPrice.Text);
                string category = txtCategory.Text;
                int stockQuantity = int.Parse(txtQuantity.Text);

                _repository.UpdateConsumable(consumableId, name, description, price, category, stockQuantity);
                LoadConsumables();
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
            if (dgvConsumable.SelectedRows.Count > 0)
            {
                try
                {
                    int consumableId = int.Parse(dgvConsumable.SelectedRows[0].Cells["ConsumableId"].Value.ToString());
                    _repository.DeleteConsumable(consumableId);
                    LoadConsumables();
                    MessageBox.Show("Consumable deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting consumable: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a consumable to delete.");
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
            string searchValue = txtSearch.Text.ToLower();
            if (_consumablesTable != null)
            {
                _consumablesTable.DefaultView.RowFilter = string.Format("Name LIKE '%{0}%' OR Category LIKE '%{0}%'", searchValue);
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
                txtDescription.Text = row.Cells["Description"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtCategory.Text = row.Cells["Category"].Value.ToString();
                txtQuantity.Text = row.Cells["StockQuantity"].Value.ToString();
                txtCreatedAt.Text = row.Cells["CreatedDate"].Value.ToString();
                txtModifiedAt.Text = row.Cells["ModifiedDate"].Value.ToString();
            }
        }
    }
}