using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with PyPackedRow lists (which are just a PyList of PyPackedRows)
    /// </summary>
    public class PyPackedRowList
    {
        /// <summary>
        /// Simple helper method that creates the correct PackedRowList data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        public static PyList<PyPackedRow> FromMySqlDataReader(MySqlDataReader reader)
        {
            DBRowDescriptor descriptor = DBRowDescriptor.FromMySqlReader(reader);
            PyList<PyPackedRow> list = new PyList<PyPackedRow>();

            while (reader.Read() == true)
            {
                list.Add(PyPackedRow.FromMySqlDataReader(reader, descriptor));
            }

            return list;
        }
    }
}