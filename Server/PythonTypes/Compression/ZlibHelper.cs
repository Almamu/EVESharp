using System.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace PythonTypes.Compression
{
    public class ZlibHelper
    {
        public static ZInputStream DecompressStream(Stream stream)
        {
            return new ZInputStream(stream);
        }
        
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