// ServiceDecorator.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace Spa_Management_System
{
    // Component interface
    public interface IServiceComponent
    {
        DataTable GetAllServices();
        void InsertService(ServiceModel service);
        void UpdateService(ServiceModel service);
        void DeleteService(int serviceId);
    }

    // Concrete Component
    public class BasicServiceComponent : IServiceComponent
    {
        private readonly string _connectionString;

        public BasicServiceComponent()
        {
            // You should adjust this to match your actual connection string source
            _connectionString = "data source=SOCHEAT\\MSSQLEXPRESS2022; initial catalog=SpaManagement; trusted_connection=true; encrypt=false";
        }

        // Get all services
        public DataTable GetAllServices()
        {
            DataTable services = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Using stored procedure if available
                    using (SqlCommand command = new SqlCommand("sp_GetAvailableServices", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // If stored procedure is unavailable, comment above and uncomment below
                        // using (SqlCommand command = new SqlCommand("SELECT ServiceId, ServiceName, Description, Price, CreatedDate, ModifiedDate FROM tbService ORDER BY ServiceName", connection))

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(services);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving services: " + ex.Message);
            }
            return services;
        }

        // Insert service
        public void InsertService(ServiceModel service)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO tbService (ServiceName, Description, Price, ImagePath, CreatedDate, ModifiedDate) " +
                                   "VALUES (@ServiceName, @Description, @Price, @ImagePath, @CreatedDate, @ModifiedDate)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                        command.Parameters.AddWithValue("@Description", (object)service.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Price", service.Price);
                        command.Parameters.AddWithValue("@ImagePath", (object)service.ImagePath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDate", service.CreatedDate);
                        command.Parameters.AddWithValue("@ModifiedDate", service.ModifiedDate);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting service", ex);
            }
        }

        // Update service
        public void UpdateService(ServiceModel service)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE tbService SET ServiceName = @ServiceName, Description = @Description, " +
                                   "Price = @Price, ImagePath = @ImagePath, ModifiedDate = @ModifiedDate WHERE ServiceId = @ServiceId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ServiceId", service.ServiceId);
                        command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                        command.Parameters.AddWithValue("@Description", (object)service.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Price", service.Price);
                        command.Parameters.AddWithValue("@ImagePath", (object)service.ImagePath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating service", ex);
            }
        }

        // Delete service
        public void DeleteService(int serviceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM tbService WHERE ServiceId = @ServiceId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ServiceId", serviceId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting service", ex);
            }
        }
    }

    // Decorator abstract class
    public abstract class ServiceDecorator : IServiceComponent
    {
        protected IServiceComponent _component;

        public ServiceDecorator(IServiceComponent component)
        {
            _component = component;
        }

        public virtual DataTable GetAllServices()
        {
            return _component.GetAllServices();
        }

        public virtual void InsertService(ServiceModel service)
        {
            _component.InsertService(service);
        }

        public virtual void UpdateService(ServiceModel service)
        {
            _component.UpdateService(service);
        }

        public virtual void DeleteService(int serviceId)
        {
            _component.DeleteService(serviceId);
        }
    }

    // Concrete Decorator - Adds logging functionality
    public class LoggingServiceDecorator : ServiceDecorator
    {
        public LoggingServiceDecorator(IServiceComponent component) : base(component)
        {
        }

        public override DataTable GetAllServices()
        {
            LogOperation("GetAllServices");
            return base.GetAllServices();
        }

        public override void InsertService(ServiceModel service)
        {
            LogOperation($"InsertService: {service.ServiceName}");
            base.InsertService(service);
        }

        public override void UpdateService(ServiceModel service)
        {
            LogOperation($"UpdateService: ID={service.ServiceId}, Name={service.ServiceName}");
            base.UpdateService(service);
        }

        public override void DeleteService(int serviceId)
        {
            LogOperation($"DeleteService: ID={serviceId}");
            base.DeleteService(serviceId);
        }

        private void LogOperation(string operation)
        {
            string logMessage = $"{DateTime.Now}: {operation}";
            // In a real application, this would log to a file or database
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
    }

    // Concrete Decorator - Adds validation functionality
    public class ValidationServiceDecorator : ServiceDecorator
    {
        public ValidationServiceDecorator(IServiceComponent component) : base(component)
        {
        }

        public override void InsertService(ServiceModel service)
        {
            ValidateService(service);
            base.InsertService(service);
        }

        public override void UpdateService(ServiceModel service)
        {
            ValidateService(service);
            base.UpdateService(service);
        }

        private void ValidateService(ServiceModel service)
        {
            if (string.IsNullOrWhiteSpace(service.ServiceName))
            {
                throw new ArgumentException("Service name cannot be empty");
            }

            if (service.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative");
            }
        }
    }

    // Concrete Decorator - Adds caching functionality
    public class CachingServiceDecorator : ServiceDecorator
    {
        private DataTable _cachedServices;
        private DateTime _cacheTime;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public CachingServiceDecorator(IServiceComponent component) : base(component)
        {
        }

        public override DataTable GetAllServices()
        {
            if (_cachedServices != null && DateTime.Now - _cacheTime < _cacheDuration)
            {
                System.Diagnostics.Debug.WriteLine("Returning cached services");
                return _cachedServices.Copy();
            }

            _cachedServices = base.GetAllServices();
            _cacheTime = DateTime.Now;
            return _cachedServices.Copy();
        }

        public override void InsertService(ServiceModel service)
        {
            base.InsertService(service);
            InvalidateCache();
        }

        public override void UpdateService(ServiceModel service)
        {
            base.UpdateService(service);
            InvalidateCache();
        }

        public override void DeleteService(int serviceId)
        {
            base.DeleteService(serviceId);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cachedServices = null;
        }
    }
} 