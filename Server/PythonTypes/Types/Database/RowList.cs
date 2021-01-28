using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class RowList
    {
        /// <summary>
        /// Simple helper method that creates a correct RowList and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        ///
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyList colums = new PyList(reader.FieldCount);
            PyList lines = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
                colums[i] = reader.GetName(i);

            while (reader.Read() == true)
            {
                lines.Add(Row.FromMySqlDataReader(reader, colums));
            }

            return new PyTuple(2)
            {
                [0] = colums,
                [1] = lines
            };
        }
    }
}