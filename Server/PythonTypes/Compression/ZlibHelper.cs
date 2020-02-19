using System.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace PythonTypes.Compression
{
    /// <summary>
    /// Small utilities to work with Zlib-compressed data
    /// </summary>
    public class ZlibHelper
    {
        /// <summary>
        /// Creates a stream to decompress the data in another <paramref name="stream" />
        /// </summary>
        /// <param name="stream">The stream with the compressed data</param>
        /// <returns></returns>
        public static ZInputStream DecompressStream(Stream stream)
        {
            return new ZInputStream(stream);
        }
        
        /// <summary>
        /// Compresses the given <paramref name="input" />
        /// </summary>
        /// <param name="input">The data to compress</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] input)
        {
            var sourceStream = new MemoryStream();
            var stream = new ZOutputStream(sourceStream, -1);
            // write zlib header
            stream.Write(input);
            stream.Finish();
            
            return sourceStream.GetBuffer();
        }
    }
}