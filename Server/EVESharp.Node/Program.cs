/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EVESharp.Common.Configuration;
using EVESharp.Common.Logging;
using EVESharp.Database;
using EVESharp.Database.Configuration;
using EVESharp.Database.Dogma;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Characters;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Old;
using EVESharp.EVE;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Corporations;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Market;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Messages.Queue;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Relationships;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Accounts;
using EVESharp.Node.Agents;
using EVESharp.Node.Cache;
using EVESharp.Node.Chat;
using EVESharp.Node.Configuration;
using EVESharp.Node.Corporations;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Dogma;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Relationships;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.Node.Services;
using EVESharp.Node.Services.Account;
using EVESharp.Node.Services.Alliances;
using EVESharp.Node.Services.Authentication;
using EVESharp.Node.Services.CacheSvc;
using EVESharp.Node.Services.Characters;
using EVESharp.Node.Services.Chat;
using EVESharp.Node.Services.Config;
using EVESharp.Node.Services.Contracts;
using EVESharp.Node.Services.Corporations;
using EVESharp.Node.Services.Data;
using EVESharp.Node.Services.Dogma;
using EVESharp.Node.Services.Inventory;
using EVESharp.Node.Services.Market;
using EVESharp.Node.Services.Navigation;
using EVESharp.Node.Services.Network;
using EVESharp.Node.Services.Stations;
using EVESharp.Node.Services.Tutorial;
using EVESharp.Node.Services.War;
using EVESharp.Node.SimpleInject;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using SimpleInjector;
using Constants = EVESharp.EVE.Configuration.Constants;
using Container = SimpleInjector.Container;
using ItemDB = EVESharp.Database.Old.ItemDB;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;
using MessageQueue = EVESharp.Node.Server.Single.Messages.MessageQueue;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node;

internal class Program
{
    private static async Task InitializeItemFactory (ILogger logChannel, Container dependencies)
    {
        await Task.Run (
            () =>
            {
                logChannel.Information ("Initializing item factory");
                dependencies.GetInstance <IItems> ().Init ();
                logChannel.Debug ("Item Factory Initialized");
            }
        );
    }

    private static async Task InitializeCache (ILogger logChannel, Container dependencies)
    {
        await Task.Run (
            () =>
            {
                logChannel.Information ("Initializing cache");
                ICacheStorage cacheStorage = dependencies.GetInstance <ICacheStorage> ();

                // prime bulk data
                cacheStorage.Load (
                    EVE.Data.Cache.LoginCacheTable,
                    EVE.Data.Cache.LoginCacheQueries,
                    EVE.Data.Cache.LoginCacheTypes
                );

                // prime character creation cache
                cacheStorage.Load (
                    EVE.Data.Cache.CreateCharacterCacheTable,
                    EVE.Data.Cache.CreateCharacterCacheQueries,
                    EVE.Data.Cache.CreateCharacterCacheTypes
                );

                // prime character appearance cache
                cacheStorage.Load (
                    EVE.Data.Cache.CharacterAppearanceCacheTable,
                    EVE.Data.Cache.CharacterAppearanceCacheQueries,
                    EVE.Data.Cache.CharacterAppearanceCacheTypes
                );

                logChannel.Information ("Cache Initialized");
            }
        );
    }

    private static Logger SetupLogger (General configuration)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration ().MinimumLevel.Verbose ();

        // create a default expression template to ensure the text has the correct format
        ExpressionTemplate template = new ExpressionTemplate (
            "{UtcDateTime(@t):yyyy-MM-dd HH:mm:ss} {@l:u1} {Coalesce(Coalesce(Name, Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)), 'Program')}: {@m:lj}\n{@x}"
        );

