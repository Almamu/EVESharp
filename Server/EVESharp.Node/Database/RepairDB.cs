using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Database;

public class RepairDB : DatabaseAccessor
{
    public RepairDB (IDatabaseConnection db) : base (db) { }

    public ItemRepackageEntry GetItemToRepackage (int itemID, int ownerID, int locationID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            $"SELECT invItems.singleton, invItems.nodeID, IF(valueInt IS NULL, valueFloat, valueInt) AS damage, insuranceID, invItems.typeID, upgrades.itemID, invItems.locationID AS hasUpgrades FROM invItems LEFT JOIN invItems upgrades ON upgrades.locationID = invItems.itemID AND upgrades.flag >= {(int) Flags.RigSlot0} AND upgrades.flag <= {(int) Flags.RigSlot7} LEFT JOIN chrShipInsurances ON shipID = invItems.itemID LEFT JOIN invItemsAttributes ON invItemsAttributes.itemID = invItems.itemID AND invItemsAttributes.attributeID = @damageAttributeID WHERE invItems.itemID = @itemID AND invItems.locationID = @locationID AND invItems.ownerID = @ownerID",
            new Dictionary <string, object>
            {
                {"@locationID", locationID},
                {"@ownerID", ownerID},
                {"@itemID", itemID},
                {"@damageAttributeID", (int) AttributeTypes.damage}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new ItemRepackageEntry
            {
                ItemID      = itemID,
                NodeID      = reader.GetInt32 (1),
                Singleton   = reader.GetBoolean (0),
                Damage      = reader.GetDoubleOrDefault (2),
                HasContract = reader.IsDBNull (3) == false,
                TypeID      = reader.GetInt32 (4),
                HasUpgrades = reader.IsDBNull (5) == false,
                LocationID  = reader.GetInt32 (6)
            };
        }
    }

    public void RepackageItem (int itemID, int stationID)
    {
        // remove any rigs inside the item (if any)
        Database.Prepare (
            $"DELETE FROM invItems WHERE locationID = @itemID AND flag >= {(int) Flags.RigSlot0} AND flag <= {(int) Flags.RigSlot7}",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );

        // move the rest of the items inside the ship to the station the ship is at
        Database.Prepare (
            "UPDATE invItems SET locationID = @stationID, flag = @flag WHERE locationID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@stationID", stationID},
                {"@flag", (int) Flags.Hangar}
            }
        );

        // change singleton
        Database.Prepare (
            "UPDATE invItems SET singleton = 0 WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );
    }

    public class ItemRepackageEntry
    {
        public int    ItemID      { get; set; }
        public int    NodeID      { get; set; }
        public bool   Singleton   { get; set; }
        public double Damage      { get; set; }
        public bool   HasContract { get; set; }
        public int    TypeID      { get; set; }
        public bool   HasUpgrades { get; set; }
        public int    LocationID  { get; set; }
    }
}