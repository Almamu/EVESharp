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
using Common.Game;
using Common.Packets;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class Client
    {
        private readonly Session mSession = new Session();
        
        private NodeContainer Container { get; }
        public ClusterConnection ClusterConnection { get; }
        public ServiceManager ServiceManager { get; }
        private ItemFactory ItemFactory { get; }
        
        public Client(NodeContainer container, ClusterConnection clusterConnection, ServiceManager serviceManager, ItemFactory itemFactory)
        {
            this.Container = container;
            this.ClusterConnection = clusterConnection;
            this.ServiceManager = serviceManager;
            this.ItemFactory = itemFactory;
        }

        public void UpdateSession(PyPacket packet)
        {
            this.mSession.LoadChanges((packet.Payload[0] as PyTuple)[1] as PyDictionary);
            
            // check for specific situations
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

                        station.Guests[characterID] = this.ItemFactory.ItemManager.LoadItem(characterID) as Character;
                    }

                    if (intOldStationID != 0)
                    {
                        Station station = this.ItemFactory.ItemManager.GetStation(intOldStationID);

                        station.Guests.Remove(characterID);
                    }
                }
            }
        }

        public PyPacket CreateEmptySessionChange(NodeContainer container)
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

            packet.NamedPayload = new PyDictionary()
            {
                {"channel", "sessionchange"}
            };

            return packet;
        }

        public void SendSessionChange()
        {
            // build the session change
            PyPacket sessionChangeNotification = this.CreateEmptySessionChange(this.Container);
            
            // and finally send the client the required data
            this.ClusterConnection.Socket.Send(sessionChangeNotification);
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
        
        public void NotifyBalanceUpdate(double balance)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
            {
                "cash", this.CharacterID, balance
            });
            
            this.ClusterConnection.SendNotification("OnAccountChange", "charid", (int) this.CharacterID, notification);
        }

        public void NotifyCloneUpdate()
        {
            this.ClusterConnection.SendNotification("OnJumpCloneCacheInvalidated", "charid", (int) this.CharacterID, new PyTuple(0));
        }

        public void NotifyAttributeChange(ItemAttribute attribute, ItemEntity item)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
                {
                    attribute.Info.Name, item.GetEntityRow(), attribute
                }
            );

            this.ClusterConnection.SendNotification("OnAttribute", "charid", (int) this.CharacterID, notification);
        }

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

        public void NotifyItemQuantityChange(ItemEntity item, int oldQuantity)
        {
            PyDictionary changes = new PyDictionary
            {
                [(int) ItemChange.Quantity] = oldQuantity
            };

            this.NotifyItemChange(item, changes);
        }
        public void NotifyItemLocationChange(ItemEntity item, ItemFlags oldFlag, int oldLocation)
        {
            PyDictionary changes = new PyDictionary();
            
            if (oldFlag != item.Flag)
                changes[(int) ItemChange.Flag] = (int) oldFlag;
            if (oldLocation != item.LocationID)
                changes[(int) ItemChange.LocationID] = oldLocation;

            this.NotifyItemChange(item, changes);
        }

        protected void NotifyItemChange(ItemEntity item, PyDictionary changes)
        {
            PyTuple notification = new PyTuple(new PyDataType[]
            {
                item.GetEntityRow(), changes
            });

            this.ClusterConnection.SendNotification("OnItemChange", "charid", (int) this.CharacterID, notification);
        }

        public void NotifyNewItem(ItemEntity item)
        {
            this.NotifyItemLocationChange(item, ItemFlags.None, 0);
        }

        public void NotifySkillTrained(Skill skill)
        {
            PyTuple onSkillTrained = new PyTuple(new PyDataType[]
                {
                    "OnSkillTrained", skill.ID
                }
            );
            
            // TODO: THESE SEEM TO BE TIED TO DESTINY FOR ONE REASON OR ANOTHER
            PyTuple eventTuple = new PyTuple(new PyDataType[]
                {
                    (PyList) new PyDataType[]
                    {
                        onSkillTrained
                    }
                }
            );
            
            this.ClusterConnection.SendNotification("OnMultiEvent", "charid", (int) this.CharacterID, eventTuple);
        }

        public void NotifySkillTrainingStopped(Skill skill)
        {
            PyTuple onSkillTrainingStopped = new PyTuple(new PyDataType[]
                {
                    "OnSkillTrainingStopped", skill.ID, 0
                }
            );

            PyTuple eventTuple = new PyTuple(new PyDataType[]
                {
                    (PyList) new PyDataType[]
                    {
                        onSkillTrainingStopped
                    }
                }
            );

            this.ClusterConnection.SendNotification("OnMultiEvent", "charid", (int) this.CharacterID, eventTuple);
        }

        public void NotifySkillStartTraining(Skill skill)
        {
            PyTuple onSkillStartTraining = new PyTuple(new PyDataType[]
                {
                    "OnSkillStartTraining", skill.ID, skill.ExpiryTime
                }
            );

            PyTuple eventTuple = new PyTuple(new PyDataType[]
                {
                    (PyList) new PyDataType[]
                    {
                        onSkillStartTraining
                    }
                }
            );

            this.ClusterConnection.SendNotification("OnMultiEvent", "charid", (int) this.CharacterID, eventTuple);
        }

        public void NotifySkillInjected()
        {
            PyTuple onSkillStartTraining = new PyTuple(new PyDataType[]
                {
                    "OnSkillInjected"
                }
            );

            PyTuple eventTuple = new PyTuple(new PyDataType[]
                {
                    (PyList) new PyDataType[]
                    {
                        onSkillStartTraining
                    }
                }
            );

            this.ClusterConnection.SendNotification("OnMultiEvent", "charid", (int) this.CharacterID, eventTuple);

        }

        public void SendCallResponse(CallInformation answerTo, PyDataType result)
        {
            this.ClusterConnection.SendCallResult(answerTo, result);
        }

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
    }
}