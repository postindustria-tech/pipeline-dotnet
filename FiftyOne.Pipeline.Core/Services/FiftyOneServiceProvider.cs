using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Services
{
    /// <summary>
    /// Basic implementation of <see cref="IServiceProvider"/>.
    /// An instance contains a list of services which can be added
    /// to.
    /// </summary>
    public class FiftyOneServiceProvider : IServiceProvider
    {
        private readonly IList<object> _services = new List<object>();

        /// <summary>
        /// Add a service instance to the provider. This builds the
        /// collection used to return services from the GetService
        /// method.
        /// </summary>
        /// <param name="service">
        /// Service instance to add.
        /// </param>
        public void AddService(object service)
        {
            _services.Add(service);
        }


        /// <summary>
        /// Get the service from the service collection if it exists, otherwise
        /// return null.
        /// Note that if more than one instance implementing the same service
        /// is added to the services, the first will be returned.
        /// </summary>
        /// <param name="serviceType">
        /// The service type to be returned.
        /// </param>
        /// <returns>
        /// Service or null.
        /// </returns>
        public object GetService(Type serviceType)
        {
            if (serviceType != null)
            {
                foreach (var service in _services)
                {
                    if (serviceType.IsAssignableFrom(service.GetType()))
                    {
                        return service;
                    }
                }
            }
            return null;
        }
    }
}
