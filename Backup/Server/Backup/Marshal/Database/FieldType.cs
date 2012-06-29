using System;

namespace Marshal.Database
{

    public enum FieldType
    {
        Empty = 0x00,
        Null = 0x01,
        I2 = 0x02,
        I4 = 0x03,
        R4 = 0x04,
        R8 = 0x05,

        /// <summary>
        /// Currency
        /// </summary>
        CY = 0x06,

        Error = 0x0A,
        Bool = 0x0B,
        I1 = 0x10,
        UI1 = 0x11,
        UI2 = 0x12,
        UI4 = 0x13,
        I8 = 0x14,
        UI8 = 0x15,

        /// <summary>
        /// Win32 FILETIME 64 bit timestamp
        /// </summary>
        FileTime = 0x40,

        Bytes = 0x80,
        Str = 0x81,
        WStr = 0x82,
    }

    public static class FieldTypeHelper
    {
        public static int GetTypeBytes(FieldType type)
        {
            return ((GetTypeBits(type) + 7) >> 3);
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