using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace Spa_Management_System
{
    public static class ServiceFactory
    {
        public static ServiceModel CreateService(string serviceName, string description, decimal price)
        {
            return new ServiceModel
            {
                ServiceName = serviceName,
                Description = description,
                Price = price,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }

        public static ServiceModel CreateService(int serviceId, string serviceName, string description, decimal price, DateTime createdDate, DateTime modifiedDate)
        {
            return new ServiceModel
            {
                ServiceId = serviceId,
                ServiceName = serviceName,
                Description = description,
                Price = price,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            };
        }
    }
}
