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

using System.Collections.Generic;
using Node.Database;

namespace Node.Inventory
{
    public class ItemInventory : Entity
    {
        public ItemInventory(string entityName, int entityId, ItemType entityType, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory)
            : base(entityName, entityId, entityType, entityOwnerID, entityLocationID, entityFlag, entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo, attributes, itemFactory)
        {
            loaded = LoadContents();
        }

        public ItemInventory(Entity from) : base(from.Name, from.ID, from.Type, from.OwnerID, from.LocationID, from.Flag, from.Contraband, from.Singleton, from.Quantity, from.X, from.Y, from.Z, from.CustomInfo, from.Attributes, from.mItemFactory)
        {
            loaded = LoadContents();
        }

        private bool LoadContents()
        {
            items = this.mItemFactory.ItemManager.LoadInventory(ID);

            if (items == null)
                return false;

            return true;
        }

        public void UpdateItem(Entity item)
        {
            foreach (Entity i in items)
            {
                if (i.ID == item.ID)
                {
                    items[items.IndexOf(i)] = item;
                }
            }
        }

        private List<Entity> items;
        public bool loaded { private set; get; }
    }
}