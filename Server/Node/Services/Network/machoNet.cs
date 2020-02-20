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
using Common.Services;
using PythonTypes.Types.Primitives;

namespace Node.Services.Network
{
    public class machoNet : Service
    {
        private readonly CacheStorage mCacheStorage = null;

        public machoNet(CacheStorage cacheStorage)
            : base("machoNet")
        {
            this.mCacheStorage = cacheStorage;
        }

        public PyDataType GetInitVals(PyTuple args, Client client)
        {
            if (this.mCacheStorage.Exists("machoNet.serviceInfo") == false)
            {
                // Cache does not exists, create it
                PyDictionary dict = new PyDictionary();

                // this cached object indicates where the packets should be directed to when the client
                // wants to communicate with a service (as in PyAddress types or integers)
                dict["trademgr"] = "station";
                dict["tutorialSvc"] = "station";
                dict["bookmark"] = "station";
                dict["slash"] = "station";
                dict["wormholeMgr"] = "station";
                dict["account"] = "station";
                dict["gangSvc"] = "station";
                dict["contractMgr"] = "station";

                dict["LSC"] = "location";
                dict["station"] = "location";
                dict["config"] = "locationPreferred";

                dict["scanMgr"] = "solarsystem";
                dict["keeper"] = "solarsystem";
                
                dict["stationSvc"] = new PyNone();
                dict["zsystem"] = new PyNone();
                dict["invbroker"] = new PyNone();
                dict["droneMgr"] = new PyNone();
                dict["userSvc"] = new PyNone();
                dict["map"] = new PyNone();
                dict["beyonce"] = new PyNone();
                dict["standing2"] = new PyNone();
                dict["ram"] = new PyNone();
                dict["DB"] = new PyNone();
                dict["posMgr"] = new PyNone();
                dict["voucher"] = new PyNone();
                dict["entity"] = new PyNone();
                dict["damageTracker"] = new PyNone();
                dict["agentMgr"] = new PyNone();
                dict["dogmaIM"] = new PyNone();
                dict["machoNet"] = new PyNone();
                dict["dungeonExplorationMgr"] = new PyNone();
                dict["watchdog"] = new PyNone();
                dict["ship"] = new PyNone();
                dict["DB2"] = new PyNone();
                dict["market"] = new PyNone();
                dict["dungeon"] = new PyNone();
                dict["npcSvc"] = new PyNone();
                dict["sessionMgr"] = new PyNone();
                dict["allianceRegistry"] = new PyNone();
                dict["cache"] = new PyNone();
                dict["character"] = new PyNone();
                dict["factory"] = new PyNone();
                dict["facWarMgr"] = new PyNone();
                dict["corpStationMgr"] = new PyNone();
                dict["authentication"] = new PyNone();
                dict["effectCompiler"] = new PyNone();
                dict["charmgr"] = new PyNone();
                dict["BSD"] = new PyNone();
                dict["reprocessingSvc"] = new PyNone();
                dict["billingMgr"] = new PyNone();
                dict["billMgr"] = new PyNone();
                dict["lookupSvc"] = new PyNone();
                dict["emailreader"] = new PyNone();
                dict["lootSvc"] = new PyNone();
                dict["http"] = new PyNone();
                dict["repairSvc"] = new PyNone();
                dict["gagger"] = new PyNone();
                dict["dataconfig"] = new PyNone();
                dict["lien"] = new PyNone();
                dict["i2"] = new PyNone();
                dict["pathfinder"] = new PyNone();
                dict["alert"] = new PyNone();
                dict["director"] = new PyNone();
                dict["dogma"] = new PyNone();
                dict["aggressionMgr"] = new PyNone();
                dict["corporationSvc"] = new PyNone();
                dict["certificateMgr"] = new PyNone();
                dict["clones"] = new PyNone();
                dict["jumpCloneSvc"] = new PyNone();
                dict["insuranceSvc"] = new PyNone();
                dict["corpmgr"] = new PyNone();
                dict["warRegistry"] = new PyNone();
                dict["corpRegistry"] = new PyNone();
                dict["objectCaching"] = new PyNone();
                dict["counter"] = new PyNone();
                dict["petitioner"] = new PyNone();
                dict["LPSvc"] = new PyNone();
                dict["clientStatsMgr"] = new PyNone();
                dict["jumpbeaconsvc"] = new PyNone();
                dict["debug"] = new PyNone();
                dict["languageSvc"] = new PyNone();
                dict["skillMgr"] = new PyNone();
                dict["voiceMgr"] = new PyNone();
                dict["onlineStatus"] = new PyNone();
                dict["gangSvcObjectHandler"] = new PyNone();

                this.mCacheStorage.Store("machoNet.serviceInfo", dict, DateTime.Now.ToFileTimeUtc());
            }

            PyDataType srvInfo = this.mCacheStorage.GetHint("machoNet.serviceInfo");
            PyTuple res = new PyTuple(2);
            PyDictionary initvals = this.mCacheStorage.GetHints(CacheStorage.LoginCacheTable);

            res[0] = srvInfo;
            res[1] = initvals;

            return res;
        }

        public PyDataType GetTime(PyTuple args, object client)
        {
            return new PyInteger(DateTime.Now.ToFileTimeUtc());
        }
    }
}