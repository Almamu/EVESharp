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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Common.Database;
using Common.Logging;
using Common.Logging.Streams;
using MySql.Data.MySqlClient;
using Node.Configuration;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Services.Account;
using Node.Services.CacheSvc;
using Node.Services.Characters;
using Node.Services.Chat;
using Node.Services.Config;
using Node.Services.Contracts;
using Node.Services.Corporations;
using Node.Services.Data;
using Node.Services.Dogma;
using Node.Services.Inventory;
using Node.Services.Market;
using Node.Services.Navigation;
using Node.Services.Network;
using Node.Services.Stations;
using Node.Services.Tutorial;
using Node.Services.War;
using PythonTypes;
using PythonTypes.Marshal;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node
{
    class Program
    {
        private static NodeContainer sNodeContainer = null;
        private static CacheStorage sCacheStorage = null;
        private static DatabaseConnection sDatabase = null;
        private static General sConfiguration = null;
        private static Logger sLog = null;
        private static Channel sChannel = null;

        private static Container sContainer = null;
        static public long NodeID
        {
            get => sNodeContainer.NodeID;
            private set { }
        }

        static void Main(string[] argv)
        {
            try
            {
                // create the dependency injector container
                sContainer = new Container();
                
                // register basic dependencies first
                sContainer.Register<Logger>(Lifestyle.Singleton);
                sContainer.Register<DatabaseConnection>(Lifestyle.Singleton);
                sContainer.Register<ClientManager>(Lifestyle.Singleton);
                sContainer.Register<NodeContainer>(Lifestyle.Singleton);
                sContainer.Register<CacheStorage>(Lifestyle.Singleton);
                sContainer.Register<ItemManager>(Lifestyle.Singleton);
                sContainer.Register<MetaInventoryManager>(Lifestyle.Singleton);
                sContainer.Register<AttributeManager>(Lifestyle.Singleton);
                sContainer.Register<TypeManager>(Lifestyle.Singleton);
                sContainer.Register<CategoryManager>(Lifestyle.Singleton);
                sContainer.Register<GroupManager>(Lifestyle.Singleton);
                sContainer.Register<StationManager>(Lifestyle.Singleton);
                sContainer.Register<ItemFactory>(Lifestyle.Singleton);
                sContainer.Register<TimerManager>(Lifestyle.Singleton);
                sContainer.Register<SystemManager>(Lifestyle.Singleton);
                sContainer.Register<ServiceManager>(Lifestyle.Singleton);
                sContainer.Register<BoundServiceManager>(Lifestyle.Singleton);
                sContainer.Register<ClusterConnection>(Lifestyle.Singleton);
                sContainer.Register<Client>(Lifestyle.Transient);
                
                // register the database accessors dependencies
                sContainer.Register<AccountDB>(Lifestyle.Singleton);
                sContainer.Register<AgentDB>(Lifestyle.Singleton);
                sContainer.Register<BookmarkDB>(Lifestyle.Singleton);
                sContainer.Register<CertificatesDB>(Lifestyle.Singleton);
                sContainer.Register<CharacterDB>(Lifestyle.Singleton);
                sContainer.Register<ChatDB>(Lifestyle.Singleton);
                sContainer.Register<ConfigDB>(Lifestyle.Singleton);
                sContainer.Register<ContractDB>(Lifestyle.Singleton);
                sContainer.Register<CorporationDB>(Lifestyle.Singleton);
                sContainer.Register<GeneralDB>(Lifestyle.Singleton);
                sContainer.Register<ItemDB>(Lifestyle.Singleton);
                sContainer.Register<MarketDB>(Lifestyle.Singleton);
                sContainer.Register<MessagesDB>(Lifestyle.Singleton);
                sContainer.Register<SkillDB>(Lifestyle.Singleton);
                sContainer.Register<StandingDB>(Lifestyle.Singleton);
                sContainer.Register<StationDB>(Lifestyle.Singleton);
                sContainer.Register<LookupDB>(Lifestyle.Singleton);
                sContainer.Register<InsuranceDB>(Lifestyle.Singleton);

                // register all the services
                sContainer.Register<account>(Lifestyle.Singleton);
                sContainer.Register<machoNet>(Lifestyle.Singleton);
                sContainer.Register<objectCaching>(Lifestyle.Singleton);
                sContainer.Register<alert>(Lifestyle.Singleton);
                sContainer.Register<authentication>(Lifestyle.Singleton);
                sContainer.Register<character>(Lifestyle.Singleton);
                sContainer.Register<userSvc>(Lifestyle.Singleton);
                sContainer.Register<charmgr>(Lifestyle.Singleton);
                sContainer.Register<config>(Lifestyle.Singleton);
                sContainer.Register<dogmaIM>(Lifestyle.Singleton);
                sContainer.Register<invbroker>(Lifestyle.Singleton);
                sContainer.Register<warRegistry>(Lifestyle.Singleton);
                sContainer.Register<station>(Lifestyle.Singleton);
                sContainer.Register<map>(Lifestyle.Singleton);
                sContainer.Register<skillMgr>(Lifestyle.Singleton);
                sContainer.Register<contractMgr>(Lifestyle.Singleton);
                sContainer.Register<corpStationMgr>(Lifestyle.Singleton);
                sContainer.Register<bookmark>(Lifestyle.Singleton);
                sContainer.Register<LSC>(Lifestyle.Singleton);
                sContainer.Register<onlineStatus>(Lifestyle.Singleton);
                sContainer.Register<billMgr>(Lifestyle.Singleton);
                sContainer.Register<facWarMgr>(Lifestyle.Singleton);
                sContainer.Register<corporationSvc>(Lifestyle.Singleton);
                sContainer.Register<clientStatsMgr>(Lifestyle.Singleton);
                sContainer.Register<voiceMgr>(Lifestyle.Singleton);
                sContainer.Register<standing2>(Lifestyle.Singleton);
                sContainer.Register<tutorialSvc>(Lifestyle.Singleton);
                sContainer.Register<agentMgr>(Lifestyle.Singleton);
                sContainer.Register<corpRegistry>(Lifestyle.Singleton);
                sContainer.Register<marketProxy>(Lifestyle.Singleton);
                sContainer.Register<stationSvc>(Lifestyle.Singleton);
                sContainer.Register<certificateMgr>(Lifestyle.Singleton);
                sContainer.Register<jumpCloneSvc>(Lifestyle.Singleton);
                sContainer.Register<LPSvc>(Lifestyle.Singleton);
                sContainer.Register<lookupSvc>(Lifestyle.Singleton);
                sContainer.Register<insuranceSvc>(Lifestyle.Singleton);
                sContainer.Register<slash>(Lifestyle.Singleton);
                
                sContainer.RegisterInstance(General.LoadFromFile("configuration.conf", sContainer));
                // disable auto-verification on the container as it triggers creation of instances before they're needed
                sContainer.Options.EnableAutoVerification = false;
                
                sConfiguration = sContainer.GetInstance<General>(); // TODO: REMOVE THIS ONCE ALL THE CODE USES DEPENDENCY INJECTION

                sLog = sContainer.GetInstance<Logger>();

                sChannel = sLog.CreateLogChannel("main");
                // add log streams
                sLog.AddLogStream(new ConsoleLogStream());

                // TODO: REMOVE WHEN THE WHOLE APP USES DEPENDENCY INJECTION
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
                sDatabase = sContainer.GetInstance<DatabaseConnection>();
                // sDatabase.Query("SET global max_allowed_packet=1073741824");
                
                // create the node container
                sNodeContainer = sContainer.GetInstance<NodeContainer>();

                sChannel.Info("Initializing timer manager");
                sContainer.GetInstance<TimerManager>().Start();
                sChannel.Debug("Done");
                
                sChannel.Info("Priming cache...");
                sCacheStorage = sContainer.GetInstance<CacheStorage>();
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
                sContainer.GetInstance<ItemFactory>().Init();
                sChannel.Debug("Done");

                sChannel.Info("Initializing solar system manager");
                sContainer.GetInstance<SystemManager>();
                sChannel.Debug("Done");
                
                sChannel.Info("Connecting to proxy...");

                ClusterConnection clusterConnection = sContainer.GetInstance<ClusterConnection>();
                clusterConnection.Socket.Connect(sConfiguration.Proxy.Hostname, sConfiguration.Proxy.Port);

                sChannel.Trace("Node startup done");

                while (true)
                    Thread.Sleep(1);
            }
            catch (Exception e)
            {
                if (sChannel == null || sLog == null)
                {
                    Console.WriteLine(e.ToString());
                }
                else
                {
                    sChannel?.Error(e.ToString());
                    sChannel?.Fatal("Node stopped...");
                    sLog?.Flush();                    
                }
            }
        }
    }
}