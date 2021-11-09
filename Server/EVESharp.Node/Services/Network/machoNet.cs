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
using EVESharp.Common.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Network
{
    public class machoNet : IService
    {
        private CacheStorage CacheStorage { get; }
        public machoNet(CacheStorage cacheStorage)
        {
            this.CacheStorage = cacheStorage;
        }

        public PyTuple GetInitVals(CallInformation call)
        {
            if (this.CacheStorage.Exists("machoNet.serviceInfo") == false)
            {
                // this cached object indicates where the packets should be directed to when the client
                // wants to communicate with a service (as in PyAddress types or integers)
                PyDictionary dict = new PyDictionary
                {
                    ["trademgr"] = "station",
                    ["tutorialSvc"] = "station",
                    ["bookmark"] = "station",
                    ["slash"] = "station",
                    ["wormholeMgr"] = "station",
                    ["account"] = "station",
                    ["gangSvc"] = "station",
                    ["contractMgr"] = "station",
                    
                    ["LSC"] = "location",
                    ["station"] = "location",
                    ["config"] = "locationPreferred",
                    
                    ["scanMgr"] = "solarsystem",
                    ["keeper"] = "solarsystem",
                    
                    ["stationSvc"] = null,
                    ["zsystem"] = null,
                    ["invbroker"] = null,
                    ["droneMgr"] = null,
                    ["userSvc"] = null,
                    ["map"] = null,
                    ["beyonce"] = null,
                    ["standing2"] = null,
                    ["ram"] = null,
                    ["DB"] = null,
                    ["posMgr"] = null,
                    ["voucher"] = null,
                    ["entity"] = null,
                    ["damageTracker"] = null,
                    ["agentMgr"] = null,
                    ["dogmaIM"] = null,
                    ["machoNet"] = null,
                    ["dungeonExplorationMgr"] = null,
                    ["watchdog"] = null,
                    ["ship"] = null,
                    ["DB2"] = null,
                    ["market"] = null,
                    ["dungeon"] = null,
                    ["npcSvc"] = null,
                    ["sessionMgr"] = null,
                    ["allianceRegistry"] = null,
                    ["cache"] = null,
                    ["character"] = null,
                    ["factory"] = null,
                    ["facWarMgr"] = null,
                    ["corpStationMgr"] = null,
                    ["authentication"] = null,
                    ["effectCompiler"] = null,
                    ["charmgr"] = null,
                    ["BSD"] = null,
                    ["reprocessingSvc"] = null,
                    ["billingMgr"] = null,
                    ["billMgr"] = null,
                    ["lookupSvc"] = null,
                    ["emailreader"] = null,
                    ["lootSvc"] = null,
                    ["http"] = null,
                    ["repairSvc"] = null,
                    ["gagger"] = null,
                    ["dataconfig"] = null,
                    ["lien"] = null,
                    ["i2"] = null,
                    ["pathfinder"] = null,
                    ["alert"] = null,
                    ["director"] = null,
                    ["dogma"] = null,
                    ["aggressionMgr"] = null,
                    ["corporationSvc"] = null,
                    ["certificateMgr"] = null,
                    ["clones"] = null,
                    ["jumpCloneSvc"] = null,
                    ["insuranceSvc"] = null,
                    ["corpmgr"] = null,
                    ["warRegistry"] = null,
                    ["corpRegistry"] = null,
                    ["objectCaching"] = null,
                    ["counter"] = null,
                    ["petitioner"] = null,
                    ["LPSvc"] = null,
                    ["clientStatsMgr"] = null,
                    ["jumpbeaconsvc"] = null,
                    ["debug"] = null,
                    ["languageSvc"] = null,
                    ["skillMgr"] = null,
                    ["voiceMgr"] = null,
                    ["onlineStatus"] = null,
                    ["gangSvcObjectHandler"] = null
                };

                this.CacheStorage.Store("machoNet.serviceInfo", dict, DateTime.UtcNow.ToFileTimeUtc());
            }

            return new PyTuple(2)
            {
                [0] = this.CacheStorage.GetHint("machoNet.serviceInfo"),
                [1] = this.CacheStorage.GetHints(CacheStorage.LoginCacheTable)
            };
        }

        public PyInteger GetTime(CallInformation call)
        {
            return DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}