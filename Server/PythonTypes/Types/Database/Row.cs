using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class Row
    {
        /// <summary>
        /// Type of row
        /// </summary>
        private const string ROW_TYPE_NAME = "util.Row";
        /// <summary>
        /// The columns for this row
        /// </summary>
        public PyList Header { get; }
        /// <summary>
        /// The values for each column
        /// </summary>
        public PyList Line { get; }

        public Row(PyList header, PyList line)
        {
            this.Header = header;
            this.Line = line;
        }
        
        public static implicit operator PyDataType(Row row)
        {
            PyDictionary data = new PyDictionary();

            data["header"] = row.Header;
            data["line"] = row.Line;

            return new PyObjectData(ROW_TYPE_NAME, data);
        }
        
        /// <summary>
        /// Simple helper method that creates the correct Row data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Row FromMySqlDataReader(MySqlDataReader reader, PyList header)
        {
            PyList row = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = Utils.ObjectFromColumn(reader, i);

            return new Row(header, row);
        }
        
        /// <summary>
        /// Simple helper method that creates the correct Row data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Row FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList header = new PyList(reader.FieldCount);
            PyList row = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                header[i] = reader.GetName(i);
                row[i] = Utils.ObjectFromColumn(reader, i);
            }
            
            return new Row(header, row);
        }
    }
}