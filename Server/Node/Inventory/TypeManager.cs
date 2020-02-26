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
using System.Drawing;
using Common.Database;
using Node.Database;
using Node.Inventory.Items;

namespace Node.Inventory
{
    public class TypeManager
    {
        public ItemFactory ItemFactory { get; }
        private Dictionary<int, ItemType> mTypes = null;

        public void Load()
        {
            this.mTypes = this.ItemFactory.ItemDB.LoadItemTypes();
        }
        
        public ItemType this[int id] { get => this.mTypes[id]; }

        public TypeManager(ItemFactory factory)
        {
            this.ItemFactory = factory;
        }
    }
}