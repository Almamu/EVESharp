using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class KeyVal
    {
        /// <summary>
        /// Simple helper method that creates the correct KeyVal data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyDictionary data = new PyDictionary();

            for (int i = 0; i < reader.FieldCount; i++)
                data[reader.GetName(i)] = Utils.ObjectFromColumn(reader, i);
            
            return new PyObjectData("util.KeyVal", data);
        }
    }
}