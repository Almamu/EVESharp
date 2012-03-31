using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Network;
using Common.Services;
using System.Threading;
using MySql.Data.MySqlClient;
using EVESharp.Database;
using System.Security.Cryptography;

namespace EVESharp
{
    class Program
    {
        static private TCPSocket connection = null;
        static public List<Client> clients = null;
        static private int nodeID = 0xFFAA;
        static private TCPSocket proxyConnection = null;
        static public ServiceManager SvcMgr = new ServiceManager();

        // Nodes list
        static string[,] nodes = new string[,]
        {
            // IP         Port
            {"127.0.0.1", "15000"}
        };

        // Proxy info
        static string[,] proxy = new string[,]
        {
            // IP         Port
            {"127.0.0.1", "26000"}
        };

        static public int GetNodeID()
        {
            return nodeID;
        }

        static void Main(string[] args)
        {
            clients = new List<Client>();

            Log.Init("evesharp");
            Log.Info("Main", "Connecting to proxy...");
            proxyConnection = new TCPSocket(ushort.Parse(proxy[0, 1]), false);
            proxyConnection.Connect(proxy[0, 0]);

            Log.Trace("Main", "Starting server...");
            Log.Trace("Database", "Connecting to database...");

            if (Database.Database.Init() == false)
            {
                while (true) ;
            }

            Log.Trace("Main", "Adding an test account...");

            /* Code to ADD an account:
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes("password"));
            char[] strHash = new char[20];

            for (int i = 0; i < 20; i++)
            {
                strHash[i] = (char)hash[i];
            }

            string str = new string(strHash);

            Database.Database.Query("INSERT INTO account(accountID, accountName, password, role, online, banned)VALUES(NULL, 'Username', '" + str + "', 2, 0, 0);");
            */

            Log.Info("Main", "Connection to the DB sucessfull");

            connection = new TCPSocket(26000, false);

            if (connection.Listen(5) == false)
            {
                Log.Error("Main", "Can't start listening mode");
                while (true) ;
            }

            Log.Trace("Main", "Listening on port 26000");
            Log.Trace("Main", "Registering services...");

            SvcMgr.AddService(new Services.Network.machoNet());
            SvcMgr.AddService(new Services.Network.alert());
            SvcMgr.AddService(new Services.CacheSvc.objectCaching());

            Log.Info("Main", "Done");
            Log.Info("Main", "Server started");

            while (true)
            {
                Thread.Sleep(1);

                TCPSocket client = connection.Accept();

                if (client != null)
                {
                    Log.Trace("Main", "Client connected");
                    Client cli = new Client(client);
                    cli.Start();
                    clients.Add(cli);
                }
            }
        }
    }
}
