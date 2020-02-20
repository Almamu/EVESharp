using System;

namespace Common.Services
{
    public class ServiceDoesNotExistsException : Exception
    {
        public string Service = "";

        public ServiceDoesNotExistsException(string svc)
            : base($"The requested service {svc} doesn't exist")
        {
            Service = svc;
        }
    }
}