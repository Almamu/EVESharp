using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Services
{
    public class ServiceDoesNotContainCallException : Exception
    {
        public string Service = "";
        public string Call = "";

        public ServiceDoesNotContainCallException(string svc, string call)
            : base("The requested service does not contains the required call")
        {
            Service = svc;
            Call = call;
            
        }
    }
}
