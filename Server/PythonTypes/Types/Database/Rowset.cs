using System.Collections.Generic;
using MySql.Data.MySqlClient;
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
        public PyList Header { get; }
        public PyList Rows { get; }

        public Rowset(PyList headers)
        {
            this.Header = headers;
            this.Rows = new PyList();
        }
        
        /// <summary>
        /// Simple helper method that creates a correct util.Rowset ready to be sent
        /// to the EVE Online client based on the given MySqlDataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Rowset FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList headers = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                headers[i] = reader.GetName(i);
            
            Rowset result = new Rowset(headers);

            while (reader.Read() == true)
            {
                PyList row = new PyList(reader.FieldCount);

                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = Utils.ObjectFromColumn(reader, i);
                
                result.Rows.Add(row);
            }

            return result;
        }

        public static implicit operator PyDataType(Rowset rowset)
        {
            // create the main container for the util.Rowset
            PyDictionary arguments = new PyDictionary();
            // store the header and specify the type of rows the Rowset contains
            arguments["header"] = rowset.Header;
            arguments["RowClass"] = new PyToken(ROW_TYPE_NAME);
            arguments["lines"] = rowset.Rows;

            return new PyObjectData(TYPE_NAME, arguments);
        }
    }
}