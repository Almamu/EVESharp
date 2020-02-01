/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory;
using Node.Inventory.SystemEntities;

namespace Node.Database
{
    public class ItemDB : DatabaseAccessor
    {
        // General items database functions
        public List<Entity> LoadItems()
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity"
            );
            
            using(connection)
            using (reader)
            {
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
                        reader.IsDBNull(12) ? "" : reader.GetString(12), // customInfo
                        this,
                        null
                    );

                    items.Add(newItem);
                }

                return items;   
            }
        }

        public List<ItemCategory> LoadItemCategories()
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT categoryID, categoryName, description, graphicID, published FROM invCategories"
            );
            
            using(connection)
            using (reader)
            {
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

                return itemCategoryesList;
            }
        }

        public List<ItemType> LoadItemTypes()
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating FROM invTypes"
            );
            
            using(connection)
            using (reader)
            {
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

                return itemTypes;    
            }
        }

        public Entity LoadItem(int itemID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM entity WHERE itemID=" +
                itemID
            );
            {
                return null;
            }
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return null;

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
                    reader.GetString(12), // customInfo
                    this,
                    null
                );

                // Update the database information
                Database.Query("UPDATE entity SET nodeID=" + Program.NodeID + " WHERE itemID=" + itemID);

                return newItem;
            }
        }

        public List<int> GetInventoryItems(int inventoryID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection, "SELECT itemID FROM entity WHERE locationID=" + inventoryID);
            
            using (connection)
            using (reader)
            {
                List<int> itemList = new List<int>();

                while (reader.Read())
                {
                    itemList.Add(reader.GetInt32(0));
                }

                return itemList;   
            }
        }

        public int GetCategoryID(Entity item)
        {
            return GetCategoryID(item.typeID);
        }

        public int GetCategoryID(int typeID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT invCategories.categoryID FROM invCategories LEFT JOIN invTypes ON invTypes.typeID=" + typeID +
                " LEFT JOIN invGroups ON invGroups.groupID = invTypes.groupID WHERE invCategories.categoryID = invGroups.groupID"
            );
            
            using(connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;
                
                int categoryID = reader.GetInt32(0);
                
                return categoryID;
            }
        }

        public void SetBlueprintInfo(int itemID, bool copy, int materialLevel, int productivityLevel, int licensedProductionRunsRemaining)
        {
            Database.Query("UPDATE invBlueprints SET blueprintID=" + itemID + " copy=" + copy + " materialLevel=" + materialLevel + " licensedProductionRunsRemaining=" + licensedProductionRunsRemaining);
        }

        public Blueprint LoadBlueprint(int itemID)
        {
            Entity item = LoadItem(itemID);

            if (item == null)
                return null;
            
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT copy, materialLevel, productivityLevel, licensedProductionRunsRemaining FROM invBlueprints WHERE blueprintID=" +
                itemID
            );
            
            using(connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return null;
                
                Blueprint bp = new Blueprint(item);
                bp.SetBlueprintInfo(reader.GetBoolean(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), false);

                return bp;   
            }
        }

        public void SetCustomInfo(int itemID, string customInfo)
        {
            Database.Query("UPDATE entity SET customInfo='" + customInfo + "' WHERE itemID=" + itemID);
        }

        public void SetQuantity(int itemID, int quantity)
        {
            Database.Query("UPDATE entity SET quantity=" + quantity + " WHERE itemID=" + itemID);
        }

        public void SetSingleton(int itemID, bool singleton)
        {
            Database.Query("UPDATE entity SET singleton=" + singleton + " WHERE itemID=" + itemID);
        }

        public void SetItemName(int itemID, string name)
        {
            Database.Query("UPDATE entity SET itemName='" + name + "' WHERE itemID=" + itemID);
        }

        public void SetItemFlag(int itemID, int flag)
        {
            Database.Query("UPDATE entity SET flag=" + flag + " WHERE itemID=" + itemID);
        }

        public void SetLocation(int itemID, int locationID)
        {
            Database.Query("UPDATE entity SET locationID=" + locationID + " WHERE itemID=" + itemID);
        }

        public void SetOwner(int itemID, int ownerID)
        {
            Database.Query("UPDATE entity SET ownerID=" + ownerID + " WHERE itemID=" + itemID);
        }

        public ulong CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            return Database.QueryLID(
                String.Format(
                    "INSERT INTO entity(itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo)VALUES(NULL, '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{12}')",
                    Database.DoEscapeString(itemName), typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo
                )
            );
        }

        public List<Entity> GetItemsLocatedAt(int locationID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity WHERE locationID=" +
                locationID
            );
            
            using(connection)
            using (reader)
            {
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
                        reader.IsDBNull(12) ? "" : reader.GetString(12), // customInfo
                        this,
                        null
                    );

                    items.Add(newItem);
                }

                return items;
            }
        }

        public SolarSystemInfo GetSolarSystemInfo(int solarSystemID)
        {
            try
            {
                MySqlDataReader reader = null;
                MySqlConnection connection = null;
                
                Database.Query(ref reader, ref connection,
                    "SELECT regionID, constellationID, x, y, z, xMin, yMin, zMin, xMax, yMax, zMax, luminosity, border, fringe, corridor, hub, international, regional, constellation, security, factionID, radius, sunTypeID, securityClass FROM mapSolarSystems WHERE solarSystemID=" +
                    solarSystemID
                );
                
                using(connection)
                using (reader)
                {
                    if(reader.Read() == false)
                        throw new SolarSystemLoadException();

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
            }
            catch (Exception e)
            {
                throw new SolarSystemLoadException();
            }
        }

        public void UnloadItem(ulong itemID)
        {
            Database.Query("UPDATE entity SET nodeID=0 WHERE itemID=" + itemID);
        }

        public Dictionary<string, ItemAttribute> GetAttributesForItem(int itemID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT dgmAttributeTypes.attributeName, entity_attributes.attributeID, valueInt, valueFloat FROM entity_attributes LEFT JOIN dgmAttributeTypes ON dgmAttributeTypes.attributeID=entity_attributes.attributeID WHERE itemID=" +
                itemID
            );
            
            using(connection)
            using (reader)
            {

                Dictionary<string, ItemAttribute> result = new Dictionary<string, ItemAttribute>();

                while (reader.Read())
                {
                    ItemAttribute attrib = new ItemAttribute();

                    attrib.attributeID = reader.GetInt32(1);
                    attrib.intValue = (reader.IsDBNull(2) == true) ? 0 : reader.GetInt32(2);
                    attrib.floatValue = (reader.IsDBNull(3) == true) ? 0.0f : reader.GetFloat(3);

                    result.Add(reader.GetString(0), attrib);
                }

                return result;                
            }
        }

        public int GetAttributeIDForName(string attributeName)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT attributeID FROM dgmAttributeTypes WHERE attributeName='" + attributeName + "'"
            );
            
            using(connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;

                return reader.GetInt32(0);
            }
        }

        public Dictionary<string, ItemAttribute> GetDefaultAttributesForType(int typeID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT dgmAttributeTypes.attributeName, dgmTypeAttributes.attributeID, valueInt, valueFloat FROM dgmTypeAttributes LEFT JOIN dgmAttributeTypes ON dgmAttributeTypes.attributeID = dgmTypeAttributes.attributeID WHERE typeID=" +
                typeID
            );
            
            using(connection)
            using (reader)
            {
                Dictionary<string, ItemAttribute> result = new Dictionary<string, ItemAttribute>();

                while (reader.Read())
                {
                    ItemAttribute attrib = new ItemAttribute();

                    attrib.attributeID = reader.GetInt32(1);
                    attrib.intValue = (reader.IsDBNull(2) == true) ? 0 : reader.GetInt32(2);
                    attrib.floatValue = (reader.IsDBNull(3) == true) ? 0.0f : reader.GetFloat(3);

                    result.Add(reader.GetString(0), attrib);
                }

                return result;   
            }
        }

        public ItemDB(DatabaseConnection db) : base(db)
        {
        }
    }
}
