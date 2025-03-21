using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class SqlConnectionManager
    {
        private static SqlConnectionManager _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        // Private constructor for Singleton pattern
        private SqlConnectionManager()
        {
            _connectionString = "data source=SOCHEAT\\MSSQLEXPRESS2022; initial catalog=SpaManagement; trusted_connection=true; encrypt=false";
        }

        // Singleton instance property with thread-safe double-check locking
        public static SqlConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SqlConnectionManager();
                        }
                    }
                }
                return _instance;
            }
        }

        // Create and return a new SqlConnection object
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Execute a query that returns a DataTable
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Database error: {ex.Message}");
                    throw;
                }
            }
            return dataTable;
        }

        // Execute a non-query command (for INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Database error: {ex.Message}");
                    throw;
                }
            }
            return rowsAffected;
        }

        // Execute a scalar query (returning a single value)
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            object result = null;
            using (SqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        result = command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Database error: {ex.Message}");
                    throw;
                }
            }
            return result;
        }
    }
}