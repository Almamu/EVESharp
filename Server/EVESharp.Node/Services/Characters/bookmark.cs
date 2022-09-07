using System;
using EVESharp.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class bookmark : Service
{
    public override AccessLevel          AccessLevel          => AccessLevel.None;
    private         IDatabaseConnection  Database             { get; }
    private         IItems               Items                { get; }
    private         IRemoteServiceManager RemoteServiceManager { get; }

    public bookmark (IDatabaseConnection connection, IItems items, IRemoteServiceManager remoteServiceManager)
    {
        Database             = connection;
        this.Items           = items;
        RemoteServiceManager = remoteServiceManager;
    }

    public PyDataType GetBookmarks (CallInformation call)
    {
        return Database.ChrBookmarksGet (call.Session.CharacterID);
    }

    public PyTuple BookmarkLocation (CallInformation call, PyInteger itemID, PyDataType unk, PyString name, PyString comment)
    {
        if (ItemRanges.IsStaticData (itemID) == false)
            throw new CustomError ("Bookmarks for non-static locations are not supported yet!");

        ItemEntity item = this.Items.GetItem (itemID);

        if (item.HasPosition == false)
            throw new CustomError ("Cannot bookmark a non-location item");

        ulong bookmarkID = Database.ChrBookmarksCreate (
            call.Session.CharacterID, itemID, item.Type.ID, name, comment, item.X ?? 0.0, item.Y ?? 0.0, item.Z ?? 0.0,
            item.LocationID
        );

        PyDataType bookmark = KeyVal.FromDictionary (
            new PyDictionary
            {
                ["bookmarkID"] = bookmarkID,
                ["itemID"]     = item.ID,
                ["typeID"]     = item.Type.ID,
                ["memo"]       = name,
                ["comment"]    = comment,
                ["x"]          = item.X,
                ["y"]          = item.Y,
                ["z"]          = item.Z,
                ["locationID"] = item.LocationID,
                ["created"]    = DateTime.UtcNow.ToFileTimeUtc ()
            }
        );

        // send a request to the client to update the bookmarks
        RemoteServiceManager.SendServiceCall (call.Session, "addressbook", "OnBookmarkAdd", new PyTuple (1) {[0] = bookmark}, new PyDictionary ());

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

    public PyDataType DeleteBookmarks (CallInformation call, PyList bookmarkIDs)
    {
        // if no ids are specified, there's nothing to do
        if (bookmarkIDs.Count == 0)
            return null;

        Database.ChrBookmarksDelete (call.Session.CharacterID, bookmarkIDs.GetEnumerable <PyInteger> ());

        return null;
    }
}