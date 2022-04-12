using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

public interface IDatabaseConnection
{
    /// <summary>
    /// Obtains important metadata used in the database functions
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="headers">Where to put the headers</param>
    /// <param name="fieldTypes">Where to put the field types</param>
    public void GetDatabaseHeaders (MySqlDataReader reader, out PyList <PyString> headers, out FieldType [] fieldTypes);

    /// <summary>
    /// Obtains the list of types for all the columns in this MySqlDataReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns></returns>
    public FieldType [] GetFieldTypes (MySqlDataReader reader);

    /// <summary>
    /// Obtains the current field type off a MySqlDataReader for the given column
    /// </summary>
    /// <param name="reader">The data reader to use</param>
    /// <param name="index">The column to get the type from</param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">If the type is not supported</exception>
    public FieldType GetFieldType (MySqlDataReader reader, int index);

    /// <summary>
    /// Creates a PyDataType of the given column (specified by <paramref name="index"/>) based off the given
    /// MySqlDataReader
    /// </summary>
    /// <param name="reader">Reader to get the data from</param>
    /// <param name="type">The type of the field to convert</param>
    /// <param name="index">Column of the current result read in the MySqlDataReader to create the PyDataType</param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">If any error was found during the creation of the PyDataType</exception>
    public static PyDataType ObjectFromColumn (MySqlDataReader reader, FieldType type, int index)
    {
        // null values should be null
        if (reader.IsDBNull (index))
            return null;

        switch (type)
        {
            case FieldType.I2:    return reader.GetInt16 (index);
            case FieldType.UI2:   return reader.GetUInt16 (index);
            case FieldType.I4:    return reader.GetInt32 (index);
            case FieldType.UI4:   return reader.GetUInt32 (index);
            case FieldType.R4:    return reader.GetFloat (index);
            case FieldType.R8:    return reader.GetFieldType (index) == typeof (decimal) ? (double) reader.GetDecimal (index) : reader.GetDouble (index);
            case FieldType.Bool:  return reader.GetBoolean (index);
            case FieldType.I1:    return reader.GetSByte (index);
            case FieldType.UI1:   return reader.GetByte (index);
            case FieldType.UI8:   return reader.GetUInt64 (index);
            case FieldType.Bytes: return (byte []) reader.GetValue (index);
            case FieldType.I8:    return reader.GetInt64 (index);
            case FieldType.WStr:  return new PyString (reader.GetString (index), true);
            case FieldType.Str:   return new PyString (reader.GetString (index));
            default:
                throw new InvalidDataException ($"Unknown data type {type}");
        }
    }
}