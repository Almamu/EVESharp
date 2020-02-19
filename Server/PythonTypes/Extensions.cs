using System.IO;

namespace PythonTypes
{
    internal static class Extensions
    {
        /// <summary>
        /// Writes a marshal opcode to the stream
        /// </summary>
        /// <param name="w">The binary writer in use</param>
        /// <param name="op">The opcode to write</param>
        public static void WriteOpcode(this BinaryWriter w, Marshal.Opcode op)
        {
            w.Write((byte) op);
        }
        
        /// <summary>
        /// Reads an extended size indicator from the stream. These indicators can be 1 or 5 bytes wide.
        /// If the first byte is 0xFF the next 4 bytes are read as those indicate the actual value
        /// If not, only one byte is read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The read size indicator</returns>
        public static uint ReadSizeEx(this BinaryReader reader)
        {
            var len = reader.ReadByte();
            if (len == 0xFF)
                return reader.ReadUInt32();
            return len;
        }

        /// <summary>
        /// Writes an extended size indicator to the stream. These indicatos can be 1 or 5 bytes wide.
        /// If the <paramref name="len"/> parameter is less than 255 just one byte is written
        /// If not, one 0xFF byte is written and then 4 bytes with the actual int value are written
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="len">The size indicator to write</param>
        public static void WriteSizeEx(this BinaryWriter writer, uint len)
        {
            if (len < 0xFF)
            {
                writer.Write((byte)len);
            }
            else
            {
                writer.Write((byte)0xFF);
                writer.Write(len);
            }
        }

        /// <summary>
        /// <see cref="WriteSizeEx(System.IO.BinaryWriter,uint)"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="len"></param>
        public static void WriteSizeEx(this BinaryWriter writer, int len)
        {
            WriteSizeEx(writer, (uint)len);
        }
    }
}