using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Network;
using Marshal;
using System.Threading;
using Proxy.Database;

namespace Proxy
{
    class Program
    {
        static private TCPSocket listener = null;
        static public List<Client> clients = null;
        static public List<Connection> waiting = null;

        static void Main(string[] args)
        {
            Log.Init("proxy");

            clients = new List<Client>();
            waiting = new List<Connection>();

            Log.Trace("Main", "Connecting to Database");
            if (Database.Database.Init() == false)
            {
                while (true) Thread.Sleep(1);
            }

            Log.Debug("Main", "Connected to database sucesfull");
            Log.Trace("Main", "Reseting Solar Systems' status");

            Log.Trace("Main", "Starting listener on port 26000");
            listener = new TCPSocket(26000, false);

            if (listener.Listen(int.MaxValue) == false)
            {
                Log.Error("Main", "Cannot listen on port 26000");
                while (true) ;
            }

            Log.Debug("Main", "Listening on port 26000");

            LoginQueue.Start();

            while (true)
            {
                Thread.Sleep(1);

                TCPSocket con = listener.Accept();

                if (con != null)
                {
                    Connection tmp = new Connection(con);
                    waiting.Add(tmp);
                }
            }
        }
    }
}
