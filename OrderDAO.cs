using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class OrderDAO
    {
        private readonly SqlConnectionManager _connectionManager;

        public OrderDAO()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllOrders()
        {
            string query = "SELECT OrderId, CustomerId, UserId, OrderTime, TotalAmount, Discount, FinalAmount, Notes, Status FROM tbOrder";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertOrder(OrderModel order)
        {
            string query = "INSERT INTO tbOrder (CustomerId, UserId, OrderTime, TotalAmount, Discount, FinalAmount, Notes, Status) " +
                           "VALUES (@CustomerId, @UserId, @OrderTime, @TotalAmount, @Discount, @FinalAmount, @Notes, @Status)";
            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", order.CustomerId),
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@OrderTime", order.OrderTime),
                new SqlParameter("@TotalAmount", order.TotalAmount),
                new SqlParameter("@Discount", order.Discount),
                new SqlParameter("@FinalAmount", order.FinalAmount),
                new SqlParameter("@Notes", order.Notes ?? (object)DBNull.Value),
                new SqlParameter("@Status", order.Status ?? (object)DBNull.Value)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdateOrder(OrderModel order)
        {
            string query = "UPDATE tbOrder SET CustomerId = @CustomerId, UserId = @UserId, OrderTime = @OrderTime, " +
                           "TotalAmount = @TotalAmount, Discount = @Discount, FinalAmount = @FinalAmount, Notes = @Notes, " +
                           "Status = @Status WHERE OrderId = @OrderId";
            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", order.CustomerId),
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@OrderTime", order.OrderTime),
                new SqlParameter("@TotalAmount", order.TotalAmount),
                new SqlParameter("@Discount", order.Discount),
                new SqlParameter("@FinalAmount", order.FinalAmount),
                new SqlParameter("@Notes", order.Notes ?? (object)DBNull.Value),
                new SqlParameter("@Status", order.Status ?? (object)DBNull.Value),
                new SqlParameter("@OrderId", order.OrderId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeleteOrder(int orderId)
        {
            string query = "DELETE FROM tbOrder WHERE OrderId = @OrderId";
            SqlParameter[] parameters = {
                new SqlParameter("@OrderId", orderId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }
}