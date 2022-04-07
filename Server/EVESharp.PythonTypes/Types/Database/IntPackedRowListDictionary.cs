using System;
using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

public static class IntPackedRowListDictionary
{
    /// <summary>
    /// Simple helper method that creates a correct IntegerIntegerListDictionary and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// IMPORTANT: The first field MUST be ordered (direction doesn't matter) for this method
    /// to properly work
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <param name="keyColumnIndex">The column to use as index for the IntPackedRowListDictionary</param>
    /// <returns></returns>
    public static PyDataType FromMySqlDataReader (IDatabaseConnection connection, MySqlDataReader reader, int keyColumnIndex)
    {
        DBRowDescriptor descriptor = DBRowDescriptor.FromMySqlReader (connection, reader);
        PyDictionary    result     = new PyDictionary ();

        Type keyType = reader.GetFieldType (keyColumnIndex);

        if (keyType != typeof (long) && keyType != typeof (int) && keyType != typeof (short) &&
            keyType != typeof (byte) && keyType != typeof (ulong) && keyType != typeof (uint) &&
            keyType != typeof (ushort) && keyType != typeof (sbyte))
            throw new InvalidDataException ("Expected key type of integer");

        // get first key and start preparing the values
        int key = 0;

        PyList currentList = new PyList ();

        while (reader.Read ())
        {
            // ignore null keys
            if (reader.IsDBNull (keyColumnIndex))
                continue;

            int newKey = reader.GetInt32 (keyColumnIndex);

            // if the read key doesn't match the one read earlier
            if (newKey != key)
            {
                // do not add an entry to the dict unless the old id was present
                if (key != 0)
                    result [key] = currentList;

                currentList = new PyList ();
                key         = newKey;
            }

            // add the current value to the list
            currentList.Add (PyPackedRow.FromMySqlDataReader (reader, descriptor));
        }

        // ensure the last key is saved to the list
        result [key] = currentList;

        return result;
    }
}