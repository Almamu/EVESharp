using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;
using Common.Services;
using Common.Packets;

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
                Log.Debug("objectCaching", "Called GetCachableObject stub");

                CacheInfo cache = new CacheInfo();

                if (cache.Decode(args) == false)
                {
                    return null;
                }

                Log.Debug("GetCachableObject", "Got cache request for cache " + cache.objectID.As<PyString>().Value);

                if (Cache.LoadCacheFor(cache.objectID.As<PyString>().Value) == false)
                {
                    return null;
                }

                return Cache.GetCache(cache.objectID.As<PyString>().Value);
            }
        }
    }
}
