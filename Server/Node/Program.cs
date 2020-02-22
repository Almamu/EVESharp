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
using System.IO;
using System.Threading;
using Common.Database;
using Common.Logging;
using Common.Logging.Streams;
using MySql.Data.MySqlClient;
using Node.Configuration;
using Node.Inventory;
using Node.Network;
using PythonTypes;
using PythonTypes.Marshal;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Node
{
    class Program
    {
        private static NodeContainer sContainer = null;
        private static ClusterConnection sConnection = null;
        private static CacheStorage sCacheStorage = null;
        private static DatabaseConnection sDatabase = null;
        private static General sConfiguration = null;
        private static ItemFactory sItemFactory = null;
        private static Logger sLog = null;
        private static Channel sChannel = null;

        static public long NodeID
        {
            get => sContainer.NodeID;
            private set { }
        }

        static void Main(string[] argv)
        {
            try
            {
                sLog = new Logger();

                sChannel = sLog.CreateLogChannel("main");
                // add log streams
                sLog.AddLogStream(new ConsoleLogStream());
                // load the configuration
                sConfiguration = General.LoadFromFile("configuration.conf");

                // update the logging configuration
                sLog.SetConfiguration(sConfiguration.Logging);

                if (sConfiguration.LogLite.Enabled == true)
                    sLog.AddLogStream(new LogLiteStream("Node", sLog, sConfiguration.LogLite));
                if (sConfiguration.FileLog.Enabled == true)
                    sLog.AddLogStream(new FileLogStream(sConfiguration.FileLog));

                // run a thread for log flushing
                new Thread(() =>
                {
                    while (true)
                    {
                        sLog.Flush();
                        Thread.Sleep(1);
                    }
                }).Start();

                sChannel.Info("Initializing EVESharp Node");
                sChannel.Fatal("Initializing EVESharp Node");
                sChannel.Error("Initializing EVESharp Node");
                sChannel.Warning("Initializing EVESharp Node");
                sChannel.Debug("Initializing EVESharp Node");
                sChannel.Trace("Initializing EVESharp Node");
                
                // connect to the database
                sDatabase = DatabaseConnection.FromConfiguration(sConfiguration.Database, sLog);
                sDatabase.Query("SET global max_allowed_packet=1073741824");
                
                // create the node container
                sContainer = new NodeContainer(sDatabase);
                sContainer.Logger = sLog;
                sContainer.ClientManager = new ClientManager();

                sChannel.Info("Priming cache...");
                sCacheStorage = new CacheStorage(sContainer, sDatabase, sLog);
                // prime bulk data
                sCacheStorage.Load(
                    CacheStorage.LoginCacheTable,
                    CacheStorage.LoginCacheQueries,
                    CacheStorage.LoginCacheTypes
                );
                // prime character creation cache
                sCacheStorage.Load(
                    CacheStorage.CreateCharacterCacheTable,
                    CacheStorage.CreateCharacterCacheQueries,
                    CacheStorage.CreateCharacterCacheTypes
                );
                // prime character appearance cache
                sCacheStorage.Load(
                    CacheStorage.CharacterAppearanceCacheTable,
                    CacheStorage.CharacterAppearanceCacheQueries,
                    CacheStorage.CharacterAppearanceCacheTypes
                );
                sChannel.Debug("Done");
                sChannel.Info("Initializing item factory");
                sContainer.ItemFactory = new ItemFactory(sContainer);
                sContainer.ItemFactory.Init();
                sChannel.Debug("Done");

                sChannel.Info("Initializing solar system manager");
                sContainer.SystemManager = new SystemManager(sDatabase, sItemFactory);
                sChannel.Debug("Done");

                sChannel.Info("Initializing service manager");
                sContainer.ServiceManager = new ServiceManager(sContainer, sDatabase, sCacheStorage, sConfiguration);
                sChannel.Debug("Done");

                sChannel.Info("Connecting to proxy...");

                sConnection = new ClusterConnection(sContainer);
                sConnection.Socket.Connect(sConfiguration.Proxy.Hostname, sConfiguration.Proxy.Port);

                sChannel.Trace("Node startup done");

                while (true)
                    Thread.Sleep(1);
            }
            catch (Exception e)
            {
                sChannel?.Error(e.ToString());
                sChannel?.Fatal("Node stopped...");
                sLog?.Flush();
            }
        }
    }
}