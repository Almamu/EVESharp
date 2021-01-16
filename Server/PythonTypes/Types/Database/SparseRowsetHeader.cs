using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Encodings;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class SparseRowsetHeader
    {
        
        /// <summary>
        /// Type of the rowset
        /// </summary>
        private const string TYPE_NAME = "util.SparseRowset";
        
        public int Count { get; private set; }
        public PyList Headers { get; private set; } 
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

        public PyDataType DataFromMySqlReader(int pkFieldIndex, MySqlDataReader reader, Dictionary<PyDataType, int> rowsIndex)
        {
            PyList result = new PyList();

            while (reader.Read() == true)
            {
                PyDataType keyValue = Utils.ObjectFromColumn(reader, pkFieldIndex);
                
                result.Add(new PyTuple(new PyDataType[]
                        {
                            keyValue, rowsIndex[keyValue], Row.FromMySqlDataReader(reader, this.Headers) 
                        }
                    )
                );
            }
            
            return result;
        }
    }
}