using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class CacheInfo
    {
        public long cacheTime = 0;
        public PyObject objectID = null;
        public int nodeID = 0;
        public int version = 0;

        public PyObject Encode()
        {
            PyTuple info = new PyTuple();

            info.Items.Add(objectID);
            info.Items.Add(new PyInt(nodeID));

            PyTuple timestamp = new PyTuple();
            timestamp.Items.Add(new PyLongLong(cacheTime));
            timestamp.Items.Add(new PyInt(version));

            info.Items.Add(timestamp);

            return new PyObjectData("util.CachedObject", info);
        }
    }
}
