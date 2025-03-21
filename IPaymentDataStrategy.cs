using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Spa_Management_System
{
    public interface IPaymentDataStrategy
    {
        DataTable GetAllPayments();
        void InsertPayment(PaymentModel payment);
        void UpdatePayment(PaymentModel payment);
        void DeletePayment(int paymentId);
    }
}
