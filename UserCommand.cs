// UserFormCommand.cs
using System;
using System.Windows.Forms;

namespace Spa_Management_System
{
    // Command interface
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
        void Undo();
    }

    // Command invoker
    public class CommandInvoker
    {
        private ICommand _command;

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public void ExecuteCommand()
        {
            if (_command != null && _command.CanExecute())
            {
                _command.Execute();
            }
            else
            {
                MessageBox.Show("Command cannot be executed.", "Invalid Operation", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void UndoCommand()
        {
            if (_command != null)
            {
                _command.Undo();
            }
        }
    }

    // Base form command
    public abstract class FormCommand : ICommand
    {
        protected readonly Form _form;

        public FormCommand(Form form)
        {
            _form = form;
        }

        public abstract bool CanExecute();
        public abstract void Execute();
        public abstract void Undo();

        protected void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // Composite command (execute multiple commands)
    public class CompositeCommand : ICommand
    {
        private readonly ICommand[] _commands;

        public CompositeCommand(params ICommand[] commands)
        {
            _commands = commands;
        }

        public bool CanExecute()
        {
            foreach (var command in _commands)
            {
                if (!command.CanExecute())
                    return false;
            }
            return true;
        }

        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            // Undo in reverse order
            for (int i = _commands.Length - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }

    // Save command base class
    public abstract class SaveCommand : FormCommand
    {
        protected SaveCommand(Form form) : base(form) { }
        
        public override bool CanExecute()
        {
            return ValidateInput();
        }
        
        protected abstract bool ValidateInput();
        protected abstract void SaveData();
        
        public override void Execute()
        {
            try
            {
                if (!CanExecute())
                    return;
                
                SaveData();
                ShowSuccessMessage("Record saved successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving record: {ex.Message}");
            }
        }

        public override void Undo()
        {
            // Implementation of Undo for SaveCommand
        }
    }
    
    // Delete command base class
    public abstract class DeleteCommand : FormCommand
    {
        protected DeleteCommand(Form form) : base(form) { }
        
        public override bool CanExecute()
        {
            return ConfirmDelete() && HasSelectedRecord();
        }
        
        protected abstract bool HasSelectedRecord();
        protected abstract void DeleteData();
        
        protected virtual bool ConfirmDelete()
        {
            return MessageBox.Show(
                "Are you sure you want to delete this record?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }
        
        public override void Execute()
        {
            try
            {
                if (!CanExecute())
                    return;
                
                DeleteData();
                ShowSuccessMessage("Record deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting record: {ex.Message}");
            }
        }

        public override void Undo()
        {
            // Implementation of Undo for DeleteCommand
        }
    }
    
    // Clear form command
    public class ClearFormCommand : FormCommand
    {
        private readonly Action _clearAction;
        
        public ClearFormCommand(Form form, Action clearAction) : base(form)
        {
            _clearAction = clearAction;
        }
        
        public override void Execute()
        {
            _clearAction?.Invoke();
        }

        public override bool CanExecute()
        {
            return _clearAction != null;
        }

        public override void Undo()
        {
            // Implementation of Undo for ClearFormCommand
        }
    }
    
    // New record command
    public class NewRecordCommand : FormCommand
    {
        private readonly Action _clearAction;
        private readonly Action _focusAction;
        
        public NewRecordCommand(Form form, Action clearAction, Action focusAction = null) : base(form)
        {
            _clearAction = clearAction;
            _focusAction = focusAction;
        }
        
        public override void Execute()
        {
            _clearAction?.Invoke();
            _focusAction?.Invoke();
        }

        public override bool CanExecute()
        {
            return _clearAction != null || _focusAction != null;
        }

        public override void Undo()
        {
            // Implementation of Undo for NewRecordCommand
        }
    }

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