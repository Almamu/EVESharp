namespace EVESharp.Common;

/// <summary>
/// Small utility class to create an hexadecimal dump of any kind of byte array
///
/// Extracted from this stackoverflow post
/// https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
/// </summary>
public static class HexDump
{
    private static readonly uint [] Lookup32 = CreateLookup32 ();

    private static uint [] CreateLookup32 ()
    {
        uint [] result = new uint[256];

        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString ("X2");
            result [i] = s [0] + ((uint) s [1] << 16);
        }

        return result;
    }

    public static string ByteArrayToHexViaLookup32 (byte [] bytes)
    {
        uint [] lookup32 = Lookup32;
        char [] result   = new char[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            uint val = lookup32 [bytes [i]];
            result [2 * i]     = (char) val;
            result [2 * i + 1] = (char) (val >> 16);
        }

        return new string (result);
    }
}