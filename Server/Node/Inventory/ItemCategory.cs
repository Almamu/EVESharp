using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVESharp.Inventory
{
    public class ItemCategory
    {
        public int categoryID { private set; get; }
        public string categoryName { private set; get; }
        public string categoryDescription { private set; get; }
        public int graphicID { private set; get; }
        public bool published { private set; get; }

        public ItemCategory(int invCategoryID, string invCategoryName, string invCategoryDescription, int invGraphicID, bool invPublished)
        {
            categoryID = invCategoryID;
            categoryName = invCategoryName;
            categoryDescription = invCategoryDescription;
            graphicID = invGraphicID;
            published = invPublished;
        }
    }
}
