using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    public class PyCacheHint
    {
        public long cacheTime = 0;
        public PyDataType objectID = null;
        public long nodeID = 0;
        public int version = 0;

        public static implicit operator PyDataType(PyCacheHint cacheHint)
        {
            PyTuple timestamp = new PyTuple(new PyDataType[]
            {
                cacheHint.cacheTime, cacheHint.version
            });

            return new PyObjectData("util.CachedObject", 
                new PyTuple(new []
                {
                    cacheHint.objectID,
                    cacheHint.nodeID,
                    timestamp
                })
            );
        }
        
        public static implicit operator PyCacheHint(PyDataType from)
        {
            PyCacheHint result = new PyCacheHint();
            PyTuple container = from as PyTuple;
            PyTuple timestamp = container[2] as PyTuple;

            result.objectID = container[1];
            result.nodeID = container[3] as PyInteger;
            result.cacheTime = timestamp[0] as PyInteger;
            result.version = timestamp[1] as PyInteger;

            return result;
        }

        public static PyCacheHint FromBuffer(string name, byte[] data, long timestamp, long nodeID)
        {
            PyCacheHint obj = new PyCacheHint();

            obj.version = (int) CRC32.Checksum(data);
            obj.nodeID = nodeID;
            obj.objectID = new PyString(name);
            obj.cacheTime = timestamp;

            return obj;
        }

        public static PyCacheHint FromPyObject(string name, PyDataType data, long timestamp, long nodeID)
        {
            return FromBuffer(name, PythonTypes.Marshal.Marshal.ToByteArray(data), timestamp, nodeID);
        }
    }
}
