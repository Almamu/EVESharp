using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using EVESharp.Database.Exceptions;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types.Information;
using EVESharp.EVE.Database;

namespace EVESharp.Database;

public static class ItemDB
{
    public static void InvSetItemNode (this IDatabaseConnection Database, int itemID, long nodeID)
    {
        Database.Procedure (
            "InvSetItemNode",
            new Dictionary <string, object>
            {
                {"_itemID", itemID},
                {"_nodeID", nodeID}
            }
        );
    }

    public static long InvGetItemNode (this IDatabaseConnection Database, int itemID)
    {
        return Database.Scalar <long> (
            "InvGetItemNode",
            new Dictionary <string, object> {{"_itemID", itemID}}
        );
    }

    public static void InvClearNodeAssociation (this IDatabaseConnection Database)
    {
        Database.Procedure ("InvClearNodeAssociation");
    }

    public static void InvDestroyItem (this IDatabaseConnection Database, int itemID)
    {
        Database.Procedure (
            "InvDestroyItem",
            new Dictionary <string, object>
            {
                {"_itemID", itemID}
            }
        );
    }

    public static void InvDestroyItem (this IDatabaseConnection Database, ItemEntity item)
    {
        Database.InvDestroyItem (item.ID);
    }

    public static IEnumerable <Item> InvGetStaticItems (this IDatabaseConnection Database, ITypes types, IAttributes attributes)
    {
        IDataReader reader = Database.Select (
            $"SELECT itemID, eveNames.itemName, invItems.typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM invItems LEFT JOIN eveNames USING(itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID < {ItemRanges.USERGENERATED_ID_MIN} AND (groupID = {(int) GroupID.Station} OR groupID = {(int) GroupID.Faction} OR groupID = {(int) GroupID.SolarSystem} OR groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.System})"
        );

        using (reader)
        {
            while (reader.Read () == true)
                yield return BuildItemFromReader (Database, reader, types, attributes);
        }
    }

    public static Item InvLoadItem (this IDatabaseConnection Database, int itemID, long nodeID, ITypes types, IAttributes attributes)
    {
        IDataReader reader = Database.Select (
            "SELECT itemID, eveNames.itemName, invItems.typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo, nodeID FROM invItems LEFT JOIN eveNames USING (itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );

        using (reader)
        {
            if (reader.Read () == false)
                return null;

            Item newItem = BuildItemFromReader (Database, reader, types, attributes);

            if (itemID < ItemRanges.USERGENERATED_ID_MIN)
                return newItem;

            if (reader.IsDBNull (13) == false && reader.GetInt32 (13) != 0)
                throw new ItemNotLoadedException (itemID, "Trying to load an item that is loaded on another node!");

            // update the database information
            Database.InvSetItemNode (itemID, nodeID);

            return newItem;
        }
    }

    private static Item BuildItemFromReader (IDatabaseConnection Database, IDataReader reader, ITypes types, IAttributes attributes)
    {
        Type itemType = types [reader.GetInt32 (2)];

        return new Item
        {
            ID         = reader.GetInt32 (0),
            Name       = reader.GetStringOrNull (1),
            Type       = itemType,
            OwnerID    = reader.GetInt32 (3),
            LocationID = reader.GetInt32 (4),
            Flag       = (Flags) reader.GetInt32 (5),
            Contraband = reader.GetBoolean (6),
            Singleton  = reader.GetBoolean (7),
            Quantity   = reader.GetInt32 (8),
            X          = reader.GetDoubleOrNull (9),
            Y          = reader.GetDoubleOrNull (10),
            Z          = reader.GetDoubleOrNull (11),
            CustomInfo = reader.GetStringOrNull (12),
            Attributes = new AttributeList (itemType, Database.InvDgmLoadAttributesForEntity (0, attributes))
        };
    }

    public static ulong InvCreateItem (
        this IDatabaseConnection Database,
        string itemName,  Type type, int? owner, int? location, Flags flag, bool contraband,
        bool singleton, int quantity, double? x, double? y, double? z, string customInfo)
    {
        ulong newItemID = Database.PrepareLID (
            "INSERT INTO invItems(itemID, typeID, ownerID, locationID, flag, contraband, singleton, quantity, customInfo)VALUES(NULL, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @customInfo)",
            new Dictionary <string, object>
            {
                {"@typeID", type.ID},
                {"@ownerID", owner},
                {"@locationID", location},
                {"@flag", flag},
                {"@contraband", contraband},
                {"@singleton", singleton},
                {"@quantity", quantity},
                {"@customInfo", customInfo}
            }
        );

        if (itemName is not null)
            Database.InvSaveItemName (newItemID, type, itemName);

        if (x is not null && y is not null && z is not null)
            Database.InvSaveItemPosition (newItemID, x, y, z);

        return newItemID;
    }

    public static ulong InvCreateItem (
        this IDatabaseConnection Database,
        string itemName,  Type type, ItemEntity owner, ItemEntity location, Flags flag, bool contraband,
        bool singleton, int quantity, double? x, double? y, double? z, string customInfo)
    {
        return Database.InvCreateItem (
            itemName, type, owner?.ID, location?.ID, flag, contraband, singleton, quantity,
            x, y, z, customInfo
        );
    }

    private static void InvSaveItemName (this IDatabaseConnection Database, ulong itemID, Type type, string itemName)
    {
        // save item name if exists
        Database.Prepare (
            "REPLACE INTO eveNames (itemID, itemName, categoryID, groupID, typeID)VALUES(@itemID, @itemName, @categoryID, @groupID, @typeID)",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@itemName", itemName},
                {"@categoryID", type.Group.Category.ID},
                {"@groupID", type.Group.ID},
                {"@typeID", type.ID}
            }
        );
    }

    private static void InvSaveItemName (this IDatabaseConnection Database, ItemEntity item)
    {
        Database.InvSaveItemName ((ulong) item.ID, item.Type, item.Name);
    }
    
    private static void InvSaveItemPosition (this IDatabaseConnection Database, ulong itemID, double? x, double? y, double? z)
    {
        Database.Prepare (
            "REPLACE INTO invPositions (itemID, x, y, z)VALUES(@itemID, @x, @y, @z)",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@x", x},
                {"@y", y},
                {"@z", z}
            }
        );
    }

    private static void InvSaveItemPosition (this IDatabaseConnection Database, ItemEntity item)
    {
        Database.InvSaveItemPosition ((ulong) item.ID, item.X, item.Y, item.Z);
    }

    public static void InvPersistItem (this IDatabaseConnection Database, ItemEntity item)
    {
        Database.Prepare (
            "UPDATE invItems SET typeID = @typeID, ownerID = @ownerID, locationID = @locationID, flag = @flag, contraband = @contraband, singleton = @singleton, quantity = @quantity, customInfo = @customInfo WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemName", item.Name},
                {"@typeID", item.Type.ID},
                {"@ownerID", item.OwnerID},
                {"@locationID", item.LocationID},
                {"@flag", item.Flag},
                {"@contraband", item.Contraband},
                {"@singleton", item.Singleton},
                {"@quantity", item.Quantity},
                {"@customInfo", item.CustomInfo},
                {"@itemID", item.ID}
            }
        );

        // ensure naming information is up to date
        if (item.HasName)
            Database.InvSaveItemName (item);
        else if (item.HadName)
            Database.Prepare (
                "DELETE FROM eveNames WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", item.ID}}
            );

        if (item.HasPosition)
            Database.InvSaveItemPosition (item);
        else if (item.HadPosition)
            Database.Prepare (
                "DELETE FROM invPositions WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", item.ID}}
            );
    }
}