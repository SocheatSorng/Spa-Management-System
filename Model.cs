using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Management_System
{
    public class ServiceModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ImagePath { get; set; }
    }

    public class PaymentModel
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public int UserId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class OrderModel
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
    }

    public class InvoiceModel
    {
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
    }

    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class ConsumableModel
    {
        public int ConsumableId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ImagePath { get; set; }
    }
}