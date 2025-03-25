using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public class ServiceDAO
    {
        private readonly string _connectionString;

        public ServiceDAO()
        {
            // You should adjust this to match your actual connection string source
            _connectionString = "Data Source=localhost;Initial Catalog=SpaManagement;Integrated Security=True;TrustServerCertificate=True";
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
}