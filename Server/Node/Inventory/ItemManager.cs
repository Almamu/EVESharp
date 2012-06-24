using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;
using Common;

namespace EVESharp.Inventory
{
    public class ItemManager
    {
        private Dictionary<ulong, Entity> itemList = new Dictionary<ulong, Entity>();

        public bool Load()
        {
            List<Entity> items = ItemDB.LoadItems();

            if (items == null)
            {
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    itemList.Add((ulong)items[i].itemID, items[i]);
                }
                catch (Exception)
                {
                    Log.Error("ItemManager", "Cannot load item " + i + " from the list");
                }
            }

            foreach (KeyValuePair<ulong, Entity> item in itemList)
            {
                item.Value.LoadAttributes();
            }

            return true;
        }

        public bool LoadItem(int itemID)
        {
            if (IsItemLoaded(itemID) == false)
            {
                Entity item = ItemDB.LoadItem(itemID);

                if (item == null)
                {
                    return false;
                }

                if (IsItemLoaded(item.locationID))
                {
                    Inventory inv = (Inventory)itemList[(ulong)item.locationID];
                    inv.UpdateItem(item);
                }

                switch ((ItemCategory)ItemDB.GetCategoryID(item.typeID))
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
            List<int> items = ItemDB.GetInventoryItems(inventoryID);
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
            Blueprint bp = ItemDB.LoadBlueprint(itemID);
            
            if (bp == null)
            {
                return false;
            }

            itemList.Add((ulong)bp.itemID, bp);

            return true;
        }

        public Entity CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            ulong itemID = ItemDB.CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo);

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
                ItemDB.UnloadItem(itemID);
            }
            catch
            {

            }
        }
    }
}
