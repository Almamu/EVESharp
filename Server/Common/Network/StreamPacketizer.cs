using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Network
{
    public class StreamPacketizer
    {
        private Queue<byte[]> mOut;
        private List<byte> input;

        public StreamPacketizer()
        {
            mOut = new Queue<byte[]>();
            input = new List<byte>();
        }

        // Lots of thread safe code here to prevent data corruption
        public void QueuePackets(byte[] data, int bytes)
        {
            lock (input)
            {
                byte[] tmp = new byte[bytes];

                Array.Copy(data, tmp, bytes);

                input.AddRange(tmp);
            }
        }

        public int ProcessPackets()
        {
            try
            {
                // Get the packet size:
                int cur = 0;
                int packets = 0;
                byte[] tmp = input.ToArray();

                while (cur != tmp.Length)
                {
                    int size = BitConverter.ToInt32(tmp, cur);

                    cur += 4;

                    if (size + cur > tmp.Length) // The packet is longer than we have here
                    {
                        cur -= 4; // Go back to the size bytes

                        // Get rid off the data before it
                        input.RemoveRange(0, cur);

                        return packets + mOut.Count;
                    }

                    byte[] packet = new byte[size];

                    Array.Copy(tmp, cur, packet, 0, size);
                    cur += size;

                    mOut.Enqueue(packet);

                    packets++;
                }

                // If we are here all the packets were parsed correctly
                input.RemoveRange(0, input.Count);

                return packets + mOut.Count; // We need to let the user know that there are packets in the queue too
            }
            catch (Exception)
            {
                // The packets are malformed
                return 0;
            }
        }

        public byte[] PopItem()
        {
            lock (mOut)
            {
                if (mOut.Count > 0)
                    return mOut.Dequeue();

                return null;
            }
        }

        public void ClearPackets()
        {
            lock(mOut)
            {
                lock (input)
                {
                    mOut = new Queue<byte[]>();
                    input = new List<byte>();
                }
            }
        }
    }
}
