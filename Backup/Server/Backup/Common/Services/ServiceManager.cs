using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Services
{
    public class ServiceManager
    {
        private Dictionary<string, Service> services = null;

        public ServiceManager()
        {
            services = new Dictionary<string, Service>();
        }

        public void AddService(Service service)
        {
            services.Add(service.GetServiceName(), service);
        }

        public bool FindService(string name)
        {
            return services.ContainsKey(name);
        }

        public void DelService(string name)
        {
            if (FindService(name) == true)
            {
                services.Remove(name);
            }
        }

        public Service GetService(string name)
        {
            if (FindService(name) == true)
            {
                return services[name];
            }

            return null;
        }

        public PyObject Call(string svc, string call, PyTuple args, object client)
        {
            Service service = GetService(svc);

            if (service == null)
                return null;

            return service.Call(call, args, client);
        }
    }
}
