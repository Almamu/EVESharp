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
using Common.Services;
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
using Node.Services.Navigation;
using Node.Services.Network;
using Node.Services.Stations;
using Node.Services.War;

namespace Node
{
    public class ServiceManager : Common.Services.ServiceManager
    {
        public NodeContainer Container { get; }
        public CacheStorage CacheStorage { get; }
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
        private readonly DatabaseConnection mDatabaseConnection = null;

        public ServiceManager(NodeContainer container, DatabaseConnection db, CacheStorage storage, Configuration.General configuration)
        {
            this.Container = container;
            this.mDatabaseConnection = db;
            this.CacheStorage = storage;

            // initialize services
            this.machoNet = new machoNet(this);
            this.objectCaching = new objectCaching(container.Logger, this);
            this.alert = new alert(container.Logger, this);
            this.authentication = new authentication(configuration.Authentication, this);
            this.character = new character(db, configuration.Character, this);
            this.userSvc = new userSvc(this);
            this.charmgr = new charmgr(db, this);
            this.config = new config(db, this);
            this.dogmaIM = new dogmaIM(this);
            this.invbroker = new invbroker(this);
            this.warRegistry = new warRegistry(this);
            this.station = new station(this);
            this.map = new map(this);
            this.account = new account(db, this);
            this.skillMgr = new skillMgr(this);
            this.contractMgr = new contractMgr(db, this);
            this.corpStationMgr = new corpStationMgr(this);
            this.bookmark = new bookmark(db, this);
            this.LSC = new LSC(this);
            this.onlineStatus = new onlineStatus(this);
            this.billMgr = new billMgr(this);
            this.facWarMgr = new facWarMgr(this);
            this.corporationSvc = new corporationSvc(db, this);
        }
    }
}