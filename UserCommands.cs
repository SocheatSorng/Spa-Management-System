using System;
using System.Windows.Forms;

namespace Spa_Management_System
{
    // Command implementation for inserting a user
    public class InsertUserCommand : ICommand
    {
        private readonly Form _form;
        private readonly UserDataAccess _userDataAccess;
        private readonly Func<UserModel> _getUserFunc;
        
        public InsertUserCommand(Form form, UserDataAccess userDataAccess, Func<UserModel> getUserFunc)
        {
            _form = form;
            _userDataAccess = userDataAccess;
            _getUserFunc = getUserFunc;
        }
        
        public bool CanExecute()
        {
            UserModel user = _getUserFunc();
            
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            return true;
        }
        
        public void Execute()
        {
            try
            {
                UserModel user = _getUserFunc();
                
                int newUserId = _userDataAccess.InsertAndGetId(user);
                if (newUserId > 0)
                {
                    MessageBox.Show("User created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to create user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void Undo()
        {
            // Implementation for undo functionality if needed
        }
    }
    
    // Command implementation for updating a user
    public class UpdateUserCommand : ICommand
    {
        private readonly Form _form;
        private readonly UserDataAccess _userDataAccess;
        private readonly Func<UserModel> _getUserFunc;
        private UserModel _originalUser;
        
        public UpdateUserCommand(Form form, UserDataAccess userDataAccess, Func<UserModel> getUserFunc)
        {
            _form = form;
            _userDataAccess = userDataAccess;
            _getUserFunc = getUserFunc;
        }
        
        public bool CanExecute()
        {
            UserModel user = _getUserFunc();
            
            if (user.UserId <= 0)
            {
                MessageBox.Show("Please select a user to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            try
            {
                _originalUser = _userDataAccess.GetById(user.UserId);
                if (_originalUser == null)
                {
                    MessageBox.Show("User not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        public void Execute()
        {
            try
            {
                UserModel user = _getUserFunc();
                bool success = _userDataAccess.Update(user);
                
                if (success)
                {
                    MessageBox.Show("User updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void Undo()
        {
            if (_originalUser != null)
            {
                try
                {
                    _userDataAccess.Update(_originalUser);
                }
                catch
                {
                    // Handle undo failure
                }
            }
        }
    }
    
    // Command implementation for deleting a user
    public class DeleteUserCommand : ICommand
    {
        private readonly Form _form;
        private readonly UserDataAccess _userDataAccess;
        private readonly Func<int> _getUserIdFunc;
        private UserModel _deletedUser;
        
        public DeleteUserCommand(Form form, UserDataAccess userDataAccess, Func<int> getUserIdFunc)
        {
            _form = form;
            _userDataAccess = userDataAccess;
            _getUserIdFunc = getUserIdFunc;
        }
        
        public bool CanExecute()
        {
            int userId = _getUserIdFunc();
            
            if (userId <= 0)
            {
                MessageBox.Show("Please select a user to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            try
            {
                _deletedUser = _userDataAccess.GetById(userId);
                if (_deletedUser == null)
                {
                    MessageBox.Show("User not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                
                // Confirm deletion
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this user?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                return result == DialogResult.Yes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        public void Execute()
        {
            try
            {
                int userId = _getUserIdFunc();
                bool success = _userDataAccess.Delete(userId);
                
                if (success)
                {
                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void Undo()
        {
            if (_deletedUser != null)
            {
                try
                {
                    int newUserId = _userDataAccess.InsertAndGetId(_deletedUser);
                    _deletedUser.UserId = newUserId;
                }
                catch
                {
                    // Handle undo failure
                }
            }
        }
    }
} 