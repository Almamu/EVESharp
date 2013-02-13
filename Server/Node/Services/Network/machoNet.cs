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
using System.Linq;
using System.Text;
using Common.Services;
using Marshal;
using Common;

namespace EVESharp.Services.Network
{
    public class machoNet : Service
    {
        public machoNet()
            : base("machoNet")
        {

        }

        public PyObject GetInitVals(PyTuple args, object client)
        {
            Log.Debug("machoNet", "Called GetInitVals stub");

            if (Cache.LoadCacheFor("machoNet.serviceInfo") == false)
            {
                // Cache does not exists, create it
                PyDict dict = new PyDict();

                dict.Set("trademgr", new PyString("station"));
                dict.Set("tutorialSvc", new PyString("station"));
                dict.Set("bookmark", new PyString("station"));
                dict.Set("slash", new PyString("station"));
                dict.Set("wormholeMgr", new PyString("station"));
                dict.Set("account", new PyString("station"));
                dict.Set("gangSvc", new PyString("station"));
                dict.Set("contractMgr", new PyString("station"));

                dict.Set("LSC", new PyString("location"));
                dict.Set("station", new PyString("location"));
                dict.Set("config", new PyString("locationPreferred"));

                dict.Set("scanMgr", new PyString("solarsystem"));
                dict.Set("keeper", new PyString("solarsystem"));

                dict.Set("stationSvc", new PyNone());
                dict.Set("zsystem", new PyNone());
                dict.Set("invbroker", new PyNone());
                dict.Set("droneMgr", new PyNone());
                dict.Set("userSvc", new PyNone());
                dict.Set("map", new PyNone());
                dict.Set("beyonce", new PyNone());
                dict.Set("standing2", new PyNone());
                dict.Set("ram", new PyNone());
                dict.Set("DB", new PyNone());
                dict.Set("posMgr", new PyNone());
                dict.Set("voucher", new PyNone());
                dict.Set("entity", new PyNone());
                dict.Set("damageTracker", new PyNone());
                dict.Set("agentMgr", new PyNone());
                dict.Set("dogmaIM", new PyNone());
                dict.Set("machoNet", new PyNone());
                dict.Set("dungeonExplorationMgr", new PyNone());
                dict.Set("watchdog", new PyNone());
                dict.Set("ship", new PyNone());
                dict.Set("DB2", new PyNone());
                dict.Set("market", new PyNone());
                dict.Set("dungeon", new PyNone());
                dict.Set("npcSvc", new PyNone());
                dict.Set("sessionMgr", new PyNone());
                dict.Set("allianceRegistry", new PyNone());
                dict.Set("cache", new PyNone());
                dict.Set("character", new PyNone());
                dict.Set("factory", new PyNone());
                dict.Set("facWarMgr", new PyNone());
                dict.Set("corpStationMgr", new PyNone());
                dict.Set("authentication", new PyNone());
                dict.Set("effectCompiler", new PyNone());
                dict.Set("charmgr", new PyNone());
                dict.Set("BSD", new PyNone());
                dict.Set("reprocessingSvc", new PyNone());
                dict.Set("billingMgr", new PyNone());
                dict.Set("billMgr", new PyNone());
                dict.Set("lookupSvc", new PyNone());
                dict.Set("emailreader", new PyNone());
                dict.Set("lootSvc", new PyNone());
                dict.Set("http", new PyNone());
                dict.Set("repairSvc", new PyNone());
                dict.Set("gagger", new PyNone());
                dict.Set("dataconfig", new PyNone());
                dict.Set("lien", new PyNone());
                dict.Set("i2", new PyNone());
                dict.Set("pathfinder", new PyNone());
                dict.Set("alert", new PyNone());
                dict.Set("director", new PyNone());
                dict.Set("dogma", new PyNone());
                dict.Set("aggressionMgr", new PyNone());
                dict.Set("corporationSvc", new PyNone());
                dict.Set("certificateMgr", new PyNone());
                dict.Set("clones", new PyNone());
                dict.Set("jumpCloneSvc", new PyNone());
                dict.Set("insuranceSvc", new PyNone());
                dict.Set("corpmgr", new PyNone());
                dict.Set("warRegistry", new PyNone());
                dict.Set("corpRegistry", new PyNone());
                dict.Set("objectCaching", new PyNone());
                dict.Set("counter", new PyNone());
                dict.Set("petitioner", new PyNone());
                dict.Set("LPSvc", new PyNone());
                dict.Set("clientStatsMgr", new PyNone());
                dict.Set("jumpbeaconsvc", new PyNone());
                dict.Set("debug", new PyNone());
                dict.Set("languageSvc", new PyNone());
                dict.Set("skillMgr", new PyNone());
                dict.Set("voiceMgr", new PyNone());
                dict.Set("onlineStatus", new PyNone());
                dict.Set("gangSvcObjectHandler", new PyNone());

                Cache.SaveCacheFor("machoNet.serviceInfo", dict, DateTime.Now.ToFileTimeUtc());
            }

            PyObject srvInfo = Cache.GetCacheData("machoNet.serviceInfo");
            PyTuple res = new PyTuple();
            PyDict initvals = Cache.GetCacheHints();

            res.Items.Add(srvInfo);
            res.Items.Add(initvals); // Rest of the cache data

            return res;
        }

        public PyObject GetTime(PyTuple args, object client)
        {
            return new PyLongLong(DateTime.Now.ToFileTimeUtc());
        }
    }
}
