using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class PyCachedObject
    {
        public long timestamp = 0;
        public int version = 0;
        public int nodeID = 0;
        public int shared = 0;
        public PyBuffer cache = null;
        public int compressed = 0;
        public PyObject objectID = null;

        public PyObject Encode()
        {
            if (cache == null)
            {
                Log.Error("PyCachedObject", "Cache data is null");
                return null;
            }

            if (objectID == null)
            {
                Log.Error("PyCachedObject", "objectID is null");
                return null;
            }

            PyTuple args = new PyTuple();

            PyTuple versiont = new PyTuple();
            versiont.Items.Add(new PyLongLong(timestamp));
            versiont.Items.Add(new PyInt(version));

            args.Items.Add(versiont);
            args.Items.Add(new PyNone());
            args.Items.Add(new PyInt(nodeID));
            args.Items.Add(new PyInt(shared));
            args.Items.Add(cache);
            args.Items.Add(new PyInt(compressed));
            args.Items.Add(objectID);

            return new PyObjectData("objectCaching.CachedObject", args);
        }

        /* This should never be used in the node, just the Cache Tool */
        public bool Decode(PyObject from)
        {
            PyTuple data = (from as PyObjectData).Arguments as PyTuple;

            // Just decode the cache info for now..
            cache = data.Items[4] as PyBuffer;

            return true;
        }

        public static PyCachedObject FromCacheInfo(CacheInfo cacheInfo, PyObject data)
        {
            PyCachedObject obj = new PyCachedObject();
            
            obj.nodeID = cacheInfo.nodeID;
            obj.objectID = cacheInfo.objectID;
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = new PyBuffer (Marshal.Marshal.Process(data));
            obj.timestamp = cacheInfo.cacheTime;
            obj.version = cacheInfo.version;

            return obj;
        }
    }
}
