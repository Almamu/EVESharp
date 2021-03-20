using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public static class IntPackedRowListDictionary
    {
        /// <summary>
        /// Simple helper method that creates a correct IntegerIntegerListDictionary and returns
        /// it's PyDataType representation, ready to be sent to the EVE Online client
        ///
        /// IMPORTANT: The first field MUST be ordered (direction doesn't matter) for this method
        /// to properly work
        /// </summary>
        /// <param name="reader">The MySqlDataReader to read the data from</param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader, int keyColumnIndex)
        {
            DBRowDescriptor descriptor = DBRowDescriptor.FromMySqlReader(reader);
            PyDictionary result = new PyDictionary();

            Type keyType = reader.GetFieldType(keyColumnIndex);
            
            if (keyType != typeof(long) && keyType != typeof(int) && keyType != typeof(short) &&
                keyType != typeof(byte) && keyType != typeof(ulong) && keyType != typeof(uint) &&
                keyType != typeof(ushort) && keyType != typeof(sbyte) )
                throw new InvalidDataException("Expected key type of integer");

            // get first key and start preparing the values
            int key = 0;
            
            PyList currentList = new PyList();
            
            while (reader.Read() == true)
            {
                // ignore null keys
                if (reader.IsDBNull(keyColumnIndex) == true)
                    continue;

                int newKey = reader.GetInt32(keyColumnIndex);
                int val = 0;

                // if the read key doesn't match the one read earlier
                if (newKey != key)
                {
                    // do not add an entry to the dict unless the old id was present
                    if (key != 0)
                        result[key] = currentList;
                    
                    currentList = new PyList();
                    key = newKey;
                }

                // add the current value to the list
                currentList.Add(PyPackedRow.FromMySqlDataReader(reader, descriptor));
            }

            // ensure the last key is saved to the list
            result[key] = currentList;

            return result;
        }
    }
}