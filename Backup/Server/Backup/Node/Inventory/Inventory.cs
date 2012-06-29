using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVESharp.Inventory
{
    public class Inventory : Entity
    {
        public Inventory(string entityItemName, int entityItemID, int entityTypeID, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo)
            : base(entityItemName, entityItemID, entityTypeID, entityOwnerID, entityLocationID, entityFlag, entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo)
        {
            loaded = LoadContents();
            
        }

        public Inventory(Entity from) : base(from.itemName, from.itemID, from.typeID, from.ownerID, from.locationID, from.flag, from.contraband, from.singleton, from.quantity, from.x, from.y, from.Z, from.customInfo)
        {
            loaded = LoadContents();
        }

        private bool LoadContents()
        {
            items = ItemFactory.GetItemManager().LoadInventory(itemID);

            if (items == null)
            {
                return false;
            }

            return true;
        }

        public void UpdateItem(Entity item)
        {
            foreach (Entity i in items)
            {
                if (i.itemID == item.itemID)
                {
                    items[items.IndexOf(i)] = item;
                }
            }
        }

        private List<Entity> items;
        public bool loaded { private set; get; }
    }
}
