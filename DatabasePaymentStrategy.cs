using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class DatabasePaymentStrategy : IPaymentDataStrategy
    {
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
}
