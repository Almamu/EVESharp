using System.IO;

namespace PythonTypes
{
    internal static class Extensions
    {
        public static void WriteOpcode(this BinaryWriter w, Marshal.Opcode op)
        {
            w.Write((byte) op);
        }
        
        public static uint ReadSizeEx(this BinaryReader reader)
        {
            var len = reader.ReadByte();
            if (len == 0xFF)
                return reader.ReadUInt32();
            return len;
        }

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

        public static void WriteSizeEx(this BinaryWriter writer, int len)
        {
            WriteSizeEx(writer, (uint)len);
        }
    }
}