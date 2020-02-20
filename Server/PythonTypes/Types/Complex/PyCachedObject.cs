using System;
using PythonTypes.Compression;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    public class PyCachedObject
    {
        private const string TYPE_NAME = "objectCaching.CachedObject";
        
        public long timestamp = 0;
        public int version = 0;
        public long nodeID = 0;
        public int shared = 0;
        public PyBuffer cache = null;
        public int compressed = 0;
        public PyDataType objectID = null;

        public static implicit operator PyDataType(PyCachedObject data)
        {
            if (data.cache == null)
                throw new Exception("Cache data is null");

            if (data.objectID == null)
                throw new Exception("objectID is null");

            PyTuple args = new PyTuple(7);
            PyTuple version = new PyTuple(2);

            version[0] = data.timestamp;
            version[1] = data.version;

            args[0] = version;
            args[1] = new PyNone();
            args[2] = data.nodeID;
            args[3] = data.shared;
            args[4] = data.cache;
            args[5] = data.compressed;
            args[6] = data.objectID;

            return new PyObjectData(TYPE_NAME, args);
        }

        public static PyCachedObject FromCacheHint(PyCacheHint cacheInfo, PyDataType data)
        {
            PyCachedObject obj = new PyCachedObject();

            obj.nodeID = cacheInfo.nodeID;
            obj.objectID = cacheInfo.objectID;
            obj.shared = 1;
            obj.compressed = 1;
            obj.cache = new PyBuffer(ZlibHelper.Compress(PythonTypes.Marshal.Marshal.ToByteArray(data)));
            obj.timestamp = cacheInfo.cacheTime;
            obj.version = cacheInfo.version;

            return obj;
        }
    }
}