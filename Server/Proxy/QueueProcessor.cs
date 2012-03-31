using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Marshal;
using Common.Network;
using Common;
using Common.Packets;

namespace Proxy
{
    public static class QueueProcessor
    {
        public static Queue<TCPSocket> queue = new Queue<TCPSocket>();
        private static Thread main = null;

        public static void Start()
        {
            main = new Thread(Run);
            main.Start();
        }

        private static void SendLowLevelVersionExchange(TCPSocket to)
        {
            Log.Debug("Client", "Sending LowLevelVersionExchange...");
            LowLevelVersionExchange data = new LowLevelVersionExchange();
            data.codename = Common.Constants.Game.codename;
            data.birthday = Common.Constants.Game.birthday;
            data.build = Common.Constants.Game.build;
            data.machoVersion = Common.Constants.Game.machoVersion;
            data.version = Common.Constants.Game.version;
            data.usercount = Program.clients;
            data.region = Common.Constants.Game.region;
            to.Send(Marshal.Marshal.Process(data.Encode()));
        }

        public static void Run()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (queue.Count <= 0)
                {
                    continue;
                }

                // Get the first item of the queue
                TCPSocket con = queue.Dequeue();

                if (con == null)
                {
                    continue;
                }

                // Send the LowLevelVersionExchange
                SendLowLevelVersionExchange(con);

                // Wait 5 seconds for an answer
                long end = DateTime.Now.AddSeconds(5).ToFileTime();

                while (DateTime.Now.ToFileTime() < end)
                {
                    int bytes = 0;
                    byte[] data = null;

                    try
                    {
                        data = new byte[con.Available];
                        bytes = con.Recv(data);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("QueueProcessor", "Exception " + ex.Message);
                        con.Close();
                        break;
                    }

                    if (bytes == -1)
                    {
                        con.Close();
                        break;
                    }
                    else if (bytes == 0)
                    {
                        continue;
                    }
                    else
                    {
                        StreamPacketizer packetizer = new StreamPacketizer();
                        int p = packetizer.QueuePackets(data);

                        for (int i = 0; i < p; i++)
                        {

                        }
                        
                    }
                }

                // We can do nothing here, because if we do and the client has disconnected we will crash the server
            }
        }
    }
}
