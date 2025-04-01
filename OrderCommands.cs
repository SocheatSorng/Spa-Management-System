//using System;
//using System.Windows.Forms;

//namespace Spa_Management_System
//{
//    // Command implementations for Order form
    
//    // Concrete Insert Command
//    public class InsertOrderCommand : SaveCommand
//    {
//        private readonly OrderManager _orderManager;
//        private readonly Func<OrderModel> _getOrderFunc;
        
//        public InsertOrderCommand(Form form, OrderManager orderManager, Func<OrderModel> getOrderFunc) 
//            : base(form)
//        {
//            _orderManager = orderManager;
//            _getOrderFunc = getOrderFunc;
//        }
        
//        protected override bool ValidateInput()
//        {
//            try
//            {
//                OrderModel order = _getOrderFunc();
                
//                if (order.CustomerId <= 0 || order.UserId <= 0 || 
//                    order.TotalAmount < 0 || order.Discount < 0 || 
//                    order.FinalAmount < 0 || string.IsNullOrEmpty(order.Status))
//                {
//                    ShowError("Valid Customer ID, User ID, non-negative amounts, and status are required.");
//                    return false;
//                }
                
//                return true;
//            }
//            catch (Exception ex)
//            {
//                ShowError("Invalid input values", ex);
//                return false;
//            }
//        }
        
//        protected override void SaveData()
//        {
//            _orderManager.InsertOrder(_getOrderFunc());
            
//            // Transition to view state (handled by the form's State Pattern)
//            if (_form.Tag is IFormState state)
//            {
//                state.HandleSave();
//            }
//        }
//    }
    
//    // Concrete Update Command
//    public class UpdateOrderCommand : SaveCommand
//    {
//        private readonly OrderManager _orderManager;
//        private readonly Func<OrderModel> _getOrderFunc;
        
//        public UpdateOrderCommand(Form form, OrderManager orderManager, Func<OrderModel> getOrderFunc) 
//            : base(form)
//        {
//            _orderManager = orderManager;
//            _getOrderFunc = getOrderFunc;
//        }
        
//        protected override bool ValidateInput()
//        {
//            try
//            {
//                OrderModel order = _getOrderFunc();
                
//                if (order.OrderId <= 0)
//                {
//                    ShowError("Please select an order to update.");
//                    return false;
//                }
                
//                if (order.CustomerId <= 0 || order.UserId <= 0 || 
//                    order.TotalAmount < 0 || order.Discount < 0 || 
//                    order.FinalAmount < 0 || string.IsNullOrEmpty(order.Status))
//                {
//                    ShowError("Valid Customer ID, User ID, non-negative amounts, and status are required.");
//                    return false;
//                }
                
//                return true;
//            }
//            catch (Exception ex)
//            {
//                ShowError("Invalid input values", ex);
//                return false;
//            }
//        }
        
//        protected override void SaveData()
//        {
//            _orderManager.UpdateOrder(_getOrderFunc());
            
//            // Transition to view state (handled by the form's State Pattern)
//            if (_form.Tag is IFormState state)
//            {
//                state.HandleSave();
//            }
//        }
//    }
    
//    // Concrete Delete Command
//    public class DeleteOrderCommand : DeleteCommand
//    {
//        private readonly OrderManager _orderManager;
//        private readonly Func<int> _getOrderIdFunc;
        
//        public DeleteOrderCommand(Form form, OrderManager orderManager, Func<int> getOrderIdFunc) 
//            : base(form)
//        {
//            _orderManager = orderManager;
//            _getOrderIdFunc = getOrderIdFunc;
//        }
        
//        protected override bool HasSelectedRecord()
//        {
//            int orderId = _getOrderIdFunc();
            
//            if (orderId <= 0)
//            {
//                ShowError("Please select an order to delete.");
//                return false;
//            }
            
//            return true;
//        }
        
//        protected override void DeleteData()
//        {
//            _orderManager.DeleteOrder(_getOrderIdFunc());
            
//            // Transition to view state (handled by the form's State Pattern)
//            if (_form.Tag is IFormState state)
//            {
//                state.HandleDelete();
//            }
//        }
//    }
//} 