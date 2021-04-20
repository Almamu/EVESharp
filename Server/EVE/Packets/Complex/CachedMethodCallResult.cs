using System;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets.Complex
{
    /// <summary>
    /// Helper class to work with PyCachedObject's (objectCaching.CachedObject) to be sent to the EVE Online client
    /// when cache requests are performed
    /// </summary>
    public class CachedMethodCallResult
    {
        private const string TYPE_NAME = "objectCaching.CachedMethodCallResult";
        
        public PyDataType CacheHint { get; private set; }

        public static implicit operator PyDataType(CachedMethodCallResult cachedObject)
        {
            if (cachedObject.CacheHint is null)
                throw new Exception("CacheHint data is null");

            PyTuple args = new PyTuple(3)
            {
                [0] = new PyDictionary {["versionCheck"] = "run"},
                [1] = cachedObject.CacheHint,
                [2] = null
            };

            return new PyObjectData(TYPE_NAME, args);
        }

        public static CachedMethodCallResult FromCacheHint(PyDataType cacheInfo)
        {
            return new CachedMethodCallResult
            {
                CacheHint = cacheInfo
            };
        }
    }
}