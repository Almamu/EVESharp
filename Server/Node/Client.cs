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
using Common.Game;
using Common.Packets;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Node
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
        private PyList PendingNotifications { get; set; }
        
        public Client(NodeContainer container, ClusterConnection clusterConnection, ServiceManager serviceManager, ItemFactory itemFactory)
        {
            this.Container = container;
            this.ClusterConnection = clusterConnection;
            this.ServiceManager = serviceManager;
            this.ItemFactory = itemFactory;
            this.PendingNotifications = new PyList();
        }

        /// <summary>
        /// Updates the current client's session based of a session change notification
        /// (for example, coming from another node or from the cluster controller itself)
        /// </summary>
        /// <param name="packet">The packet that contains the session change notification</param>
        public void UpdateSession(PyPacket packet)
        {
            // load the new session data
            this.mSession.LoadChanges((packet.Payload[0] as PyTuple)[1] as PyDictionary);
            
            // now check for specific situations to properly increase/decrease counters
            // on various areas
            if (this.mSession.ContainsKey("charid") == false)
                return;

            if (this.mSession["charid"] is PyInteger == false)
                return;

            int characterID = this.mSession["charid"] as PyInteger;
            
            // has the player got into a station?
            if (this.mSession.ContainsKey("stationid") == true)
            {
                PyDataType newStationID = this.mSession["stationid"];
                PyDataType oldStationID = this.mSession.GetPrevious("stationid");
                int intNewStationID = 0;
                int intOldStationID = 0;

                if (oldStationID is PyInteger oldID)
                    intOldStationID = oldID;
                if (newStationID is PyInteger newID)
                    intNewStationID = newID;

                if (intNewStationID != intOldStationID)
                {
                    if (intNewStationID != 0)
                    {
                        Station station = this.ItemFactory.ItemManager.GetStation (intNewStationID);
                        SolarSystem solarSystem = this.ItemFactory.ItemManager.GetSolarSystem(station.SolarSystemID);

                        if (solarSystem.BelongsToUs == true)
                        {
                            station.Guests[characterID] = this.ItemFactory.ItemManager.LoadItem(characterID) as Character;

                            // notify station guests
                            this.ClusterConnection.SendNotification("OnCharNowInStation", "stationid", intNewStationID,
                                new PyTuple(1)
                                {
                                    [0] = new PyTuple(4)
                                    {
                                        [0] = characterID,
                                        [1] = this.CorporationID,
                                        [2] = this.AllianceID,
                                        [3] = this.WarFactionID
                                    }
                                }
                            );
                        }
                    }

                    if (intOldStationID != 0)
                    {
                        Station station = this.ItemFactory.ItemManager.GetStation(intOldStationID);
                        SolarSystem solarSystem = this.ItemFactory.ItemManager.GetSolarSystem(station.SolarSystemID);

                        if (solarSystem.BelongsToUs == true)
                        {
                            station.Guests.Remove(characterID);

                            // notify station guests
                            this.ClusterConnection.SendNotification("OnCharNoLongerInStation", "stationid", intOldStationID,
                                new PyTuple(1)
                                {
                                    [0] = new PyTuple(4)
                                    {
                                        [0] = characterID,
                                        [1] = this.CorporationID,
                                        [2] = this.AllianceID,
                                        [3] = this.WarFactionID
                                    }
                                }
                            );
                        }
                    }
                }
            }

            if (this.Session.ContainsKey("solarsystemid2") == true)
            {
                PyDataType newSolarSystemID2 = this.mSession["solarsystemid2"];
                PyDataType oldSolarSystemID2 = this.mSession.GetPrevious("solarsystemid2");
                int intNewSolarSystemID2 = 0;
                int intOldSolarSystemID2 = 0;

                if (oldSolarSystemID2 is PyInteger oldID)
                    intOldSolarSystemID2 = oldID;
                if (newSolarSystemID2 is PyInteger newID)
                    intNewSolarSystemID2 = newID;

                // load character if required
                if (intNewSolarSystemID2 != intOldSolarSystemID2 && intNewSolarSystemID2 > 0)
                {
                    SolarSystem solarSystem = this.ItemFactory.ItemManager.GetSolarSystem(intNewSolarSystemID2);

                    if (solarSystem.BelongsToUs == true)
                        this.ItemFactory.ItemManager.LoadItem(characterID);
                }
            }
        }

        /// <summary>
        /// Creates a session change notification including only the last changes to the session
        /// </summary>
        /// <param name="container">Information about the current node the client is in</param>
        /// <returns>The packet ready to be sent</returns>
        private PyPacket CreateEmptySessionChange(NodeContainer container)
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = Session.GenerateSessionChange();

            if (scn.changes.Length == 0)
                // Nothing to do
                return null;
            
            // add ourselves as nodes of interest
            // TODO: DETECT NODE OF INTEREST BASED ON THE SERVER THAT HAS THE SOLAR SYSTEM LOADED IN
            scn.nodesOfInterest.Add(container.NodeID);

            PyPacket packet = new PyPacket(PyPacket.PacketType.SESSIONCHANGENOTIFICATION);

            packet.Source = new PyAddressNode(container.NodeID);
            packet.Destination = new PyAddressClient(this.Session["userid"] as PyInteger);
            packet.UserID = this.Session["userid"] as PyInteger;

            packet.Payload = scn;

            packet.OutOfBounds = new PyDictionary()
            {
                {"channel", "sessionchange"}
            };

            return packet;
        }

        /// <summary>
        /// Prepares and sends a session change notification to this client
        /// </summary>
        public void SendSessionChange()
        {
            // build the session change
            PyPacket sessionChangeNotification = this.CreateEmptySessionChange(this.Container);
            
            // and finally send the client the required data
            this.ClusterConnection.Socket.Send(sessionChangeNotification);
            
            // do some checks for unloading data for the character of this client if required
            // TODO: FIND A BETTER PLACE TO DO THIS AS RIGHT NOW EVERYTIME THE PLAYER CHANGES SOLAR SYSTEM IT'S DATA
            // TODO: IS UNLOADED AND LOADED AGAIN, WHICH IS LESS THAN DESIRABLE
            if (this.Session.ContainsKey("solarsystemid2") == true)
            {
                PyDataType newSolarSystemID2 = this.mSession["solarsystemid2"];
                PyDataType oldSolarSystemID2 = this.mSession.GetPrevious("solarsystemid2");
                int intNewSolarSystemID2 = 0;
                int intOldSolarSystemID2 = 0;

                if (oldSolarSystemID2 is PyInteger oldID)
                    intOldSolarSystemID2 = oldID;
                if (newSolarSystemID2 is PyInteger newID)
                    intNewSolarSystemID2 = newID;

                if (intOldSolarSystemID2 > 0 && intOldSolarSystemID2 != intNewSolarSystemID2)
                {
                    SolarSystem oldSolarSystem = this.ItemFactory.ItemManager.GetSolarSystem(intOldSolarSystemID2);
                    SolarSystem newSolarSystem = this.ItemFactory.ItemManager.GetSolarSystem(intNewSolarSystemID2);

                    if (oldSolarSystem.BelongsToUs == false && newSolarSystem.BelongsToUs == true)
                    {
                        this.ItemFactory.ItemManager.LoadItem(this.EnsureCharacterIsSelected());
                    }
                    else if (oldSolarSystem.BelongsToUs == true && newSolarSystem.BelongsToUs == false)
                    {
                        this.ItemFactory.ItemManager.UnloadItem(this.EnsureCharacterIsSelected());
                    }
                }
            }
        }

        public Session Session => this.mSession;

        public string LanguageID
        {
            get => this.mSession["languageID"] as PyString;
        }

        public int AccountID
        {
            get => this.mSession["userid"] as PyInteger;
        }

        public int Role
        {
            get => this.mSession["role"] as PyInteger;
            set => this.mSession["role"] = value;
        }

        public string Address
        {
            get => this.mSession["address"] as PyString;
        }

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
        
        /// <summary>
        /// Sends an OnAccountChange notification to the client to let it know that their ISK
        /// balance has changed
        ///
        /// TODO: THIS SHOULD BE EXPANDED TO INCLUDE THE POSSIBILITY TO ALSO NOTIFY ABOUT CORP WALLET CHANGES
        /// </summary>
        /// <param name="balance">The new balance</param>
        public void NotifyBalanceUpdate(double balance)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
            {
                "cash", this.CharacterID, balance
            });
            
            this.ClusterConnection.SendNotification("OnAccountChange", "charid", (int) this.CharacterID, notification);
        }

        /// <summary>
        /// Sends an OnJumpCloneCacheInvalidated notification to the client to force it to re-fresh
        /// any clone information stored in cache
        /// </summary>
        public void NotifyCloneUpdate()
        {
            this.ClusterConnection.SendNotification("OnJumpCloneCacheInvalidated", "charid", (int) this.CharacterID, new PyTuple(0));
        }

        /// <summary>
        /// Notifies the client of a single attribute change on a specific item
        /// </summary>
        /// <param name="attribute">The attribute to notify about</param>
        /// <param name="item">The item to notify about</param>
        public void NotifyAttributeChange(ItemAttribute attribute, ItemEntity item)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
                {
                    attribute.Info.Name, item.GetEntityRow(), attribute
                }
            );

            this.ClusterConnection.SendNotification("OnAttribute", "charid", (int) this.CharacterID, notification);
        }

        /// <summary>
        /// Notifies the client of a multiple attribute change on different items
        /// </summary>
        /// <param name="attributes">The list of attributes that have changed</param>
        /// <param name="items">The list of items those attributes belong to</param>
        /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
        public void NotifyMultipleAttributeChange(ItemAttribute[] attributes, ItemEntity[] items)
        {
            if (attributes.Length != items.Length)
                throw new ArgumentOutOfRangeException(
                    "attributes list and items list must have the same amount of elements");

            PyList notification = new PyList();

            for (int i = 0; i < attributes.Length; i++)
            {
                notification.Add(new PyTuple(new PyDataType[]
                        {
                            attributes[i].Info.Name, items[i].GetEntityRow(), attributes[i]
                        }
                    )
                );
            }

            this.ClusterConnection.SendNotification("OnAttributes", "charid", (int) this.CharacterID, new PyTuple (new PyDataType [] { notification }));
        }

        /// <summary>
        /// Notifies the client of a change in the quantity on a specific item
        /// </summary>
        /// <param name="item">The item to notify about</param>
        /// <param name="oldQuantity">The old quantity the item had</param>
        public void NotifyItemQuantityChange(ItemEntity item, int oldQuantity)
        {
            PyDictionary changes = new PyDictionary
            {
                [(int) ItemChange.Quantity] = oldQuantity
            };

            this.NotifyItemChange(item, changes);
        }
        
        /// <summary>
        /// Notifies the client of a change in the location and/or flag on a specific item
        /// </summary>
        /// <param name="item">The item to notify about</param>
        /// <param name="oldFlag">The old value of the flag</param>
        /// <param name="oldLocation">The old location ID</param>
        public void NotifyItemLocationChange(ItemEntity item, ItemFlags oldFlag, int oldLocation)
        {
            PyDictionary changes = new PyDictionary();
            
            if (oldFlag != item.Flag)
                changes[(int) ItemChange.Flag] = (int) oldFlag;
            if (oldLocation != item.LocationID)
                changes[(int) ItemChange.LocationID] = oldLocation;

            this.NotifyItemChange(item, changes);
        }

        /// <summary>
        /// Notifies the client of a change on the singleton flag on a specific item
        /// </summary>
        /// <param name="item">The item to notify about</param>
        /// <param name="oldSingleton">The old value of the singleton flag</param>
        public void NotifySingletonChange(ItemEntity item, bool oldSingleton)
        {
            PyDictionary changes = new PyDictionary();

            if (oldSingleton != item.Singleton)
                changes[(int) ItemChange.Singleton] = oldSingleton;

            this.NotifyItemChange(item, changes);
        }

        /// <summary>
        /// Notifies the client of a item change in one of their inventories
        /// </summary>
        /// <param name="item">The item to notify about</param>
        /// <param name="changes">List of old data for the item that has changed</param>
        protected void NotifyItemChange(ItemEntity item, PyDictionary changes)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
            {
                item.GetEntityRow(), changes
            });

            this.ClusterConnection.SendNotification("OnItemChange", "charid", (int) this.CharacterID, notification);
        }

        /// <summary>
        /// Notifies this client of a new item in one of their inventories
        /// </summary>
        /// <param name="item">The item to notify about</param>
        public void NotifyNewItem(ItemEntity item)
        {
            this.NotifyItemLocationChange(item, ItemFlags.None, 0);
        }

        /// <summary>
        /// Queues a OnSkillTrained notification for this client
        /// </summary>
        /// <param name="skill"></param>
        public void NotifySkillTrained(Skill skill)
        {
            PyTuple onSkillTrained = new PyTuple(new PyDataType[]
                {
                    "OnSkillTrained", skill.ID
                }
            );

            this.PendingNotifications.Add(onSkillTrained);
        }

        /// <summary>
        /// Queues a OnSkillTrainingStopped notification for this client
        /// </summary>
        /// <param name="skill"></param>
        public void NotifySkillTrainingStopped(Skill skill)
        {
            PyTuple onSkillTrainingStopped = new PyTuple(new PyDataType[]
                {
                    "OnSkillTrainingStopped", skill.ID, 0
                }
            );
            
            this.PendingNotifications.Add(onSkillTrainingStopped);
        }

        /// <summary>
        /// Queues a OnSkillStartTraining notification for this client
        /// </summary>
        /// <param name="skill"></param>
        public void NotifySkillStartTraining(Skill skill)
        {
            PyTuple onSkillStartTraining = new PyTuple(new PyDataType[]
                {
                    "OnSkillStartTraining", skill.ID, skill.ExpiryTime
                }
            );

            this.PendingNotifications.Add(onSkillStartTraining);
        }

        /// <summary>
        /// Queues a OnSkillInjected notification for this client
        /// </summary>
        public void NotifySkillInjected()
        {
            PyTuple onSkillStartTraining = new PyTuple(new PyDataType[]
                {
                    "OnSkillInjected"
                }
            );

            this.PendingNotifications.Add(onSkillStartTraining);
        }

        /// <summary>
        /// Checks if there's any pending notifications and sends them to the client
        ///
        /// This includes sending session change notifications when there has been any changes to it
        /// </summary>
        public void SendPendingNotifications()
        {
            if (this.PendingNotifications.Count > 0)
            {
                this.ClusterConnection.SendNotification("OnMultiEvent", "charid", (int) this.CharacterID,
                    new PyTuple(1) {[0] = this.PendingNotifications});

                this.PendingNotifications = new PyList();
            }

            if (this.Session.IsDirty)
            {
                // send session change
                this.SendSessionChange();
            }
        }

        /// <summary>
        /// Sends a response to a call performed by the client
        /// </summary>
        /// <param name="answerTo">The call to answer to</param>
        /// <param name="result">The data to send as response</param>
        public void SendCallResponse(CallInformation answerTo, PyDataType result)
        {
            this.ClusterConnection.SendCallResult(answerTo, result);
        }

        /// <summary>
        /// Sends an exception to a call performed by the client
        /// </summary>
        /// <param name="call">The call to answer with the exception</param>
        /// <param name="content">The contents of the exception</param>
        public void SendException(CallInformation call, PyDataType content)
        {
            this.ClusterConnection.SendException(call, content);
        }

        /// <summary>
        /// Checks session data to ensure a character is selected and returns it's characterID
        /// </summary>
        /// <returns>CharacterID for the client</returns>
        public int EnsureCharacterIsSelected()
        {
            int? characterID = this.CharacterID;

            if (characterID == null)
                throw new UserError("NoCharacterSelected");

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

            if (stationID == null)
                throw new CanOnlyDoInStations();

            return (int) stationID;
        }
    }
}