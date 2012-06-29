using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Inventory;
using MySql.Data.MySqlClient;

using EVESharp.Inventory.SystemEntities;

namespace EVESharp.Database
{
    public static class ItemDB
    {
        // General items database functions
        public static List<Entity> LoadItems()
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity") == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            List<Entity> items = new List<Entity>();

            while (reader.Read())
            {
                Entity newItem = new Entity(reader.GetString(1), // itemName
                    reader.GetInt32(0), // itemID
                    reader.GetInt32(2), // typeID
                    reader.GetInt32(3), // ownerID
                    reader.GetInt32(4), // locationID
                    reader.GetInt32(5), // flag
                    reader.GetBoolean(6), // contraband
                    reader.GetBoolean(7), // singleton
                    reader.GetInt32(8), // quantity
                    reader.GetDouble(9), // x
                    reader.GetDouble(10), // y
                    reader.GetDouble(11), // z
                    reader.IsDBNull(12) ? "" : reader.GetString(12) // customInfo
                    );

                items.Add(newItem);
            }

            reader.Close();

            return items;
        }

        public static List<ItemCategory> LoadItemCategories()
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT categoryID, categoryName, description, graphicID, published FROM invCategories") == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            List<ItemCategory> itemCategoryesList = new List<ItemCategory>();

            while (reader.Read())
            {
                ItemCategory itemCategory = new ItemCategory(reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    reader.GetBoolean(4)
                    );

                itemCategoryesList.Add(itemCategory);
            }

            reader.Close();

