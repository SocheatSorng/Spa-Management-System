using Microsoft.Data.SqlClient;
using System;
using System.Windows.Forms;

namespace Spa_Management_System
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Database connection setup loop - keeps asking until a valid connection is established
            bool connectionEstablished = false;
            
            while (!connectionEstablished)
            {
                // Show the SQL Server connection dialog
                using (ConnectionDialog connectionDialog = new ConnectionDialog())
                {
                    DialogResult result = connectionDialog.ShowDialog();
                    
                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            // Set the connection string for the application
                            SqlConnectionManager.Instance.SetConnectionString(connectionDialog.ConnectionString);
                            
                            // Verify the connection works by testing it
                            if (SqlConnectionManager.Instance.HasValidConnection())
                            {
                                connectionEstablished = true;
                                
                                // No success message, just continue to the main application
                            }
                            else
                            {
                                // Connection failed
                                MessageBox.Show(
                                    "Could not establish a database connection. Please try again with different settings.",
                                    "Connection Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Exception while trying to connect
                            MessageBox.Show(
                                $"Error connecting to database: {ex.Message}\n\nPlease try again.",
                                "Connection Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        // Ask if the user wants to exit the application
                        DialogResult exitResult = MessageBox.Show(
                            "A database connection is required to run the application. Do you want to exit?",
                            "Exit Application",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                            
                        if (exitResult == DialogResult.Yes)
                        {
                            // User chose to exit
                            return;
                        }
                        // If No, the loop continues and shows the connection dialog again
                    }
                }
            }
            
            // Now start the main application with the established connection
            Application.Run(new Dashboard());
        }
    }
}