using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Spa_Management_System
{
    public class OrderManager
    {
        private readonly OrderDAO _dao;
        private readonly List<IOrderObserver> _observers;

        public OrderManager()
        {
            _dao = new OrderDAO();
            _observers = new List<IOrderObserver>();
        }

        public void AddObserver(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnOrderUpdated();
            }
        }

        public DataTable GetAllOrders()
        {
            return _dao.GetAllOrders();
        }

        public void InsertOrder(OrderModel order)
        {
            _dao.InsertOrder(order);
            NotifyObservers();
        }

        public void UpdateOrder(OrderModel order)
        {
            _dao.UpdateOrder(order);
            NotifyObservers();
        }

        public void DeleteOrder(int orderId)
        {
            _dao.DeleteOrder(orderId);
            NotifyObservers();
        }
    }
}
