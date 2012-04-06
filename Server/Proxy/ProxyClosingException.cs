using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy
{
    class ProxyClosingException : Exception
    {
        public ProxyClosingException()
            : base("The proxy is being closed")
        {

        }
    }
}
