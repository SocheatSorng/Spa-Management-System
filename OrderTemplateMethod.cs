// OrderTemplateMethod.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Template Method Pattern (Gang of Four)
    // Abstract base class defining the algorithm template
    public abstract class DataAccessTemplate
    {
        protected readonly SqlConnectionManager _connectionManager;

        protected DataAccessTemplate()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        // Template method that defines the algorithm
        public DataTable GetAll()
        {
            string query = DefineSelectQuery();
            return _connectionManager.ExecuteQuery(query);
        }

        // Template method for insertion
        public void Insert(object entity)
        {
            string query = DefineInsertQuery();
            SqlParameter[] parameters = DefineInsertParameters(entity);
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Template method for updating
        public void Update(object entity)
        {
            string query = DefineUpdateQuery();
            SqlParameter[] parameters = DefineUpdateParameters(entity);
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Template method for deletion
        public void Delete(int id)
        {
            string query = DefineDeleteQuery();
            SqlParameter[] parameters = DefineDeleteParameters(id);
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Hook methods to be implemented by subclasses
        protected abstract string DefineSelectQuery();
        protected abstract string DefineInsertQuery();
        protected abstract string DefineUpdateQuery();
        protected abstract string DefineDeleteQuery();
        protected abstract SqlParameter[] DefineInsertParameters(object entity);
        protected abstract SqlParameter[] DefineUpdateParameters(object entity);
        protected abstract SqlParameter[] DefineDeleteParameters(int id);
    }

    // Concrete implementation of the template for Order data access
    public class OrderDataAccess : DataAccessTemplate
    {
        protected override string DefineSelectQuery()
        {
            return "SELECT OrderId, CustomerId, UserId, OrderTime, TotalAmount, Discount, FinalAmount, Notes, Status FROM tbOrder";
        }

        protected override string DefineInsertQuery()
        {
            return "INSERT INTO tbOrder (CustomerId, UserId, OrderTime, TotalAmount, Discount, FinalAmount, Notes, Status) " +
                   "VALUES (@CustomerId, @UserId, @OrderTime, @TotalAmount, @Discount, @FinalAmount, @Notes, @Status)";
        }

        protected override string DefineUpdateQuery()
        {
            return "UPDATE tbOrder SET CustomerId = @CustomerId, UserId = @UserId, OrderTime = @OrderTime, " +
                   "TotalAmount = @TotalAmount, Discount = @Discount, FinalAmount = @FinalAmount, Notes = @Notes, " +
                   "Status = @Status WHERE OrderId = @OrderId";
        }

        protected override string DefineDeleteQuery()
        {
            return "DELETE FROM tbOrder WHERE OrderId = @OrderId";
        }

        protected override SqlParameter[] DefineInsertParameters(object entity)
        {
            OrderModel order = (OrderModel)entity;
            return new SqlParameter[] {
                new SqlParameter("@CustomerId", order.CustomerId),
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@OrderTime", order.OrderTime),
                new SqlParameter("@TotalAmount", order.TotalAmount),
                new SqlParameter("@Discount", order.Discount),
                new SqlParameter("@FinalAmount", order.FinalAmount),
                new SqlParameter("@Notes", order.Notes ?? (object)DBNull.Value),
                new SqlParameter("@Status", order.Status ?? (object)DBNull.Value)
            };
        }

        protected override SqlParameter[] DefineUpdateParameters(object entity)
        {
            OrderModel order = (OrderModel)entity;
            return new SqlParameter[] {
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
        }

        protected override SqlParameter[] DefineDeleteParameters(int id)
        {
            return new SqlParameter[] {
                new SqlParameter("@OrderId", id)
            };
        }
    }
} 