using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Services
{
    public class ServiceDoesNotExistsException : Exception
    {
        public string Service = "";

        public ServiceDoesNotExistsException(string svc)
            : base("The required service does not exists")
        {
            Service = svc;
        }
    }
}
