using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class IntRowDictionary
    {
        /// <summary>
        /// Simple helper method that creates a correct IntegerIntegerList dictionary and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        ///
        /// IMPORTANT: The first field MUST be ordered (direction doesn't matter) for this method
        /// to properly work
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader, int keyColumnIndex)
        {
            PyDictionary result = new PyDictionary();
            PyList header = new PyList();

            for (int i = 0; i < reader.FieldCount; i++)
                header.Add(reader.GetName(i));

            while (reader.Read() == true)
            {
                result[reader.GetInt32(keyColumnIndex)] = Row.FromMySqlDataReader(reader, header);
            }
            
            return result;
        }
    }
}