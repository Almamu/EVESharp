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
using System.Net.Http;
using System.Threading.Tasks;
using EVESharp.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions.corpStationMgr;
using EVESharp.EVE.Network;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Network;

namespace EVESharp.Node.Data.Inventory;

public class SolarSystems : Dictionary<int, SolarSystem>, ISolarSystems
{
    private readonly Dictionary <int, long> mSolarsystemToNodeID = new Dictionary <int, long> ();
    private          IDatabaseConnection    Database       { get; }
    private          IStations              Stations       { get; }
    private          IMachoNet              MachoNet       { get; }
    private          HttpClient             HttpClient     { get; }
    private          IClusterManager         ClusterManager { get; }

    public SolarSystem this [TypeID id]
    {
        get => this [(int) id];
        set => this [(int) id] = value;
    }
    
    public SolarSystems (HttpClient httpClient, IStations stations, IClusterManager clusterManager, IDatabaseConnection databaseConnection, IMachoNet machoNet)
    {
        this.HttpClient     = httpClient;
        this.MachoNet       = machoNet;
        this.Database       = databaseConnection;
        this.Stations       = stations;
        this.ClusterManager = clusterManager;
    }

    private void LoadSolarSystemOnNode (int solarSystemID, long nodeID)
    {
        this.SignalSolarSystemLoaded (solarSystemID, nodeID);

        // now tell the server to load it
        this.MachoNet.QueueOutputPacket (
            new PyPacket (PyPacket.PacketType.NOTIFICATION)
            {
                Destination = new PyAddressNode (nodeID),
                Source      = new PyAddressNode (this.MachoNet.NodeID),
                Payload = new PyTuple (2)
                {
                    [0] = "OnSolarSystemLoad",
                    [1] = new PyTuple (1) {[0] = solarSystemID}
                },
                OutOfBounds = new PyDictionary (),
                UserID      = this.MachoNet.NodeID
            }
        );
    }

    public long LoadSolarSystemOnCluster (int solarSystemID)
    {
        // first check if the solar system is already loaded, otherwise load it
        long nodeID = this.GetNodeSolarSystemBelongsTo (solarSystemID);

        if (nodeID != 0)
        {
            // if the id is ours means that this belongs to us
            if (nodeID == this.MachoNet.NodeID)
            {
                // TODO: CHECK FOR STATIC OR DYNAMIC SOLAR SYSTEMS!
                // make sure it is marked as loaded locally
                this.mSolarsystemToNodeID [solarSystemID]                   = nodeID;
                this [solarSystemID].BelongsToUs                            = true;
            }

            return nodeID;
        }

        // determine mode the server is running on
        if (this.MachoNet.Mode == RunMode.Single)
        {
            this.LoadSolarSystemLocally (solarSystemID);

            return this.MachoNet.NodeID;
        }

        if (this.MachoNet.Mode == RunMode.Proxy)
        {
            // determine what node is going to load it and let it know
            Task <long> task = this.ClusterManager.GetLessLoadedNode ();

            task.Wait ();

            // mark the solar system to load on the given node
            this.LoadSolarSystemOnNode (solarSystemID, task.Result);

            return nodeID;
        }

        throw new Exception ("Cannot signal a solar system as loaded from a server");
    }

    private void LoadSolarSystemLocally (int solarSystemID)
    {
        this.SignalSolarSystemLoaded (solarSystemID, this.MachoNet.NodeID);
    }

    private void SignalSolarSystemLoaded (int solarSystemID, long nodeID)
    {
        // mark the item as loaded by that node
        this.Database.InvSetItemNode (solarSystemID, nodeID);
        
        this.mSolarsystemToNodeID [solarSystemID] = nodeID;
    }

    public bool StationBelongsToUs (int stationID)
    {
        Station station = this.Stations [stationID];

        return this.SolarSystemBelongsToUs (station.SolarSystemID);
    }

    public bool SolarSystemBelongsToUs (int solarSystemID)
    {
        // TODO: CHECK FOR STATIC OR DYNAMIC SOLAR SYSTEMS!
        return this [solarSystemID].BelongsToUs;
    }

    public long GetNodeStationBelongsTo (int stationID)
    {
        // TODO: CHECK FOR STATIC OR DYNAMIC STATIONS!
        Station station = this.Stations [stationID];

        return this.GetNodeSolarSystemBelongsTo (station.LocationID);
    }

    public long GetNodeSolarSystemBelongsTo (int solarSystemID)
    {
        // check if it's loaded locally first, otherwise hit the database
        if (this.mSolarsystemToNodeID.TryGetValue (solarSystemID, out long nodeID))
            return nodeID;

        return this.Database.InvGetItemNode (solarSystemID);
    }
}