using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Extended Python type that represents a normal row in the database that is compressed when marshaled
/// </summary>
public class PyPackedRow : PyDataType
{
    private readonly Dictionary <string, PyDataType> mValues = new Dictionary <string, PyDataType> ();
    /// <summary>
    /// The header for this PyPackedRow
    /// </summary>
    public DBRowDescriptor Header { get; }

    public virtual PyDataType this [string key]
    {
        get => this.mValues [key];
        set => this.mValues [key] = value;
    }

    public PyPackedRow (DBRowDescriptor descriptor)
    {
        Header = descriptor;
    }

    public PyPackedRow (DBRowDescriptor descriptor, Dictionary <string, PyDataType> values)
    {
        Header = descriptor;

        if (values.Count != Header.Columns.Count)
            throw new Exception ("PackedRow must have the same value count as DBRowDescriptor");

        this.mValues = values;
    }

    public override int GetHashCode ()
    {
        // a similar implementation to PyTuple to make my life easy
        int length      = this.mValues.Count;
        int mult        = 1000003;
        int mul2        = 1000005;
        int currentHash = 0x63521485;

        IOrderedEnumerable <DBRowDescriptor.Column> enumerator = Header.Columns.OrderByDescending (c => Utils.GetTypeBits (c.Type));

        foreach (DBRowDescriptor.Column column in enumerator)
        {
            PyDataType value = this [column.Name];

            mult += 52368 + length + length; // shift the multiplier
            int elementHash = column.Name?.GetHashCode () ?? PyNone.HASH_VALUE * mult;
            mul2        += 58212 + length + length; // shift the multiplier
            elementHash ^= (value?.GetHashCode () ?? PyNone.HASH_VALUE * mul2) << 3;
            currentHash =  (currentHash ^ elementHash) * mult;
            mult        += 82520 + length + length; // shift the multiplier
        }

        currentHash += +97531;

        return (Header?.GetHashCode () ?? 0) ^ (currentHash << 8) ^ 0x56478512;
    }

    /// <summary>
    /// Simple helper method that creates the correct PackedRow data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static PyPackedRow FromMySqlDataReader (MySqlDataReader reader, DBRowDescriptor descriptor)
    {
        PyPackedRow row = new PyPackedRow (descriptor);

        int i = 0;

        foreach (DBRowDescriptor.Column column in descriptor.Columns)
            row [column.Name] = IDatabaseConnection.ObjectFromColumn (reader, column.Type, i++);

        return row;
    }

    public static PyPackedRow FromMySqlDataReader (MySqlDataReader reader, CRowset rowset)
    {
        return FromMySqlDataReader (reader, rowset.Header);
    }
}