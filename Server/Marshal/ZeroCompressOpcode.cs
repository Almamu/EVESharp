namespace Marshal
{
    public class ZeroCompressOpcode
    {
        public byte FirstLength;
        public bool FirstIsZero;
        public byte SecondLength;
        public bool SecondIsZero;

        public ZeroCompressOpcode(byte source)
        {
            FirstLength = (byte)(source & 0x07);
            FirstIsZero = (source & 0x08) > 0;
            SecondLength = (byte) ((source & 0x70) >> 4);
            SecondIsZero = (source & 0x80) > 0;
        }

        public byte Value
        {
            get
            {
                byte value = 0;

                value |= (byte)(FirstLength & 0x07);
                value |= (byte)((FirstIsZero == true) ? 0x08 : 0x00);
                value |= (byte)((SecondLength & 0x07) << 4);
                value |= (byte)((SecondIsZero == true) ? 0x80 : 0x00);

                return value;
            }

            set
            {
                FirstLength = (byte)(value & 0x07);
                FirstIsZero = (value & 0x08) > 0;
                SecondLength = (byte)((value >> 4) & 0x07);
                SecondIsZero = (value & 0x80) > 0;
            }
        }
    }

}