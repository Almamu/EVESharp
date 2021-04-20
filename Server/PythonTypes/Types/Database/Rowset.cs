using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with util.Rowset types to be sent to the EVE Online client
    /// </summary>
    public class Rowset
    {
        /// <summary>
        /// Type of the rowser
        /// </summary>
        private const string TYPE_NAME = "util.Rowset";
        /// <summary>
        /// Type of every row
        /// </summary>
        private const string ROW_TYPE_NAME = "util.Row";
        
        /// <summary>
        /// Headers of the rowset
        /// </summary>
        public PyList<PyString> Header { get; }
        /// <summary>
        /// All the rows of the Rowset
        /// </summary>
        public PyList<PyList> Rows { get; }

        public Rowset(PyList<PyString> headers)
        {
            this.Header = headers;
            this.Rows = new PyList<PyList>();
        }

        public Rowset(PyList<PyString> headers, PyList<PyList> rows)
        {
            this.Header = headers;
            this.Rows = rows;
        }

        /// <summary>
        /// Simple helper method that creates a correct Rowset ready to be sent
        /// to the EVE Online client based on the given MySqlDataReader
        /// </summary>
        /// <param name="connection">The connection used</param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Rowset FromMySqlDataReader(IDatabaseConnection connection, MySqlDataReader reader)
        {
            connection.GetDatabaseHeaders(reader, out PyList<PyString> headers, out FieldType[] fieldTypes);
            Rowset result = new Rowset(headers);

            while (reader.Read() == true)
            {
                PyList row = new PyList(reader.FieldCount);

                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = IDatabaseConnection.ObjectFromColumn(reader, fieldTypes[i], i);
                
                result.Rows.Add(row);
            }

            return result;
        }

        public static implicit operator PyDataType(Rowset rowset)
        {
            // create the main container for the util.Rowset
            PyDictionary arguments = new PyDictionary
            {
                // store the header and specify the type of rows the Rowset contains
                ["header"] = rowset.Header,
                ["RowClass"] = new PyToken(ROW_TYPE_NAME),
                ["lines"] = rowset.Rows
            };

            return new PyObjectData(TYPE_NAME, arguments);
        }
    }
}