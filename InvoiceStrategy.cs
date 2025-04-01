// InvoiceStrategy.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Strategy Pattern (Gang of Four)
    // Define strategy interface
    public interface IInvoiceStrategy
    {
        DataTable GetAllInvoices();
        void InsertInvoice(InvoiceModel invoice);
        void UpdateInvoice(InvoiceModel invoice);
        void DeleteInvoice(int invoiceId);
    }

    // Concrete strategy implementation for SQL Server
    public class SqlServerInvoiceStrategy : IInvoiceStrategy
    {
        private readonly SqlConnectionManager _connectionManager;

        public SqlServerInvoiceStrategy()
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

    // Optional: Add more concrete strategies if needed (e.g., for testing or other databases)
    public class InMemoryInvoiceStrategy : IInvoiceStrategy
    {
        private readonly DataTable _invoiceTable;

        public InMemoryInvoiceStrategy()
        {
            _invoiceTable = new DataTable();
            _invoiceTable.Columns.Add("InvoiceId", typeof(int));
            _invoiceTable.Columns.Add("OrderId", typeof(int));
            _invoiceTable.Columns.Add("InvoiceDate", typeof(DateTime));
            _invoiceTable.Columns.Add("TotalAmount", typeof(decimal));
            _invoiceTable.Columns.Add("Notes", typeof(string));
        }

        public DataTable GetAllInvoices()
        {
            return _invoiceTable.Copy();
        }

        public void InsertInvoice(InvoiceModel invoice)
        {
            // For simplicity, we'll generate a new ID
            int newId = _invoiceTable.Rows.Count + 1;
            DataRow row = _invoiceTable.NewRow();
            row["InvoiceId"] = newId;
            row["OrderId"] = invoice.OrderId;
            row["InvoiceDate"] = invoice.InvoiceDate;
            row["TotalAmount"] = invoice.TotalAmount;
            row["Notes"] = invoice.Notes ?? string.Empty;
            _invoiceTable.Rows.Add(row);
        }

        public void UpdateInvoice(InvoiceModel invoice)
        {
            foreach (DataRow row in _invoiceTable.Rows)
            {
                if ((int)row["InvoiceId"] == invoice.InvoiceId)
                {
                    row["OrderId"] = invoice.OrderId;
                    row["InvoiceDate"] = invoice.InvoiceDate;
                    row["TotalAmount"] = invoice.TotalAmount;
                    row["Notes"] = invoice.Notes ?? string.Empty;
                    break;
                }
            }
        }

        public void DeleteInvoice(int invoiceId)
        {
            foreach (DataRow row in _invoiceTable.Rows)
            {
                if ((int)row["InvoiceId"] == invoiceId)
                {
                    _invoiceTable.Rows.Remove(row);
                    break;
                }
            }
        }
    }
} 