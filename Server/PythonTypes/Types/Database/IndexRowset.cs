using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with util.Rowset types to be sent to the EVE Online client
    /// </summary>
    public class IndexRowset
    {
        /// <summary>
        /// Type of the rowser
        /// </summary>
        private const string TYPE_NAME = "util.IndexRowset";
        /// <summary>
        /// Type of every row
        /// </summary>
        private const string ROW_TYPE_NAME = "util.Row";
        
        protected PyList Headers { get; }
        protected PyDictionary Lines { get; }
        public string IDName { get; set; }

        public IndexRowset(string idName, PyList headers)
        {
            this.Headers = headers;
            this.Lines = new PyDictionary();
            this.IDName = idName;
        }

        public static implicit operator PyDataType(IndexRowset rowset)
        {
            PyDictionary container = new PyDictionary()
            {
                {"header", rowset.Headers},
                {"RowClass", new PyToken(ROW_TYPE_NAME)},
                {"idName", rowset.IDName},
                {"items", rowset.Lines}
            };

            return new PyObjectData(TYPE_NAME, container);
        }

        protected void AddRow(int index, PyList data)
        {
            if (data.Count != this.Headers.Count)
                throw new InvalidParameterException("The row doesn't have the same amount of items as the header of the IndexRowset");

            this.Lines[index] = data;
        }

        public static IndexRowset FromMySqlDataReader(MySqlDataReader reader, int indexField)
        {
            string indexFieldName = reader.GetName(indexField);
            
            PyList headers = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                headers[i] = reader.GetName(i);
            
            IndexRowset rowset = new IndexRowset(indexFieldName, headers);

            while (reader.Read() == true)
            {
                PyList row = new PyList(reader.FieldCount);

                for (int i = 0; i < row.Count; i++)
                    row[i] = Utils.ObjectFromColumn(reader, i);

                rowset.AddRow(reader.GetInt32(indexField), row);
            }

            return rowset;
        }
    }
}