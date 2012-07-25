using System.Runtime.InteropServices;

namespace Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        /// <summary>
        /// seen 0x00, 0x01
        /// affects incrementally updating the ballpark or something
        /// </summary>
        public byte PacketType;
        
        /// <summary>
        /// also occours in the marshal destiny updates, unk
        /// </summary>
        public int Stamp;
    }
}