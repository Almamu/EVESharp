using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Services
{
    public class ServiceCall
    {
        private string call_name = "";

        public ServiceCall(string name)
        {
            call_name = name;
        }

        public string GetCallName()
        {
            return call_name;
        }

        public virtual PyObject Run(PyTuple args, object client)
        {
            return new PyNone();
        }
    }
}
