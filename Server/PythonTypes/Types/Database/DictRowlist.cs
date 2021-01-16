using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class DictRowlist
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
            PyDictionary result = new PyDictionary();
            PyList header = new PyList(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                header [i] = reader.GetName(i);

            int index = 0;
            
            while (reader.Read() == true)
            {
                result[index++] = Row.FromMySqlDataReader(reader, header);
            }

            return result;
        }
    }
}