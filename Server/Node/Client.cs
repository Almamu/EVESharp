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
using Common.Game;
using Common.Packets;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class Client
    {
        private readonly Session mSession = new Session();

        public void UpdateSession(PyPacket packet)
        {
            this.mSession.LoadChanges((packet.Payload[0] as PyTuple)[1] as PyDictionary);
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
    }
}