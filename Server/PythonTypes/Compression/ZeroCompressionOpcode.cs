namespace PythonTypes.Compression
{
    public struct ZeroCompressionOpcode
    {
        public byte FirstLength;
        public bool FirstIsZero;
        public byte SecondLength;
        public bool SecondIsZero;

        public ZeroCompressionOpcode(byte firstLength, bool firstIsZero, byte secondLength, bool secondIsZero)
        {
            this.FirstLength = firstLength;
            this.FirstIsZero = firstIsZero;
            this.SecondLength = secondLength;
            this.SecondIsZero = secondIsZero;
        }

        public static implicit operator byte(ZeroCompressionOpcode opcode)
        {
            byte value = 0;
            
            value |= (byte)(opcode.FirstLength & 0x07);
            value |= (byte)((opcode.FirstIsZero == true) ? 0x08 : 0x00);
            value |= (byte)((opcode.SecondLength & 0x07) << 4);
            value |= (byte)((opcode.SecondIsZero == true) ? 0x80 : 0x00);

            return value;
        }

        public static implicit operator ZeroCompressionOpcode(byte source)
        {
            return new ZeroCompressionOpcode(
                (byte)(source & 0x07),
                (source & 0x08) == 0x08,
                (byte)((source & 0x70) >> 4),
                (source & 0x80) == 0x80
            );
        }
    }
}