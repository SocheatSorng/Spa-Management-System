// CardProxy.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Proxy Pattern (Gang of Four)
    
    // Subject interface
    public interface ICardDataAccess
    {
        DataTable GetAllCards();
        void RegisterCard(string cardId);
        void RegisterCardBatch(string prefix, int startNumber, int count);
        void SetCardAsDamaged(string cardId);
    }

    // Real Subject
    public class RealCardDataAccess : ICardDataAccess
    {
        private readonly SqlConnectionManager _connectionManager;

        public RealCardDataAccess()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllCards()
        {
            string query = "SELECT CardId, Status, LastUsed, CreatedDate FROM tbCard ORDER BY CreatedDate DESC";
            return _connectionManager.ExecuteQuery(query);
        }

        public void RegisterCard(string cardId)
        {
            string query = "EXEC sp_RegisterCard @CardId";
            SqlParameter param = new SqlParameter("@CardId", cardId);
            _connectionManager.ExecuteNonQuery(query, param);
        }

        public void RegisterCardBatch(string prefix, int startNumber, int count)
        {
            string query = "EXEC sp_RegisterCardBatch @Prefix, @StartNumber, @Count";
            SqlParameter[] parameters = {
                new SqlParameter("@Prefix", prefix),
                new SqlParameter("@StartNumber", startNumber),
                new SqlParameter("@Count", count)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void SetCardAsDamaged(string cardId)
        {
            string query = "EXEC sp_SetCardAsDamaged @CardId";
            SqlParameter param = new SqlParameter("@CardId", cardId);
            _connectionManager.ExecuteNonQuery(query, param);
        }
    }

    // Proxy
    public class CardProtectionProxy : ICardDataAccess
    {
        private RealCardDataAccess _realDataAccess;
        private readonly bool _useTransactionLogging;
        private readonly bool _requiresAdminPrivileges;

        public CardProtectionProxy(bool useTransactionLogging = true, bool requiresAdminPrivileges = false)
        {
            _useTransactionLogging = useTransactionLogging;
            _requiresAdminPrivileges = requiresAdminPrivileges;
        }

        // Lazy initialization of the real subject
        private RealCardDataAccess RealDataAccess
        {
            get
            {
                if (_realDataAccess == null)
                {
                    _realDataAccess = new RealCardDataAccess();
                }
                return _realDataAccess;
            }
        }

        public DataTable GetAllCards()
        {
            // No special restrictions for reading data
            if (_useTransactionLogging)
            {
                LogTransaction("GetAllCards", "Read operation");
            }
            return RealDataAccess.GetAllCards();
        }

        public void RegisterCard(string cardId)
        {
            // Check permissions and validate input before proceeding
            if (_requiresAdminPrivileges && !IsCurrentUserAdmin())
            {
                throw new UnauthorizedAccessException("Admin privileges required to register cards.");
            }

            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new ArgumentException("Card ID cannot be empty.");
            }

            if (_useTransactionLogging)
            {
                LogTransaction("RegisterCard", $"Card ID: {cardId}");
            }

            RealDataAccess.RegisterCard(cardId);
        }

        public void RegisterCardBatch(string prefix, int startNumber, int count)
        {
            // Check permissions and validate input before proceeding
            if (_requiresAdminPrivileges && !IsCurrentUserAdmin())
            {
                throw new UnauthorizedAccessException("Admin privileges required to register card batches.");
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix cannot be empty.");
            }

            if (startNumber < 0)
            {
                throw new ArgumentException("Start number must be positive.");
            }

            if (count <= 0 || count > 1000)
            {
                throw new ArgumentException("Count must be between 1 and 1000.");
            }

            if (_useTransactionLogging)
            {
                LogTransaction("RegisterCardBatch", $"Prefix: {prefix}, Start: {startNumber}, Count: {count}");
            }

            RealDataAccess.RegisterCardBatch(prefix, startNumber, count);
        }

        public void SetCardAsDamaged(string cardId)
        {
            // Check permissions and validate input before proceeding
            if (_requiresAdminPrivileges && !IsCurrentUserAdmin())
            {
                throw new UnauthorizedAccessException("Admin privileges required to mark cards as damaged.");
            }

            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new ArgumentException("Card ID cannot be empty.");
            }

            if (_useTransactionLogging)
            {
                LogTransaction("SetCardAsDamaged", $"Card ID: {cardId}");
            }

            RealDataAccess.SetCardAsDamaged(cardId);
        }

        // Helper methods for the proxy functionality
        private void LogTransaction(string operation, string details)
        {
            // In a real application, this would log to a file or database
            string logEntry = $"{DateTime.Now}: {operation} - {details}";
            System.Diagnostics.Debug.WriteLine(logEntry);
        }

        private bool IsCurrentUserAdmin()
        {
            // In a real application, this would check user roles
            // This is a simplified implementation for demonstration
            return true; // Assume everyone is admin for now
        }
    }
} 