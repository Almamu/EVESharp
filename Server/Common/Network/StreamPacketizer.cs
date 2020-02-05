using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.Network
{
    public class StreamPacketizer
    { 
        private MemoryStream mInputStream = new MemoryStream();
        private BinaryWriter mInputWriter = null;
        private BinaryReader mInputReader = null;
        private Queue<byte[]> mOutputQueue = new Queue<byte[]>();

        public int PacketCount
        {
            get { return this.mOutputQueue.Count; }
            private set {}
        }
        public StreamPacketizer()
        {
            this.mInputReader = new BinaryReader(this.mInputStream);
            this.mInputWriter = new BinaryWriter(this.mInputStream);
        }

        // Lots of thread safe code here to prevent data corruption
        public void QueuePackets(byte[] data, int bytes)
        {
            lock (this.mInputStream)
            {
                // go to the end of the memory stream and write the data
                this.mInputWriter.Seek(0, SeekOrigin.End);
                this.mInputWriter.Write(data, 0, bytes);
                // return to the beginning of the stream
                this.mInputWriter.Seek(0, SeekOrigin.Begin);
            }
        }

        public int ProcessPackets()
        {
            lock (this.mInputStream)
            {
                while (this.mInputStream.Position != this.mInputStream.Length)
                {
                    // get size flag
                    int size = this.mInputReader.ReadInt32();

                    // ensure this packet is completely received
                    if ((size + this.mInputStream.Position) > this.mInputStream.Length)
                        return this.mOutputQueue.Count;
                    
                    lock(this.mOutputQueue)
                        // read the packet's data and queue it on the packets queue
                        this.mOutputQueue.Enqueue(this.mInputReader.ReadBytes(size));
                    
                    // remove the packet from the stream
                    byte[] currentBuffer = this.mInputStream.GetBuffer();
                    Buffer.BlockCopy(
                        currentBuffer,
                        (int) this.mInputStream.Position,
                        currentBuffer,
                        0,
                        (int) (this.mInputStream.Length - this.mInputStream.Position)
                    );
                    this.mInputStream.SetLength(this.mInputStream.Length - this.mInputStream.Position);
                    // seek to the beginning of the stream so the next packet can be handled
                    this.mInputStream.Seek(0, SeekOrigin.Begin);
                }

                return this.mOutputQueue.Count;   
            }
        }

        public byte[] PopItem()
        {
            lock (this.mOutputQueue)
                return this.mOutputQueue.Dequeue();
        }
    }
}
