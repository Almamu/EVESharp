using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public static class IntIntDictionary
    {
        /// <summary>
        /// Simple helper method that creates a correct IntegerIntegerDictionary and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyDictionary result = new PyDictionary();

            Type keyType = reader.GetFieldType(0);
            Type valType = reader.GetFieldType(1);
            
            if (keyType != typeof(long) && keyType != typeof(int) && keyType != typeof(short) &&
                keyType != typeof(byte) && keyType != typeof(ulong) && keyType != typeof(uint) &&
                keyType != typeof(ushort) && keyType != typeof(sbyte) && valType != typeof(long) &&
                valType != typeof(int) && valType != typeof(short) && valType != typeof(byte) &&
                valType != typeof(ulong) && valType != typeof(uint) && valType != typeof(ushort) &&
                valType != typeof(sbyte))
                throw new InvalidDataException("Expected two fields of type int");
            
            while (reader.Read() == true)
            {
                // ignore null keys
                if (reader.IsDBNull(0) == true)
                    continue;

                int key = reader.GetInt32(0);
                int val = 0;

                if (reader.IsDBNull(1) == false)
                    val = reader.GetInt32(1);

                result[key] = val;
            }

            return result;
        }
    }
}