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
    }

}