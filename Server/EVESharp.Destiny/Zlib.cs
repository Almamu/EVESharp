using System.IO;
using System.IO.Compression;

namespace EVESharp.Destiny;

public static class Zlib
{
    public static byte[] Decompress(byte[] input)
    {
        // two bytes shaved off (zlib header)
        MemoryStream sourceStream = new MemoryStream(input, 2, input.Length - 2);
        DeflateStream stream       = new DeflateStream(sourceStream, CompressionMode.Decompress);
        return stream.ReadAllBytes();
    }

    public static byte[] Compress(byte[] input)
    {
        MemoryStream  stream         = new MemoryStream(input);
        DeflateStream compressStream = new DeflateStream(stream, CompressionMode.Compress);
        return compressStream.ReadAllBytes();
    }
}