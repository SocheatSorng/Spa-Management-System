using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Spa_Management_System
{
    public class InvoiceManager
    {
        private readonly InvoiceDAO _dao;
        private readonly List<IInvoiceObserver> _observers;

        public InvoiceManager()
        {
            _dao = new InvoiceDAO();
            _observers = new List<IInvoiceObserver>();
        }

        public void AddObserver(IInvoiceObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IInvoiceObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnInvoiceUpdated();
            }
        }

        public DataTable GetAllInvoices()
        {
            return _dao.GetAllInvoices();
        }

        public void InsertInvoice(InvoiceModel invoice)
        {
            _dao.InsertInvoice(invoice);
            NotifyObservers();
        }

        public void UpdateInvoice(InvoiceModel invoice)
        {
            _dao.UpdateInvoice(invoice);
            NotifyObservers();
        }

        public void DeleteInvoice(int invoiceId)
        {
            _dao.DeleteInvoice(invoiceId);
            NotifyObservers();
        }
    }
}
