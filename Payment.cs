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

        public Payment()
        {
            InitializeComponent();
            // Strategy Pattern: Start with the database implementation by default
            _paymentContext = new PaymentContext(new DatabasePaymentStrategy());
            LoadPayments();
            WireUpEvents();
        }

        // Load all payments into the DataGridView
        private void LoadPayments()
        {
            _paymentsTable = _paymentContext.GetAllPayments();
            dgvPayment.DataSource = _paymentsTable;
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
            dgvPayment.CellClick += DgvPayment_CellClick;
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtID.Clear();
            txtInvoiceID.Clear();
            txtUserID.Clear();
            txtPaymentDate.Clear();
            txtPaymentMethod.Clear();
            txtTransactionReference.Clear();
            txtStatus.Clear();
            txtNotes.Clear();
        }

        // Insert button click
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                int invoiceId = int.Parse(txtInvoiceID.Text.Trim());
                int userId = int.Parse(txtUserID.Text.Trim());
                DateTime paymentDate = DateTime.Parse(txtPaymentDate.Text.Trim());
                string paymentMethod = txtPaymentMethod.Text.Trim();
                string transactionReference = txtTransactionReference.Text.Trim();
                string status = txtStatus.Text.Trim();
                string notes = txtNotes.Text.Trim();

                if (invoiceId <= 0 || userId <= 0 || string.IsNullOrEmpty(paymentMethod) || string.IsNullOrEmpty(status))
                {
                    MessageBox.Show("Valid Invoice ID, User ID, Payment Method, and Status are required.");
                    return;
                }

                PaymentModel newPayment = new PaymentModel
                {
                    InvoiceId = invoiceId,
                    UserId = userId,
                    PaymentDate = paymentDate,
                    PaymentMethod = paymentMethod,
                    TransactionReference = transactionReference,
                    Status = status,
                    Notes = notes
                };

                _paymentContext.InsertPayment(newPayment);
                LoadPayments();
                ClearFields();
                MessageBox.Show("Payment inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting payment: " + ex.Message);
            }
        }

        // Update button click
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a payment to update.");
                    return;
                }

                int paymentId = int.Parse(txtID.Text);
                int invoiceId = int.Parse(txtInvoiceID.Text.Trim());
                int userId = int.Parse(txtUserID.Text.Trim());
                DateTime paymentDate = DateTime.Parse(txtPaymentDate.Text.Trim());
                string paymentMethod = txtPaymentMethod.Text.Trim();
                string TransactionReference = txtTransactionReference.Text.Trim();
                string status = txtStatus.Text.Trim();
                string notes = txtNotes.Text.Trim();

                if (invoiceId <= 0 || userId <= 0 || string.IsNullOrEmpty(paymentMethod) || string.IsNullOrEmpty(status))
                {
                    MessageBox.Show("Valid Invoice ID, User ID, Payment Method, and Status are required.");
                    return;
                }

                PaymentModel updatedPayment = new PaymentModel
                {
                    PaymentId = paymentId,
                    InvoiceId = invoiceId,
                    UserId = userId,
                    PaymentDate = paymentDate,
                    PaymentMethod = paymentMethod,
                    TransactionReference = TransactionReference,
                    Status = status,
                    Notes = notes
                };

                _paymentContext.UpdatePayment(updatedPayment);
                LoadPayments();
                ClearFields();
                MessageBox.Show("Payment updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating payment: " + ex.Message);
            }
        }

        // Delete button click
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtID.Text))
                {
                    MessageBox.Show("Please select a payment to delete.");
                    return;
                }

                int paymentId = int.Parse(txtID.Text);
                DialogResult result = MessageBox.Show("Are you sure you want to delete this payment?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _paymentContext.DeletePayment(paymentId);
                    LoadPayments();
                    ClearFields();
                    MessageBox.Show("Payment deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting payment: " + ex.Message);
            }
        }

        // New button click (clear fields and focus on InvoiceID)
        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtInvoiceID.Focus();
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
            if (_paymentsTable != null)
            {
                _paymentsTable.DefaultView.RowFilter = string.Format(
                    "Convert(PaymentId, 'System.String') LIKE '%{0}%' OR Convert(InvoiceId, 'System.String') LIKE '%{0}%' OR Status LIKE '%{0}%'",
                    searchValue);
            }
        }

        // Populate textboxes when a cell is clicked
        private void DgvPayment_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                DataGridViewRow row = dgvPayment.Rows[e.RowIndex];
                txtID.Text = row.Cells["PaymentId"].Value.ToString();
                txtInvoiceID.Text = row.Cells["InvoiceId"].Value.ToString();
                txtUserID.Text = row.Cells["UserId"].Value.ToString();
                txtPaymentDate.Text = row.Cells["PaymentDate"].Value.ToString();
                txtPaymentMethod.Text = row.Cells["PaymentMethod"].Value.ToString();
                txtTransactionReference.Text = row.Cells["TransactionReference"].Value?.ToString() ?? "";
                txtStatus.Text = row.Cells["Status"].Value?.ToString() ?? "";
                txtNotes.Text = row.Cells["Notes"].Value?.ToString() ?? "";
            }
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

    // You could implement additional concrete strategies here, like:
    // - MockPaymentStrategy for testing
    // - ApiPaymentStrategy for external payment services
    // - CachePaymentStrategy for caching results
}