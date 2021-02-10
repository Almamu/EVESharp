using System;
using System.Collections.Generic;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

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

            if (item.HasPosition == false)
            {
                throw new CustomError("Cannot bookmark a non-location item");
            }

            ulong bookmarkID = this.DB.CreateBookmark(call.Client.EnsureCharacterIsSelected(), item.ID, item.Type.ID,
                name, comment, (double) item.X, (double) item.Y, (double) item.Z, item.LocationID);

            PyDataType bookmark = KeyVal.FromDictionary(new PyDictionary
                {
                    ["bookmarkID"] = bookmarkID,
                    ["itemID"] = item.ID,
                    ["typeID"] = item.Type.ID,
                    ["memo"] = name,
                    ["comment"] = comment,
                    ["x"] = item.X,
                    ["y"] = item.Y,
                    ["z"] = item.Z,
                    ["locationID"] = item.LocationID,
                    ["created"] = DateTime.UtcNow.ToFileTimeUtc()
                }
            );
            
            // send a request to the client to update the bookmarks
            call.Client.ClusterConnection.SendServiceCall(call.Client, "addressbook", "OnBookmarkAdd",
                new PyTuple(1) { [0] = bookmark }, new PyDictionary(), null, null, null, 0);
            
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