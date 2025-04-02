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
    public partial class Statistic : Form
    {
        private readonly SqlConnectionManager _connectionManager;
        private System.Windows.Forms.Timer refreshTimer;

        public Statistic()
        {
            InitializeComponent();
            
            // Add dragging capability to panels
            bunifuPanel1.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            
            bunifuPanel2.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            
            _connectionManager = SqlConnectionManager.Instance;

            // Setup label fonts and formatting
            SetupLabels();

            // Load statistics data
            LoadStatistics();

            // Setup a timer to refresh statistics
            SetupRefreshTimer();
        }
        // Add these at the top of your class, right after the "public partial class Service : Form" line
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += (sender, e) => LoadStatistics();
            refreshTimer.Start();
        }

        private void SetupLabels()
        {
            // Set font and formatting for all statistic labels
            Font labelFont = new Font("Segoe UI", 14, FontStyle.Bold);

            lbTotalCardIssue.Font = labelFont;
            lbTotalCustomer.Font = labelFont;
            lbActiveCustomer.Font = labelFont;
            lbCardInUse.Font = labelFont;
            lbTotalInvoice.Font = labelFont;
            lbTotalPayment.Font = labelFont;

            // Center text
            lbTotalCardIssue.TextAlignment = ContentAlignment.MiddleCenter;
            lbTotalCustomer.TextAlignment = ContentAlignment.MiddleCenter;
            lbActiveCustomer.TextAlignment = ContentAlignment.MiddleCenter;
            lbCardInUse.TextAlignment = ContentAlignment.MiddleCenter;
            lbTotalInvoice.TextAlignment = ContentAlignment.MiddleCenter;
            lbTotalPayment.TextAlignment = ContentAlignment.MiddleCenter;
        }

        private void LoadStatistics()
        {
            try
            {
                // Get Total Card Issue (All cards)
                string cardQuery = "SELECT COUNT(*) FROM tbCard";
                object totalCards = _connectionManager.ExecuteScalar(cardQuery);
                lbTotalCardIssue.Text = totalCards?.ToString() ?? "0";

                // Get Total Customer
                string customerQuery = "SELECT COUNT(*) FROM tbCustomer";
                object totalCustomers = _connectionManager.ExecuteScalar(customerQuery);
                lbTotalCustomer.Text = totalCustomers?.ToString() ?? "0";

                // Get Active Customer (Customers with ReleasedTime IS NULL)
                string activeCustomerQuery = "SELECT COUNT(*) FROM tbCustomer WHERE ReleasedTime IS NULL";
                object activeCustomers = _connectionManager.ExecuteScalar(activeCustomerQuery);
                lbActiveCustomer.Text = activeCustomers?.ToString() ?? "0";

                // Get Card In Use (Cards with Status = 'In Use')
                string cardInUseQuery = "SELECT COUNT(*) FROM tbCard WHERE Status = 'In Use'";
                object cardsInUse = _connectionManager.ExecuteScalar(cardInUseQuery);
                lbCardInUse.Text = cardsInUse?.ToString() ?? "0";

                // Get Total Invoice
                string invoiceQuery = "SELECT COUNT(*) FROM tbInvoice";
                object totalInvoices = _connectionManager.ExecuteScalar(invoiceQuery);
                lbTotalInvoice.Text = totalInvoices?.ToString() ?? "0";
                
                // Get Total Payments
                string paymentQuery = "SELECT COUNT(*) FROM tbPayment";
                object totalPayments = _connectionManager.ExecuteScalar(paymentQuery);
                lbTotalPayment.Text = totalPayments?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Stop the timer when the form is closed
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void bunifuPictureBox4_Click(object sender, EventArgs e)
        {
            // Refresh statistics when the picture is clicked
            LoadStatistics();
        }

        private void btnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
