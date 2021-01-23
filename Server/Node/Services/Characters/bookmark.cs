using System.Collections.Generic;
using System.Runtime.InteropServices;
using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class bookmark : Service
    {
        private BookmarkDB DB { get; }
        private ItemManager ItemManager { get; }
        
        public bookmark(BookmarkDB db, ItemManager itemManager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
        }

        public PyDataType GetBookmarks(CallInformation call)
        {
            return this.DB.GetBookmarks(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType BookmarkLocation(PyInteger itemID, PyNone unk, PyString name, PyString comment, CallInformation call)
        {
            if (ItemManager.IsStaticData(itemID) == false)
            {
                throw new CustomError("Bookmarks for non-static locations are not supported yet!");
            }

            ItemEntity item = this.ItemManager.GetItem(itemID);

            ulong bookmarkID = this.DB.CreateBookmark(call.Client.EnsureCharacterIsSelected(), item.ID, item.Type.ID,
                name, comment, item.X, item.Y, item.Z, item.LocationID);
            
            // send a request to the client to update the bookmarks
            call.Client.ClusterConnection.SendServiceCall(call.Client, "addressbook", "OnBookmarkAdded",
                new PyTuple(0), new PyDictionary(), null, null, null, 0);
            
            return new PyTuple (7)
            {
                [0] = bookmarkID, // bookmarkID
                [1] = itemID, // itemID
                [2] = item.Type.ID, // typeID
                [3] = item.X, // x
                [4] = item.Y, // y
                [5] = item.Z, // z
                [6] = item.LocationID //locationID
            };
        }

        public PyDataType DeleteBookmarks(PyList bookmarkIDs, CallInformation call)
        {
            List<int> idsList = new List<int>();
            
            foreach (PyDataType bookmarkID in bookmarkIDs)
            {
                // ignore incorrect onews
                if (bookmarkID is PyInteger == false)
                    continue;
                
                idsList.Add(bookmarkID as PyInteger);
            }
            
            this.DB.DeleteBookmark(idsList, call.Client.EnsureCharacterIsSelected());

            return null;
        }
    }
}