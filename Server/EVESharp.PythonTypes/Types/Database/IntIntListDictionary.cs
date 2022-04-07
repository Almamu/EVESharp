using System;
using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

public static class IntIntListDictionary
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
    public static PyDictionary <PyInteger, PyList <PyInteger>> FromMySqlDataReader (MySqlDataReader reader)
    {
        PyDictionary <PyInteger, PyList <PyInteger>> result = new PyDictionary <PyInteger, PyList <PyInteger>> ();

        Type keyType = reader.GetFieldType (0);
        Type valType = reader.GetFieldType (1);

        if (keyType != typeof (long) && keyType != typeof (int) && keyType != typeof (short) &&
            keyType != typeof (byte) && keyType != typeof (ulong) && keyType != typeof (uint) &&
            keyType != typeof (ushort) && keyType != typeof (sbyte) && valType != typeof (long) &&
            valType != typeof (int) && valType != typeof (short) && valType != typeof (byte) &&
            valType != typeof (ulong) && valType != typeof (uint) && valType != typeof (ushort) &&
            valType != typeof (sbyte))
            throw new InvalidDataException ("Expected two fields of type int");

        // get first key and start preparing the values
        int key = 0;

        PyList <PyInteger> currentList = new PyList <PyInteger> ();

        while (reader.Read ())
        {
            // ignore null keys
            if (reader.IsDBNull (0))
                continue;

            int newKey = reader.GetInt32 (0);
            int val    = 0;

            // if the read key doesn't match the one read earlier
            if (newKey != key)
            {
                // do not add an entry to the dict unless the old id was present
                if (key != 0)
                    result [key] = currentList;

                currentList = new PyList <PyInteger> ();
                key         = newKey;
            }

            if (reader.IsDBNull (1) == false)
                val = reader.GetInt32 (1);

            // add the current value to the list
            currentList.Add (val);
        }

        // ensure the last key is saved to the list
        result [key] = currentList;

        return result;
    }
}