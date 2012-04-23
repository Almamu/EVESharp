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
            timestamp.Items.Add(new PyIntegerVar(cacheTime));
            timestamp.Items.Add(new PyIntegerVar(version));

            info.Items.Add(timestamp);

            return new PyObjectData("util.CachedObject", info);
        }

        public bool Decode(PyObject from)
        {
            if (from.Type != PyObjectType.Tuple)
            {
                Log.Error("CacheInfo", "Wrong type, expected Tuple");
                return false;
            }

            PyTuple container = from as PyTuple;

            objectID = container[1];

            if ((container[3].Type != PyObjectType.IntegerVar) && (container[3].Type != PyObjectType.Long))
            {
                Log.Error("CacheInfo", "Wrong node ID, got " + container[3].ToString() + " expected Int");
                return false;
            }

            nodeID = (container[3] as PyInt).Value;

            if (container[2].Type != PyObjectType.Tuple)
            {
                Log.Error("CacheInfo", "Wrong identifier type, expected Tuple");
                return false;
            }

            PyTuple timestamp = container[2] as PyTuple;

            if ((timestamp[0].Type != PyObjectType.LongLong) && (timestamp[0].Type != PyObjectType.IntegerVar))
            {
                Log.Error("CacheInfo", "cacheTime is the wrong type, expected LongLong");
                return false;
            }

            cacheTime = timestamp[0].IntValue;

            if ((timestamp[1].Type != PyObjectType.Long) && (timestamp[1].Type != PyObjectType.IntegerVar))
            {
                Log.Error("CacheInfo", "Checksum is the wrong type, expected Int");
                return false;
            }

            version = (timestamp[1] as PyInt).Value;

            return true;
        }
    }
}
