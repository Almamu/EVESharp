using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    /// <summary>
    /// Helper class to work with PyCacheHint's (util.CachedObject) to be sent to the EVE Online client
    /// when cache requests are performed
    /// </summary>
    public class PyCacheHint
    {
        private const string TYPE_NAME = "util.CachedObject";

        /// <summary>
        /// The datetime this cached object was generated
        /// </summary>
        public long CacheTime { get; private set; }
        /// <summary>
        /// ObjectID representation of the cached object to identify it
        /// </summary>
        public PyDataType ObjectID { get; private set; }
        /// <summary>
        /// The node that generated this object
        /// </summary>
        public long NodeID { get; private set; }
        /// <summary>
        /// The version (CRC32) of this cached object
        /// </summary>
        public int Version { get; private set; }

        public static implicit operator PyDataType(PyCacheHint cacheHint)
        {
            PyTuple timestamp = new PyTuple(new PyDataType[]
            {
                cacheHint.CacheTime, cacheHint.Version
            });

            return new PyObjectData(TYPE_NAME,
                new PyTuple(new[]
                {
                    cacheHint.ObjectID,
                    cacheHint.NodeID,
                    timestamp
                })
            );
        }

        public static implicit operator PyCacheHint(PyDataType from)
        {
            PyTuple container = from as PyTuple;
            PyTuple timestamp = container[2] as PyTuple;

            return new PyCacheHint
            {
                ObjectID = container[1],
                NodeID = container[3] as PyInteger,
                CacheTime = timestamp[0] as PyInteger,
                Version = timestamp[1] as PyInteger
            };
        }

        /// <summary>
        /// Creates a new PyCacheHint based on the given data
        /// </summary>
        /// <param name="name">The name of the cache hint</param>
        /// <param name="data">The data for the cache hint</param>
        /// <param name="timestamp">The timestamp of the creation of the cache hint</param>
        /// <param name="nodeID">The node that created the cache</param>
        /// <returns></returns>
        public static PyCacheHint FromBuffer(string name, byte[] data, long timestamp, long nodeID)
        {
            return new PyCacheHint
            {
                Version = (int) CRC32.Checksum(data),
                NodeID = nodeID,
                ObjectID = new PyString(name),
                CacheTime = timestamp
            };
        }
        
        /// <summary>
        /// Creates a new PyCacheHint based on the given data
        /// </summary>
        /// <param name="objectID">The objectID for this cache hint</param>
        /// <param name="data">The data for the cache hint</param>
        /// <param name="timestamp">The timestamp of the creation of the cache hint</param>
        /// <param name="nodeID">The node that created the cache</param>
        /// <returns></returns>
        public static PyCacheHint FromBuffer(PyDataType objectID, byte[] data, long timestamp, long nodeID)
        {
            return new PyCacheHint
            {
                Version = (int) CRC32.Checksum(data),
                NodeID = nodeID,
                ObjectID = objectID,
                CacheTime = timestamp
            };
        }
        
        /// <summary>
        /// Creates a new PyCacheHint based on the given data
        /// </summary>
        /// <param name="name">The name of the cache hint</param>
        /// <param name="data">The data for the cache hint</param>
        /// <param name="timestamp">The timestamp of the creation of the cache hint</param>
        /// <param name="nodeID">The node that created the cache hint</param>
        /// <returns></returns>
        public static PyCacheHint FromPyObject(PyDataType objectID, PyDataType data, long timestamp, long nodeID)
        {
            return FromBuffer(objectID, PythonTypes.Marshal.Marshal.ToByteArray(data), timestamp, nodeID);
        }
    }
}