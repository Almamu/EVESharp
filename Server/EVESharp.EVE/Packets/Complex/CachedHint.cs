using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets.Complex
{
    /// <summary>
    /// Helper class to work with PyCacheHint's (util.CachedObject) to be sent to the EVE Online client
    /// when cache requests are performed
    /// </summary>
    public class CachedHint
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

        public static implicit operator PyDataType(CachedHint cacheHint)
        {
            PyTuple timestamp = new PyTuple(2)
            {
                [0] = cacheHint.CacheTime,
                [1] = cacheHint.Version
            };

            return new PyObjectData(TYPE_NAME,
                new PyTuple(3)
                {
                    [0] = cacheHint.ObjectID,
                    [1] = cacheHint.NodeID,
                    [2] = timestamp
                }
            );
        }

        public static implicit operator CachedHint(PyDataType from)
        {
            PyTuple container = from as PyTuple;
            PyTuple timestamp = container[2] as PyTuple;

            return new CachedHint
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
        public static CachedHint FromBuffer(string name, byte[] data, long timestamp, long nodeID)
        {
            return new CachedHint
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
        public static CachedHint FromBuffer(PyDataType objectID, byte[] data, long timestamp, long nodeID)
        {
            return new CachedHint
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
        public static CachedHint FromPyObject(PyDataType objectID, PyDataType data, long timestamp, long nodeID)
        {
            return FromBuffer(objectID, EVESharp.PythonTypes.Marshal.Marshal.ToByteArray(data), timestamp, nodeID);
        }
    }
}