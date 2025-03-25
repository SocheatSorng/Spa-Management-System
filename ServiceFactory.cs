// ServiceFactory.cs
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

        // Create new service with image path and default dates
        public static ServiceModel CreateService(string name, string description, decimal price, string imagePath)
        {
            return new ServiceModel
            {
                ServiceName = name,
                Description = description,
                Price = price,
                ImagePath = imagePath,
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

        // Create service with all properties including image path
        public static ServiceModel CreateService(int id, string name, string description, decimal price,
                                                string imagePath, DateTime createdDate, DateTime modifiedDate)
        {
            return new ServiceModel
            {
                ServiceId = id,
                ServiceName = name,
                Description = description,
                Price = price,
                ImagePath = imagePath,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            };
        }
    }
}