        // setup channels to be ignored based on the logging configuration
        loggerConfiguration.Filter.ByExcluding (
            logEvent =>
            {
                // check if it should be hidden by default
                if (logEvent.Properties.TryGetValue (LoggingExtensions.HIDDEN_PROPERTY_NAME, out LogEventPropertyValue _) == false)
                    return false;

                // now check if the name is in the allowed list
                string name = "";

                if (logEvent.Properties.TryGetValue ("Name", out LogEventPropertyValue nameProp))
                    name = nameProp.ToString ();
                else if (logEvent.Properties.TryGetValue ("SourceContext", out LogEventPropertyValue sourceContext))
                    name = sourceContext.ToString ();

                return !configuration.Logging.EnableChannels.Contains (name);
            }
        );

        // setup all the required logging sinks
        loggerConfiguration.WriteTo.Console (template);
            
        if (configuration.FileLog.Enabled)
            loggerConfiguration.WriteTo.File (template, $"{configuration.FileLog.Directory}/{configuration.FileLog.LogFile}");
        if (configuration.LogLite.Enabled)
            loggerConfiguration.WriteTo.LogLite (configuration.LogLite);

        return loggerConfiguration.CreateLogger ();
    }

    private static Container SetupDependencyInjection (General configuration, ILogger baseLogger)
    {
        Container container = new Container ();

        // change how dependencies are resolved to ensure serilog instances are properly provided
        container.Options.DependencyInjectionBehavior =
            new SerilogContextualLoggerInjectionBehavior (container.Options, baseLogger);

        
        // register configuration instances
        container.RegisterInstance (configuration);
        container.RegisterInstance (configuration.Database);
        container.RegisterInstance (configuration.MachoNet);
        container.RegisterInstance (configuration.Authentication);
        container.RegisterInstance (configuration.LogLite);
        container.RegisterInstance (configuration.FileLog);
        container.RegisterInstance (configuration.Logging);
        container.RegisterInstance (configuration.Character);
        
        // register logging system
        container.RegisterInstance (baseLogger);
        
        // register all the dependencies we have available
        container.RegisterInstance (new HttpClient ());
        container.Register <IDatabase, Database.Database> (Lifestyle.Singleton);
        container.Register <ISessionManager, SessionManager> (Lifestyle.Singleton);
        container.Register <ICacheStorage, CacheStorage> (Lifestyle.Singleton);
        container.Register <IMetaInventories, MetaInventories> (Lifestyle.Singleton);
        container.Register <IDefaultAttributes, DefaultAttributes> (Lifestyle.Singleton);
        container.Register <IAttributes, Attributes> (Lifestyle.Singleton);
        container.Register <IFactions, Factions> (Lifestyle.Singleton);
        container.Register <ITypes, Data.Inventory.Types> (Lifestyle.Singleton);
        container.Register <ICategories, Categories> (Lifestyle.Singleton);
        container.Register <IGroups, Groups> (Lifestyle.Singleton);
        container.Register <IStations, Stations> (Lifestyle.Singleton);
        container.Register <IItems, Items> (Lifestyle.Singleton);
        container.Register <IStandings, Standings>(Lifestyle.Singleton);
        container.Register <ITimers, Timers> (Lifestyle.Singleton);
        container.Register <ISolarSystems, SolarSystems> (Lifestyle.Singleton);
        container.Register <ServiceManager> (Lifestyle.Singleton);
        container.Register <IBoundServiceManager, BoundServiceManager> (Lifestyle.Singleton);
        container.Register <IRemoteServiceManager, RemoteServiceManager> (Lifestyle.Singleton);
        container.Register <PacketCallHelper> (Lifestyle.Singleton);
        container.Register <INotificationSender, NotificationSender> (Lifestyle.Singleton);
        container.Register <IExpressions, Expressions> (Lifestyle.Singleton);
        container.Register <IWallets, Wallets> (Lifestyle.Singleton);
        container.Register <MailManager> (Lifestyle.Singleton);
        container.Register <AgentManager> (Lifestyle.Singleton);
        container.Register <IAncestries, Ancestries> (Lifestyle.Singleton);
        container.Register <IBloodlines, Bloodlines> (Lifestyle.Singleton);
        container.Register <IConstants, Constants> (Lifestyle.Singleton);
        container.Register <IDogmaNotifications, DogmaNotifications> (Lifestyle.Singleton);
        container.Register <IAudit, Audit> (Lifestyle.Singleton);
        container.Register <IShares, Shares> (Lifestyle.Singleton);
        container.Register <IContracts, Contracts> (Lifestyle.Singleton);
        container.Register <IDogmaItems, DogmaItems> (Lifestyle.Singleton);

        // register the database accessors dependencies
        container.Register <OldCharacterDB> (Lifestyle.Singleton);
        container.Register <ChatDB> (Lifestyle.Singleton);
        container.Register <ConfigDB> (Lifestyle.Singleton);
        container.Register <ContractDB> (Lifestyle.Singleton);
        container.Register <CorporationDB> (Lifestyle.Singleton);
        container.Register <ItemDB> (Lifestyle.Singleton);
        container.Register <MarketDB> (Lifestyle.Singleton);
        container.Register <SkillDB> (Lifestyle.Singleton);
        container.Register <StandingDB> (Lifestyle.Singleton);
        container.Register <StationDB> (Lifestyle.Singleton);
        container.Register <LookupDB> (Lifestyle.Singleton);
        container.Register <InsuranceDB> (Lifestyle.Singleton);
        container.Register <RepairDB> (Lifestyle.Singleton);
        container.Register <ReprocessingDB> (Lifestyle.Singleton);
        container.Register <RAMDB> (Lifestyle.Singleton);
        container.Register <FactoryDB> (Lifestyle.Singleton);
        container.Register <TutorialsDB> (Lifestyle.Singleton);

        // register all the services
        container.Register <account> (Lifestyle.Singleton);
        container.Register <machoNet> (Lifestyle.Singleton);
        container.Register <objectCaching> (Lifestyle.Singleton);
        container.Register <alert> (Lifestyle.Singleton);
        container.Register <authentication> (Lifestyle.Singleton);
        container.Register <character> (Lifestyle.Singleton);
        container.Register <userSvc> (Lifestyle.Singleton);
        container.Register <charmgr> (Lifestyle.Singleton);
        container.Register <config> (Lifestyle.Singleton);
        container.Register <dogmaIM> (Lifestyle.Singleton);
        container.Register <invbroker> (Lifestyle.Singleton);
        container.Register <warRegistry> (Lifestyle.Singleton);
        container.Register <station> (Lifestyle.Singleton);
        container.Register <map> (Lifestyle.Singleton);
        container.Register <skillMgr> (Lifestyle.Singleton);
        container.Register <contractMgr> (Lifestyle.Singleton);
        container.Register <corpStationMgr> (Lifestyle.Singleton);
        container.Register <bookmark> (Lifestyle.Singleton);
        container.Register <LSC> (Lifestyle.Singleton);
        container.Register <onlineStatus> (Lifestyle.Singleton);
        container.Register <billMgr> (Lifestyle.Singleton);
        container.Register <facWarMgr> (Lifestyle.Singleton);
        container.Register <corporationSvc> (Lifestyle.Singleton);
        container.Register <clientStatsMgr> (Lifestyle.Singleton);
        container.Register <voiceMgr> (Lifestyle.Singleton);
        container.Register <standing2> (Lifestyle.Singleton);
        container.Register <tutorialSvc> (Lifestyle.Singleton);
        container.Register <agentMgr> (Lifestyle.Singleton);
        container.Register <corpRegistry> (Lifestyle.Singleton);
        container.Register <marketProxy> (Lifestyle.Singleton);
        container.Register <stationSvc> (Lifestyle.Singleton);
        container.Register <certificateMgr> (Lifestyle.Singleton);
        container.Register <jumpCloneSvc> (Lifestyle.Singleton);
        container.Register <LPSvc> (Lifestyle.Singleton);
        container.Register <lookupSvc> (Lifestyle.Singleton);
        container.Register <insuranceSvc> (Lifestyle.Singleton);
        container.Register <slash> (Lifestyle.Singleton);
        container.Register <ship> (Lifestyle.Singleton);
        container.Register <corpmgr> (Lifestyle.Singleton);
        container.Register <repairSvc> (Lifestyle.Singleton);
        container.Register <reprocessingSvc> (Lifestyle.Singleton);
        container.Register <ramProxy> (Lifestyle.Singleton);
        container.Register <factory> (Lifestyle.Singleton);
        container.Register <petitioner> (Lifestyle.Singleton);
        container.Register <allianceRegistry> (Lifestyle.Singleton);
        container.Register <IMessageQueue <LoginQueueEntry>, LoginQueue> (Lifestyle.Singleton);
        container.Register <IQueueProcessor <LoginQueueEntry>, ThreadedProcessor <LoginQueueEntry>> (Lifestyle.Singleton);
        container.Register <IClusterManager, ClusterManager> (Lifestyle.Singleton);
        container.Register <ITransportManager, TransportManager> (Lifestyle.Singleton);
        container.Register <EffectsManager> (Lifestyle.Singleton);
        
        // depending on the server mode initialize a different macho instance
        switch (configuration.MachoNet.Mode)
        {
            case MachoNetMode.Single:
                container.Register <IMachoNet, MachoNet> (Lifestyle.Singleton);
                container.Register <IMessageQueue <MachoMessage>, MessageQueue> (Lifestyle.Singleton);
                break;

            case MachoNetMode.Proxy:
                container.Register <IMachoNet, Server.Proxy.MachoNet> (Lifestyle.Singleton);
                container.Register <IMessageQueue <MachoMessage>, Server.Proxy.Messages.MessageQueue> (Lifestyle.Singleton);
                break;

            case MachoNetMode.Server:
                container.Register <IMachoNet, Server.Node.MachoNet> (Lifestyle.Singleton);
                container.Register <IMessageQueue <MachoMessage>, Server.Node.Messages.MessageQueue> (Lifestyle.Singleton);
                break;
        }

        container.Register <IQueueProcessor <MachoMessage>, MachoMessageProcessor>(Lifestyle.Singleton);

        return container;
    }

    private static void Main (string [] argv)
    {
        // load configuration first
        General configuration = Loader.Load <General> ("configuration.conf");
        // initialize the logging system
        Logger log = SetupLogger (configuration);
        // finally initialize the dependency injection
        Container dependencies = SetupDependencyInjection (configuration, log);

        using (log)
        {
            try
            {
                log.Information ("Initializing EVESharp Node");
                log.Fatal ("Initializing EVESharp Node");
                log.Error ("Initializing EVESharp Node");
                log.Warning ("Initializing EVESharp Node");
                log.Debug ("Initializing EVESharp Node");
                log.Verbose ("Initializing EVESharp Node");

                // do some parallel initialization, cache priming and static item loading can be performed in parallel
                // this makes the changes quicker
                Task cacheStorage = InitializeCache (log, dependencies);
                Task itemFactory  = InitializeItemFactory (log, dependencies);

                // wait for all the tasks to be done
                Task.WaitAll (itemFactory, cacheStorage);

                // register the current machoNet handler
                IMachoNet machoNet = dependencies.GetInstance <IMachoNet> ();

                // initialize the machoNet protocol
                machoNet.Initialize ();

                // based on the mode do some things with the cluster manager
                IClusterManager cluster = dependencies.GetInstance <IClusterManager> ();

                // register with the server
                if (machoNet.Mode != RunMode.Single)
                    cluster.RegisterNode ();

                if (machoNet.Mode == RunMode.Server)
                {
                    // wait for 5 seconds and connect to proxies
                    Thread.Sleep (5000);
                    // connect to proxies
                    cluster.EstablishConnectionWithProxies ();
                }

                log.Verbose ("Node startup done");

                // idle for infinity
                // yes, i know this is not ideal, but there's actual work that needs to happen
                // before this can be properly rewritten
                Thread.Sleep (Timeout.Infinite);
            }
            catch (Exception e)
            {
                log.Fatal ("Node stopped...");
                log.Fatal (e.ToString ());
            }
        }
    }
}