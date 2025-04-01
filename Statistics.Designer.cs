//using System.Windows.Forms.DataVisualization.Charting;

//namespace Spa_Management_System
//{
//    partial class Statistics
//    {
//        private System.ComponentModel.IContainer components = null;

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        private void InitializeComponent()
//        {
//            ChartArea chartArea1 = new ChartArea();
//            Legend legend1 = new Legend();
//            ChartArea chartArea2 = new ChartArea();
//            Legend legend2 = new Legend();
//            ChartArea chartArea3 = new ChartArea();
//            Legend legend3 = new Legend();

//            this.panel1 = new Bunifu.UI.WinForms.BunifuPanel();
//            this.label1 = new System.Windows.Forms.Label();
//            this.btnExitProgram = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
//            this.datePicker = new Bunifu.UI.WinForms.BunifuDatePicker();

//            // Summary Cards
//            this.cardSummary = new Bunifu.UI.WinForms.BunifuPanel();
//            this.lblTotalOrders = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalOrdersTitle = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblGrossSales = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblGrossSalesTitle = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalDiscounts = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalDiscountsTitle = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblNetSales = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblNetSalesTitle = new Bunifu.UI.WinForms.BunifuLabel();

//            // Customer Stats
//            this.cardCustomers = new Bunifu.UI.WinForms.BunifuPanel();
//            this.lblTotalCustomers = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalCustomersTitle = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblAvgServiceTime = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblAvgServiceTimeTitle = new Bunifu.UI.WinForms.BunifuLabel();

//            // Inventory Stats
//            this.cardInventory = new Bunifu.UI.WinForms.BunifuPanel();
//            this.lblLowStockItems = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblLowStockItemsTitle = new Bunifu.UI.WinForms.BunifuLabel();

//            // Charts
//            this.chartPayments = new Chart();
//            this.chartTopServices = new Chart();
//            this.chartTopConsumables = new Chart();

//            // Initialize components
//            this.SuspendLayout();

//            // Form
//            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.BackColor = System.Drawing.Color.White;
//            this.ClientSize = new System.Drawing.Size(1370, 728);
//            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

//            // Header Panel
//            this.panel1 = new Bunifu.UI.WinForms.BunifuPanel();
//            this.panel1.BackgroundColor = System.Drawing.Color.FromArgb(51, 51, 76);
//            this.panel1.Location = new System.Drawing.Point(0, 0);
//            this.panel1.Size = new System.Drawing.Size(1370, 60);
//            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;

//            // Title
//            this.label1 = new System.Windows.Forms.Label();
//            this.label1.Text = "Statistics Dashboard";
//            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold);
//            this.label1.ForeColor = System.Drawing.Color.White;
//            this.label1.Location = new System.Drawing.Point(12, 15);
//            this.label1.Size = new System.Drawing.Size(300, 30);
//            this.panel1.Controls.Add(this.label1);

//            // Exit Button
//            this.btnExitProgram = new Bunifu.UI.WinForms.BunifuButton.BunifuIconButton();
//            this.btnExitProgram.BackColor = System.Drawing.Color.Transparent;
//            this.btnExitProgram.Location = new System.Drawing.Point(1330, 15);
//            this.btnExitProgram.Size = new System.Drawing.Size(30, 30);
//            this.panel1.Controls.Add(this.btnExitProgram);

//            // Date Picker
//            this.datePicker = new Bunifu.UI.WinForms.BunifuDatePicker();
//            this.datePicker.Location = new System.Drawing.Point(1100, 15);
//            this.datePicker.Size = new System.Drawing.Size(200, 30);
//            this.panel1.Controls.Add(this.datePicker);

//            // Summary Cards Panel
//            this.cardSummary = new Bunifu.UI.WinForms.BunifuPanel();
//            this.cardSummary.BackgroundColor = System.Drawing.Color.White;
//            this.cardSummary.BorderColor = System.Drawing.Color.Silver;
//            this.cardSummary.BorderRadius = 10;
//            this.cardSummary.BorderThickness = 1;
//            this.cardSummary.Location = new System.Drawing.Point(20, 80);
//            this.cardSummary.Size = new System.Drawing.Size(1330, 100);

