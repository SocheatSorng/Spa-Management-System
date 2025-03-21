using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Spa_Management_System
{
    public class PaymentContext
    {
        private IPaymentDataStrategy _strategy;

        public PaymentContext(IPaymentDataStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IPaymentDataStrategy strategy)
        {
            _strategy = strategy;
        }

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
}
