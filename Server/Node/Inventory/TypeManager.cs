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
using Common.Database;
using Node.Database;

namespace Node.Inventory
{
    public class TypeManager : DatabaseAccessor
    {
        private readonly ItemDB mItemDB = null;
        readonly Dictionary<int, ItemType> itemTypes = new Dictionary<int, ItemType>();

        public bool Load()
        {
            List<ItemType> types = this.mItemDB.LoadItemTypes();

            if (types == null)
                return false;

            foreach (ItemType type in types) 
                itemTypes.Add(type.typeID, type);

            return true;
        }

        public ItemType GetType(int typeID)
        {
            try
            {
                return itemTypes[typeID];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public TypeManager(DatabaseConnection db) : base(db)
        {
            this.mItemDB = new ItemDB(db);
        }
    }
}