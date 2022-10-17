using System;
using System.Data;
using System.IO;
using System.Text;
using EVESharp.Database.MySql;
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database;

public static class ReaderExtensions
{
    public static int? GetInt32OrNull (this IDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetInt32 (columnIndex);
    }

    public static long? GetInt64OrNull (this IDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetInt64 (columnIndex);
    }

    public static double? GetDoubleOrNull (this IDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetDouble (columnIndex);
    }

    public static string GetStringOrNull (this IDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetString (columnIndex);
    }

    public static int GetInt32OrDefault (this IDataReader reader, int columnIndex, int defaultValue = 0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetInt32 (columnIndex);
    }

    public static long GetInt64OrDefault (this IDataReader reader, int columnIndex, long defaultValue = 0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetInt64 (columnIndex);
    }

    public static double GetDoubleOrDefault (this IDataReader reader, int columnIndex, double defaultValue = 0.0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetDouble (columnIndex);
    }

    public static object GetValueOrNull (this IDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetValue (columnIndex);
    }

    public static string GetStringOrDefault (this IDataReader reader, int columnIndex, string defaultValue = "")
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetString (columnIndex);
    }    /// <summary>
    /// Creates a PyDataType of the given column (specified by <paramref name="index"/>) based off the given reader
    /// </summary>
    /// <param name="reader">Reader to get the data from</param>
    /// <param name="type">The type of the field to convert</param>
    /// <param name="index">Column of the current result read in the MySqlDataReader to create the PyDataType</param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">If any error was found during the creation of the PyDataType</exception>
    public static PyDataType GetPyDataType (this IDataReader reader, FieldType type, int index)
    {
        // null values should be null
        if (reader.IsDBNull (index))
            return null;

        return type switch
        {
            FieldType.I2    => reader.GetInt16 (index),
            FieldType.UI2   => (ushort) reader.GetValue (index),
            FieldType.I4    => reader.GetInt32 (index),
            FieldType.UI4   => (uint) reader.GetValue (index),
            FieldType.R4    => reader.GetFloat (index),
            FieldType.R8    => reader.GetFieldType (index) == typeof (decimal) ? (double) reader.GetDecimal (index) : reader.GetDouble (index),
            FieldType.Bool  => reader.GetBoolean (index),
            FieldType.I1    => (sbyte) reader.GetValue (index),
            FieldType.UI1   => reader.GetByte (index),
            FieldType.UI8   => (ulong) reader.GetValue (index),
            FieldType.Bytes => (byte []) reader.GetValue (index),
            FieldType.I8    => reader.GetInt64 (index),
            FieldType.WStr  => new PyString (reader.GetString (index), true),
            FieldType.Str   => new PyString (reader.GetString (index)),
            _               => throw new InvalidDataException ($"Unknown data type {type}")
        };
    }

    /// <summary>
    /// Obtains the current field type off a MySqlDataReader for the given column
    /// </summary>
    /// <param name="reader">The data reader to use</param>
    /// <param name="index">The column to get the type from</param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">If the type is not supported</exception>
    public static FieldType GetFieldType (this IDataReader reader, int index)
    {
        Type type = reader.GetFieldType (index);

        if (type == typeof (string))
        {
            // unwrap the reader if needed
            if (reader is WrappedDbDataReader wrapper)
                reader = wrapper.Unwrap ();
            
            MySqlDataReader mysqlReader = (MySqlDataReader) reader;

            Encoding encoding = mysqlReader.GetEncoding (index);

            return encoding.Equals (Encoding.ASCII) == true ? FieldType.Str : FieldType.WStr;
        }
        
        if (type == typeof (byte []))
            return FieldType.Bytes;
        
        return Type.GetTypeCode (type) switch
        {
            TypeCode.Boolean => FieldType.Bool,
            TypeCode.Byte => FieldType.UI1,
            TypeCode.SByte => FieldType.I1,
            TypeCode.Int16 => FieldType.I2,
            TypeCode.UInt16 => FieldType.UI2,
            TypeCode.Int32 => FieldType.I4,
            TypeCode.UInt32 => FieldType.UI4,
            TypeCode.Int64 => FieldType.I8,
            TypeCode.UInt64 => FieldType.UI8,
            TypeCode.Single => FieldType.R4,
            TypeCode.Decimal => FieldType.R8,
            TypeCode.Double => FieldType.R8,
            _ => throw new InvalidDataException ($"Unknown field type {type}")
        };
    }

    /// <summary>
    /// Obtains the list of types for all the columns in this MySqlDataReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns></returns>
    public static FieldType [] GetFieldTypes (this IDataReader reader)
    {
        FieldType [] result = new FieldType[reader.FieldCount];

        for (int i = 0; i < result.Length; i++)
            result [i] = GetFieldType (reader, i);

        return result;
    }

    /// <summary>
    /// Obtains important metadata used in the database functions
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="headers">Where to put the headers</param>
    /// <param name="fieldTypes">Where to put the field types</param>
    public static void GetDatabaseHeaders (this IDataReader reader, out PyList <PyString> headers, out FieldType [] fieldTypes)
    {
        headers    = new PyList <PyString> (reader.FieldCount);
        fieldTypes = new FieldType[reader.FieldCount];

        for (int i = 0; i < reader.FieldCount; i++)
        {
            headers [i]    = reader.GetName (i);
            fieldTypes [i] = GetFieldType (reader, i);
        }
    }

    public static PyPackedRow PackedRow (this IDataReader reader)
    {
        using (reader)
        {
            DBRowDescriptor descriptor = DBRowDescriptor (reader);
            return PackedRow (reader, descriptor);    
        }
    }

    /// <summary>
    /// Simple helper method that creates the correct PackedRow data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static PyPackedRow PackedRow (this IDataReader reader, DBRowDescriptor descriptor)
    {
        PyPackedRow row = new PyPackedRow (descriptor);

        int i = 0;

        foreach (DBRowDescriptor.Column column in descriptor.Columns)
            row [column.Name] = reader.GetPyDataType (column.Type, i++);

        return row;
    }

    /// <summary>
    /// Simple helper method that creates the correct PackedRow data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="rowset"></param>
    /// <returns></returns>
    public static PyPackedRow PackedRow (this IDataReader reader, CRowset rowset)
    {
        return PackedRow (reader, rowset.Header);
    }
    
    /// <summary>
    /// Simple helper method that creates the correct PackedRowList data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    public static PyList <PyPackedRow> PackedRowList (this IDataReader reader)
    {
        using (reader)
        {
            DBRowDescriptor      descriptor = DBRowDescriptor (reader);
            PyList <PyPackedRow> list       = new PyList <PyPackedRow> ();

            while (reader.Read ())
                list.Add (PackedRow (reader, descriptor));

            return list;
        }
    }
    
    /// <summary>
    /// Simple helper method that creates a correct IntegerIntegerListDictionary and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// IMPORTANT: The first field MUST be ordered (direction doesn't matter) for this method
    /// to properly work
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <param name="keyColumnIndex">The column to use as index for the IntPackedRowListDictionary</param>
    /// <returns></returns>
    public static PyDataType IntPackedRowListDictionary (this IDataReader reader, int keyColumnIndex)
    {
        using (reader)
        {
            DBRowDescriptor descriptor = DBRowDescriptor (reader);
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
                currentList.Add (PackedRow (reader, descriptor));
            }

            // ensure the last key is saved to the list
            result [key] = currentList;

            return result;
        }
    }
    
    /// <summary>
    /// Simple helper method that creates a correct DictRowList and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDataType DictRowList (this IDataReader reader)
    {
        using (reader)
        {
            PyDictionary result = new PyDictionary ();

            GetDatabaseHeaders (reader, out PyList <PyString> header, out FieldType [] fieldTypes);

            int index = 0;

            while (reader.Read ())
                result [index++] = Row (reader, header, fieldTypes);

            return result;
        }
    }
    
    /// <summary>
    /// Simple helper method that creates a correct RowList and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDataType RowList (this IDataReader reader)
    {
        using (reader)
        {
            GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);
            PyList lines = new PyList ();

            while (reader.Read ())
                lines.Add (Row (reader, headers, fieldTypes));

            return new PyTuple (2)
            {
                [0] = headers,
                [1] = lines
            };
        }
    }
    
    /// <summary>
    /// Simple helper method that creates a correct tupleset and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDataType TupleSet (this IDataReader reader)
    {
        using (reader)
        {
            GetDatabaseHeaders (reader, out PyList <PyString> columns, out FieldType [] fieldTypes);
            PyList rows = new PyList ();

            while (reader.Read ())
            {
                PyList linedata = new PyList (columns.Count);

                for (int i = 0; i < columns.Count; i++)
                    linedata [i] = reader.GetPyDataType (fieldTypes [i], i);

                rows.Add (linedata);
            }

            return new PyTuple (2)
            {
                [0] = columns,
                [1] = rows
            };
        }
    }

    public static PyDictionary <TKey, TValue> Dictionary <TKey, TValue> (this IDataReader reader) where TKey : PyDataType where TValue : PyDataType
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("No data returned");
            
            PyDictionary <TKey, TValue> result = new PyDictionary <TKey, TValue> ();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                FieldType type = GetFieldType (reader, i);
                result [reader.GetName (i)] = reader.GetPyDataType (type, i);
            }

            return result;
        }
    }

    public static PyList <T> List <T> (this IDataReader reader) where T : PyDataType
    {
        using (reader)
        {
            PyList <T> result = new PyList <T> ();
            FieldType  type   = GetFieldType (reader, 0);

            while (reader.Read ())
                result.Add (reader.GetPyDataType (type, 0));

            return result;
        }
    }

    /// <summary>
    /// Simple helper method that creates the correct KeyVal data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static PyDataType KeyVal (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("No data returned");
            
            PyDictionary data = new PyDictionary ();

            for (int i = 0; i < reader.FieldCount; i++)
                data [reader.GetName (i)] = reader.GetPyDataType (GetFieldType (reader, i), i);

            return new PyObjectData (Types.KeyVal.OBJECT_NAME, data);
        }
    }

    /// <summary>
    /// Helper method to instantiate a dbutil.CRowset type from a MySqlDataReader, this consumes the result
    /// but does not close it, so calling code has to take care of this. Ideally, please use "using" statements
    /// </summary>
    /// <param name="reader">The reader to use as source of the information</param>
    /// <returns>The CRowset object ready to be used</returns>
    public static CRowset CRowset (this IDataReader reader)
    {
        using (reader)
        {
            DBRowDescriptor descriptor = DBRowDescriptor (reader);
            CRowset         rowset     = new CRowset (descriptor);

            while (reader.Read ())
                rowset.Add (PackedRow (reader, descriptor));

            return rowset;
        }
    }

    /// <summary>
    /// Simple helper method that creates a correct Rowset ready to be sent
    /// to the EVE Online client based on the given MySqlDataReader
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static Rowset Rowset (this IDataReader reader)
    {
        using (reader)
        {
            GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);
            Rowset result = new Rowset (headers);

            while (reader.Read ())
            {
                PyList row = new PyList (reader.FieldCount);

                for (int i = 0; i < reader.FieldCount; i++)
                    row [i] = reader.GetPyDataType (fieldTypes [i], i);

                result.Rows.Add (row);
            }

            return result;
        }
    }

    /// <summary>
    /// Simple helper method that creates a correct IndexRowset and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <param name="indexField">The field to use as index for the rowset</param>
    /// <returns></returns>
    public static IndexRowset IndexRowset (this IDataReader reader, int indexField)
    {
        using (reader)
        {
            string indexFieldName = reader.GetName (indexField);

            GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);

            IndexRowset rowset = new IndexRowset (indexFieldName, headers);

            while (reader.Read ())
            {
                PyList row = new PyList (reader.FieldCount);

                for (int i = 0; i < row.Count; i++)
                    row [i] = reader.GetPyDataType (fieldTypes [i], i);

                rowset.AddRow (reader.GetInt32 (indexField), row);
            }

            return rowset;
        }
    }

    /// <summary>
    /// Creates a new DBRowDescriptor based off a specific result set from a MySqlDataReader
    /// </summary>
    /// <param name="connection">The database connection used</param>
    /// <param name="reader">The MySqlDataReader to use when creating the DBRowDescriptor</param>
    /// <returns>Instance of a new DBRowDescriptor</returns>
    /// <exception cref="InvalidDataException">If any error was found on the creation</exception>
    public static DBRowDescriptor DBRowDescriptor (this IDataReader reader)
    {
        DBRowDescriptor descriptor = new DBRowDescriptor ();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            FieldType fieldType = GetFieldType(reader, i);

            descriptor.Columns.Add (new DBRowDescriptor.Column (reader.GetName (i), fieldType));
        }

        return descriptor;
    }
    
    /// <summary>
    /// Simple helper method that creates a correct IntegerIntegerDictionary and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDictionary <PyInteger, PyInteger> IntIntDictionary (this IDataReader reader)
    {
        using (reader)
        {
            PyDictionary <PyInteger, PyInteger> result = new PyDictionary <PyInteger, PyInteger> ();

            Type keyType = reader.GetFieldType (0);
            Type valType = reader.GetFieldType (1);

            if (keyType != typeof (long) && keyType != typeof (int) && keyType != typeof (short) &&
                keyType != typeof (byte) && keyType != typeof (ulong) && keyType != typeof (uint) &&
                keyType != typeof (ushort) && keyType != typeof (sbyte) && valType != typeof (long) &&
                valType != typeof (int) && valType != typeof (short) && valType != typeof (byte) &&
                valType != typeof (ulong) && valType != typeof (uint) && valType != typeof (ushort) &&
                valType != typeof (sbyte))
                throw new InvalidDataException ("Expected two fields of type int");

            while (reader.Read ())
            {
                // ignore null keys
                if (reader.IsDBNull (0))
                    continue;

                int key = reader.GetInt32 (0);
                int val = 0;

                if (reader.IsDBNull (1) == false)
                    val = reader.GetInt32 (1);

                result [key] = val;
            }

            return result;
        }
    }
    
    /// <summary>
    /// Simple helper method that creates a correct IntegerIntegerListDictionary and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    ///
    /// IMPORTANT: The first field MUST be ordered (direction doesn't matter) for this method
    /// to properly work
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDictionary <PyInteger, PyList <PyInteger>> IntIntListDictionary (this IDataReader reader)
    {
        using (reader)
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
    
    /// <summary>
    /// Simple helper method that creates a correct IntRowDictionary and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <param name="keyColumnIndex">The column to use as index for the IntRowDictionary</param>
    /// <returns></returns>
    public static PyDictionary IntRowDictionary (this IDataReader reader, int keyColumnIndex)
    {
        using (reader)
        {
            PyDictionary result = new PyDictionary ();
            GetDatabaseHeaders (reader, out PyList <PyString> header, out FieldType [] fieldTypes);

            while (reader.Read ())
                result [reader.GetInt32 (keyColumnIndex)] = Row (reader, header, fieldTypes);

            return result;
        }
    }

    /// <summary>
    /// Simple helper method that creates the correct Row data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    /// <param name="fieldTypes"></param>
    /// <returns></returns>
    public static Row Row (this IDataReader reader, PyList <PyString> header, FieldType [] fieldTypes)
    {
        PyList row = new PyList (reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
            row [i] = reader.GetPyDataType(fieldTypes [i], i);

        return new Row (header, row);
    }

    /// <summary>
    /// Simple helper method that creates the correct Row data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static Row Row (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("No data returned");
            
            PyList <PyString> header = new PyList <PyString> (reader.FieldCount);
            PyList            row    = new PyList (reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                header [i] = reader.GetName (i);
                row [i]    = reader.GetPyDataType (GetFieldType (reader, i), i);
            }

            return new Row (header, row);
        }
    }

    public static T1 Scalar <T1> (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Expected at least one row back, but couldn't get any");
            if (reader.FieldCount != 1)
                throw new InvalidDataException ($"Expected two columns but returned {reader.FieldCount}");

            return (T1) reader.GetValueOrNull (0);
        }
    }

    public static (T1, T2) Scalar <T1, T2> (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Expected at least one row back, but couldn't get any");
            if (reader.FieldCount != 2)
                throw new InvalidDataException ($"Expected two columns but returned {reader.FieldCount}");
            
            return (
                (T1) reader.GetValueOrNull (0),
                (T2) reader.GetValueOrNull (1)
            );
        }
    }

    public static (T1, T2, T3) Scalar <T1, T2, T3> (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Expected at least one row back, but couldn't get any");
            if (reader.FieldCount != 3)
                throw new InvalidDataException ($"Expected two columns but returned {reader.FieldCount}");
            
            return (
                (T1) reader.GetValueOrNull (0),
                (T2) reader.GetValueOrNull (1),
                (T3) reader.GetValueOrNull (2)
            );
        }
    }

    public static PyDictionary <PyString, PyTuple> DifferenceDict (this IDataReader reader)
    {
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Expected at least one row back, but couldn't get any");
            
            PyDictionary <PyString, PyTuple> result = new PyDictionary <PyString, PyTuple> ();
        
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result [reader.GetName (i)] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = reader.GetPyDataType (GetFieldType (reader, i), i)
                };
            }

            return result;
        }
    }
}