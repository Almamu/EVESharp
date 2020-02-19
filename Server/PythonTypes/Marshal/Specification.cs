namespace PythonTypes.Marshal
{
    public static class Specification
    {
        /// <summary>
        /// Magic value used by the marshal package to identify valid marshal streams
        /// </summary>
        public const byte MarshalHeader = (byte) '~';

        /// <summary>
        /// Not an actual header, but it's the compressed-value of the MarshalHeader
        /// and as it should be on the beginning of the marshal will always be the same
        /// </summary>
        public const byte ZlibHeader = 0x78;

        /// <summary>
        /// Bit mask to extract the object saving flag, used by Unmarshal to properly parse save lists
        /// </summary>
        public const byte SaveMask = 0x40;

        /// <summary>
        /// Bit mask used to extract the actual opcode from a python type in a marshal stream
        /// </summary>
        public const byte OpcodeMask = 0x3F;
    }
}