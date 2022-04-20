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
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;

namespace EVESharp.Node.Inventory;

public class SystemManager
{
    private readonly Dictionary <int, long> mSolarsystemToNodeID = new Dictionary <int, long> ();
    private          DatabaseConnection     Database       { get; }
    private          ItemFactory            ItemFactory    { get; }
    private          IMachoNet              MachoNet       { get; }
    private          HttpClient             HttpClient     { get; }
    private          ClusterManager         ClusterManager { get; }

    public SystemManager (HttpClient httpClient, ItemFactory itemFactory, ClusterManager clusterManager, DatabaseConnection databaseConnection, IMachoNet machoNet)
    {
        HttpClient     = httpClient;
        MachoNet       = machoNet;
        Database       = databaseConnection;
        ItemFactory    = itemFactory;
        ClusterManager = clusterManager;
    }

    private void LoadSolarSystemOnNode (int solarSystemID, long nodeID)
    {
        this.SignalSolarSystemLoaded (solarSystemID, nodeID);

        // now tell the server to load it
        MachoNet.QueueOutputPacket (
            new PyPacket (PyPacket.PacketType.NOTIFICATION)
            {
                Destination = new PyAddressNode (nodeID),
                Source      = new PyAddressNode (MachoNet.NodeID),
                Payload = new PyTuple (2)
                {
                    [0] = "OnSolarSystemLoad",
                    [1] = new PyTuple (1) {[0] = solarSystemID}
                },
                OutOfBounds = new PyDictionary (),
                UserID      = MachoNet.NodeID
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
            if (nodeID == MachoNet.NodeID)
            {
                // make sure it is marked as loaded locally
                this.mSolarsystemToNodeID [solarSystemID]                    = nodeID;
                ItemFactory.GetStaticSolarSystem (solarSystemID).BelongsToUs = true;
            }

            return nodeID;
        }

        // determine mode the server is running on
        if (MachoNet.Mode == RunMode.Single)
        {
            this.LoadSolarSystemLocally (solarSystemID);

            return MachoNet.NodeID;
        }

        if (MachoNet.Mode == RunMode.Proxy)
        {
            // determine what node is going to load it and let it know
            Task <long> task = ClusterManager.GetLessLoadedNode ();

            task.Wait ();

            // mark the solar system to load on the given node
            this.LoadSolarSystemOnNode (solarSystemID, task.Result);

            return nodeID;
        }

        throw new Exception ("Cannot signal a solar system as loaded from a server");
    }

    private void LoadSolarSystemLocally (int solarSystemID)
    {
        this.SignalSolarSystemLoaded (solarSystemID, MachoNet.NodeID);
    }

    private void SignalSolarSystemLoaded (int solarSystemID, long nodeID)
    {
        // mark the item as loaded by that node
        Database.InvSetItemNode (solarSystemID, nodeID);
        
        this.mSolarsystemToNodeID [solarSystemID] = nodeID;
    }

    public bool StationBelongsToUs (int stationID)
    {
        Station station = ItemFactory.GetStaticStation (stationID);

        return this.SolarSystemBelongsToUs (station.SolarSystemID);
    }

    public bool SolarSystemBelongsToUs (int solarSystemID)
    {
        return ItemFactory.GetStaticSolarSystem (solarSystemID).BelongsToUs;
    }

    public long GetNodeStationBelongsTo (int stationID)
    {
        Station station = ItemFactory.GetStaticStation (stationID);

        return this.GetNodeSolarSystemBelongsTo (station.LocationID);
    }

    public long GetNodeSolarSystemBelongsTo (int solarSystemID)
    {
        // check if it's loaded locally first, otherwise hit the database
        if (this.mSolarsystemToNodeID.TryGetValue (solarSystemID, out long nodeID))
            return nodeID;

        return Database.InvGetItemNode (solarSystemID);
    }
}