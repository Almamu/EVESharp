using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class PyCachedObject : Encodeable, Decodeable
    {
        public long timestamp = 0;
        public int version = 0;
        public long nodeID = 0;
        public int shared = 0;
        public PyBuffer cache = null;
        public int compressed = 0;
        public PyObject objectID = null;

        public PyObject Encode()
        {
            if (cache == null)
            {
                throw new Exception("Cache data is null");
            }

            if (objectID == null)
            {
                throw new Exception("objectID is null");
            }

            PyTuple args = new PyTuple();

            PyTuple versiont = new PyTuple();
            versiont.Items.Add(new PyLongLong(timestamp));
            versiont.Items.Add(new PyInt(version));

            args.Items.Add(versiont);
            args.Items.Add(new PyNone());
            args.Items.Add(new PyIntegerVar(nodeID));
            args.Items.Add(new PyInt(shared));
            args.Items.Add(cache);
            args.Items.Add(new PyInt(compressed));
            args.Items.Add(objectID);

            return new PyObjectData("objectCaching.CachedObject", args);
        }

        /* This should never be used in the node, just the Cache Tool */
        public void Decode(PyObject from)
        {
            PyTuple data = (from as PyObjectData).Arguments as PyTuple;

            // Just decode the cache info for now..
            cache = data.Items[4] as PyBuffer;
        }

        public static PyCachedObject FromCacheInfo(CacheInfo cacheInfo, PyObject data)
        {
            PyCachedObject obj = new PyCachedObject();
            
            obj.nodeID = cacheInfo.nodeID;
            obj.objectID = cacheInfo.objectID;
            obj.shared = 1;
            obj.compressed = 1;
            obj.cache = new PyBuffer (Zlib.Compress(Marshal.Marshal.Process(data)));
            obj.timestamp = cacheInfo.cacheTime;
            obj.version = cacheInfo.version;

            return obj;
        }
    }
}
