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
            : base($"The service {svc} does not contain a definition for {call}")
        {
            Service = svc;
            Call = call;
        }
    }
}
