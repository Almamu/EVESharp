using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Encodings;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Header for SparseRowset, which is a special object that acts as a bound service to return results 
    /// </summary>
    public class SparseRowsetHeader
    {
        /// <summary>
        /// Type of the rowset
        /// </summary>
        private const string TYPE_NAME = "util.SparseRowset";
        
        /// <summary>
        /// The number of records found
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// The columns of the result
        /// </summary>
        public PyList Headers { get; private set; }
        /// <summary>
        /// The Bound ID for this SparseRowset
        /// </summary>
        public PyDataType BoundObjectIdentifier { get; set; }

        public SparseRowsetHeader(int count, PyList headers)
        {
            this.Count = count;
            this.Headers = headers;
        }

        public static implicit operator PyDataType(SparseRowsetHeader rowsetHeader)
        {
            PyTuple container = new PyTuple(3);

            container[0] = rowsetHeader.Headers;
            container[1] = rowsetHeader.BoundObjectIdentifier;
            container[2] = rowsetHeader.Count;

            return new PyObjectData(TYPE_NAME, container);
        }
        
        /// <summary>
        /// Simple helper method that creates rows to be returned from a SparseRowset-based bound service
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public PyList<PyTuple> DataFromMySqlReader(int pkFieldIndex, MySqlDataReader reader, Dictionary<PyDataType, int> rowsIndex)
        {
            PyList<PyTuple> result = new PyList<PyTuple>();

            while (reader.Read() == true)
            {
                PyDataType keyValue = Utils.ObjectFromColumn(reader, pkFieldIndex);
                
                result.Add(
                    new PyTuple(3)
                    {
                        [0] = keyValue,
                        [1] = rowsIndex[keyValue],
                        [2] = Row.FromMySqlDataReader(reader, this.Headers) 
                    }
                );
            }
            
            return result;
        }
    }
}