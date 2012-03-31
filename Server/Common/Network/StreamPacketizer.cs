using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Network
{
    public class StreamPacketizer
    {
        private Queue<byte[]> mOut;

        public StreamPacketizer()
        {
            mOut = new Queue<byte[]>();
        }

        public int QueuePackets(byte[] data)
        {
            // Get the packet size:
            int cur = 0;
            int packets = 0;

            for (; cur != data.Length; )
            {
                int count = BitConverter.ToInt32(data, cur);
                cur += 4;
                byte[] packet = new byte[count];
                Array.Copy(data, cur, packet, 0, count);
                cur += count;
                mOut.Enqueue(packet);
                packets += 1;
            }

            return packets;
        }

        public byte[] PopItem()
        {
            if( mOut.Count > 0 )
                return mOut.Dequeue();

            return null;
        }
    }
}
