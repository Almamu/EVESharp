using System;
using PythonTypes.Compression;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    /// <summary>
    /// Helper class to work with PyCachedObject's (objectCaching.CachedObject) to be sent to the EVE Online client
    /// when cache requests are performed
    /// </summary>
    public class PyCachedObject
    {
        private const string TYPE_NAME = "objectCaching.CachedObject";
        
        public long Timestamp { get; private set; }
        public int Version { get; private set; }
        public long NodeID { get; private set; }
        public int Shared { get; private set; }
        public PyBuffer Cache { get; private set; }
        public int Compressed { get; private set; }
        public PyDataType ObjectID { get; private set; }

        public static implicit operator PyDataType(PyCachedObject cachedObject)
        {
            if (cachedObject.Cache == null)
                throw new Exception("Cache data is null");

            if (cachedObject.ObjectID == null)
                throw new Exception("objectID is null");

            PyTuple args = new PyTuple(7);
            PyTuple version = new PyTuple(2);

            version[0] = cachedObject.Timestamp;
            version[1] = cachedObject.Version;

            args[0] = version;
            args[1] = new PyNone();
            args[2] = cachedObject.NodeID;
            args[3] = cachedObject.Shared;
            args[4] = cachedObject.Cache;
            args[5] = cachedObject.Compressed;
            args[6] = cachedObject.ObjectID;

            return new PyObjectData(TYPE_NAME, args);
        }

        public static PyCachedObject FromCacheHint(PyCacheHint cacheInfo, PyDataType data)
        {
            PyCachedObject cachedObject = new PyCachedObject();

            cachedObject.NodeID = cacheInfo.NodeID;
            cachedObject.ObjectID = cacheInfo.ObjectID;
            cachedObject.Shared = 1;
            cachedObject.Compressed = 1;
            cachedObject.Cache = new PyBuffer(ZlibHelper.Compress(PythonTypes.Marshal.Marshal.ToByteArray(data)));
            cachedObject.Timestamp = cacheInfo.CacheTime;
            cachedObject.Version = cacheInfo.Version;

            return cachedObject;
        }
    }
}