//            // Summary Labels
//            this.lblTotalOrdersTitle = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalOrdersTitle.Text = "Total Orders";
//            this.lblTotalOrdersTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
//            this.lblTotalOrdersTitle.Location = new System.Drawing.Point(20, 20);
//            this.cardSummary.Controls.Add(this.lblTotalOrdersTitle);

//            this.lblTotalOrders = new Bunifu.UI.WinForms.BunifuLabel();
//            this.lblTotalOrders.Text = "0";
//            this.lblTotalOrders.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold);
//            this.lblTotalOrders.Location = new System.Drawing.Point(20, 50);
//            this.cardSummary.Controls.Add(this.lblTotalOrders);

//            // Charts
//            this.chartPayments = new Chart();
//            this.chartPayments.Location = new System.Drawing.Point(20, 200);
//            this.chartPayments.Size = new System.Drawing.Size(400, 300);
//            chartArea1.Name = "ChartArea1";
//            this.chartPayments.ChartAreas.Add(chartArea1);
//            legend1.Name = "Legend1";
//            this.chartPayments.Legends.Add(legend1);
//            this.chartPayments.Text = "Payment Methods";

//            this.chartTopServices = new Chart();
//            this.chartTopServices.Location = new System.Drawing.Point(440, 200);
//            this.chartTopServices.Size = new System.Drawing.Size(400, 300);
//            chartArea2.Name = "ChartArea1";
//            this.chartTopServices.ChartAreas.Add(chartArea2);
//            legend2.Name = "Legend1";
//            this.chartTopServices.Legends.Add(legend2);
//            this.chartTopServices.Text = "Top Services";

//            this.chartTopConsumables = new Chart();
//            this.chartTopConsumables.Location = new System.Drawing.Point(860, 200);
//            this.chartTopConsumables.Size = new System.Drawing.Size(400, 300);
//            chartArea3.Name = "ChartArea1";
//            this.chartTopConsumables.ChartAreas.Add(chartArea3);
//            legend3.Name = "Legend1";
//            this.chartTopConsumables.Legends.Add(legend3);
//            this.chartTopConsumables.Text = "Top Consumables";

//            // Add controls to form
//            this.Controls.Add(this.panel1);
//            this.Controls.Add(this.cardSummary);
//            this.Controls.Add(this.chartPayments);
//            this.Controls.Add(this.chartTopServices);
//            this.Controls.Add(this.chartTopConsumables);

//            this.ResumeLayout(false);
//        }

//        #endregion

//        private Bunifu.UI.WinForms.BunifuPanel panel1;
//        private System.Windows.Forms.Label label1;
//        private Bunifu.UI.WinForms.BunifuButton.BunifuIconButton btnExitProgram;
//        private Bunifu.UI.WinForms.BunifuDatePicker datePicker;

//        private Bunifu.UI.WinForms.BunifuPanel cardSummary;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalOrders;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalOrdersTitle;
//        private Bunifu.UI.WinForms.BunifuLabel lblGrossSales;
//        private Bunifu.UI.WinForms.BunifuLabel lblGrossSalesTitle;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalDiscounts;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalDiscountsTitle;
//        private Bunifu.UI.WinForms.BunifuLabel lblNetSales;
//        private Bunifu.UI.WinForms.BunifuLabel lblNetSalesTitle;

//        private Bunifu.UI.WinForms.BunifuPanel cardCustomers;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalCustomers;
//        private Bunifu.UI.WinForms.BunifuLabel lblTotalCustomersTitle;
//        private Bunifu.UI.WinForms.BunifuLabel lblAvgServiceTime;
//        private Bunifu.UI.WinForms.BunifuLabel lblAvgServiceTimeTitle;

//        private Bunifu.UI.WinForms.BunifuPanel cardInventory;
//        private Bunifu.UI.WinForms.BunifuLabel lblLowStockItems;
//        private Bunifu.UI.WinForms.BunifuLabel lblLowStockItemsTitle;

//        private Chart chartPayments;
//        private Chart chartTopServices;
//        private Chart chartTopConsumables;
//    }
//} 