using System.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace EVESharp.Common.Compression;

/// <summary>
/// Small utilities to work with Zlib-compressed data
/// </summary>
public static class ZlibHelper
{
    /// <summary>
    /// Creates a stream to decompress the data in another <paramref name="stream" />
    /// </summary>
    /// <param name="stream">The stream with the compressed data</param>
    /// <returns></returns>
    public static ZInputStream DecompressStream (Stream stream)
    {
        return new ZInputStream (stream);
    }

    /// <summary>
    /// Compresses the given <paramref name="input" />
    /// </summary>
    /// <param name="input">The data to compress</param>
    /// <returns></returns>
    public static byte [] Compress (byte [] input)
    {
        MemoryStream  sourceStream = new MemoryStream ();
        ZOutputStream stream       = new ZOutputStream (sourceStream, 1);
        // write zlib header
        stream.Write (input);
        stream.Finish ();

        return sourceStream.GetBuffer ();
    }
}