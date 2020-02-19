using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class PyPackedRowList
    {
        public static PyList FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList list = new PyList();
            DBRowDescriptor descriptor = DBRowDescriptor.FromMySqlReader(reader);

            while (reader.Read() == true)
                list.Add(PyPackedRow.FromMySqlDataReader(reader, descriptor));

            return list;
        }
    }
}