using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Network;
using Marshal;
using System.Threading;

namespace Proxy
{
    class Program
    {
        static private TCPSocket listener = null;
        static public int clients = 0;

        static void Main(string[] args)
        {
            Log.Init("proxy");

            Log.Trace("Main", "Starting listener on port 26000");
            listener = new TCPSocket(26000, false);

            if (listener.Listen(int.MaxValue) == false)
            {
                Log.Error("Main", "Cannot listen on port 26000");
                while (true) ;
            }

            Log.Debug("Main", "Listening on port 26000");

            while (true)
            {
                Thread.Sleep(1);

                TCPSocket con = listener.Accept();

                if (con != null)
                {
                    QueueProcessor.queue.Enqueue(con);
                }
            }
        }
    }
}
