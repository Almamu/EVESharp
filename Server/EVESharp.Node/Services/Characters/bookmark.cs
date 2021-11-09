using System;
using System.Collections.Generic;
using EVESharp.Common.Services;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters
{
    public class bookmark : IService
    {
        private BookmarkDB DB { get; }
        private ItemFactory ItemFactory { get; }
        private MachoNet MachoNet { get; }
        
        public bookmark(BookmarkDB db, ItemFactory itemFactory, MachoNet machoNet)
        {
            this.DB = db;
            this.ItemFactory = itemFactory;
            this.MachoNet = machoNet;
        }

        public PyDataType GetBookmarks(CallInformation call)
        {
            return this.DB.GetBookmarks(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType BookmarkLocation(PyInteger itemID, PyDataType unk, PyString name, PyString comment, CallInformation call)
        {
            if (ItemFactory.IsStaticData(itemID) == false)
            {
                throw new CustomError("Bookmarks for non-static locations are not supported yet!");
            }

            ItemEntity item = this.ItemFactory.GetItem(itemID);

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
            this.MachoNet.SendServiceCall(call.Client, "addressbook", "OnBookmarkAdd",
                new PyTuple(1) {[0] = bookmark}, new PyDictionary(), null);
            
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
            this.DB.DeleteBookmark(bookmarkIDs.GetEnumerable<PyInteger>(), call.Client.EnsureCharacterIsSelected());

            return null;
        }
    }
}