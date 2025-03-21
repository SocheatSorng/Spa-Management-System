using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Management_System
{
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
}
