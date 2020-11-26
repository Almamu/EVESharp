using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with PyPackedRow lists (which are just a PyList of PyPackedRows)
    /// </summary>
    public class PyPackedRowList
    {
        public static PyList FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList list = new PyList();

            while (reader.Read() == true)
            {
                list.Add(PyPackedRow.FromMySqlDataReader(reader, DBRowDescriptor.FromMySqlReader(reader, true)));
            }

            return list;
        }
    }
}