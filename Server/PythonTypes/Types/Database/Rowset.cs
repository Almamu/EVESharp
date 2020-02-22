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
        /// Simple helper method that creates a correct util.Rowset ready to be sent
        /// to the EVE Online client based on the given MySqlDataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            // ensure the result is not empty
            if (reader.FieldCount == 0)
                return new PyObjectData(TYPE_NAME, new PyDictionary());

            // create the main container for the util.Rowset
            PyDictionary arguments = new PyDictionary();
            // create the header for the rows
            PyList header = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
                header.Add(reader.GetName(i));

            // store the header and specify the type of rows the Rowset contains
            arguments["header"] = header;
            arguments["RowClass"] = new PyToken(ROW_TYPE_NAME);

            // finally fill the list of lines the rowset has with the final PyDataTypes
            // based off the column's values
            PyList rowlist = new PyList();

            while (reader.Read() == true)
            {
                PyList linedata = new PyList();

                for (int i = 0; i < reader.FieldCount; i++)
                    linedata.Add(Utils.ObjectFromColumn(reader, i));

                rowlist.Add(linedata);
            }

            arguments["lines"] = rowlist;

            return new PyObjectData(TYPE_NAME, arguments);
        }
    }
}