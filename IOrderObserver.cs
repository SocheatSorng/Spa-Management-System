using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Management_System
{
    public interface IOrderObserver
    {
        void OnOrderUpdated(); // Called when an order is inserted, updated, or deleted
    }
}
