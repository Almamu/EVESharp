using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common.Logging;
using Common.Packets;
using PythonTypes;
using PythonTypes.Compression;
using PythonTypes.Marshal;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Common.Network
{
    public class EVEClientSocket : EVESocket
    {
        private Semaphore mSendingSemaphore = new Semaphore(1, 1);
        private Semaphore mReceivingSemaphore = new Semaphore(1, 1);
        private AsyncCallback mSendCallback = null;
        private AsyncCallback mReceiveCallback = null;
        private Queue<byte[]> mOutputQueue = new Queue<byte[]>();
        private StreamPacketizer mPacketizer = new StreamPacketizer();
        private Action<PyDataType> mPacketReceiveCallback = null;
        #if DEBUG
        private Channel mPacketLog = null;
        #endif
        public Channel Log { get; set; }
        
        public EVEClientSocket(Socket socket, Channel logChannel) : base(socket)
        {
            this.Log = logChannel;
            
            // take into account network debugging for developers
#if DEBUG
            this.mPacketLog = this.Log.Logger.CreateLogChannel("NetworkDebug", true);
#endif
            
            // setup async callback handlers
            this.SetupCallbacks();
            // start the receiving callbacks
            this.BeginReceive(new byte[64 * 1024], this.mReceiveCallback);
        }

        public EVEClientSocket(Channel logChannel) : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            this.Log = logChannel;
            
            // take into account network debugging for developers
#if DEBUG
            this.mPacketLog = this.Log.Logger.CreateLogChannel("NetworkDebug", true);
#endif
            
            this.SetupCallbacks();
        }

        private void SetupCallbacks()
        {
            // setup async callback handlers
            this.mSendCallback = new AsyncCallback(SendCallback);
            this.mReceiveCallback = new AsyncCallback(ReceiveCallback);
        }

        public void Connect(string address, int port)
        {
            IPAddress ip;

            // check if the address is an IP, if not, query the DNS servers to resolve it
            if (IPAddress.TryParse(address, out ip) == false)
            {
                IPHostEntry entry = Dns.GetHostEntry(address);
                int i = 0;

                do
                {
                    ip = entry.AddressList[i++];
                } while (ip.AddressFamily != AddressFamily.InterNetwork && i < entry.AddressList.Length);
            }

            // connect to the server
            this.Socket.Connect(new IPEndPoint(ip, port));
            // start the receiving callbacks
            this.BeginReceive(new byte[64 * 1024], this.mReceiveCallback);
        }

        public void SetReceiveCallback(Action<PyDataType> callback)
        {
            bool shouldFlush = this.mPacketReceiveCallback == null;
            
            this.mPacketReceiveCallback = callback;
            // check if there is any packet queued and not handled
            if(shouldFlush)
                this.FlushReceivingQueue();
        }
        
        private void FlushReceivingQueue()
        {
            if (this.mReceivingSemaphore.WaitOne(0) == true)
            {
                // handle required packets
                while (this.mPacketizer.PacketCount > 0 && this.mPacketReceiveCallback != null)
                {
                    try
                    {
                        // unmarshal the packet
                        PyDataType packet = Unmarshal.ReadFromByteArray(this.mPacketizer.PopItem());
#if DEBUG
                        this.mPacketLog.Trace(PrettyPrinter.Print(packet));
#endif
                        // and invoke the callback for the packet handling if it is present
                        this.mPacketReceiveCallback.Invoke(packet);
                    }
                    catch (Exception e)
                    {
                        this.HandleException(e);
                    }
                }
                
                // finally free the receiving semaphore
                this.mReceivingSemaphore.Release();
            }
            
            // semaphore not acquired, there's something already sending data, so we're sure the data will get there eventually
        }

        private void BeginReceive(byte[] buffer, AsyncCallback callback)
        {
            try
            {
                this.Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, new ReceiveCallbackState(buffer));
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        private int EndReceive(IAsyncResult asyncResult)
        {
            try
            {
                return this.Socket.EndReceive(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleException(e);
                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            ReceiveCallbackState state = asyncResult.AsyncState as ReceiveCallbackState;

            try
            {
                state.Received = this.EndReceive(asyncResult);
            }
            catch (Exception e)
            {
                // an exception here means the connection closed
                return;
            }
            
            // queue the packets and process them
            this.mPacketizer.QueuePackets(state.Buffer, state.Received);
            this.mPacketizer.ProcessPackets();
            
            // if there is any pending to be processed pop it and call the receive callback
            this.FlushReceivingQueue();
            
            // begin receiving again
            this.BeginReceive(state.Buffer, this.mReceiveCallback);
        }

        /// <summary>
        /// Sends a PyObject through this socket
        /// </summary>
        /// <param name="packet">The packet data to send</param>
        public void Send(PyDataType packet)
        {
#if DEBUG
            this.mPacketLog.Trace(PrettyPrinter.Print(packet));
#endif
            
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            // marshal the packet first
            byte[] encodedPacket = PythonTypes.Marshal.Marshal.ToByteArray(packet);
            
            // compress the packet if it exceeds the maximum size
            if (encodedPacket.Length > Constants.Network.MAX_PACKET_SIZE)
                encodedPacket = ZlibHelper.Compress(encodedPacket);
            
            // write the size to the stream
            writer.Write(encodedPacket.Length);
            // finally copy the packet buffer
            writer.Write(encodedPacket);
            // after processing the whole packet queue the actual data
            this.Send(stream.ToArray());
        }

        /// <summary>
        /// Sends raw bytes through this socket
        /// </summary>
        /// <param name="buffer">The bytes to send</param>
        public void Send(byte[] buffer)
        {
            // add the buffer to the output queue when the queue is not in use
            lock(this.mOutputQueue)
                this.mOutputQueue.Enqueue(buffer);
            
            this.FlushSendingQueue();
        }

        private void FlushSendingQueue(bool shouldCheckForSemaphore = true)
        {
            if (shouldCheckForSemaphore == false || this.mSendingSemaphore.WaitOne(0) == true)
                // semaphore acquired, start the send callback
                this.BeginSend();
            
            // semaphore not acquired, the send callbacks are already running
        }

        private void BeginSend()
        {
            byte[] output;

            // get oldest packet
            lock (this.mOutputQueue)
            {
                if (this.mOutputQueue.Count == 0)
                {
                    // no packet to send, release the semaphore
                    this.mSendingSemaphore.Release();
                    return;
                }
                
                output = this.mOutputQueue.Dequeue();   
            }

            try
            {
                // begin the sending callback
                this.Socket.BeginSend(output, 0, output.Length, SocketFlags.None, mSendCallback,
                    new SendCallbackState(output));
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            SendCallbackState state = ar.AsyncState as SendCallbackState;
            state.Sent += this.Socket.EndSend(ar);
            
            // make sure the packet was sent completely
            if (state.Sent != state.Buffer.Length)
            {
                try
                {
                    this.Socket.BeginSend(state.Buffer, state.Sent, state.Buffer.Length - state.Sent,
                        SocketFlags.None, mSendCallback, state);
                }
                catch (Exception e)
                {
                    this.HandleException(e);
                }
                return;
            }
            
            // packet was completely sent, free the semaphore and try to send the next packet (if any)
            this.FlushSendingQueue(false);
        }

        public override void GracefulDisconnect()
        {
            // wait for semaphore to end sending data
            this.mSendingSemaphore.WaitOne();
            // there shouldn't be anything left
            // keep the semaphore active so no one can send any more data
            // finally disconnect the socket
            this.ForcefullyDisconnect();
        }

        protected override void DefaultExceptionHandler(Exception ex)
        {
            Log.Error("Unhandled exception on underlying socket:");
            Log.Error(ex.Message);
        }
    }
}