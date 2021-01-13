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

using Common.Database;
using Common.Logging;
using Common.Services;
using Node.Configuration;
using Node.Services;
using Node.Services.Account;
using Node.Services.CacheSvc;
using Node.Services.Characters;
using Node.Services.Chat;
using Node.Services.Config;
using Node.Services.Contracts;
using Node.Services.Corporations;
using Node.Services.Dogma;
using Node.Services.Inventory;
using Node.Services.Market;
using Node.Services.Navigation;
using Node.Services.Network;
using Node.Services.Stations;
using Node.Services.Tutorial;
using Node.Services.War;
using SimpleInjector;

namespace Node
{
    public class ServiceManager : Common.Services.ServiceManager
    {
        public NodeContainer Container { get; }
        public CacheStorage CacheStorage { get; }
        public BoundServiceManager BoundServiceManager { get; }
        public Logger Logger { get; }
        public objectCaching objectCaching { get; private set; }
        public machoNet machoNet { get; private set; }
        public alert alert { get; private set; }
        public authentication authentication { get; private set; }
        public character character { get; private set; }
        public userSvc userSvc { get; private set; }
        public charmgr charmgr { get; private set; }
        public config config { get; private set; }
        public dogmaIM dogmaIM { get; private set; }
        public invbroker invbroker { get; private set; }
        public warRegistry warRegistry { get; private set; }
        public station station { get; private set; }
        public map map { get; private set; }
        public account account { get; private set; }
        public skillMgr skillMgr { get; private set; }
        public contractMgr contractMgr { get; private set; }
        public corpStationMgr corpStationMgr { get; private set; }
        public bookmark bookmark { get; private set; }
        public LSC LSC { get; private set; }
        public onlineStatus onlineStatus { get; private set; }
        public billMgr billMgr { get; private set; }
        public facWarMgr facWarMgr { get; private set; }
        public corporationSvc corporationSvc { get; private set; }
        public clientStatsMgr clientStatsMgr { get; private set; }
        public voiceMgr voiceMgr { get; private set; }
        public standing2 standing2 { get; private set; }
        public tutorialSvc tutorialSvc { get; private set; }
        public agentMgr agentMgr { get; private set; }
        public corpRegistry corpRegistry { get; private set; }
        public marketProxy marketProxy { get; private set; }
        public stationSvc stationSvc { get; private set; }
        public certificateMgr certificateMgr { get; private set; }
        public jumpCloneSvc jumpCloneSvc { get; private set; }
        
        public ServiceManager(
            NodeContainer container, CacheStorage storage, Logger logger, BoundServiceManager boundServiceManager,
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
            jumpCloneSvc jumpCloneSvc)
        {
            this.Container = container;
            this.CacheStorage = storage;
            this.BoundServiceManager = boundServiceManager;
            this.Logger = logger;
            
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
        }
    }
}