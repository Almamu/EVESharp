namespace PythonTypes.Types.Database
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
}