// UserFormState.cs
using System;
using System.Windows.Forms;

namespace Spa_Management_System
{
    // State interface
    public interface IFormState
    {
        void Enter();
        void HandleNew();
        void HandleEdit();
        void HandleSave();
        void HandleCancel();
        void HandleDelete();
        string StateName { get; }
    }

    // Form context class that works with state
    public class FormStateContext
    {
        private Form _form;
        private IFormState _currentState;
        
        // Actions for common control state changes
        private readonly Action _enableControls;
        private readonly Action _disableControls;
        private readonly Action _clearControls;
        
        public FormStateContext(Form form, Action enableControls, Action disableControls, Action clearControls)
        {
            _form = form;
            _enableControls = enableControls;
            _disableControls = disableControls;
            _clearControls = clearControls;
            
            // Default state is view state
            _currentState = new ViewState(this, _enableControls, _disableControls, _clearControls);
            _currentState.Enter();
        }
        
        public void SetState(IFormState state)
        {
            _currentState = state;
            _currentState.Enter();
        }
        
        public void New() => _currentState.HandleNew();
        public void Edit() => _currentState.HandleEdit();
        public void Save() => _currentState.HandleSave();
        public void Cancel() => _currentState.HandleCancel();
        public void Delete() => _currentState.HandleDelete();
        
        public string CurrentState => _currentState.StateName;
    }
    
    // Base abstract state class
    public abstract class FormStateBase : IFormState
    {
        protected readonly FormStateContext _context;
        protected readonly Action _enableControls;
        protected readonly Action _disableControls;
        protected readonly Action _clearControls;
        
        public abstract string StateName { get; }
        
        protected FormStateBase(FormStateContext context, Action enableControls, Action disableControls, Action clearControls)
        {
            _context = context;
            _enableControls = enableControls;
            _disableControls = disableControls;
            _clearControls = clearControls;
        }
        
        public abstract void Enter();
        public abstract void HandleNew();
        public abstract void HandleEdit();
        public abstract void HandleSave();
        public abstract void HandleCancel();
        public abstract void HandleDelete();
    }
    
    // Concrete state: View state
    public class ViewState : FormStateBase
    {
        public override string StateName => "View";
        
        public ViewState(FormStateContext context, Action enableControls, Action disableControls, Action clearControls) 
            : base(context, enableControls, disableControls, clearControls)
        {
        }
        
        public override void Enter()
        {
            _disableControls();
        }
        
        public override void HandleNew()
        {
            _context.SetState(new NewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleEdit()
        {
            _context.SetState(new EditState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleSave()
        {
            // Cannot save in view state
            MessageBox.Show("Cannot save in view state.", "Invalid Operation", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        public override void HandleCancel()
        {
            // Already in view state, nothing to do
        }
        
        public override void HandleDelete()
        {
            // Implementation depends on application requirements
            // Usually allowed in view state
        }
    }
    
    // Concrete state: New state
    public class NewState : FormStateBase
    {
        public override string StateName => "New";
        
        public NewState(FormStateContext context, Action enableControls, Action disableControls, Action clearControls) 
            : base(context, enableControls, disableControls, clearControls)
        {
        }
        
        public override void Enter()
        {
            _clearControls();
            _enableControls();
        }
        
        public override void HandleNew()
        {
            // Already in new state, just clear the form
            _clearControls();
        }
        
        public override void HandleEdit()
        {
            // Cannot edit in new state
            MessageBox.Show("Cannot edit in new state.", "Invalid Operation", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        public override void HandleSave()
        {
            // After saving, transition to view state
            _context.SetState(new ViewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleCancel()
        {
            // Cancel new record, go back to view state
            _context.SetState(new ViewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleDelete()
        {
            // Cannot delete in new state
            MessageBox.Show("Cannot delete in new state.", "Invalid Operation", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    
    // Concrete state: Edit state
    public class EditState : FormStateBase
    {
        public override string StateName => "Edit";
        
        public EditState(FormStateContext context, Action enableControls, Action disableControls, Action clearControls) 
            : base(context, enableControls, disableControls, clearControls)
        {
        }
        
        public override void Enter()
        {
            _enableControls();
        }
        
        public override void HandleNew()
        {
            _context.SetState(new NewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleEdit()
        {
            // Already in edit state, nothing to do
        }
        
        public override void HandleSave()
        {
            // After saving, transition to view state
            _context.SetState(new ViewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleCancel()
        {
            // Cancel edit, go back to view state
            _context.SetState(new ViewState(_context, _enableControls, _disableControls, _clearControls));
        }
        
        public override void HandleDelete()
        {
            // Delete record, then go back to view state
            _context.SetState(new ViewState(_context, _enableControls, _disableControls, _clearControls));
        }
    }
} 