            return itemCategoryesList;
        }

        public static List<ItemType> LoadItemTypes()
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating FROM invtypes") == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            List<ItemType> itemTypes = new List<ItemType>();

            while (reader.Read())
            {
                ItemType type = new ItemType(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetDouble(7),
                    reader.GetDouble(8),
                    reader.GetInt32(9),
                    reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                    reader.GetDouble(11),
                    reader.GetBoolean(12),
                    reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                    reader.GetDouble(14)
                    );

                itemTypes.Add(type);
            }

            reader.Close();

            return itemTypes;
        }

        public static Entity LoadItem(int itemID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM entity WHERE itemID=" + itemID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            if (reader.Read() == false)
            {
                reader.Close();
                return null;
            }

            Entity newItem = new Entity(reader.GetString(1), // itemName
                reader.GetInt32(0), // itemID
                reader.GetInt32(2), // typeID
                reader.GetInt32(3), // ownerID
                reader.GetInt32(4), // locationID
                reader.GetInt32(5), // flag
                reader.GetBoolean(6), // contraband
                reader.GetBoolean(7), // singleton
                reader.GetInt32(8), // quantity
                reader.GetDouble(9), // x
                reader.GetDouble(10), // y
                reader.GetDouble(11), // z
                reader.GetString(12) // customInfo
                );

            reader.Close();

            // Update the database information
            Database.Query("UPDATE entity SET nodeID=" + Program.GetNodeID() + " WHERE itemID=" + itemID);

            return newItem;
        }

        public static List<int> GetInventoryItems(int inventoryID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT itemID FROM entity WHERE locationID=" + inventoryID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            List<int> itemList = new List<int>();

            while (reader.Read())
            {
                itemList.Add(reader.GetInt32(0));
            }

            reader.Close();

            return itemList;
        }

        public static int GetCategoryID(Entity item)
        {
            return GetCategoryID(item.typeID);
        }

        public static int GetCategoryID(int typeID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT invCategories.categoryID FROM invCategories LEFT JOIN invTypes ON invTypes.typeID=" + typeID + " LEFT JOIN invGroups ON invGroups.groupID = invTypes.groupID WHERE invCategories.categoryID = invGroups.groupID") == false)
            {
                return 0;
            }

            if (reader == null)
            {
                return 0;
            }

            if (reader.Read() == false)
            {
                return 0;
            }

            int categoryID = reader.GetInt32(0);

            reader.Close();

            return categoryID;
        }

        public static void SetBlueprintInfo(int itemID, bool copy, int materialLevel, int productivityLevel, int licensedProductionRunsRemaining)
        {
            Database.Query("UPDATE invblueprints SET blueprintID=" + itemID + " copy=" + copy + " materialLevel=" + materialLevel + " licensedProductionRunsRemaining=" + licensedProductionRunsRemaining);
        }

        public static Blueprint LoadBlueprint(int itemID)
        {
            Entity item = LoadItem(itemID);

            if (item == null)
            {
                return null;
            }

            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT copy, materialLevel, productivityLevel, licensedProductionRunsRemaining FROM invBlueprints WHERE blueprintID=" + itemID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            if (reader.Read() == false)
            {
                reader.Close();
                return null;
            }

            Blueprint bp = new Blueprint(item);
            bp.SetBlueprintInfo(reader.GetBoolean(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), false);

            reader.Close();

            return bp;
        }

        public static void SetCustomInfo(int itemID, string customInfo)
        {
            Database.Query("UPDATE invtypes SET customInfo='" + customInfo + "' WHERE itemID=" + itemID);
        }

        public static void SetQuantity(int itemID, int quantity)
        {
            Database.Query("UPDATE invtypes SET quantity=" + quantity + " WHERE itemID=" + itemID);
        }

        public static void SetSingleton(int itemID, bool singleton)
        {
            Database.Query("UPDATE invtypes SET singleton=" + singleton + " WHERE itemID=" + itemID);
        }

        public static void SetItemName(int itemID, string name)
        {
            Database.Query("UPDATE invtypes SET itemName='" + name + "' WHERE itemID=" + itemID);
        }

        public static void SetItemFlag(int itemID, int flag)
        {
            Database.Query("UPDATE invtypes SET flag=" + flag + " WHERE itemID=" + itemID);
        }

        public static void SetLocation(int itemID, int locationID)
        {
            Database.Query("UPDATE invtypes SET locationID=" + locationID + " WHERE itemID=" + itemID);
        }

        public static void SetOwner(int itemID, int ownerID)
        {
            Database.Query("UPDATE invtypes SET ownerID=" + ownerID + " WHERE itemID=" + itemID);
        }

        public static ulong CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            ulong itemID = 0;

            if (Database.QueryLID(ref itemID, "INSERT INTO entity(itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo)VALUES(NULL, '" + itemName + "', " + typeID + ", " + ownerID + ", " + locationID + ", " + flag + ", " + contraband + ", " + singleton + ", " + quantity + ", " + x + ", " + y + ", " + z+ ", '" + customInfo + "')") == false)
            {
                return 0;
            }

            return itemID;
        }

        public static List<Entity> GetItemsLocatedAt(int locationID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity WHERE locationID=" + locationID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            List<Entity> items = new List<Entity>();

            while (reader.Read())
            {
                Entity newItem = new Entity(reader.GetString(1), // itemName
                    reader.GetInt32(0), // itemID
                    reader.GetInt32(2), // typeID
                    reader.GetInt32(3), // ownerID
                    reader.GetInt32(4), // locationID
                    reader.GetInt32(5), // flag
                    reader.GetBoolean(6), // contraband
                    reader.GetBoolean(7), // singleton
                    reader.GetInt32(8), // quantity
                    reader.GetDouble(9), // x
                    reader.GetDouble(10), // y
                    reader.GetDouble(11), // z
                    reader.IsDBNull(12) ? "" : reader.GetString(12) // customInfo
                    );

                items.Add(newItem);
            }

            reader.Close();

            return items;
        }

        public static SolarSystemInfo GetSolarSystemInfo(int solarSystemID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT regionID, constellationID, x, y, z, xMin, yMin, zMin, xMax, yMax, zMax, luminosity, border, fringe, corridor, hub, international, regional, constellation, security, factionID, radius, sunTypeID, securityClass FROM mapsolarsystems WHERE solarSystemID=" + solarSystemID) == false)
            {
                throw new SolarSystemLoadException();
            }

            if (reader == null)
            {
                throw new SolarSystemLoadException();
            }

            if (reader.Read() == false)
            {
                reader.Close();
                throw new SolarSystemLoadException();
            }

            SolarSystemInfo info = new SolarSystemInfo();

            info.regionID = reader.GetInt32(0);
            info.constellationID = reader.GetInt32(1);
            info.x = reader.GetDouble(2);
            info.y = reader.GetDouble(3);
            info.z = reader.GetDouble(4);
            info.xMin = reader.GetDouble(5);
            info.yMin = reader.GetDouble(6);
            info.zMin = reader.GetDouble(7);
            info.xMax = reader.GetDouble(8);
            info.yMax = reader.GetDouble(9);
            info.zMax = reader.GetDouble(10);
            info.luminosity = reader.GetDouble(11);
            info.border = reader.GetBoolean(12);
            info.fringe = reader.GetBoolean(13);
            info.corridor = reader.GetBoolean(14);
            info.hub = reader.GetBoolean(15);
            info.international = reader.GetBoolean(16);
            info.regional = reader.GetBoolean(17);
            info.constellation = reader.GetBoolean(18);
            info.security = reader.GetDouble(19);
            info.factionID = reader.GetInt32(20);
            info.radius = reader.GetDouble(21);
            info.sunTypeID = reader.GetInt32(22);
            info.securityClass = reader.GetString(23);

            return info;
        }

        public static void UnloadItem(ulong itemID)
        {
            Database.Query("UPDATE entity SET nodeID=0 WHERE itemID=" + itemID);
        }

        public static Dictionary<string, ItemAttribute> GetAttributesForItem(int itemID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT dgmattributetypes.attributeName, entity_attributes.attributeID, valueInt, valueFloat FROM entity_attributes LEFT JOIN dgmattributetypes ON dgmattributetypes.attributeID=entity_attributes.attributeID WHERE itemID=" + itemID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            Dictionary<string, ItemAttribute> result = new Dictionary<string, ItemAttribute>();

            while (reader.Read())
            {
                ItemAttribute attrib = new ItemAttribute();

                attrib.attributeID = reader.GetInt32(1);
                attrib.intValue = (reader.IsDBNull(2) == true) ? 0 : reader.GetInt32(2);
                attrib.floatValue = (reader.IsDBNull(3) == true) ? 0.0f : reader.GetFloat(3);

                result.Add(reader.GetString(0), attrib);
            }

            reader.Close();

            return result;
        }

        public static int GetAttributeIDForName(string attributeName)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT attributeID FROM dgmattributetypes WHERE attributeName='" + attributeName + "'") == false)
            {
                return 0;
            }

            if (reader == null)
            {
                return 0;
            }

            if (reader.Read() == false)
            {
                reader.Close();

                return 0;
            }

            int attributeID = reader.GetInt32(0);

            reader.Close();

            return attributeID;
        }

        public static Dictionary<string, ItemAttribute> GetDefaultAttributesForType(int typeID)
        {
            MySqlDataReader reader = null;

            if (Database.Query(ref reader, "SELECT dgmattributetypes.attributeName, dgmtypeattributes.attributeID, valueInt, valueFloat FROM dgmtypeattributes LEFT JOIN dgmattributetypes ON dgmattributetypes.attributeID = dgmtypeattributes.attributeID WHERE typeID=" + typeID) == false)
            {
                return null;
            }

            if (reader == null)
            {
                return null;
            }

            Dictionary<string, ItemAttribute> result = new Dictionary<string, ItemAttribute>();

            while (reader.Read())
            {
                ItemAttribute attrib = new ItemAttribute();

                attrib.attributeID = reader.GetInt32(1);
                attrib.intValue = (reader.IsDBNull(2) == true) ? 0 : reader.GetInt32(2);
                attrib.floatValue = (reader.IsDBNull(3) == true) ? 0.0f : reader.GetFloat(3);

                result.Add(reader.GetString(0), attrib);
            }

            reader.Close();

            return result;
        }
    }
}
