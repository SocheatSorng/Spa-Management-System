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

        public Invoice()
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

                       _invoiceManager = new InvoiceManager();
            _invoiceManager.AddObserver(this); // Register form as an observer
            LoadInvoices();
            WireUpEvents();
        }
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        // Load all invoices into the DataGridView
        private void LoadInvoices()
        {
            _invoicesTable = _invoiceManager.GetAllInvoices();
            dgvInvoice.DataSource = _invoicesTable;
        }

        // Wire up events
        private void WireUpEvents()
        {
            btnInsert.Click += BtnInsert_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged; // Renamed from textBox1 to txtSearch in event handler
            dgvInvoice.CellClick += DgvInvoice_CellClick;
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtID.Clear();
            txtOrderID.Clear();
            txtInvoiceDate.Clear();
            txtTotalAmount.Clear();
            txtNotes.Clear();
        }

        // Insert button click
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                int orderId = int.Parse(txtOrderID.Text.Trim());
                DateTime invoiceDate = DateTime.Parse(txtInvoiceDate.Text.Trim());
                decimal totalAmount = decimal.Parse(txtTotalAmount.Text.Trim());
                string notes = txtNotes.Text.Trim();

                if (orderId <= 0 || totalAmount < 0)
                {
                    MessageBox.Show("Valid Order ID and non-negative Total Amount are required.");
                    return;
                }

                InvoiceModel newInvoice = new InvoiceModel
                {
                    OrderId = orderId,
                    InvoiceDate = invoiceDate,
                    TotalAmount = totalAmount,
                    Notes = notes
                };

                _invoiceManager.InsertInvoice(newInvoice);
                ClearFields();
                MessageBox.Show("Invoice inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting invoice: " + ex.Message);
            }
        }

        // Update button click
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select an invoice to update.");
                    return;
                }

                int invoiceId = int.Parse(txtID.Text);
                int orderId = int.Parse(txtOrderID.Text.Trim());
                DateTime invoiceDate = DateTime.Parse(txtInvoiceDate.Text.Trim());
                decimal totalAmount = decimal.Parse(txtTotalAmount.Text.Trim());
                string notes = txtNotes.Text.Trim();

                if (orderId <= 0 || totalAmount < 0)
                {
                    MessageBox.Show("Valid Order ID and non-negative Total Amount are required.");
                    return;
                }

                InvoiceModel updatedInvoice = new InvoiceModel
                {
                    InvoiceId = invoiceId,
                    OrderId = orderId,
                    InvoiceDate = invoiceDate,
                    TotalAmount = totalAmount,
                    Notes = notes
                };

                _invoiceManager.UpdateInvoice(updatedInvoice);
                ClearFields();
                MessageBox.Show("Invoice updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating invoice: " + ex.Message);
            }
        }

        // Delete button click
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select an invoice to delete.");
                    return;
                }

                int invoiceId = int.Parse(txtID.Text);
                DialogResult result = MessageBox.Show("Are you sure you want to delete this invoice?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _invoiceManager.DeleteInvoice(invoiceId);
                    ClearFields();
                    MessageBox.Show("Invoice deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting invoice: " + ex.Message);
            }
        }

        // New button click (clear fields and focus on OrderID)
        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtOrderID.Focus();
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
            if (_invoicesTable != null)
            {
                _invoicesTable.DefaultView.RowFilter = string.Format(
                    "Convert(InvoiceId, 'System.String') LIKE '%{0}%' OR Convert(OrderId, 'System.String') LIKE '%{0}%'",
                    searchValue);
            }
        }

        // Populate textboxes when a cell is clicked
        private void DgvInvoice_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                DataGridViewRow row = dgvInvoice.Rows[e.RowIndex];
                txtID.Text = row.Cells["InvoiceId"].Value.ToString();
                txtOrderID.Text = row.Cells["OrderId"].Value.ToString();
                txtInvoiceDate.Text = row.Cells["InvoiceDate"].Value.ToString();
                txtTotalAmount.Text = row.Cells["TotalAmount"].Value.ToString();
                txtNotes.Text = row.Cells["Notes"].Value?.ToString() ?? "";
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