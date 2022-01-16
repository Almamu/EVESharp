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
using System.IO;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Notifications.Client.Station;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Notifications.Client.Character;
using EVESharp.Node.Notifications.Client.Clones;
using EVESharp.Node.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Network
{
    /// <summary>
    /// Class that represents a normal client transport
    /// </summary>
    public class Client
    {
        private readonly Session mSession = new Session();
        
        private NodeContainer Container { get; }
        public ClusterConnection ClusterConnection { get; }
        public ServiceManager ServiceManager { get; }
        private ItemFactory ItemFactory { get; }
        private TimerManager TimerManager { get; }
        private PyList<PyTuple> PendingMultiEvents { get; set; }
        private SystemManager SystemManager { get; }
        private NotificationManager NotificationManager { get; }
        private ClientManager ClientManager { get; }
        private MachoNet MachoNet { get; }

        /// <summary>
        /// Event handler for when a client gets disconnected
        /// </summary>
        public event EventHandler<ClientEventArgs> OnClientDisconnectedEvent;
        
        /// <summary>
        /// Event handler for when a client's session is updated
        /// </summary>
        public event EventHandler<ClientSessionEventArgs> OnSessionUpdateEvent;

        /// <summary>
        /// Event handler for when this client selects a character
        /// </summary>
        public event EventHandler<ClientEventArgs> OnCharacterSelectedEvent;

        public Client(NodeContainer container, ClusterConnection clusterConnection, ServiceManager serviceManager,
            TimerManager timerManager, ItemFactory itemFactory, SystemManager systemManager,
            NotificationManager notificationManager, ClientManager clientManager, MachoNet machoNet)
        {
            this.Container = container;
            this.ClusterConnection = clusterConnection;
            this.ServiceManager = serviceManager;
            this.TimerManager = timerManager;
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
            this.NotificationManager = notificationManager;
            this.PendingMultiEvents = new PyList<PyTuple>();
            this.ClientManager = clientManager;
            this.MachoNet = machoNet;
        }

        private void OnCharEnteredStation(int stationID)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);
            station.Guests[(int) this.CharacterID] = this.ItemFactory.GetItem<Character>((int) this.CharacterID);

            // notify station guests
            this.NotificationManager.NotifyStation(station.ID, new OnCharNowInStation(this));
        }

        private void OnCharLeftStation(int stationID)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);
            station.Guests.Remove((int) this.CharacterID);

            // notify station guests
            this.NotificationManager.NotifyStation(station.ID, new OnCharNoLongerInStation(this));
        }

        public void OnClientDisconnected()
        {
            // call any event handlers for desconnection of this client
            this.OnClientDisconnectedEvent?.Invoke(this, new ClientEventArgs {Client = this});
            
            // no character selected means no worries
            if (this.CharacterID is null)
                return;
            
            // free meta inventories for this client
            this.ItemFactory.MetaInventoryManager.FreeOwnerInventories((int) this.CharacterID);
            
            if (this.StationID is not null && this.SystemManager.StationBelongsToUs((int) this.StationID) == true)
                this.OnCharLeftStation((int) this.StationID);

            // remove timers
            this.ItemFactory.UnloadItem((int) this.CharacterID);
            this.ItemFactory.UnloadItem((int) this.ShipID);
        }

        private void OnSolarSystemChanged(int? oldSolarSystemID, int? newSolarSystemID)
        {
            // load the character information if it's not loaded in already
            if (this.SystemManager.SolarSystemBelongsToUs((int) newSolarSystemID) == true)
                // load the character into this node
                this.ItemFactory.LoadItem((int) this.CharacterID);

            // check if the old solar system belong to us (and this new one doesnt) and unload things
            if (oldSolarSystemID is null)
                return;

            if (this.SystemManager.SolarSystemBelongsToUs((int) oldSolarSystemID) == true && this.SystemManager.SolarSystemBelongsToUs((int) newSolarSystemID) == false)
                // unload the character from this node
                this.ItemFactory.UnloadItem((int) this.CharacterID);
        }

        private void OnStationChanged(int? oldStationID, int? newStationID)
        {
            if (oldStationID is not null && this.SystemManager.StationBelongsToUs((int) oldStationID) == true)
                this.OnCharLeftStation((int) oldStationID);

            if (newStationID is not null && this.SystemManager.StationBelongsToUs((int) newStationID) == true)
                this.OnCharEnteredStation((int) newStationID);
        }

        private void HandleSessionDifferencies(Session session)
        {
            lock (this.mSession)
            {
                // now check for specific situations to properly increase/decrease counters
                // on various areas
                if (session.ContainsKey("charid") == false)
                    return;

                if (session["charid"] is PyInteger == false)
                    return;
                
                if (session.GetPrevious("charid") is null)
                    this.OnCharacterSelectedEvent?.Invoke(this, new ClientEventArgs {Client = this});
                
                // TODO: MOVE THIS HANDLING CODE SOMEWHERE ELSE
                if (session.ContainsKey("solarsystemid2") == true)
                {
                    PyDataType newSolarSystemID2 = session["solarsystemid2"];
                    PyDataType oldSolarSystemID2 = session.GetPrevious("solarsystemid2");
                    int? intNewSolarSystemID2 = null;
                    int? intOldSolarSystemID2 = null;

                    if (oldSolarSystemID2 is PyInteger oldID)
                        intOldSolarSystemID2 = oldID;
                    if (newSolarSystemID2 is PyInteger newID)
                        intNewSolarSystemID2 = newID;

                    // load character if required
                    if (intNewSolarSystemID2 != intOldSolarSystemID2 && intNewSolarSystemID2 > 0)
                        this.OnSolarSystemChanged(intOldSolarSystemID2, intNewSolarSystemID2);
                }
                
                // has the player got into a station?
                if (session.ContainsKey("stationid") == true)
                {
                    PyDataType newStationID = session["stationid"];
                    PyDataType oldStationID = session.GetPrevious("stationid");
                    int? intNewStationID = null;
                    int? intOldStationID = null;

                    if (oldStationID is PyInteger oldID)
                        intOldStationID = oldID;
                    if (newStationID is PyInteger newID)
                        intNewStationID = newID;

                    if (intNewStationID != intOldStationID)
                        this.OnStationChanged(intOldStationID, intNewStationID);
                }   
            }
        }

        /// <summary>
        /// Updates the current client's session based of a session change notification
        /// (for example, coming from another node or from the cluster controller itself)
        /// </summary>
        /// <param name="packet">The packet that contains the session change notification</param>
        public void UpdateSession(PyPacket packet)
        {
            lock (this.mSession)
            {
                if (packet.Payload.TryGetValue(0, out PyTuple sessionData) == false)
                    throw new InvalidDataException("SessionChangeNotification expected a payload of size 1");
                if (sessionData.TryGetValue(1, out PyDictionary differences) == false)
                    throw new InvalidDataException("SessionChangeNotification expected a differences collection");
                
                // load the new session data
                this.mSession.LoadChanges(differences);
                // normalize the values in the session
                this.mSession.LoadChanges(differences);
                // handle the differencies
                Session sessionDiff = new Session(differences.GetEnumerable<PyString, PyTuple>());
                this.HandleSessionDifferencies(sessionDiff);
                // call any events that listen for our session changes
                this.OnSessionUpdateEvent?.Invoke(this, new ClientSessionEventArgs { Client = this, Session = sessionDiff});
            }
        }

        /// <summary>
        /// Creates a session change notification including only the last changes to the session
        /// </summary>
        /// <param name="container">Information about the current node the client is in</param>
        /// <returns>The packet ready to be sent or null if no change is detected on the session</returns>
        private PyPacket CreateEmptySessionChange(NodeContainer container)
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification
            {
                Changes = Session.GenerateSessionChange()
            };

            if (scn.Changes.Length == 0)
                // Nothing to do
                return null;
            
            // add ourselves as nodes of interest
            // TODO: DETECT NODE OF INTEREST BASED ON THE SERVER THAT HAS THE SOLAR SYSTEM LOADED IN
            scn.NodesOfInterest.Add(container.NodeID);

            PyPacket packet = new PyPacket(PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Source = new PyAddressNode(container.NodeID),
                Destination = new PyAddressClient(this.Session["userid"] as PyInteger),
                UserID = this.Session["userid"] as PyInteger,
                Payload = scn,
                OutOfBounds = new PyDictionary() {{"channel", "sessionchange"}}
            };
            
            return packet;
        }

        /// <summary>
        /// Prepares and sends a session change notification to this client
        /// </summary>
        public void SendSessionChange()
        {
            // do not send the session change if the session hasn't changed
            if (this.mSession.IsDirty == false)
                return;
            
            // build the session change
            PyPacket sessionChangeNotification = this.CreateEmptySessionChange(this.Container);
            
            // and finally send the client the required data
            if (sessionChangeNotification is not null)
                this.ClusterConnection.Send(sessionChangeNotification);
        }

        public Session Session => this.mSession;

        public string LanguageID => this.mSession["languageID"] as PyString;

        public int AccountID => this.mSession["userid"] as PyInteger;

        public int Role => this.mSession["role"] as PyInteger;

        public string Address => this.mSession["address"] as PyString;

        public int? CharacterID
        {
            get => this.mSession["charid"] as PyInteger;
            set => this.mSession["charid"] = value;
        }

        public int CorporationID
        {
            get => this.mSession["corpid"] as PyInteger;
            set => this.mSession["corpid"] = value;
        }

        public int CorpAccountKey
        {
            get => this.mSession["corpAccountKey"] as PyInteger ?? WalletKeys.MAIN_WALLET;
            set => this.mSession["corpAccountKey"] = value;
        }

        public int SolarSystemID2
        {
            get => this.mSession["solarsystemid2"] as PyInteger;
            set => this.mSession["solarsystemid2"] = value;
        }

        public int ConstellationID
        {
            get => this.mSession["constellationid"] as PyInteger;
            set => this.mSession["constellationid"] = value;
        }

        public int RegionID
        {
            get => this.mSession["regionid"] as PyInteger;
            set => this.mSession["regionid"] = value;
        }

        public int HQID
        {
            get => this.mSession["hqID"] as PyInteger;
            set => this.mSession["hqID"] = value;
        }

        public long CorporationRole
        {
            get => this.mSession["corprole"] as PyInteger;
            set => this.mSession["corprole"] = value;
        }

        public long RolesAtAll
        {
            get => this.mSession["rolesAtAll"] as PyInteger;
            set => this.mSession["rolesAtAll"] = value;
        }

        public long RolesAtBase
        {
            get => this.mSession["rolesAtBase"] as PyInteger;
            set => this.mSession["rolesAtBase"] = value;
        }

        public long RolesAtHQ
        {
            get => this.mSession["rolesAtHQ"] as PyInteger;
            set => this.mSession["rolesAtHQ"] = value;
        }

        public long RolesAtOther
        {
            get => this.mSession["rolesAtOther"] as PyInteger;
            set => this.mSession["rolesAtOther"] = value;
        }

        public int? ShipID
        {
            get => this.mSession["shipid"] as PyInteger;
            set => this.mSession["shipid"] = value;
        }

        public int? StationID
        {
            get => this.mSession["stationid"] as PyInteger;
            set
            {
                this.mSession["solarsystemid"] = null;
                this.mSession["stationid"] = value;
                this.mSession["locationid"] = value;
            }
        }

        public int? SolarSystemID
        {
            get => this.mSession["solarsystemid"] as PyInteger;
            set
            {
                this.mSession["stationid"] = null;
                this.mSession["solarsystemid"] = value;
                this.mSession["locationid"] = value;
            }
        }

        public int LocationID
        {
            get => this.mSession["locationid"] as PyInteger;
            set => this.mSession["locationid"] = value;
        }

        public int? AllianceID
        {
            get => this.mSession["allianceid"] as PyInteger;
            set => this.mSession["allianceid"] = value;
        }

        public int? WarFactionID
        {
            get => this.mSession["warfactionid"] as PyInteger;
            set => this.mSession["warfactionid"] = value;
        }

        public int? RaceID
        {
            get => this.mSession["raceID"] as PyInteger;
            set => this.mSession["raceID"] = value;
        }

        /// <summary>
        /// Notifies the client of a single attribute change on a specific item
        /// </summary>
        /// <param name="attribute">The attribute to notify about</param>
        /// <param name="item">The item to notify about</param>
        public void NotifyAttributeChange(Inventory.Items.Attributes.Attribute attribute, ItemEntity item)
        {
            this.NotifyMultiEvent(new OnModuleAttributeChange(item, attribute));
        }

        /// <summary>
        /// Notifies the client of a single attribute change on a specific item
        /// </summary>
        /// <param name="attribute">The attribute to notify about</param>
        /// <param name="item">The item to notify about</param>
        public void NotifyAttributeChange(Attributes attribute, ItemEntity item)
        {
            this.NotifyMultiEvent(new OnModuleAttributeChange(item, item.Attributes[attribute]));
        }

        /// <summary>
        /// Notifies the client of a multiple attribute change on a specific item
        /// </summary>
        /// <param name="attributes">The attributes to notify about</param>
        /// <param name="item">The item to notify about</param>
        public void NotifyAttributeChange(Attributes[] attributes, ItemEntity item)
        {
            OnModuleAttributeChanges changes = new OnModuleAttributeChanges();

            foreach (Attributes attribute in attributes)
                changes.AddChange(new OnModuleAttributeChange(item, item.Attributes[attribute]));

            this.NotifyMultiEvent(changes);
        }

        /// <summary>
        /// Notifies the client of a multiple attribute change on different items
        /// </summary>
        /// <param name="attributes">The list of attributes that have changed</param>
        /// <param name="items">The list of items those attributes belong to</param>
        /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
        public void NotifyAttributeChange(Inventory.Items.Attributes.Attribute[] attributes, ItemEntity[] items)
        {
            if (attributes.Length != items.Length)
                throw new ArgumentOutOfRangeException(
                    "attributes list and items list must have the same amount of elements");

            OnModuleAttributeChanges changes = new OnModuleAttributeChanges();

            for (int i = 0; i < attributes.Length; i++)
                changes.AddChange(new OnModuleAttributeChange(items[i], attributes[i]));

            this.NotifyMultiEvent(changes);
        }

        /// <summary>
        /// Notifies the client of a multiple attribute change on an item
        /// </summary>
        /// <param name="attributes">The list of attributes that have changed</param>
        /// <param name="item">The item these attributes belong to</param>
        public void NotifyAttributeChange(Inventory.Items.Attributes.Attribute[] attributes, ItemEntity item)
        {
            OnModuleAttributeChanges changes = new OnModuleAttributeChanges();

            foreach (Inventory.Items.Attributes.Attribute attribute in attributes)
                changes.AddChange(new OnModuleAttributeChange(item, attribute));

            this.NotifyMultiEvent(changes);
        }

        public void NotifyAttributeChange(Attributes attribute, ItemEntity[] items)
        {
            OnModuleAttributeChanges changes = new OnModuleAttributeChanges();

            foreach (ItemEntity item in items)
                changes.AddChange(new OnModuleAttributeChange(item, item.Attributes[attribute]));
            
            this.NotifyMultiEvent(changes);
        }

        /// <summary>
        /// Adds a MultiEvent notification to the list of pending notifications to be sent
        /// </summary>
        /// <param name="entry">The MultiEvent entry to enqueue</param>
        public void NotifyMultiEvent(ClientNotification entry)
        {
            lock (this.PendingMultiEvents)
                this.PendingMultiEvents.Add(entry);
        }
        
        /// <summary>
        /// Checks if there's any pending notifications and sends them to the client
        ///
        /// This includes sending session change notifications when there has been any changes to it
        /// </summary>
        public void SendPendingNotifications()
        {
            lock (this.PendingMultiEvents)
            {
                if (this.PendingMultiEvents.Count > 0)
                {
                    this.NotificationManager.NotifyCharacter((int) this.CharacterID, "OnMultiEvent", 
                        new PyTuple(1) {[0] = this.PendingMultiEvents});

                    this.PendingMultiEvents = new PyList<PyTuple>();
                }
            }

            if (this.Session.IsDirty)
            {
                // send session change
                this.SendSessionChange();
            }
        }

        /// <summary>
        /// Sends an exception to a call performed by the client
        /// </summary>
        /// <param name="call">The call to answer with the exception</param>
        /// <param name="content">The contents of the exception</param>
        public void SendException(CallInformation call, PyDataType content)
        {
            this.MachoNet.SendException(call, content);
        }

        /// <summary>
        /// Checks session data to ensure a character is selected and returns it's characterID
        /// </summary>
        /// <returns>CharacterID for the client</returns>
        public int EnsureCharacterIsSelected()
        {
            int? characterID = this.CharacterID;

            if (characterID is null)
                throw new CustomError("NoCharacterSelected");

            return (int) characterID;
        }

        /// <summary>
        /// Checks session data to ensure the character is in a station
        /// </summary>
        /// <returns>The StationID where the character is at</returns>
        /// <exception cref="CanOnlyDoInStations"></exception>
        public int EnsureCharacterIsInStation()
        {
            int? stationID = this.StationID;

            if (stationID is null)
                throw new CanOnlyDoInStations();

            return (int) stationID;
        }
    }
}