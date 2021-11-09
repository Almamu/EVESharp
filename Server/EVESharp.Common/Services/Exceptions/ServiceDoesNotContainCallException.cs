using System;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Common.Services.Exceptions
{
    public class ServiceDoesNotContainCallException : Exception
    {
        public string Service { get; }
        public string Call { get; }

        public ServiceDoesNotContainCallException(string svc, string call)
            : base($"Cannot find an appropiate function definition for {call} on service {svc}")
        {
            Service = svc;
            Call = call;
        }
    }
}