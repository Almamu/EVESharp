using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVESharp.Services.CacheSvc;
using EVESharp.Services.Network;

using Common.Services;

namespace EVESharp
{
    public class ServiceManager : Common.Services.ServiceManager
    {
        private objectCaching objectCachingSvc = new objectCaching();
        private machoNet machoNetSvc = new machoNet();
        private alert alertSvc = new alert();

        public Service objectCaching()
        {
            return objectCachingSvc;
        }

        public Service machoNet()
        {
            return machoNetSvc;
        }

        public Service alert()
        {
            return alertSvc;
        }
    }
}
