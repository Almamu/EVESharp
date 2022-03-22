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
using System.Collections.Generic;
using EVESharp.Common.Logging;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services.Account;
using EVESharp.Node.Services.Alliances;
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
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class ServiceManager : IServiceManager<string>
    {
        private int mNextCallID = 0;
        private Dictionary<int, RemoteCall> mCallCallbacks = new Dictionary<int, RemoteCall>();
        public NodeContainer Container { get; }
        public CacheStorage CacheStorage { get; }
        public BoundServiceManager BoundServiceManager { get; }
        public TimerManager TimerManager { get; }
        public Logger Logger { get; }
        private Channel Log { get; }
        public objectCaching objectCaching { get; }
        public machoNet machoNet { get; }
        public alert alert { get; }
        public authentication authentication { get; }
        public character character { get; }
        public userSvc userSvc { get; }
        public charmgr charmgr { get; }
        public config config { get; }
        public dogmaIM dogmaIM { get; }
        public invbroker invbroker { get; }
        public warRegistry warRegistry { get; }
        public station station { get; }
        public map map { get; }
        public account account { get; }
        public skillMgr skillMgr { get; }
        public contractMgr contractMgr { get; }
        public corpStationMgr corpStationMgr { get; }
        public bookmark bookmark { get; }
        public LSC LSC { get; }
        public onlineStatus onlineStatus { get; }
        public billMgr billMgr { get; }
        public facWarMgr facWarMgr { get; }
        public corporationSvc corporationSvc { get; }
        public clientStatsMgr clientStatsMgr { get; }
        public voiceMgr voiceMgr { get; }
        public standing2 standing2 { get; }
        public tutorialSvc tutorialSvc { get; }
        public agentMgr agentMgr { get; }
        public corpRegistry corpRegistry { get; }
        public marketProxy marketProxy { get; }
        public stationSvc stationSvc { get; }
        public certificateMgr certificateMgr { get; }
        public jumpCloneSvc jumpCloneSvc { get; }
        public LPSvc LPSvc { get; }
        public lookupSvc lookupSvc { get; }
        public insuranceSvc insuranceSvc { get; }
        public slash slash { get; }
        public ship ship { get; }
        public corpmgr corpmgr { get; }
        public repairSvc repairSvc { get; }
        public reprocessingSvc reprocessingSvc { get; }
        public ramProxy ramProxy { get; }
        public factory factory { get; }
        public petitioner petitioner { get; }
        public allianceRegistry allianceRegistry { get; }

        public ServiceManager(
            NodeContainer container, CacheStorage storage, Logger logger, TimerManager timerManager,
            BoundServiceManager boundServiceManager,
            machoNet machoNet,
            objectCaching objectCaching,
            alert alert,
            authentication authentication,
            character character,
            userSvc userSvc,
            charmgr charmgr,
            config config,
            dogmaIM dogmaIM,
            invbroker invbroker,
            warRegistry warRegistry,
            station station,
            map map,
            account account,
            skillMgr skillMgr,
            contractMgr contractMgr,
            corpStationMgr corpStationMgr,
            bookmark bookmark,
            LSC LSC,
            onlineStatus onlineStatus,
            billMgr billMgr,
            facWarMgr facWarMgr,
            corporationSvc corporationSvc,
            clientStatsMgr clientStatsMgr,
            voiceMgr voiceMgr,
            standing2 standing2,
            tutorialSvc tutorialSvc,
            agentMgr agentMgr,
            corpRegistry corpRegistry,
            marketProxy marketProxy,
            stationSvc stationSvc,
            certificateMgr certificateMgr,
            jumpCloneSvc jumpCloneSvc,
            LPSvc LPSvc,
            lookupSvc lookupSvc,
            insuranceSvc insuranceSvc,
            slash slash,
            ship ship,
            corpmgr corpmgr,
            repairSvc repairSvc,
            reprocessingSvc reprocessingSvc,
            ramProxy ramProxy,
            factory factory,
            petitioner petitioner,
            allianceRegistry allianceRegistry)
        {
            this.Container = container;
            this.CacheStorage = storage;
            this.BoundServiceManager = boundServiceManager;
            this.TimerManager = timerManager;
            this.Logger = logger;
            this.Log = this.Logger.CreateLogChannel("ServiceManager");
            
            // store all the services
            this.machoNet = machoNet;
            this.objectCaching = objectCaching;
            this.alert = alert;
            this.authentication = authentication;
            this.character = character;
            this.userSvc = userSvc;
            this.charmgr = charmgr;
            this.config = config;
            this.dogmaIM = dogmaIM;
            this.invbroker = invbroker;
            this.warRegistry = warRegistry;
            this.station = station;
            this.map = map;
            this.account = account;
            this.skillMgr = skillMgr;
            this.contractMgr = contractMgr;
            this.corpStationMgr = corpStationMgr;
            this.bookmark = bookmark;
            this.LSC = LSC;
            this.onlineStatus = onlineStatus;
            this.billMgr = billMgr;
            this.facWarMgr = facWarMgr;
            this.corporationSvc = corporationSvc;
            this.clientStatsMgr = clientStatsMgr;
            this.voiceMgr = voiceMgr;
            this.standing2 = standing2;
            this.tutorialSvc = tutorialSvc;
            this.agentMgr = agentMgr;
            this.corpRegistry = corpRegistry;
            this.marketProxy = marketProxy;
            this.stationSvc = stationSvc;
            this.certificateMgr = certificateMgr;
            this.jumpCloneSvc = jumpCloneSvc;
            this.LPSvc = LPSvc;
            this.lookupSvc = lookupSvc;
            this.insuranceSvc = insuranceSvc;
            this.slash = slash;
            this.ship = ship;
            this.corpmgr = corpmgr;
            this.repairSvc = repairSvc;
            this.reprocessingSvc = reprocessingSvc;
            this.ramProxy = ramProxy;
            this.factory = factory;
            this.petitioner = petitioner;
            this.allianceRegistry = allianceRegistry;
        }

        /// <summary>
        /// Callback fired by the <seealso cref="TimerManager"/> when a call timeout has been reached
        /// </summary>
        /// <param name="callID">The callID that expired</param>
        public void CallTimeoutExpired(int callID)
        {
            Log.Warning($"Timeout for call {callID} expired before getting an answer.");
            
            // get call id and call the timeout callback
            RemoteCall call = this.mCallCallbacks[callID];

            // call the callback if available
            call.TimeoutCallback?.Invoke(call);

            // finally remove from the list
            this.mCallCallbacks.Remove(callID);
        }

        /// <summary>
        /// Tells the ServiceManager that a call was completed successfully and invokes the success callback
        /// so the server can continue processing further
        /// </summary>
        /// <param name="callID">The callID that completed</param>
        /// <param name="result">The result of the call</param>
        public void ReceivedRemoteCallAnswer(int callID, PyDataType result)
        {
            if (this.mCallCallbacks.ContainsKey(callID) == false)
            {
                Log.Warning($"Received an answer for call {callID} after the timeout expired, ignoring answer...");
                return;
            }
            
            // remove the timer from the list
            this.TimerManager.DequeueCallTimer(callID);
            
            // get the callback information
            RemoteCall call = this.mCallCallbacks[callID];
            
            // invoke the handler
            call.Callback?.Invoke(call, result);

            // remove the call from the list
            this.mCallCallbacks.Remove(callID);
        }

        /// <summary>
        /// Reserves a slot in the call list and prepares timeout timers in case the call wait time expires 
        /// </summary>
        /// <param name="entry">The RemoteCall entry to associate with this call</param>
        /// <param name="timeoutSeconds">The amount of seconds to wait until timing out</param>
        /// <returns>The callID to be notified to the client</returns>
        private int ExpectRemoteServiceResult(RemoteCall entry, int timeoutSeconds = 0)
        {
            // get the new callID
            int callID = ++this.mNextCallID;

            // add the callback to the list
            this.mCallCallbacks[callID] = entry;
            
            // create the timer (if needed)
            if (timeoutSeconds > 0)
            {
                this.TimerManager.EnqueueCallTimer(
                    DateTime.UtcNow.AddSeconds(timeoutSeconds).ToFileTimeUtc(),
                    CallTimeoutExpired,
                    callID
                );
            }

            return callID;
        }

        /// <summary>
        /// Reserves a slot in the call list and prepares timeout timers in case the call wait time expires 
        /// </summary>
        /// <param name="callback">The function to call when the answer is received</param>
        /// <param name="client">The client that is getting the call</param>
        /// <param name="extraInfo">Any extra information to store for later usage</param>
        /// <param name="timeoutCallback">The function to call if the call timeout expires</param>
        /// <param name="timeoutSeconds">The amount of seconds to wait until timing out</param>
        /// <returns>The callID to be notified to the client</returns>
        public int ExpectRemoteServiceResult(Action<RemoteCall, PyDataType> callback, Session session, object extraInfo = null,
            Action<RemoteCall> timeoutCallback = null, int timeoutSeconds = 0)
        {
            RemoteCall entry = new RemoteCall
            {
                Callback = callback, ExtraInfo = extraInfo, TimeoutCallback = timeoutCallback, Session = session
            };

            return this.ExpectRemoteServiceResult(entry, timeoutSeconds);
        }

        /// <summary>
        /// Reserves a slot in the call list and prepares timeout timers in case the call wait time expires 
        /// </summary>
        /// <param name="callback">The function to call when the answer is received</param>
        /// <param name="nodeID">The node that is getting the call</param>
        /// <param name="extraInfo">Any extra information to store for later usage</param>
        /// <param name="timeoutCallback">The function to call if the call timeout expires</param>
        /// <param name="timeoutSeconds">The amount of seconds to wait until timing out</param>
        /// <returns>The callID to be notified to the client</returns>
        public int ExpectRemoteServiceResult(Action<RemoteCall, PyDataType> callback, int nodeID, object extraInfo = null,
            Action<RemoteCall> timeoutCallback = null, int timeoutSeconds = 0)
        {
            RemoteCall entry = new RemoteCall
            {
                Callback = callback, ExtraInfo = extraInfo, TimeoutCallback = timeoutCallback, Session = null, NodeID = nodeID
            };

            return this.ExpectRemoteServiceResult(entry, timeoutSeconds);
        }

        public PyDataType ServiceCall(string service, string method, ServiceCall call)
        {
            // search for the service locally
            object svc = this.GetType().GetProperty(service)?.GetValue(this);

            if (svc is not Service svcInstance)
                throw new MissingServiceException<string>(service, method);
            
            // check the access level value to ensure the client can call this service here
            switch (svcInstance.AccessLevel)
            {
                case AccessLevel.Location:
                    if (call.Session.ContainsKey(Session.LOCATION_ID) == false)
                        throw new UnauthorizedCallException<string>(service, method, call.Session.Role);
                    break;
                case AccessLevel.Station:
                    if (call.Session.ContainsKey(Session.STATION_ID) == false)
                        throw new UnauthorizedCallException<string>(service, method, call.Session.Role);
                    break;
                case AccessLevel.SolarSystem:
                    if (call.Session.ContainsKey(Session.SOLAR_SYSTEM_ID) == false)
                        throw new UnauthorizedCallException<string>(service, method, call.Session.Role);
                    break;
            }

            return svcInstance.ExecuteCall(method, call);
        }
    }
}