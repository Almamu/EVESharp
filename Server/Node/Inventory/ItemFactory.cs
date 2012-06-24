using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVESharp.Inventory
{
    public static class ItemFactory
    {
        private static ItemManager itemManager = new ItemManager();
        private static CategoryManager categoryManager = new CategoryManager();
        private static TypeManager typeManager = new TypeManager();

        public static bool LoadData()
        {
            if (itemManager.Load() == false)
            {
                return false;
            }

            if (categoryManager.LoadCategories() == false)
            {
                return false;
            }

            if (typeManager.LoadTypes() == false)
            {
                return false;
            }

            return true;
        }

        public static ItemManager GetItemManager()
        {
            return itemManager;
        }

        public static ItemCategory GetCategory(int categoryID)
        {
            return categoryManager.GetCategory(categoryID);
        }

        public static ItemType GetType(int typeID)
        {
            return typeManager.GetType(typeID);
        }
    }
}
