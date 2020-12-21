using System;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Extra database utilities that are used in more than one place
    /// </summary>
    public class Utils
    {
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
            Type type = reader.GetFieldType(index);
            PyDataType data = null;
            bool isnull = reader.IsDBNull(index);

            // null values should be null
            if (isnull == true)
                return null;
            
            if (type == typeof(string))
                data = new PyString(reader.GetString(index), true);
            else if (type == typeof(ulong))
                data = (long) reader.GetUInt64(index);
            else if (type == typeof(long))
                data = reader.GetInt64(index);
            else if (type == typeof(uint))
                data = (int) reader.GetUInt32(index);
            else if (type == typeof(int))
                data = reader.GetInt32(index);
            else if (type == typeof(ushort))
                data = (short) reader.GetUInt16(index);
            else if (type == typeof(short))
                data = reader.GetInt16(index);
            else if (type == typeof(byte))
                data = reader.GetByte(index);
            else if (type == typeof(sbyte))
                data = reader.GetSByte(index);
            else if (type == typeof(byte[]))
                data = (byte[]) reader.GetValue(index);
            else if (type == typeof(float))
                data = reader.GetFloat(index);
            else if (type == typeof(double))
                data = reader.GetDouble(index);
            else if (type == typeof(bool))
                data = reader.GetBoolean(index);
            else
                throw new InvalidDataException($"Unknown data type {type}");

            return data;
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