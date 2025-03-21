using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Management_System
{
    public interface IInvoiceObserver
    {
        void OnInvoiceUpdated(); // Called when an invoice is inserted, updated, or deleted
    }
}
