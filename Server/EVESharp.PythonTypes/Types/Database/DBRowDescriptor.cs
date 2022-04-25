using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with DBRowDescriptors used by the EVE Online client.
///
/// DBRowDescriptors are a Python representation of a table's header (column names and types)
/// </summary>
public class DBRowDescriptor
{
    /// <summary>
    /// Name of the PyObjects that represent a DBRowDescriptor
    /// </summary>
    private const string TYPE_NAME = "blue.DBRowDescriptor";

    public List <Column> Columns { get; }

    public DBRowDescriptor ()
    {
        Columns = new List <Column> ();
    }

    public override int GetHashCode ()
    {
        int length      = Columns.Count;
        int mult        = 100005;
        int currentHash = 0x24157585;

        foreach (Column column in Columns)
        {
            currentHash =  (currentHash ^ column.GetHashCode ()) * mult;
            mult        += 52418 + length + length;
        }

        return currentHash;
    }

    public static implicit operator PyObject (DBRowDescriptor descriptor)
    {
        PyTuple args  = new PyTuple (descriptor.Columns.Count);
        int     index = 0;

        foreach (Column col in descriptor.Columns)
            args [index++] = col;

        args = new PyTuple (1) {[0] = args};

        // build the args tuple
        return new PyObject (
            false,
            new PyTuple (2)
            {
                [0] = new PyToken (TYPE_NAME),
                [1] = args
            }
        );
    }

    public static implicit operator DBRowDescriptor (PyObject descriptor)
    {
        if (descriptor.Header.Count != 2)
            throw new Exception ($"{TYPE_NAME} does not contain 2 elements in the header");
        if (descriptor.Header [0] is PyToken == false || descriptor.Header [0] as PyToken != TYPE_NAME)
            throw new Exception ($"Expected PyObject of type {TYPE_NAME}");
        if (descriptor.Header [1] is PyTuple == false)
            throw new Exception ($"{TYPE_NAME} does not contain an args tuple");

        PyTuple args = descriptor.Header [1] as PyTuple;

        DBRowDescriptor output = new DBRowDescriptor ();

        foreach (PyTuple tuple in args [0] as PyTuple)
            output.Columns.Add (tuple);

        return output;
    }

    public static implicit operator DBRowDescriptor (PyDataType descriptor)
    {
        return descriptor as PyObject;
    }

    /// <summary>
    /// Creates a new DBRowDescriptor based off a specific result set from a MySqlDataReader
    /// </summary>
    /// <param name="connection">The database connection used</param>
    /// <param name="reader">The MySqlDataReader to use when creating the DBRowDescriptor</param>
    /// <returns>Instance of a new DBRowDescriptor</returns>
    /// <exception cref="InvalidDataException">If any error was found on the creation</exception>
    public static DBRowDescriptor FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        DBRowDescriptor descriptor = new DBRowDescriptor ();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            FieldType fieldType = connection.GetFieldType (reader, i);

            descriptor.Columns.Add (new Column (reader.GetName (i), fieldType));
        }

        return descriptor;
    }

    /// <summary>
    /// Column representation for the DBRowDescriptors
    /// </summary>
    public class Column
    {
        public string    Name { get; }
        public FieldType Type { get; }

        public Column (string name, FieldType type)
        {
            Name = name;
            Type = type;
        }

        public Column (string name, int type)
        {
            Name = name;
            Type = (FieldType) type;
        }

        public override int GetHashCode ()
        {
            return Name.GetHashCode () ^ Type.GetHashCode ();
        }

        public static implicit operator PyDataType (Column column)
        {
            return new PyTuple (2)
            {
                [0] = column.Name,
                [1] = (int) column.Type
            };
        }

        public static implicit operator Column (PyDataType column)
        {
            PyTuple tuple = column as PyTuple;

            return new Column (
                tuple [0] as PyString,
                tuple [1] as PyInteger
            );
        }
    }
}