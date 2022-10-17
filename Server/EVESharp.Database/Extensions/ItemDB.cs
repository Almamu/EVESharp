using System.Collections.Generic;
using System.Data;
using EVESharp.Database.Exceptions;
using EVESharp.Database.Extensions.Inventory;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Inventory.Types.Information;

namespace EVESharp.Database.Extensions;

public static class ItemDB
{
    public static void InvSetItemNode (this IDatabase Database, int itemID, long nodeID)
    {
        Database.QueryProcedure (
            "InvSetItemNode",
            new Dictionary <string, object>
            {
                {"_itemID", itemID},
                {"_nodeID", nodeID}
            }
        );
    }

    public static long InvGetItemNode (this IDatabase Database, int itemID)
    {
        return Database.Scalar <long> (
            "InvGetItemNode",
            new Dictionary <string, object> {{"_itemID", itemID}}
        );
    }

    public static void InvClearNodeAssociation (this IDatabase Database)
    {
        Database.QueryProcedure ("InvClearNodeAssociation");
    }

    public static void InvDestroyItem (this IDatabase Database, int itemID)
    {
        Database.QueryProcedure (
            "InvDestroyItem",
            new Dictionary <string, object>
            {
                {"_itemID", itemID}
            }
        );
    }
    
    public static IEnumerable <Item> InvGetStaticItems (this IDatabase Database, ITypes types, IAttributes attributes)
    {
        IDataReader reader = Database.Select (
            $"SELECT itemID, eveNames.itemName, invItems.typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM invItems LEFT JOIN eveNames USING(itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID < {ItemRanges.UserGenerated.MIN} AND (groupID = {(int) GroupID.Station} OR groupID = {(int) GroupID.Faction} OR groupID = {(int) GroupID.SolarSystem} OR groupID = {(int) GroupID.Corporation} OR groupID = {(int) GroupID.System})"
        );

        using (reader)
        {
            while (reader.Read () == true)
                yield return BuildItemFromReader (Database, reader, types, attributes);
        }
    }

    public static Item InvLoadItem (this IDatabase Database, int itemID, long nodeID, ITypes types, IAttributes attributes)
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

            if (ItemRanges.IsStaticData (itemID) == true)
                return newItem;

            if (reader.IsDBNull (13) == false && reader.GetInt32 (13) != 0)
                throw new ItemNotLoadedException (itemID, "Trying to load an item that is loaded on another node!");

            // update the database information
            Database.InvSetItemNode (itemID, nodeID);

            return newItem;
        }
    }

    private static Item BuildItemFromReader (IDatabase Database, IDataReader reader, ITypes types, IAttributes attributes)
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
            Attributes = new AttributeList (itemType, Database.InvDgmLoadAttributesForEntity (reader.GetInt32 (0), attributes))
        };
    }

    public static ulong InvCreateItem (
        this IDatabase Database,
        string itemName,  Type type, int? owner, int? location, Flags flag, bool contraband,
        bool singleton, int quantity, double? x, double? y, double? z, string customInfo)
    {
        ulong newItemID = Database.Insert (
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

    private static void InvSaveItemName (this IDatabase Database, ulong itemID, Type type, string itemName)
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
    
    private static void InvSaveItemPosition (this IDatabase Database, ulong itemID, double? x, double? y, double? z)
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
    // TODO: MOVE THIS TO THE EVE NAMESPACE AS IT'S NOT REALLY A DATABASE PROCEDURE IN ITSELF
    public static void InvPersistItem (this IDatabase Database, ulong id,
        string itemName,  Type type, int? owner, int? location, Flags flag, bool contraband,
        bool singleton, int quantity, double? x, double? y, double? z, string customInfo, bool hadName, bool hadPosition)
    {
        Database.Prepare (
            "UPDATE invItems SET typeID = @typeID, ownerID = @ownerID, locationID = @locationID, flag = @flag, contraband = @contraband, singleton = @singleton, quantity = @quantity, customInfo = @customInfo WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@typeID", type.ID},
                {"@ownerID", owner},
                {"@locationID", location},
                {"@flag", (int) flag},
                {"@contraband", contraband},
                {"@singleton", singleton},
                {"@quantity", quantity},
                {"@customInfo", customInfo},
                {"@itemID", id}
            }
        );

        // ensure naming information is up to date
        if (itemName is not null)
            Database.InvSaveItemName (id, type, itemName);
        else if (hadName)
            Database.Prepare (
                "DELETE FROM eveNames WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", id}}
            );

        if (x is not null && y is not null && z is not null)
            Database.InvSaveItemPosition (id, x, y, z);
        else if (hadPosition)
            Database.Prepare (
                "DELETE FROM invPositions WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", id}}
            );
    }

    public static uint InvItemsGetType (this IDatabase Database, int itemID)
    {
        return Database.Scalar <uint> (
            "InvItemsGetType",
            new Dictionary <string, object> ()
            {
                {"_itemID", itemID}
            }
        );
    }
}