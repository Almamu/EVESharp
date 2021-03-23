using System;
using PythonTypes.Compression;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    /// <summary>
    /// Helper class to work with PyCachedObject's (objectCaching.CachedObject) to be sent to the EVE Online client
    /// when cache requests are performed
    /// </summary>
    public class PyCacheMethodCallResult
    {
        private const string TYPE_NAME = "objectCaching.CachedMethodCallResult";
        
        public PyDataType CacheHint { get; private set; }

        public static implicit operator PyDataType(PyCacheMethodCallResult cachedObject)
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

        public static PyCacheMethodCallResult FromCacheHint(PyDataType cacheInfo)
        {
            return new PyCacheMethodCallResult
            {
                CacheHint = cacheInfo
            };
        }
    }
}