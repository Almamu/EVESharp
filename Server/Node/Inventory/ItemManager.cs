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
using Common.Logging;
using Node.Database;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;

namespace Node.Inventory
{
    public partial class ItemManager
    {
        private ItemFactory ItemFactory { get; }
        private Dictionary<int, ItemEntity> mItemList = new Dictionary<int, ItemEntity>();
        private Channel Log = null;

        public void Load()
        {
            // create a log channel for the rare occurence of the ItemManager wanting to log something
            this.Log = this.ItemFactory.Container.Logger.CreateLogChannel("ItemManager");
        }

        public ItemEntity LoadItem(int itemID)
        {
            if (IsItemLoaded(itemID) == false)
            {
                ItemEntity item = this.ItemFactory.ItemDB.LoadItem(itemID);

                if (item == null)
                    return null;

                switch ((ItemCategories) item.Type.Group.Category.ID)
                {
                    // celestial items are a kind of subcategory
                    // load them in specific ways based on the type of celestial item
                    case ItemCategories.Celestial:
                        return LoadCelestial(itemID, item.Type.Group.ID);

                    case ItemCategories.Blueprint:
                        return LoadBlueprint(itemID);

                    // owner items are a kind of subcategory too
                    case ItemCategories.Owner:
                        return LoadOwner(itemID, item.Type.Group.ID);
                    
                    // Not handled
                    default:
                        mItemList.Add(item.ID, item);
                        break;
                }

                return item;
            }
            else
            {
                return this.mItemList[itemID];
            }
        }

        public bool IsItemLoaded(int itemID)
        {
            return mItemList.ContainsKey(itemID);
        }

        private ItemEntity LoadCelestial(int itemID, int itemGroup)
        {
            switch ((ItemGroups) itemGroup)
            {
                case ItemGroups.SolarSystem:
                    return this.ItemFactory.ItemDB.LoadSolarSystem(itemID);
                
                default:
                    Log.Warning($"Loading celestial {itemID} from item group {itemGroup} as normal item");
                    return this.ItemFactory.ItemDB.LoadItem(itemID);
            }
        }
        
        private ItemEntity LoadBlueprint(int itemID)
        {
            Blueprint bp = this.ItemFactory.ItemDB.LoadBlueprint(itemID);

            if (bp == null)
                return null;

            mItemList.Add(bp.ID, bp);

            return bp;
        }

        private ItemEntity LoadOwner(int itemID,  int itemGroup)
        {
            switch (itemGroup)
            {
                case (int) ItemGroups.Character:
                    return this.ItemFactory.ItemDB.LoadCharacter(itemID);
                
                default:
                    Log.Warning($"Loading owner {itemID} from item group {itemGroup} as normal item");
                    return this.ItemFactory.ItemDB.LoadItem(itemID);
            }
        }

        public ItemEntity CreateSimpleItem(string itemName, int typeID, ItemEntity owner, ItemEntity location, ItemFlags flag,
            bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            int itemID = (int) this.ItemFactory.ItemDB.CreateItem(itemName, typeID, owner, location, flag, contraband, singleton, quantity, x, y, z, customInfo);

            return this.LoadItem(itemID);
        }

        public void UnloadItem(ItemEntity item)
        {
            try
            {
                mItemList.Remove(item.ID);

                // Update the database information
                this.ItemFactory.ItemDB.UnloadItem(item.ID);
            }
            catch
            {
            }
        }

        public ItemManager(ItemFactory factory)
        {
            this.ItemFactory = factory;
        }
    }
}