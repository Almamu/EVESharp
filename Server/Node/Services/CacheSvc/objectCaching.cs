using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;
using Common.Services;

namespace EVESharp.Services.CacheSvc
{
    public class objectCaching : Service
    {
        public objectCaching()
            : base("objectCaching")
        {
            AddServiceCall(new GetCachableObject());
        }

        public class GetCachableObject : ServiceCall
        {
            public GetCachableObject()
                : base("GetCachableObject")
            {

            }

            public override PyObject Run(PyTuple args, object client)
            {
                Log.Error("GetCachableObject", PrettyPrinter.Print(args));

                if (Cache.LoadCacheFor(args.Items[1].As<PyString>().Value) == false)
                {
                    return null;
                }

                return Cache.GetCache(args.Items[1].As<PyString>().Value);
            }
        }
    }
}
