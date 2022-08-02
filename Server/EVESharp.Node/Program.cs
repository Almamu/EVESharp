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
using EVESharp.Common.Database;
using EVESharp.Common.Logging;
using EVESharp.Common.Network.Messages;
using EVESharp.Node.Accounts;
using EVESharp.Node.Agents;
using EVESharp.Node.Cache;
using EVESharp.Node.Chat;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
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
using EVESharp.Node.Sessions;
using EVESharp.Node.SimpleInject;
using EVESharp.PythonTypes.Types.Database;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using SimpleInjector;
using Constants = EVESharp.Node.Configuration.Constants;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;
using MessageProcessor = EVESharp.Node.Server.Single.Messages.MessageProcessor;

namespace EVESharp.Node;

internal class Program
{
    private static async Task InitializeItemFactory (ILogger logChannel, Container dependencies)
    {
        await Task.Run (
            () =>
            {
                logChannel.Information ("Initializing item factory");
                dependencies.GetInstance <ItemFactory> ().Init ();
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
                CacheStorage cacheStorage = dependencies.GetInstance <CacheStorage> ();
                // prime bulk data
                cacheStorage.Load (
                    CacheStorage.LoginCacheTable,
                    CacheStorage.LoginCacheQueries,
                    CacheStorage.LoginCacheTypes
                );
                // prime character creation cache
                cacheStorage.Load (
                    CacheStorage.CreateCharacterCacheTable,
                    CacheStorage.CreateCharacterCacheQueries,
                    CacheStorage.CreateCharacterCacheTypes
                );
                // prime character appearance cache
                cacheStorage.Load (
                    CacheStorage.CharacterAppearanceCacheTable,
                    CacheStorage.CharacterAppearanceCacheQueries,
                    CacheStorage.CharacterAppearanceCacheTypes
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
                if (logEvent.Properties.TryGetValue (LoggingExtensions.HIDDEN_PROPERTY_NAME, out LogEventPropertyValue value))
                {
                    // now check if the name is in the allowed list
                    string name = "";

                    if (logEvent.Properties.TryGetValue ("Name", out LogEventPropertyValue nameProp))
                        name = nameProp.ToString ();
                    else if (logEvent.Properties.TryGetValue ("SourceContext", out LogEventPropertyValue sourceContext))
                        name = sourceContext.ToString ();

                    return !configuration.Logging.EnableChannels.Contains (name);
                }

                return false;
            }
        );
        // log to console by default
        loggerConfiguration.WriteTo.Console (template);

        if (configuration.FileLog.Enabled)
            loggerConfiguration.WriteTo.File (template, $"{configuration.FileLog.Directory}/{configuration.FileLog.LogFile}");

        // TODO: ADD SUPPORT FOR LOGLITE BACK

        return loggerConfiguration.CreateLogger ();
    }

    private static Container SetupDIContainer (ILogger baseLogger)
    {
        Container container = new Container ();

        // change how dependencies are resolved to ensure serilog instances are properly provided
        container.Options.DependencyInjectionBehavior =
            new SerilogContextualLoggerInjectionBehavior (container.Options, baseLogger);
        
        // disable auto-verification on the container as it triggers creation of instances before they're needed
        container.Options.EnableAutoVerification = false;
        
        // register all the dependencies we have available
        container.RegisterInstance (new HttpClient ());
        container.Register <IDatabaseConnection, DatabaseConnection> (Lifestyle.Singleton);
        container.Register <SessionManager> (Lifestyle.Singleton);
        container.Register <CacheStorage> (Lifestyle.Singleton);
        container.Register <MetaInventoryManager> (Lifestyle.Singleton);
        container.Register <AttributeManager> (Lifestyle.Singleton);
        container.Register <TypeManager> (Lifestyle.Singleton);
        container.Register <Categories> (Lifestyle.Singleton);
        container.Register <Groups> (Lifestyle.Singleton);
        container.Register <StationManager> (Lifestyle.Singleton);
        container.Register <ItemFactory> (Lifestyle.Singleton);
        container.Register <Timers> (Lifestyle.Singleton);
        container.Register <SystemManager> (Lifestyle.Singleton);
        container.Register <ServiceManager> (Lifestyle.Singleton);
        container.Register <BoundServiceManager> (Lifestyle.Singleton);
        container.Register <RemoteServiceManager>(Lifestyle.Singleton);
        container.Register <PacketCallHelper>(Lifestyle.Singleton);
        container.Register <NotificationSender> (Lifestyle.Singleton);
        container.Register <ExpressionManager> (Lifestyle.Singleton);
        container.Register <WalletManager> (Lifestyle.Singleton);
        container.Register <MailManager> (Lifestyle.Singleton);
        container.Register <AgentManager> (Lifestyle.Singleton);
        container.Register <Ancestries> (Lifestyle.Singleton);
        container.Register <Bloodlines> (Lifestyle.Singleton);
        container.Register <Constants> (Lifestyle.Singleton);
        container.Register <DogmaUtils> (Lifestyle.Singleton);

        // register the database accessors dependencies
        container.Register <CharacterDB> (Lifestyle.Singleton);
        container.Register <ChatDB> (Lifestyle.Singleton);
        container.Register <ConfigDB> (Lifestyle.Singleton);
        container.Register <ContractDB> (Lifestyle.Singleton);
        container.Register <CorporationDB> (Lifestyle.Singleton);
        container.Register <GeneralDB> (Lifestyle.Singleton);
        container.Register <ItemDB> (Lifestyle.Singleton);
        container.Register <MarketDB> (Lifestyle.Singleton);
        container.Register <MessagesDB> (Lifestyle.Singleton);
        container.Register <SkillDB> (Lifestyle.Singleton);
        container.Register <StandingDB> (Lifestyle.Singleton);
        container.Register <StationDB> (Lifestyle.Singleton);
        container.Register <LookupDB> (Lifestyle.Singleton);
        container.Register <InsuranceDB> (Lifestyle.Singleton);
        container.Register <SolarSystemDB> (Lifestyle.Singleton);
        container.Register <DogmaDB> (Lifestyle.Singleton);
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
        container.Register <LoginQueue> (Lifestyle.Singleton);
        container.Register <ClusterManager> (Lifestyle.Singleton);
        container.Register <TransportManager> (Lifestyle.Singleton);
        container.Register <EffectsManager> (Lifestyle.Singleton);
        
        return container;
    }

    private static General LoadConfiguration ()
    {
        // TODO: REPLACE WITH JSON CONFIGURATION FROM .NET INSTEAD?
        return General.LoadFromFile ("configuration.conf");
    }

    private static void Main (string [] argv)
    {
        // load configuration first
        General configuration = LoadConfiguration ();
        // initialize the logging system
        Logger log = SetupLogger (configuration);
        // finally initialize the dependency injection
        Container dependencies = SetupDIContainer (log);

        using (log)
        {
            try
            {
                // register configuration instances
                dependencies.RegisterInstance (configuration);
                dependencies.RegisterInstance (configuration.Database);
                dependencies.RegisterInstance (configuration.MachoNet);
                dependencies.RegisterInstance (configuration.Authentication);
                dependencies.RegisterInstance (configuration.LogLite);
                dependencies.RegisterInstance (configuration.FileLog);
                dependencies.RegisterInstance (configuration.Logging);
                dependencies.RegisterInstance (configuration.Character);

                // register logging system
                dependencies.RegisterInstance (log);

                // depending on the server mode initialize a different macho instance
                switch (configuration.MachoNet.Mode)
                {
                    case MachoNetMode.Single:
                        dependencies.Register <IMachoNet, MachoNet> (Lifestyle.Singleton);
                        dependencies.Register <MessageProcessor <MachoMessage>, MessageProcessor> (Lifestyle.Singleton);
                        break;
                    
                    case MachoNetMode.Proxy:
                        dependencies.Register <IMachoNet, Server.Proxy.MachoNet> (Lifestyle.Singleton);
                        dependencies.Register <MessageProcessor <MachoMessage>, Server.Proxy.Messages.MessageProcessor> (Lifestyle.Singleton);
                        break;
                    
                    case MachoNetMode.Server:
                        dependencies.Register <IMachoNet, Server.Node.MachoNet> (Lifestyle.Singleton);
                        dependencies.Register <MessageProcessor <MachoMessage>, Server.Node.Messages.MessageProcessor> (Lifestyle.Singleton);
                        break;
                }

                log.Information ("Initializing EVESharp Node");
                log.Fatal ("Initializing EVESharp Node");
                log.Error ("Initializing EVESharp Node");
                log.Warning ("Initializing EVESharp Node");
                log.Debug ("Initializing EVESharp Node");
                log.Verbose ("Initializing EVESharp Node");

                // ensure the message processor is created
                dependencies.GetInstance <MessageProcessor <MachoMessage>> ();
                
                // do some parallel initialization, cache priming and static item loading can be performed in parallel
                // this makes the changes quicker
                Task cacheStorage = InitializeCache (log, dependencies);
                Task itemFactory  = InitializeItemFactory (log, dependencies);

                // wait for all the tasks to be done
                Task.WaitAll (itemFactory, cacheStorage);

                // register the current machonet handler
                IMachoNet machoNet = dependencies.GetInstance <IMachoNet> ();

                // initialize the machonet protocol
                machoNet.Initialize ();

                // based on the mode do some things with the cluster manager
                ClusterManager cluster = dependencies.GetInstance <ClusterManager> ();

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