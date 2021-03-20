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
            if (cachedObject.CacheHint == null)
                throw new Exception("CacheHint data is null");

            PyTuple args = new PyTuple(3);
            PyDictionary versionCheck = new PyDictionary();

            versionCheck["versionCheck"] = "run";

            args[0] = versionCheck;
            args[1] = cachedObject.CacheHint;
            args[2] = new PyNone();

            return new PyObjectData(TYPE_NAME, args);
        }

        public static PyCacheMethodCallResult FromCacheHint(PyDataType cacheInfo)
        {
            PyCacheMethodCallResult cachedObject = new PyCacheMethodCallResult();

            cachedObject.CacheHint = cacheInfo;

            return cachedObject;
        }
    }
}