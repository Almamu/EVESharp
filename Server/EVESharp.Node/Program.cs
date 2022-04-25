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
                // register configuration dependencies first
                dependencies.RegisterInstance (configuration);
                dependencies.RegisterInstance (configuration.Database);
                dependencies.RegisterInstance (configuration.MachoNet);
                dependencies.RegisterInstance (configuration.Authentication);
                dependencies.RegisterInstance (configuration.LogLite);
                dependencies.RegisterInstance (configuration.FileLog);
                dependencies.RegisterInstance (configuration.Logging);
                dependencies.RegisterInstance (configuration.Character);

                // register basic dependencies first
                dependencies.RegisterInstance (new HttpClient ());
                dependencies.RegisterInstance (log);
                dependencies.Register <DatabaseConnection> (Lifestyle.Singleton);
                dependencies.Register <SessionManager> (Lifestyle.Singleton);
                dependencies.Register <CacheStorage> (Lifestyle.Singleton);
                dependencies.Register <MetaInventoryManager> (Lifestyle.Singleton);
                dependencies.Register <AttributeManager> (Lifestyle.Singleton);
                dependencies.Register <TypeManager> (Lifestyle.Singleton);
                dependencies.Register <Categories> (Lifestyle.Singleton);
                dependencies.Register <Groups> (Lifestyle.Singleton);
                dependencies.Register <StationManager> (Lifestyle.Singleton);
                dependencies.Register <ItemFactory> (Lifestyle.Singleton);
                dependencies.Register <Timers> (Lifestyle.Singleton);
                dependencies.Register <SystemManager> (Lifestyle.Singleton);
                dependencies.Register <ServiceManager> (Lifestyle.Singleton);
                dependencies.Register <BoundServiceManager> (Lifestyle.Singleton);
                dependencies.Register <RemoteServiceManager>(Lifestyle.Singleton);
                dependencies.Register <PacketCallHelper>(Lifestyle.Singleton);
                dependencies.Register <NotificationSender> (Lifestyle.Singleton);
                dependencies.Register <ExpressionManager> (Lifestyle.Singleton);
                dependencies.Register <WalletManager> (Lifestyle.Singleton);
                dependencies.Register <MailManager> (Lifestyle.Singleton);
                dependencies.Register <AgentManager> (Lifestyle.Singleton);
                dependencies.Register <Ancestries> (Lifestyle.Singleton);
                dependencies.Register <Bloodlines> (Lifestyle.Singleton);
                dependencies.Register <Constants> (Lifestyle.Singleton);
                dependencies.Register <DogmaUtils> (Lifestyle.Singleton);

                // register the database accessors dependencies
                dependencies.Register <CertificatesDB> (Lifestyle.Singleton);
                dependencies.Register <CharacterDB> (Lifestyle.Singleton);
                dependencies.Register <ChatDB> (Lifestyle.Singleton);
                dependencies.Register <ConfigDB> (Lifestyle.Singleton);
                dependencies.Register <ContractDB> (Lifestyle.Singleton);
                dependencies.Register <CorporationDB> (Lifestyle.Singleton);
                dependencies.Register <GeneralDB> (Lifestyle.Singleton);
                dependencies.Register <ItemDB> (Lifestyle.Singleton);
                dependencies.Register <MarketDB> (Lifestyle.Singleton);
                dependencies.Register <MessagesDB> (Lifestyle.Singleton);
                dependencies.Register <SkillDB> (Lifestyle.Singleton);
                dependencies.Register <StandingDB> (Lifestyle.Singleton);
                dependencies.Register <StationDB> (Lifestyle.Singleton);
                dependencies.Register <LookupDB> (Lifestyle.Singleton);
                dependencies.Register <InsuranceDB> (Lifestyle.Singleton);
                dependencies.Register <SolarSystemDB> (Lifestyle.Singleton);
                dependencies.Register <DogmaDB> (Lifestyle.Singleton);
                dependencies.Register <RepairDB> (Lifestyle.Singleton);
                dependencies.Register <ReprocessingDB> (Lifestyle.Singleton);
                dependencies.Register <RAMDB> (Lifestyle.Singleton);
                dependencies.Register <FactoryDB> (Lifestyle.Singleton);
                dependencies.Register <TutorialsDB> (Lifestyle.Singleton);

                // register all the services
                dependencies.Register <account> (Lifestyle.Singleton);
                dependencies.Register <machoNet> (Lifestyle.Singleton);
                dependencies.Register <objectCaching> (Lifestyle.Singleton);
                dependencies.Register <alert> (Lifestyle.Singleton);
                dependencies.Register <authentication> (Lifestyle.Singleton);
                dependencies.Register <character> (Lifestyle.Singleton);
                dependencies.Register <userSvc> (Lifestyle.Singleton);
                dependencies.Register <charmgr> (Lifestyle.Singleton);
                dependencies.Register <config> (Lifestyle.Singleton);
                dependencies.Register <dogmaIM> (Lifestyle.Singleton);
                dependencies.Register <invbroker> (Lifestyle.Singleton);
                dependencies.Register <warRegistry> (Lifestyle.Singleton);
                dependencies.Register <station> (Lifestyle.Singleton);
                dependencies.Register <map> (Lifestyle.Singleton);
                dependencies.Register <skillMgr> (Lifestyle.Singleton);
                dependencies.Register <contractMgr> (Lifestyle.Singleton);
                dependencies.Register <corpStationMgr> (Lifestyle.Singleton);
                dependencies.Register <bookmark> (Lifestyle.Singleton);
                dependencies.Register <LSC> (Lifestyle.Singleton);
                dependencies.Register <onlineStatus> (Lifestyle.Singleton);
                dependencies.Register <billMgr> (Lifestyle.Singleton);
                dependencies.Register <facWarMgr> (Lifestyle.Singleton);
                dependencies.Register <corporationSvc> (Lifestyle.Singleton);
                dependencies.Register <clientStatsMgr> (Lifestyle.Singleton);
                dependencies.Register <voiceMgr> (Lifestyle.Singleton);
                dependencies.Register <standing2> (Lifestyle.Singleton);
                dependencies.Register <tutorialSvc> (Lifestyle.Singleton);
                dependencies.Register <agentMgr> (Lifestyle.Singleton);
                dependencies.Register <corpRegistry> (Lifestyle.Singleton);
                dependencies.Register <marketProxy> (Lifestyle.Singleton);
                dependencies.Register <stationSvc> (Lifestyle.Singleton);
                dependencies.Register <certificateMgr> (Lifestyle.Singleton);
                dependencies.Register <jumpCloneSvc> (Lifestyle.Singleton);
                dependencies.Register <LPSvc> (Lifestyle.Singleton);
                dependencies.Register <lookupSvc> (Lifestyle.Singleton);
                dependencies.Register <insuranceSvc> (Lifestyle.Singleton);
                dependencies.Register <slash> (Lifestyle.Singleton);
                dependencies.Register <ship> (Lifestyle.Singleton);
                dependencies.Register <corpmgr> (Lifestyle.Singleton);
                dependencies.Register <repairSvc> (Lifestyle.Singleton);
                dependencies.Register <reprocessingSvc> (Lifestyle.Singleton);
                dependencies.Register <ramProxy> (Lifestyle.Singleton);
                dependencies.Register <factory> (Lifestyle.Singleton);
                dependencies.Register <petitioner> (Lifestyle.Singleton);
                dependencies.Register <allianceRegistry> (Lifestyle.Singleton);
                dependencies.Register <LoginQueue> (Lifestyle.Singleton);
                dependencies.Register <ClusterManager> (Lifestyle.Singleton);
                dependencies.Register <TransportManager> (Lifestyle.Singleton);
                dependencies.Register <EffectsManager> (Lifestyle.Singleton);

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

                // disable auto-verification on the container as it triggers creation of instances before they're needed
                dependencies.Options.EnableAutoVerification = false;

                log.Information ("Initializing EVESharp Node");
                log.Fatal ("Initializing EVESharp Node");
                log.Error ("Initializing EVESharp Node");
                log.Warning ("Initializing EVESharp Node");
                log.Debug ("Initializing EVESharp Node");
                log.Verbose ("Initializing EVESharp Node");

                // connect to the database
                dependencies.GetInstance <DatabaseConnection> ();
                // sDatabase.Query("SET global max_allowed_packet=1073741824");

                // ensure the message processor is created
                dependencies.GetInstance <MessageProcessor <MachoMessage>> ();

                log.Information ("Initializing timer manager");
                dependencies.GetInstance <Timers> ().Start ();
                log.Debug ("Done");

                // do some parallel initialization, cache priming and static item loading can be performed in parallel
                // this makes the changes quicker
                Task cacheStorage = InitializeCache (log, dependencies);
                Task itemFactory  = InitializeItemFactory (log, dependencies);

                // wait for all the tasks to be done
                Task.WaitAll (itemFactory, cacheStorage);

                log.Information ("Initializing solar system manager");
                dependencies.GetInstance <SystemManager> ();
                log.Debug ("Done");

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

                while (true)
                {
                    if (machoNet.Mode == RunMode.Single)
                        continue;

                    // wait 45 seconds to send a heartbeat
                    Thread.Sleep (45 * 1000);

                    // send the heartbeat
                    dependencies.GetInstance <ClusterManager> ().PerformHeartbeat ();
                }
            }
            catch (Exception e)
            {
                log.Fatal ("Node stopped...");
                log.Fatal (e.ToString ());
            }
        }
    }
}