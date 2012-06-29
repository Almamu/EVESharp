using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;
using Common;

namespace EVESharp.Inventory
{
    public class TypeManager
    {
        Dictionary<int, ItemType> itemTypes = new Dictionary<int, ItemType>();

        public bool LoadTypes()
        {
            List<ItemType> types = ItemDB.LoadItemTypes();

            if (types == null)
            {
                return false;
            }

            for (int i = 0; i < types.Count; i++)
            {
                try
                {
                    itemTypes.Add(types[i].typeID, types[i]);
                }
                catch (Exception)
                {
                    Log.Error("TypeManager", "Cannot load item type " + i + " from the list");
                }
            }

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
    }
}
