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
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EVESharp.Common.Database;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Inventory.SystemEntities;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using ItemDB = EVESharp.Database.ItemDB;
using MachoNet = EVESharp.Node.Network.MachoNet;

namespace EVESharp.Node.Inventory
{
    public class SystemManager
    {
        private DatabaseConnection Database { get; }
        private ItemFactory ItemFactory { get; }
        private MachoNet MachoNet { get; }
        private HttpClient HttpClient { get; }

        private Dictionary<int, long> mSolarsystemToNodeID = new Dictionary<int, long>();

        private void LoadSolarSystemOnNode(int solarSystemID, long nodeID)
        {
            this.SignalSolarSystemLoaded(solarSystemID, nodeID);
            
            // now tell the server to load it
            this.MachoNet.QueuePacket(
                new PyPacket(PyPacket.PacketType.NOTIFICATION)
                {
                    Destination = new PyAddressNode(nodeID),
                    Source = new PyAddressNode(this.MachoNet.Container.NodeID),
                    Payload = new PyTuple(2) {[0] = "OnSolarSystemLoad", [1] = new PyTuple(1){[0] = solarSystemID}},
                    OutOfBounds = new PyDictionary(),
                    UserID = this.MachoNet.Container.NodeID
                }
            );
        }

        public long LoadSolarSystemOnCluster(int solarSystemID)
        {
            // first check if the solar system is already loaded, otherwise load it
            long nodeID = this.GetNodeSolarSystemBelongsTo(solarSystemID);

            if (nodeID != 0)
            {
                // if the id is ours means that this belongs to us
                if (nodeID == this.MachoNet.Container.NodeID)
                {
                    // make sure it is marked as loaded locally
                    this.mSolarsystemToNodeID[solarSystemID] = nodeID;
                    this.ItemFactory.GetStaticSolarSystem(solarSystemID).BelongsToUs = true;
                }

                return nodeID;
            }
            
            // determine mode the server is running on
            if (this.MachoNet.Configuration.MachoNet.Mode == MachoNetMode.Single)
            {
                this.LoadSolarSystemLocally(solarSystemID);
                
                return this.MachoNet.Container.NodeID;
            }
            else if (this.MachoNet.Configuration.MachoNet.Mode == MachoNetMode.Proxy)
            {
                // determine what node is going to load it and let it know
                Task<HttpResponseMessage> task = this.HttpClient.GetAsync($"{this.MachoNet.Configuration.Cluster.OrchestatorURL}/Nodes/next");

                task.Wait();
                task.Result.EnsureSuccessStatusCode();
                // read the json and extract the required information
                Stream inputStream = task.Result.Content.ReadAsStream();

                nodeID = JsonSerializer.Deserialize<long>(inputStream);

                // mark the solar system to load on the given node
                this.LoadSolarSystemOnNode(solarSystemID, nodeID);
                
                return nodeID;
            }
            else
            {
                throw new Exception("Cannot signal a solar system as loaded from a server");
            }
        }

        private void LoadSolarSystemLocally(int solarSystemID)
        {
            this.SignalSolarSystemLoaded(solarSystemID, this.MachoNet.Container.NodeID);
        }

        private void SignalSolarSystemLoaded(int solarSystemID, long nodeID)
        {
            // mark the item as loaded by that node
            Database.Procedure(
                ItemDB.SET_ITEM_NODE,
                new Dictionary<string, object>()
                {
                    {"_itemID", solarSystemID},
                    {"_nodeID", nodeID}
                }
            );
            
            this.mSolarsystemToNodeID[solarSystemID] = nodeID;
        }
        
        public bool StationBelongsToUs(int stationID)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);

            return this.SolarSystemBelongsToUs(station.SolarSystemID);
        }

        public bool SolarSystemBelongsToUs(int solarSystemID)
        {
            return this.ItemFactory.GetStaticSolarSystem(solarSystemID).BelongsToUs;
        }

        public long GetNodeStationBelongsTo(int stationID)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);

            return this.GetNodeSolarSystemBelongsTo(station.LocationID);
        }

        public long GetNodeSolarSystemBelongsTo(int solarSystemID)
        {
            // check if it's loaded locally first, otherwise hit the database
            if (this.mSolarsystemToNodeID.TryGetValue(solarSystemID, out long nodeID) == true)
                return nodeID;
            
            return Database.Scalar<long>(
                ItemDB.GET_ITEM_NODE,
                new Dictionary<string, object>()
                {
                    {"_itemID", solarSystemID}
                }
            );
        }
        
        public SystemManager(HttpClient httpClient, ItemFactory itemFactory, DatabaseConnection databaseConnection, MachoNet machoNet)
        {
            this.HttpClient = httpClient;
            this.MachoNet = machoNet;
            this.Database = databaseConnection;
            this.ItemFactory = itemFactory;

            this.MachoNet.SystemManager = this;
        }
    }
}