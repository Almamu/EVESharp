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
        public string[] Header { get; set; }
        public List<PyList> Rows { get; }

        public Rowset(string[] headers)
        {
            this.Header = headers;
            this.Rows = new List<PyList>();
        }
        
        /// <summary>
        /// Simple helper method that creates a correct util.Rowset ready to be sent
        /// to the EVE Online client based on the given MySqlDataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Rowset FromMySqlDataReader(MySqlDataReader reader)
        {
            List<string> headers = new List<string>();
            List<PyList> rows = new List<PyList>();
            
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetName(i));
            
            Rowset result = new Rowset(headers.ToArray());

            while (reader.Read() == true)
            {
                PyList row = new PyList();

                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(Utils.ObjectFromColumn(reader, i));
                
                result.Rows.Add(row);
            }

            return result;
        }

        public static implicit operator PyDataType(Rowset rowset)
        {
            // create the main container for the util.Rowset
            PyDictionary arguments = new PyDictionary();
            // create the header for the rows
            PyList header = new PyList();

            for (int i = 0; i < rowset.Header.Length; i++)
                header.Add(rowset.Header[i]);

            // store the header and specify the type of rows the Rowset contains
            arguments["header"] = header;
            arguments["RowClass"] = new PyToken(ROW_TYPE_NAME);

            // finally fill the list of lines the rowset has with the final PyDataTypes
            // based off the column's values
            PyList rowlist = new PyList();

            for(int row = 0; row < rowset.Rows.Count; row ++)
            {
                PyList linedata = new PyList();

                for (int i = 0; i < rowset.Rows[row].Count; i++)
                    linedata.Add(rowset.Rows[row][i]);

                rowlist.Add(linedata);
            }

            arguments["lines"] = rowlist;

            return new PyObjectData(TYPE_NAME, arguments);
        }
    }
}