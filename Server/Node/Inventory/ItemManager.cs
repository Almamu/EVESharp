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
    public class ItemManager
    {
        private ItemFactory ItemFactory { get; }
        private Dictionary<int, Entity> mItemList = new Dictionary<int, Entity>();

        public void Load()
        {
            // nothing is done on loading for now
        }

        public bool LoadItem(int itemID)
        {
            if (IsItemLoaded(itemID) == false)
            {
                Entity item = this.ItemFactory.ItemDB.LoadItem(itemID);

                if (item == null) return false;

                if (IsItemLoaded(item.LocationID))
                {
                    ItemInventory inv = (ItemInventory) mItemList[item.LocationID];
                    inv.UpdateItem(item);
                }

                switch ((ItemCategory) item.Type.Group.Category.ID)
                {
                    case ItemCategory.None:
                        break;

                    case ItemCategory.Blueprint:
                        return LoadBlueprint(itemID);

                    // Not handled
                    default:
                        mItemList.Add(item.ID, item);
                        break;
                }
            }

            return true;
        }

        public bool IsItemLoaded(int itemID)
        {
            return mItemList.ContainsKey(itemID);
        }

        enum ItemCategory
        {
            None = 0,
            Owner = 1,
            Celestial = 2,
            Station = 3,
            Material = 4,
            Accessories = 5,
            Ship = 6,
            Module = 7,
            Charge = 8,
            Blueprint = 9,
            Trading = 10,
            Entity = 11,
            Bonus = 14,
            Skill = 16,
            Commodity = 17,
            Drone = 18,
            Implant = 20,
            Deployable = 22,
            Structure = 23,
            Reaction = 24,
            Asteroid = 25,
            Interiors = 26,
            Placeables = 27,
            Abstract = 29,
            Subsystem = 32,
            AncientRelics = 34,
            Decryptors = 35,
        };

        public List<Entity> LoadInventory(int inventoryID)
        {
            List<int> items = this.ItemFactory.ItemDB.GetInventoryItems(inventoryID);
            List<Entity> loaded = new List<Entity>();

            if (items == null)
                return null;

            foreach (int itemID in items)
            {
                if (LoadItem(itemID) == true)
                {
                    try
                    {
                        loaded.Add(mItemList[itemID]);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return loaded;
        }

        private bool LoadBlueprint(int itemID)
        {
            Blueprint bp = this.ItemFactory.ItemDB.LoadBlueprint(itemID);

            if (bp == null)
                return false;

            mItemList.Add(bp.ID, bp);

            return true;
        }

        public Entity CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            int itemID = (int) this.ItemFactory.ItemDB.CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo);

            if (itemID == 0)
                return null;

            if (LoadItem((int) itemID) == false)
                return null;

            return mItemList[itemID];
        }

        public void UnloadItem(int itemID)
        {
            try
            {
                mItemList.Remove(itemID);

                // Update the database information
                this.ItemFactory.ItemDB.UnloadItem(itemID);
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