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
        
        public PyList Header { get; }
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
        
        public static Row FromMySqlDataReader(MySqlDataReader reader, PyList header)
        {
            PyList row = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = Utils.ObjectFromColumn(reader, i);

            return new Row(header, row);
        }
        
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