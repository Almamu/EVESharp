using System;
using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database
{
    public class DictRowlist
    {
        /// <summary>
        /// Simple helper method that creates a correct DictRowList and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        /// 
        /// </summary>
        /// <param name="connection">The connection used</param>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(IDatabaseConnection connection, MySqlDataReader reader)
        {
            PyDictionary result = new PyDictionary();

            connection.GetDatabaseHeaders(reader, out PyList<PyString> header, out FieldType[] fieldTypes);

            int index = 0;
            
            while (reader.Read() == true)
            {
                result[index++] = Row.FromMySqlDataReader(reader, header, fieldTypes);
            }

            return result;
        }
    }
}