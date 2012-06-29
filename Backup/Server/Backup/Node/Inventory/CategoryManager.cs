using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;
using Common;

namespace EVESharp.Inventory
{
    public class CategoryManager
    {
        private Dictionary<int, ItemCategory> categoryesDict = new Dictionary<int, ItemCategory>();

        public bool LoadCategories()
        {
            List<ItemCategory> categoryes = ItemDB.LoadItemCategories();

            if (categoryes == null)
            {
                return false;
            }

            for (int i = 0; i < categoryes.Count; i++)
            {
                try
                {
                    ItemCategory category = categoryes[i];
                    categoryesDict.Add(category.categoryID, category);
                }
                catch (Exception)
                {
                    Log.Error("CategoryManager", "Cannot load item category " + i + " from the list");
                }
            }

            return true;
        }

        public ItemCategory GetCategory(int categoryID)
        {
            try
            {
                return categoryesDict[categoryID];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
