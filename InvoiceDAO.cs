using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class InvoiceDAO
    {
        private readonly SqlConnectionManager _connectionManager;

        public InvoiceDAO()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllInvoices()
        {
            string query = "SELECT InvoiceId, OrderId, InvoiceDate, TotalAmount, Notes FROM tbInvoice";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertInvoice(InvoiceModel invoice)
        {
            string query = "INSERT INTO tbInvoice (OrderId, InvoiceDate, TotalAmount, Notes) " +
                           "VALUES (@OrderId, @InvoiceDate, @TotalAmount, @Notes)";
            SqlParameter[] parameters = {
                new SqlParameter("@OrderId", invoice.OrderId),
                new SqlParameter("@InvoiceDate", invoice.InvoiceDate),
                new SqlParameter("@TotalAmount", invoice.TotalAmount),
                new SqlParameter("@Notes", invoice.Notes ?? (object)DBNull.Value)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdateInvoice(InvoiceModel invoice)
        {
            string query = "UPDATE tbInvoice SET OrderId = @OrderId, InvoiceDate = @InvoiceDate, " +
                           "TotalAmount = @TotalAmount, Notes = @Notes WHERE InvoiceId = @InvoiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@OrderId", invoice.OrderId),
                new SqlParameter("@InvoiceDate", invoice.InvoiceDate),
                new SqlParameter("@TotalAmount", invoice.TotalAmount),
                new SqlParameter("@Notes", invoice.Notes ?? (object)DBNull.Value),
                new SqlParameter("@InvoiceId", invoice.InvoiceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeleteInvoice(int invoiceId)
        {
            string query = "DELETE FROM tbInvoice WHERE InvoiceId = @InvoiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceId", invoiceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }
}
