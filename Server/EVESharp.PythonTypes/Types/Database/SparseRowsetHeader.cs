using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Encodings;

namespace EVESharp.PythonTypes.Types.Database
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
        public int Count { get; }
        /// <summary>
        /// The columns of the result
        /// </summary>
        public PyList<PyString> Headers { get; }
        /// <summary>
        /// The types for each column
        /// </summary>
        public FieldType[] FieldTypes { get; }
        /// <summary>
        /// The Bound ID for this SparseRowset
        /// </summary>
        public PyDataType BoundObjectIdentifier { get; set; }

        public SparseRowsetHeader(int count, PyList<PyString> headers, FieldType[] fieldTypes)
        {
            this.Count = count;
            this.Headers = headers;
            this.FieldTypes = fieldTypes;
        }

        public static implicit operator PyDataType(SparseRowsetHeader rowsetHeader)
        {
            PyTuple container = new PyTuple(3)
            {
                [0] = rowsetHeader.Headers,
                [1] = rowsetHeader.BoundObjectIdentifier,
                [2] = rowsetHeader.Count
            };
            
            return new PyObjectData(TYPE_NAME, container);
        }

        /// <summary>
        /// Simple helper method that creates rows to be returned from a SparseRowset-based bound service FetchByKey
        /// </summary>
        /// <param name="pkFieldIndex">The field to use as primary key</param>
        /// <param name="reader">The reader to read data from the database</param>
        /// <param name="rowsIndex">The indexed rows</param>
        /// <returns></returns>
        public PyList<PyTuple> FetchByKey(int pkFieldIndex, MySqlDataReader reader, Dictionary<PyDataType, int> rowsIndex)
        {
            PyList<PyTuple> result = new PyList<PyTuple>();

            while (reader.Read() == true)
            {
                PyDataType keyValue = IDatabaseConnection.ObjectFromColumn(reader, this.FieldTypes[pkFieldIndex], pkFieldIndex);
                
                result.Add(
                    new PyTuple(3)
                    {
                        [0] = keyValue,
                        [1] = rowsIndex[keyValue],
                        [2] = Row.FromMySqlDataReader(reader, this.Headers, this.FieldTypes) 
                    }
                );
            }
            
            return result;
        }

        /// <summary>
        /// Simple helper method that creates rows to be returned from a SparseRowset-based bound service FetchByKey
        /// </summary>
        /// <param name="pkFieldIndex">The field to use as primary key</param>
        /// <param name="reader">The reader to read data from the database</param>
        /// <returns></returns>
        public PyList<PyTuple> Fetch(int pkFieldIndex, MySqlDataReader reader)
        {
            PyList<PyTuple> result = new PyList<PyTuple>();

            while (reader.Read() == true)
            {
                PyDataType keyValue = IDatabaseConnection.ObjectFromColumn(reader, this.FieldTypes[pkFieldIndex], pkFieldIndex);
                
                result.Add(
                    new PyTuple(2)
                    {
                        [0] = keyValue,
                        [1] = Row.FromMySqlDataReader(reader, this.Headers, this.FieldTypes) 
                    }
                );
            }
            
            return result;
        }
    }
}