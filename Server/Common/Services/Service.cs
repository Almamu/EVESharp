using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Services
{
    public class Service
    {
        private string svc_name = "";

        public Service(string name)
        {
            svc_name = name;
        }
    }
}
