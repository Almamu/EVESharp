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
        
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader, PyList header)
        {
            PyDictionary data = new PyDictionary();
            
            PyList row = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
                row.Add(Utils.ObjectFromColumn(reader, i));
            
            data["header"] = header;
            data["line"] = row;

            return new PyObjectData(ROW_TYPE_NAME, data);
        }
        
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyDictionary data = new PyDictionary();
            
            PyList header = new PyList();
            PyList row = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                header.Add(reader.GetName(i));
                row.Add(Utils.ObjectFromColumn(reader, i));
            }

            data["header"] = header;
            data["line"] = row;

            return new PyObjectData(ROW_TYPE_NAME, data);
        }
    }
}