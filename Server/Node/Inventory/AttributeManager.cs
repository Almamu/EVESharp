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

using System.Collections.Generic;
using Node.Database;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory
{
    public class AttributeManager
    {
        private ItemDB ItemDB { get; }
        private Dictionary<int, AttributeInfo> mAttributes = null;
        private Dictionary<int, Dictionary<int, ItemAttribute>> mDefaultAttributes = null;

        public Dictionary<int, Dictionary<int, ItemAttribute>> DefaultAttributes
        {
            get => this.mDefaultAttributes;
        }
        
        public AttributeManager(ItemDB itemDB)
        {
            this.ItemDB = itemDB;
        }
        
        public AttributeInfo this[int id] => this.mAttributes[id];
        public AttributeInfo this[AttributeEnum id] => this[(int) id];

        public void Load()
        {
            this.mAttributes = this.ItemDB.LoadAttributesInformation();
            this.mDefaultAttributes = this.ItemDB.LoadDefaultAttributes();
        }
    }
}