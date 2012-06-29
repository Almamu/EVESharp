using System.IO;

namespace Marshal
{

    internal static class Extensions
    {
        public static void WriteOpcode(this BinaryWriter w, MarshalOpcode op)
        {
                w.Write((byte) op);
        }
    }

}