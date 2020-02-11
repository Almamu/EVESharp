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
using System.Linq;
using System.Text;
using Common;
using Common.Database;
using Node.Database;

namespace Node.Inventory
{
    public class ItemManager : DatabaseAccessor
    {
        private ItemDB mItemDB = null;
        private Dictionary<ulong, Entity> itemList = new Dictionary<ulong, Entity>();

        public bool Load()
        {
            List<Entity> items = this.mItemDB.LoadItems();

            if (items == null)
            {
                return false;
            }

            foreach (Entity item in items)
            {
                item.LoadAttributes();
                itemList.Add((ulong) item.itemID, item);
            }
            
            return true;
        }

        public bool LoadItem(int itemID)
        {
            if (IsItemLoaded(itemID) == false)
            {
                Entity item = this.mItemDB.LoadItem(itemID);

                if (item == null)
                {
                    return false;
                }

                if (IsItemLoaded(item.locationID))
                {
                    Inventory inv = (Inventory)itemList[(ulong)item.locationID];
                    inv.UpdateItem(item);
                }

                switch ((ItemCategory) this.mItemDB.GetCategoryID(item.typeID))
                {
                    case ItemCategory.None:
                        break;

                    case ItemCategory.Blueprint:
                        return LoadBlueprint(itemID);

                    // Not handled
                    default:
                        itemList.Add((ulong)item.itemID, item);
                        break;
                }
            }

            return true;
        }

        public bool IsItemLoaded(int itemID)
        {
            return itemList.ContainsKey((ulong)itemID);
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
            List<int> items = this.mItemDB.GetInventoryItems(inventoryID);
            List<Entity> loaded = new List<Entity>();

            if (items == null)
            {
                return null;
            }

            foreach (int itemID in items)
            {
                if (LoadItem(itemID) == true)
                {
                    try
                    {
                        loaded.Add(itemList[(ulong)itemID]);
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
            Blueprint bp = this.mItemDB.LoadBlueprint(itemID);
            
            if (bp == null)
            {
                return false;
            }

            itemList.Add((ulong)bp.itemID, bp);

            return true;
        }

        public Entity CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            ulong itemID = this.mItemDB.CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo);

            if (itemID == 0)
            {
                return null;
            }

            if (LoadItem((int)itemID) == false)
            {
                return null;
            }

            return itemList[itemID];
        }

        public void UnloadItem(ulong itemID)
        {
            try
            {
                itemList.Remove(itemID);
                
                // Update the database information
                this.mItemDB.UnloadItem(itemID);
            }
            catch
            {

            }
        }

        public ItemManager(DatabaseConnection db) : base(db)
        {
            this.mItemDB = new ItemDB(db);
        }
    }
}
