using System.IO;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Extra database utilities that are used in more than one place
/// </summary>
public static class Utils
{
    /// <summary>
    /// Indicates the amount of bits that a given field-type uses in the zero-compressed part of a PyPackedRow
    /// </summary>
    /// <param name="type">The type to get the bit-size for</param>
    /// <returns>The amount of bits the type uses</returns>
    /// <exception cref="InvalidDataException">If an unknown type was specified</exception>
    public static int GetTypeBits (FieldType type)
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
                throw new InvalidDataException ("Invalid FieldType");
        }
    }
}