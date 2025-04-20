using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class SqlConnectionManager
    {
        private static SqlConnectionManager _instance;
        private static readonly object _lock = new object();
        private string _connectionString;
        private bool _hasValidConnection = false;
        private bool _isAuthenticated = false;
        private string _currentUsername = null;

        // Static property to easily set and get the connection string
        public static string ConnectionString
        {
            get 
            { 
                return Instance._connectionString; 
            }
            set 
            { 
                Instance.SetConnectionString(value); 
            }
        }

        // Static property to check authentication status
        public static bool IsAuthenticated
        {
            get { return Instance._isAuthenticated; }
        }

        // Static property to get current username
        public static string CurrentUser
        {
            get { return Instance._currentUsername; }
        }

        // Private constructor for Singleton pattern
        private SqlConnectionManager()
        {
            // No default connection string - must be set before use
            _connectionString = null;
        }

        // Allow setting a custom connection string
        public void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty");
                
            _connectionString = connectionString;
            _hasValidConnection = TestConnection();
        }

        // Test if the current connection string is valid
        public bool TestConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                return false;
                
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Check if we have a valid connection string
        public bool HasValidConnection()
        {
            return _hasValidConnection;
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
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string has not been set. Call SetConnectionString first.");
                
            return new SqlConnection(_connectionString);
        }

        // Execute a query that returns a DataTable
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string has not been set. Call SetConnectionString first.");
                
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
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string has not been set. Call SetConnectionString first.");
                
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
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string has not been set. Call SetConnectionString first.");
                
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

        // Get the name of the server we're connected to
        public string GetConnectedServerName()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                    return "Not Connected";
                    
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
                return builder.DataSource;
            }
            catch
            {
                return "Unknown Server";
            }
        }
        
        // Get the name of the database we're connected to
        public string GetConnectedDatabaseName()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                    return "Not Connected";
                    
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
                return builder.InitialCatalog;
            }
            catch
            {
                return "Unknown Database";
            }
        }

        // Set user authentication status
        public static void SetAuthenticated(string username, bool isAuthenticated)
        {
            if (isAuthenticated && string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty when authenticated");
                
            Instance._isAuthenticated = isAuthenticated;
            Instance._currentUsername = isAuthenticated ? username : null;
        }
    }
}