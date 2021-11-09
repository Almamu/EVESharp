using System;
using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database
{
    public class IntRowDictionary
    {
        /// <summary>
        /// Simple helper method that creates a correct IntRowDictionary and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="connection">The database connection</param>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <param name="keyColumnIndex">The column to use as index for the IntRowDictionary</param>
        /// <returns></returns>
        public static PyDictionary FromMySqlDataReader(IDatabaseConnection connection, MySqlDataReader reader, int keyColumnIndex)
        {
            PyDictionary result = new PyDictionary();
            connection.GetDatabaseHeaders(reader, out PyList<PyString> header, out FieldType[] fieldTypes);

            while (reader.Read() == true)
            {
                result [reader.GetInt32(keyColumnIndex)] = Row.FromMySqlDataReader(reader, header, fieldTypes);
            }
            
            return result;
        }
    }
}