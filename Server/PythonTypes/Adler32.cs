namespace PythonTypes
{
    public static class Adler32
    {
        private const int ModuloPrime = 65521;

        public static uint Checksum(byte[] data)
        {
            uint checksum = 1;
            uint s1 = checksum & 0xFFFF;
            uint s2 = checksum >> 16;

            int len = data.Length;
            int i = 0;
            while (len > 0)
            {
                int maxDefer = 3800;
                if (maxDefer > len)
                    maxDefer = len;
                len -= maxDefer;
                while (--maxDefer >= 0)
                {
                    s1 = s1 + (uint) (data[i++] & 0xFF);
                    s2 = s2 + s1;
                }

                s1 %= ModuloPrime;
                s2 %= ModuloPrime;
            }

            checksum = (s2 << 16) | s1;
            return checksum;
        }
    }
}