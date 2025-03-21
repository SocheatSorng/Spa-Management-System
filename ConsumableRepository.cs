using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class ConsumableRepository
    {
        private readonly SqlConnectionManager _connectionManager;

        public ConsumableRepository()
        {
            _connectionManager = SqlConnectionManager.Instance; // Singleton instance
        }

        // Fetch all consumables from the database
        public DataTable GetAllConsumables()
        {
            string query = "SELECT * FROM tbConsumable";
            return _connectionManager.ExecuteQuery(query);
        }

        // Insert a new consumable
        public void InsertConsumable(string name, string description, decimal price, string category, int stockQuantity)
        {
            string query = "INSERT INTO tbConsumable (Name, Description, Price, Category, StockQuantity, CreatedDate, ModifiedDate) " +
                           "VALUES (@Name, @Description, @Price, @Category, @StockQuantity, GETDATE(), GETDATE())";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", name),
                new SqlParameter("@Description", description),
                new SqlParameter("@Price", price),
                new SqlParameter("@Category", category),
                new SqlParameter("@StockQuantity", stockQuantity)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Update an existing consumable
        public void UpdateConsumable(int consumableId, string name, string description, decimal price, string category, int stockQuantity)
        {
            string query = "UPDATE tbConsumable SET Name = @Name, Description = @Description, Price = @Price, Category = @Category, " +
                           "StockQuantity = @StockQuantity, ModifiedDate = GETDATE() WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@Name", name),
                new SqlParameter("@Description", description),
                new SqlParameter("@Price", price),
                new SqlParameter("@Category", category),
                new SqlParameter("@StockQuantity", stockQuantity),
                new SqlParameter("@ConsumableId", consumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        // Delete a consumable by ID
        public void DeleteConsumable(int consumableId)
        {
            string query = "DELETE FROM tbConsumable WHERE ConsumableId = @ConsumableId";
            SqlParameter[] parameters = {
                new SqlParameter("@ConsumableId", consumableId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }
}