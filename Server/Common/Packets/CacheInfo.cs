using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class CacheInfo : Decodeable, Encodeable
    {
        public long cacheTime = 0;
        public PyObject objectID = null;
        public long nodeID = 0;
        public int version = 0;

        public PyObject Encode()
        {
            PyTuple info = new PyTuple();

            info.Items.Add(objectID);
            info.Items.Add(new PyIntegerVar(nodeID));

            PyTuple timestamp = new PyTuple();
            timestamp.Items.Add(new PyIntegerVar(cacheTime));
            timestamp.Items.Add(new PyIntegerVar(version));

            info.Items.Add(timestamp);

            return new PyObjectData("util.CachedObject", info);
        }

        public void Decode(PyObject from)
        {
            if (from.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected Tuple but got {@from.Type}");
            }

            PyTuple container = from as PyTuple;

            objectID = container[1];

            if (container[3].Type != PyObjectType.IntegerVar)
            {
                throw new Exception($"Expected nodeID of type Integer or Long, got {container[3].Type}");
            }

            nodeID = (container[3] as PyIntegerVar).Value;

            if (container[2].Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected identifier of type Tuple but got {container[2].Type}");
            }

            PyTuple timestamp = container[2] as PyTuple;

            if ((timestamp[0].Type != PyObjectType.LongLong) && (timestamp[0].Type != PyObjectType.IntegerVar))
            {
                throw new Exception($"cacheTime is the wrong type, expected LongLong or Integer but got {timestamp[0].Type}");
            }

            cacheTime = timestamp[0].IntValue;

            if ((timestamp[1].Type != PyObjectType.Long) && (timestamp[1].Type != PyObjectType.IntegerVar))
            {
                throw new Exception($"Checksum is the wrong type, expected Long or integer but got {timestamp[1].Type}");
            }

            version = (int)(timestamp[1].IntValue);
        }

        public static CacheInfo FromBuffer(string name, byte[] data, long timestamp, long nodeID)
        {
            CacheInfo obj = new CacheInfo();

            obj.version = (int) Crc32.Checksum(data);
            obj.nodeID = nodeID;
            obj.objectID = new PyString(name);
            obj.cacheTime = timestamp;

            return obj;
        }

        public static CacheInfo FromPyObject(string name, PyObject data, long timestamp, long nodeID)
        {
            return FromBuffer(name, Marshal.Marshal.Process(data), timestamp, nodeID);
        }
    }
}
