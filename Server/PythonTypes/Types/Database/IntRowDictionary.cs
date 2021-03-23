using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class IntRowDictionary
    {
        /// <summary>
        /// Simple helper method that creates a correct IntRowDictionary and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <param name="keyColumnIndex">The column to use as index for the IntRowDictionary</param>
        /// <returns></returns>
        public static PyDictionary FromMySqlDataReader(MySqlDataReader reader, int keyColumnIndex)
        {
            PyDictionary result = new PyDictionary();
            PyList header = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                header [i] = reader.GetName(i);

            while (reader.Read() == true)
            {
                result [reader.GetInt32(keyColumnIndex)] = Row.FromMySqlDataReader(reader, header);
            }
            
            return result;
        }
    }
}