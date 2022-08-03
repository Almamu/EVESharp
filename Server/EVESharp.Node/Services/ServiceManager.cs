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

using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Cache;
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
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Services;

public class ServiceManager : IServiceManager <string>
{
    public           CacheStorage                 CacheStorage     { get; }
    public           Timers                 Timers     { get; }
    private          ILogger                      Log              { get; }
    public           objectCaching                objectCaching    { get; }
    public           machoNet                     machoNet         { get; }
    public           alert                        alert            { get; }
    public           authentication               authentication   { get; }
    public           character                    character        { get; }
    public           userSvc                      userSvc          { get; }
    public           charmgr                      charmgr          { get; }
    public           config                       config           { get; }
    public           dogmaIM                      dogmaIM          { get; }
    public           invbroker                    invbroker        { get; }
    public           warRegistry                  warRegistry      { get; }
    public           station                      station          { get; }
    public           map                          map              { get; }
    public           account                      account          { get; }
    public           skillMgr                     skillMgr         { get; }
    public           contractMgr                  contractMgr      { get; }
    public           corpStationMgr               corpStationMgr   { get; }
    public           bookmark                     bookmark         { get; }
    public           LSC                          LSC              { get; }
    public           onlineStatus                 onlineStatus     { get; }
    public           billMgr                      billMgr          { get; }
    public           facWarMgr                    facWarMgr        { get; }
    public           corporationSvc               corporationSvc   { get; }
    public           clientStatsMgr               clientStatsMgr   { get; }
    public           voiceMgr                     voiceMgr         { get; }
    public           standing2                    standing2        { get; }
    public           tutorialSvc                  tutorialSvc      { get; }
    public           agentMgr                     agentMgr         { get; }
    public           corpRegistry                 corpRegistry     { get; }
    public           marketProxy                  marketProxy      { get; }
    public           stationSvc                   stationSvc       { get; }
    public           certificateMgr               certificateMgr   { get; }
    public           jumpCloneSvc                 jumpCloneSvc     { get; }
    public           LPSvc                        LPSvc            { get; }
    public           lookupSvc                    lookupSvc        { get; }
    public           insuranceSvc                 insuranceSvc     { get; }
    public           slash                        slash            { get; }
    public           ship                         ship             { get; }
    public           corpmgr                      corpmgr          { get; }
    public           repairSvc                    repairSvc        { get; }
    public           reprocessingSvc              reprocessingSvc  { get; }
    public           ramProxy                     ramProxy         { get; }
    public           factory                      factory          { get; }
    public           petitioner                   petitioner       { get; }
    public           allianceRegistry             allianceRegistry { get; }

    /// <summary>
    /// Constructor created mainly for testing, should not be used anywhere else
    /// </summary>
    protected ServiceManager ()
    {
        
    }
    
    public ServiceManager (
        CacheStorage     storage, ILogger logger, Timers timers,
        machoNet         machoNet,
        objectCaching    objectCaching,
        alert            alert,
        authentication   authentication,
        character        character,
        userSvc          userSvc,
        charmgr          charmgr,
        config           config,
        dogmaIM          dogmaIM,
        invbroker        invbroker,
        warRegistry      warRegistry,
        station          station,
        map              map,
        account          account,
        skillMgr         skillMgr,
        contractMgr      contractMgr,
        corpStationMgr   corpStationMgr,
        bookmark         bookmark,
        LSC              LSC,
        onlineStatus     onlineStatus,
        billMgr          billMgr,
        facWarMgr        facWarMgr,
        corporationSvc   corporationSvc,
        clientStatsMgr   clientStatsMgr,
        voiceMgr         voiceMgr,
        standing2        standing2,
        tutorialSvc      tutorialSvc,
        agentMgr         agentMgr,
        corpRegistry     corpRegistry,
        marketProxy      marketProxy,
        stationSvc       stationSvc,
        certificateMgr   certificateMgr,
        jumpCloneSvc     jumpCloneSvc,
        LPSvc            LPSvc,
        lookupSvc        lookupSvc,
        insuranceSvc     insuranceSvc,
        slash            slash,
        ship             ship,
        corpmgr          corpmgr,
        repairSvc        repairSvc,
        reprocessingSvc  reprocessingSvc,
        ramProxy         ramProxy,
        factory          factory,
        petitioner       petitioner,
        allianceRegistry allianceRegistry
    )
    {
        CacheStorage = storage;
        Timers = timers;
        Log          = logger;

        // store all the services
        this.machoNet         = machoNet;
        this.objectCaching    = objectCaching;
        this.alert            = alert;
        this.authentication   = authentication;
        this.character        = character;
        this.userSvc          = userSvc;
        this.charmgr          = charmgr;
        this.config           = config;
        this.dogmaIM          = dogmaIM;
        this.invbroker        = invbroker;
        this.warRegistry      = warRegistry;
        this.station          = station;
        this.map              = map;
        this.account          = account;
        this.skillMgr         = skillMgr;
        this.contractMgr      = contractMgr;
        this.corpStationMgr   = corpStationMgr;
        this.bookmark         = bookmark;
        this.LSC              = LSC;
        this.onlineStatus     = onlineStatus;
        this.billMgr          = billMgr;
        this.facWarMgr        = facWarMgr;
        this.corporationSvc   = corporationSvc;
        this.clientStatsMgr   = clientStatsMgr;
        this.voiceMgr         = voiceMgr;
        this.standing2        = standing2;
        this.tutorialSvc      = tutorialSvc;
        this.agentMgr         = agentMgr;
        this.corpRegistry     = corpRegistry;
        this.marketProxy      = marketProxy;
        this.stationSvc       = stationSvc;
        this.certificateMgr   = certificateMgr;
        this.jumpCloneSvc     = jumpCloneSvc;
        this.LPSvc            = LPSvc;
        this.lookupSvc        = lookupSvc;
        this.insuranceSvc     = insuranceSvc;
        this.slash            = slash;
        this.ship             = ship;
        this.corpmgr          = corpmgr;
        this.repairSvc        = repairSvc;
        this.reprocessingSvc  = reprocessingSvc;
        this.ramProxy         = ramProxy;
        this.factory          = factory;
        this.petitioner       = petitioner;
        this.allianceRegistry = allianceRegistry;
    }

    public PyDataType ServiceCall (string service, string method, ServiceCall call)
    {
        // search for the service locally
        object svc = this.GetType ().GetProperty (service)?.GetValue (this);

        if (svc is not Service svcInstance)
            throw new MissingServiceException <string> (service, method);

        // check the access level value to ensure the client can call this service here
        switch (svcInstance.AccessLevel)
        {
            case AccessLevel.Location:
                if (call.Session.ContainsKey (Session.LOCATION_ID) == false)
                    throw new UnauthorizedCallException <string> (service, method, call.Session.Role);
                break;
            case AccessLevel.Station:
                if (call.Session.ContainsKey (Session.STATION_ID) == false)
                    throw new UnauthorizedCallException <string> (service, method, call.Session.Role);
                break;
            case AccessLevel.SolarSystem:
                if (call.Session.ContainsKey (Session.SOLAR_SYSTEM_ID) == false)
                    throw new UnauthorizedCallException <string> (service, method, call.Session.Role);
                break;
        }

        return svcInstance.ExecuteCall (method, call);
    }
}