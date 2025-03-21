using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    public class ServiceDAO
    {
        private readonly SqlConnectionManager _connectionManager;

        public ServiceDAO()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllServices()
        {
            string query = "SELECT ServiceId, ServiceName, Description, Price, CreatedDate, ModifiedDate FROM tbService";
            return _connectionManager.ExecuteQuery(query);
        }

        public void InsertService(ServiceModel service)
        {
            string query = "INSERT INTO tbService (ServiceName, Description, Price, CreatedDate, ModifiedDate) " +
                           "VALUES (@ServiceName, @Description, @Price, @CreatedDate, @ModifiedDate)";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceName", service.ServiceName),
                new SqlParameter("@Description", service.Description),
                new SqlParameter("@Price", service.Price),
                new SqlParameter("@CreatedDate", service.CreatedDate),
                new SqlParameter("@ModifiedDate", service.ModifiedDate)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void UpdateService(ServiceModel service)
        {
            string query = "UPDATE tbService SET ServiceName = @ServiceName, Description = @Description, Price = @Price, " +
                           "ModifiedDate = @ModifiedDate WHERE ServiceId = @ServiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceName", service.ServiceName),
                new SqlParameter("@Description", service.Description),
                new SqlParameter("@Price", service.Price),
                new SqlParameter("@ModifiedDate", DateTime.Now),
                new SqlParameter("@ServiceId", service.ServiceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void DeleteService(int serviceId)
        {
            string query = "DELETE FROM tbService WHERE ServiceId = @ServiceId";
            SqlParameter[] parameters = {
                new SqlParameter("@ServiceId", serviceId)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }
    }
}
