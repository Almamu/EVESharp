using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Services
{
    public class Service
    {
        private string svc_name = "";
        private Dictionary<string, ServiceCall> calls = null;

        public Service(string name)
        {
            svc_name = name;
            calls = new Dictionary<string, ServiceCall>();
        }

        public string GetServiceName()
        {
            return svc_name;
        }

        public void AddServiceCall(ServiceCall call)
        {
            calls.Add(call.GetCallName(), call);
        }

        public void DelServiceCall(string name)
        {
            if( FindServiceCall(name))
                calls.Remove(name);
        }

        public bool FindServiceCall(string name)
        {
            return calls.ContainsKey(name);
        }

        public ServiceCall GetServiceCall(string name)
        {
            if (FindServiceCall(name) == true)
            {
                return calls[name];
            }

            return null;
        }

        public PyObject Call(string name, PyTuple args, object client)
        {
            ServiceCall call = GetServiceCall(name);

            if (call == null)
            {
                return new PyTuple();
            }

            return call.Run(args, client);
        }
    }
}
