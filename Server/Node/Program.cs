/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Security.Cryptography;

using MySql.Data.MySqlClient;

using Common;
using Common.Database;
using Common.Network;
using Common.Services;
using Common.Packets;
using Node.Inventory;

using Marshal;
using Marshal.Database;
using Node.Configuration;
using Node.Network;

namespace Node
{
    class Program
    {
        private static NodeContainer sContainer = null;
        private static ClusterConnection sConnection = null;
        private static CacheStorage sCacheStorage = null;
        private static ServiceManager sServiceManager = null;
        private static DatabaseConnection sDatabase = null;
        private static SystemManager sSystemManager = null;
        private static General sConfiguration = null;
        private static ItemFactory sItemFactory = null;
        static public Dictionary<uint, Client> sClients = new Dictionary<uint, Client>();
        
        static private int nodeID = 0xFFFF;
        static private TCPSocket proxyConnection = null;

        static public long NodeID
        {
            get { return sContainer.NodeID; }
            private set { }
        }

        static void Main(string[] args)
        {
            try
            {
                Log.Init("evesharp");
                Log.Info("Main", "Starting node...");

                sConfiguration = General.LoadFromFile("configuration.conf");
                
                // update the loglevel with the new value
                Log.SetLogLevel(sConfiguration.Logging.LogLevel);

                // create the node container
                sContainer = new NodeContainer();
                sContainer.ClientManager = new ClientManager();
                
                Log.Trace("Database", "Connecting to database...");

                sDatabase = DatabaseConnection.FromConfiguration(sConfiguration.Database);

                Log.Info("Main", "Connection to the DB sucessfull");

                Log.Info("Main", "Priming cache...");
                sCacheStorage = new CacheStorage(sContainer, sDatabase);
                sCacheStorage.Load(CacheStorage.LoginCacheTable, CacheStorage.LoginCacheQueries, CacheStorage.LoginCacheTypes);
                Log.Debug("Main", "Done");
                
                Log.Info("Main", "Initializing item factory");
                sItemFactory = new ItemFactory(sDatabase);
                Log.Debug("Main", "Done");

                Log.Info("Main", "Initializing solar system manager");
                sSystemManager = new SystemManager(sDatabase, sItemFactory);
                Log.Debug("Main", "Done");

                Log.Info("Main", "Initializing service manager");
                sServiceManager = new ServiceManager(sDatabase, sCacheStorage, sConfiguration);
                Log.Debug("Main", "Done");
                
                Log.Info("Main", "Connecting to proxy...");
                
                sContainer.SystemManager = sSystemManager;
                sContainer.ServiceManager = sServiceManager;

                sConnection = new ClusterConnection(sContainer);
                sConnection.Socket.Connect(sConfiguration.Proxy.Hostname, sConfiguration.Proxy.Port);

                Log.Trace("Main", "Server started");

                while (true)
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                Log.Error("Main", e.Message);
                Log.Trace("Main", e.StackTrace);
                Log.Error("Main", "Node stopped...");
            }
        }
    }
}
