using System;

namespace Spa_Management_System
{
    public static class ServiceFactory
    {
        // Create new service with default dates
        public static ServiceModel CreateService(string name, string description, decimal price)
        {
            return new ServiceModel
            {
                ServiceName = name,
                Description = description,
                Price = price,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }

        // Create service with all properties specified
        public static ServiceModel CreateService(int id, string name, string description, decimal price,
                                                DateTime createdDate, DateTime modifiedDate)
        {
            return new ServiceModel
            {
                ServiceId = id,
                ServiceName = name,
                Description = description,
                Price = price,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            };
        }
    }
}