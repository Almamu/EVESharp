using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters
{
    public class bookmark : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        private DatabaseConnection Database { get; init; }
        private ItemFactory ItemFactory { get; }
        
        public bookmark(DatabaseConnection connection, ItemFactory itemFactory)
        {
            this.Database = connection;
            this.ItemFactory = itemFactory;
        }

        public PyDataType GetBookmarks(CallInformation call)
        {
            return Database.Rowset(
                BookmarkDB.GET,
                new Dictionary<string, object>()
                {
                    {"_ownerID", call.Session.EnsureCharacterIsSelected()}
                }
            );
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

            ulong bookmarkID = Database.Scalar<ulong>(
                BookmarkDB.CREATE,
                new Dictionary<string, object>()
                {
                    {"_ownerID", call.Session.EnsureCharacterIsSelected()},
                    {"_itemID", itemID},
                    {"_typeID", item.Type.ID},
                    {"_memo", name},
                    {"_comment", comment},
                    {"_date", DateTime.UtcNow.ToFileTimeUtc ()},
                    {"_x", (double) item.X},
                    {"_y", (double) item.Y},
                    {"_z", (double) item.Z},
                    {"_locationID", item.LocationID}
                }
            );

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
            // TODO: SUPPORT REMOTE SERVICE CALLS AGAIN
            /*
            call.Client.Transport.SendServiceCall("addressbook", "OnBookmarkAdd",
                new PyTuple(1) {[0] = bookmark}, new PyDictionary(), null);
            */
            
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
            // if no ids are specified, there's nothing to do
            if (bookmarkIDs.Count == 0)
                return null;

            Database.Procedure(
                BookmarkDB.DELETE,
                new Dictionary<string, object>()
                {
                    {"_ownerID", call.Session.EnsureCharacterIsSelected()},
                    {"_bookmarkIDs", PyString.Join(',', bookmarkIDs.GetEnumerable<PyInteger>()).Value}
                }
            );

            return null;
        }
    }
}