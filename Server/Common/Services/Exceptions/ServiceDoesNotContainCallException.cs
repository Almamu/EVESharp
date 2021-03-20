using System;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Services.Exceptions
{
    public class ServiceDoesNotContainCallException : Exception
    {
        public string Service = "";
        public string Call = "";

        public ServiceDoesNotContainCallException(string svc, string call, PyTuple parameters)
            : base($"Cannot find an appropiate function definition for {call} on service {svc}")
        {
            Service = svc;
            Call = call;
        }
    }
}