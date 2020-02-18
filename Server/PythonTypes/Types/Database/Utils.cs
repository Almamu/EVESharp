using System;
using System.IO;
using PythonTypes.Types.Database;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class Utils
    {
        public static PyDataType ObjectFromColumn(MySqlDataReader reader, int index)
        {
            Type type = reader.GetFieldType(index);
            PyDataType data = null;
            bool isnull = reader.IsDBNull(index);

            if(type == typeof(String))
                data = new PyString((isnull) ? "" : reader.GetString(index), true);
            else if (type == typeof(UInt64))
                data = (isnull) ? 0 : reader.GetUInt64(index);
            else if (type == typeof(Int64))
                data = (isnull) ? 0 : reader.GetInt64(index);
            else if (type == typeof(UInt32))
                data = (isnull) ? 0 : reader.GetUInt32(index);
            else if (type == typeof(Int32))
                data = (isnull) ? 0 : reader.GetInt32(index);
            else if(type == typeof(UInt16))
                data = (isnull) ? 0 : reader.GetUInt16(index);
            else if (type == typeof(Int16))
                data = (isnull) ? 0 : reader.GetInt16(index);
            else if (type == typeof(Byte))
                data = (isnull) ? 0 : reader.GetByte(index);
            else if (type == typeof(SByte))
                data = (isnull) ? 0 : reader.GetSByte(index);
            else if (type == typeof(Byte[]))
                data = (isnull) ? new byte[0] : (byte[]) reader.GetValue(index);
            else if (type == typeof(float))
                data = (isnull) ? 0 : reader.GetFloat(index);
            else if (type == typeof(Double))
                data = (isnull) ? 0 : reader.GetDouble(index);
            else if (type == typeof(Boolean))
                data = (!isnull) && reader.GetBoolean(index);
            else
                throw new InvalidDataException($"Unknown data type {type}");

            return data;
        }
        
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
                    throw new Exception("Invalid FieldType");
            }
        }
    }
}