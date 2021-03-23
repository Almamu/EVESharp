using System;

namespace Common.Services.Exceptions
{
    public class ServiceDoesNotExistsException : Exception
    {
        public string Service { get; }

        public ServiceDoesNotExistsException(string svc)
            : base($"The requested service {svc} doesn't exist")
        {
            Service = svc;
        }
    }
}