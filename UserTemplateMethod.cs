// UserTemplateMethod.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{

    // Template Method Pattern for User data access
    public class UserDataAccess : DataAccessTemplate
    {
        protected override string DefineSelectQuery()
        {
            return "SELECT UserId, Username, Password, CreatedDate, ModifiedDate FROM tbUser";
        }

        protected override string DefineInsertQuery()
        {
            return "EXEC sp_CreateUser @Username, @Password";
        }

        protected override string DefineUpdateQuery()
        {
            return "UPDATE tbUser SET Username = @Username, Password = @Password, ModifiedDate = GETDATE() WHERE UserId = @UserId";
        }

        protected override string DefineDeleteQuery()
        {
            return "DELETE FROM tbUser WHERE UserId = @UserId";
        }

        protected override SqlParameter[] DefineInsertParameters(object entity)
        {
            UserModel user = (UserModel)entity;
            return new SqlParameter[]
            {
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@Password", user.Password)
            };
        }

        protected override SqlParameter[] DefineUpdateParameters(object entity)
        {
            UserModel user = (UserModel)entity;
            return new SqlParameter[]
            {
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@Password", user.Password)
            };
        }

        protected override SqlParameter[] DefineDeleteParameters(int id)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@UserId", id)
            };
        }
        
        // Additional methods specific to User operations
        
        public DataTable Search(string searchText)
        {
            try
            {
                string query = "SELECT UserId, Username FROM tbUser WHERE Username LIKE @SearchText";
                SqlParameter parameter = new SqlParameter("@SearchText", "%" + searchText + "%");
                return SqlConnectionManager.Instance.ExecuteQuery(query, parameter);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching users", ex);
            }
        }
        
        public UserModel GetById(int userId)
        {
            try
            {
                string query = "SELECT UserId, Username, Password, CreatedDate, ModifiedDate FROM tbUser WHERE UserId = @UserId";
                SqlParameter parameter = new SqlParameter("@UserId", userId);
                DataTable dataTable = SqlConnectionManager.Instance.ExecuteQuery(query, parameter);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    return new UserModel
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        Password = row["Password"].ToString(),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        ModifiedDate = Convert.ToDateTime(row["ModifiedDate"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user", ex);
            }
        }
        
        // Method to insert user and return ID
        public int InsertAndGetId(UserModel user)
        {
            try
            {
                string query = DefineInsertQuery();
                SqlParameter[] parameters = DefineInsertParameters(user);
                object result = SqlConnectionManager.Instance.ExecuteScalar(query, parameters);
                
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting user", ex);
            }
        }
        
        // Method to update user and return success status
        public bool Update(UserModel user)
        {
            try
            {
                base.Update(user);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Method to delete user and return success status
        public bool Delete(int userId)
        {
            try
            {
                base.Delete(userId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 