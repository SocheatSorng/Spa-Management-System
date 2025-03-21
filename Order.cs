using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System;
using System.Data;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public partial class Order : Form, IOrderObserver
    {
        private readonly OrderManager _orderManager;
        private DataTable _ordersTable;

        public Order()
        {
            InitializeComponent();
            _orderManager = new OrderManager();
            _orderManager.AddObserver(this); // Register form as an observer
            LoadOrders();
            WireUpEvents();
        }

        // Load all orders into the DataGridView
        private void LoadOrders()
        {
            _ordersTable = _orderManager.GetAllOrders();
            dgvOrder.DataSource = _ordersTable;
        }

        // Wire up events
        private void WireUpEvents()
        {
            btnInsert.Click += BtnInsert_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvOrder.CellClick += DgvOrder_CellClick;
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtID.Clear();
            txtCustomerID.Clear();
            txtUserID.Clear();
            txtOrderTime.Clear();
            txtTotalAmount.Clear();
            txtDiscount.Clear();
            txtFinalAmount.Clear();
            txtNotes.Clear();
            txtStatus.Clear();
        }

        // Insert button click
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                int customerId = int.Parse(txtCustomerID.Text.Trim());
                int userId = int.Parse(txtUserID.Text.Trim());
                DateTime orderTime = DateTime.Parse(txtOrderTime.Text.Trim());
                decimal totalAmount = decimal.Parse(txtTotalAmount.Text.Trim());
                decimal discount = decimal.Parse(txtDiscount.Text.Trim());
                decimal finalAmount = decimal.Parse(txtFinalAmount.Text.Trim());
                string notes = txtNotes.Text.Trim();
                string status = txtStatus.Text.Trim();

                if (customerId <= 0 || userId <= 0 || totalAmount < 0 || discount < 0 || finalAmount < 0 || string.IsNullOrEmpty(status))
                {
                    MessageBox.Show("Valid Customer ID, User ID, non-negative amounts, and status are required.");
                    return;
                }

                OrderModel newOrder = new OrderModel
                {
                    CustomerId = customerId,
                    UserId = userId,
                    OrderTime = orderTime,
                    TotalAmount = totalAmount,
                    Discount = discount,
                    FinalAmount = finalAmount,
                    Notes = notes,
                    Status = status
                };

                _orderManager.InsertOrder(newOrder);
                ClearFields();
                MessageBox.Show("Order inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting order: " + ex.Message);
            }
        }

        // Update button click
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select an order to update.");
                    return;
                }

                int orderId = int.Parse(txtID.Text);
                int customerId = int.Parse(txtCustomerID.Text.Trim());
                int userId = int.Parse(txtUserID.Text.Trim());
                DateTime orderTime = DateTime.Parse(txtOrderTime.Text.Trim());
                decimal totalAmount = decimal.Parse(txtTotalAmount.Text.Trim());
                decimal discount = decimal.Parse(txtDiscount.Text.Trim());
                decimal finalAmount = decimal.Parse(txtFinalAmount.Text.Trim());
                string notes = txtNotes.Text.Trim();
                string status = txtStatus.Text.Trim();

                if (customerId <= 0 || userId <= 0 || totalAmount < 0 || discount < 0 || finalAmount < 0 || string.IsNullOrEmpty(status))
                {
                    MessageBox.Show("Valid Customer ID, User ID, non-negative amounts, and status are required.");
                    return;
                }

                OrderModel updatedOrder = new OrderModel
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

                _orderManager.UpdateOrder(updatedOrder);
                ClearFields();
                MessageBox.Show("Order updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating order: " + ex.Message);
            }
        }

        // Delete button click
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select an order to delete.");
                    return;
                }

                int orderId = int.Parse(txtID.Text);
                DialogResult result = MessageBox.Show("Are you sure you want to delete this order?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _orderManager.DeleteOrder(orderId);
                    ClearFields();
                    MessageBox.Show("Order deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting order: " + ex.Message);
            }
        }

        // New button click (clear fields and focus on CustomerID)
        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtCustomerID.Focus();
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
            if (_ordersTable != null)
            {
                _ordersTable.DefaultView.RowFilter = string.Format(
                    "Convert(CustomerId, 'System.String') LIKE '%{0}%' OR Convert(OrderId, 'System.String') LIKE '%{0}%' OR Status LIKE '%{0}%'",
                    searchValue);
            }
        }

        // Populate textboxes when a cell is clicked
        private void DgvOrder_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                DataGridViewRow row = dgvOrder.Rows[e.RowIndex];
                txtID.Text = row.Cells["OrderId"].Value.ToString();
                txtCustomerID.Text = row.Cells["CustomerId"].Value.ToString();
                txtUserID.Text = row.Cells["UserId"].Value.ToString();
                txtOrderTime.Text = row.Cells["OrderTime"].Value.ToString();
                txtTotalAmount.Text = row.Cells["TotalAmount"].Value.ToString();
                txtDiscount.Text = row.Cells["Discount"].Value.ToString();
                txtFinalAmount.Text = row.Cells["FinalAmount"].Value.ToString();
                txtNotes.Text = row.Cells["Notes"].Value?.ToString() ?? "";
                txtStatus.Text = row.Cells["Status"].Value?.ToString() ?? "";
            }
        }

        // Observer implementation
        public void OnOrderUpdated()
        {
            LoadOrders(); // Refresh the DataGridView when notified
        }
    }
}