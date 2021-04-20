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
using Common.Logging;
using Node.Database;
using Node.Dogma;
using Node.Inventory.Exceptions;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;
using Node.StaticData;
using Node.StaticData.Inventory;
using Type = Node.StaticData.Inventory.Type;

namespace Node.Inventory
{
    public class ItemManager
    {
        private Channel Log { get; }
        private NodeContainer NodeContainer { get; }
        public ItemManager(Logger logger, NodeContainer nodeContainer)
        {
            // create a log channel for the rare occurence of the ItemManager wanting to log something
            this.Log = logger.CreateLogChannel("ItemManager");
            this.NodeContainer = nodeContainer;
        }
    }
}