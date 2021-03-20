using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with TupleSet types to be sent to the EVE Online client
    /// </summary>
    public static class TupleSet
    {
        /// <summary>
        /// Simple helper method that creates a correct tupleset and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList columns = new PyList(reader.FieldCount);
            PyList rows = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
                columns[i] = new PyString(reader.GetName(i));

            while (reader.Read() == true)
            {
                PyList linedata = new PyList(columns.Count);

                for (int i = 0; i < columns.Count; i++)
                    linedata[i] = Utils.ObjectFromColumn(reader, i);

                rows.Add(linedata);
            }

            return new PyTuple(new PyDataType[]
            {
                columns, rows
            });
        }
    }
}