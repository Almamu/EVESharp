using System;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Extra database utilities that are used in more than one place
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Obtains the current field type off a MySqlDataReader for the given column
        /// </summary>
        /// <param name="reader">The data reader to use</param>
        /// <param name="index">The column to get the type from</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">If the type is not supported</exception>
        public static FieldType GetFieldType(MySqlDataReader reader, int index)
        {
            Type type = reader.GetFieldType(index);
            
            if (type == typeof(string)) return FieldType.WStr;
            if (type == typeof(ulong)) return FieldType.UI8;
            if (type == typeof(long)) return FieldType.I8;
            if (type == typeof(uint)) return FieldType.UI4;
            if (type == typeof(int)) return FieldType.I4;
            if (type == typeof(ushort)) return FieldType.UI2;
            if (type == typeof(short)) return FieldType.I2;
            if (type == typeof(sbyte)) return FieldType.I1;
            if (type == typeof(byte)) return FieldType.UI1;
            if (type == typeof(byte[])) return FieldType.Bytes;
            if (type == typeof(double) || type == typeof(decimal)) return FieldType.R8;
            if (type == typeof(float)) return FieldType.R4;
            if (type == typeof(bool)) return FieldType.Bool;

            throw new InvalidDataException($"Unknown field type {type}");
        }
        
        /// <summary>
        /// Creates a PyDataType of the given column (specified by <paramref name="index"/>) based off the given
        /// MySqlDataReader
        /// </summary>
        /// <param name="reader">Reader to get the data from</param>
        /// <param name="index">Column of the current result read in the MySqlDataReader to create the PyDataType</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">If any error was found during the creation of the PyDataType</exception>
        public static PyDataType ObjectFromColumn(MySqlDataReader reader, int index)
        {
            FieldType type = GetFieldType(reader, index);
            
            // null values should be null
            if (reader.IsDBNull(index) == true)
                return null;
            
            switch (type)
            {
                case FieldType.I2: return reader.GetInt16(index);
                case FieldType.UI2: return reader.GetUInt16(index);
                case FieldType.I4: return reader.GetInt32(index);
                case FieldType.UI4: return reader.GetUInt32(index);
                case FieldType.R4: return reader.GetFloat(index);
                case FieldType.R8: return (double) reader.GetValue(index);
                case FieldType.Bool: return reader.GetBoolean(index);
                case FieldType.I1: return reader.GetSByte(index);
                case FieldType.UI1: return reader.GetByte(index);
                case FieldType.UI8: return reader.GetUInt64(index);
                case FieldType.Bytes: return (byte[]) reader.GetValue(index);
                case FieldType.I8: return reader.GetInt64(index);
                case FieldType.WStr: return new PyString(reader.GetString(index), true);
                default:
                    throw new InvalidDataException($"Unknown data type {type}");
            }
        }

        /// <summary>
        /// Indicates the amount of bits that a given field-type uses in the zero-compressed part of a PyPackedRow
        /// </summary>
        /// <param name="type">The type to get the bit-size for</param>
        /// <returns>The amount of bits the type uses</returns>
        /// <exception cref="InvalidDataException">If an unknown type was specified</exception>
        public static int GetTypeBits(FieldType type)
        {
            switch (type)
            {
                case FieldType.I8:
                case FieldType.UI8:
                case FieldType.R8:
                case FieldType.CY:
                case FieldType.FileTime:
                    return 64;

                case FieldType.I4:
                case FieldType.UI4:
                case FieldType.R4:
                    return 32;

                case FieldType.I2:
                case FieldType.UI2:
                    return 16;

                case FieldType.I1:
                case FieldType.UI1:
                    return 8;

                case FieldType.Bool:
                    return 1;

                case FieldType.Bytes:
                case FieldType.Str:
                case FieldType.WStr:
                    // handled differently
                    return 0;

                default:
                    throw new InvalidDataException("Invalid FieldType");
            }
        }
    }
}