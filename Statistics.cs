using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Bunifu.UI.WinForms;

namespace Spa_Management_System
{
    public partial class Statistics : Form
    {
        private readonly StatisticsInvoker _invoker;

        public Statistics()
        {
            InitializeComponent();
            _invoker = new StatisticsInvoker();

            // Set up date picker
            datePicker.Value = DateTime.Today;
            datePicker.ValueChanged += DatePicker_ValueChanged;

            // Set up exit button
            btnExitProgram.Click += BtnExitProgram_Click;

            // Initialize charts
            InitializeCharts();

            // Load initial data
            LoadStatistics();
        }

        private void InitializeCharts()
        {
            // Payment Methods Chart
            chartPayments.Series.Clear();
            var paymentSeries = chartPayments.Series.Add("Payments");
            paymentSeries.ChartType = SeriesChartType.Pie;
            chartPayments.Titles.Add("Payment Methods");

            // Top Services Chart
            chartTopServices.Series.Clear();
            var serviceSeries = chartTopServices.Series.Add("Revenue");
            serviceSeries.ChartType = SeriesChartType.Column;
            chartTopServices.Titles.Add("Top Services");

            // Top Consumables Chart
            chartTopConsumables.Series.Clear();
            var consumableSeries = chartTopConsumables.Series.Add("Revenue");
            consumableSeries.ChartType = SeriesChartType.Column;
            chartTopConsumables.Titles.Add("Top Consumables");
        }

        private void LoadStatistics()
        {
            try
            {
                LoadDailySalesSummary();
                LoadPaymentMethodBreakdown();
                LoadTopServices();
                LoadTopConsumables();
                LoadCustomerStatistics();
                LoadLowStockItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading statistics: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDailySalesSummary()
        {
            // Create command
            var command = new GetDailySalesSummaryCommand(_invoker, datePicker.Value);
            
            // Execute command
            var dt = _invoker.ExecuteCommand(command);
            
            if (dt.Rows.Count > 0)
            {
                lblTotalOrders.Text = dt.Rows[0]["TotalOrders"].ToString();
                lblGrossSales.Text = $"${dt.Rows[0]["GrossSales"]:N2}";
                lblTotalDiscounts.Text = $"${dt.Rows[0]["TotalDiscounts"]:N2}";
                lblNetSales.Text = $"${dt.Rows[0]["NetSales"]:N2}";
            }
            else
            {
                lblTotalOrders.Text = "0";
                lblGrossSales.Text = "$0.00";
                lblTotalDiscounts.Text = "$0.00";
                lblNetSales.Text = "$0.00";
            }
        }

        private void LoadPaymentMethodBreakdown()
        {
            // Create command
            var command = new GetPaymentMethodBreakdownCommand(_invoker, datePicker.Value);
            
            // Execute command
            var dt = _invoker.ExecuteCommand(command);
            
            var series = chartPayments.Series[0];
            series.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                var point = series.Points.Add(Convert.ToDouble(row["TotalAmount"]));
                point.LegendText = $"{row["PaymentMethod"]} ({row["Count"]})";
                point.Label = $"${Convert.ToDouble(row["TotalAmount"]):N2}";
            }
        }

        private void LoadTopServices()
        {
            // Create command
            var command = new GetTopServicesCommand(_invoker, datePicker.Value);
            
            // Execute command
            var dt = _invoker.ExecuteCommand(command);
            
            var series = chartTopServices.Series[0];
            series.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                var point = series.Points.Add(Convert.ToDouble(row["TotalRevenue"]));
                point.AxisLabel = row["ServiceName"].ToString();
                point.Label = $"${Convert.ToDouble(row["TotalRevenue"]):N2}";
                point.ToolTip = $"Ordered: {row["TimesOrdered"]} times\nQuantity: {row["TotalQuantity"]}";
            }
        }

        private void LoadTopConsumables()
        {
            // Create command
            var command = new GetTopConsumablesCommand(_invoker, datePicker.Value);
            
            // Execute command
            var dt = _invoker.ExecuteCommand(command);
            
            var series = chartTopConsumables.Series[0];
            series.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                var point = series.Points.Add(Convert.ToDouble(row["TotalRevenue"]));
                point.AxisLabel = row["Name"].ToString();
                point.Label = $"${Convert.ToDouble(row["TotalRevenue"]):N2}";
                point.ToolTip = $"Category: {row["Category"]}\nOrdered: {row["TimesOrdered"]} times\nQuantity: {row["TotalQuantity"]}";
            }
        }

        private void LoadCustomerStatistics()
        {
            try
            {
                // Create command
                var command = new GetCustomerStatisticsCommand(_invoker, datePicker.Value);
                
                // Execute command
                var dt = _invoker.ExecuteCommand(command);
                
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    lblTotalCustomers.Text = row["TotalCustomers"]?.ToString() ?? "0";
                    
                    var avgServiceTime = row["AvgServiceTimeMinutes"];
                    lblAvgServiceTime.Text = avgServiceTime == DBNull.Value ? 
                        "0 minutes" : 
                        $"{Convert.ToDouble(avgServiceTime):N0} minutes";
                }
                else
                {
                    lblTotalCustomers.Text = "0";
                    lblAvgServiceTime.Text = "0 minutes";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer statistics: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblTotalCustomers.Text = "0";
                lblAvgServiceTime.Text = "0 minutes";
            }
        }

        private void LoadLowStockItems()
        {
            // Create command
            var command = new GetLowStockItemsCommand(_invoker);
            
            // Execute command
            var dt = _invoker.ExecuteCommand(command);
            
            var lowStockCount = dt.Rows.Count;
            lblLowStockItems.Text = $"{lowStockCount} items";
            lblLowStockItems.ForeColor = lowStockCount > 0 ? Color.Red : Color.Green;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private void BtnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 