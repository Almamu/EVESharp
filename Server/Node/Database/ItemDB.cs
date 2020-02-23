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
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory;
using Node.Inventory.SystemEntities;
using Org.BouncyCastle.X509.Extension;

namespace Node.Database
{
    public class ItemDB : DatabaseAccessor
    {
        private ItemFactory mItemFactory = null;
        
        // General items database functions
        public Dictionary<int, Entity> LoadItems()
        {
            MySqlConnection connection = null;

            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity"
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, Entity> items = new Dictionary<int, Entity>();

                while (reader.Read())
                {
                    ItemType itemType = this.mItemFactory.TypeManager[reader.GetInt32(3)];
                    
                    Entity newItem = new Entity(
                        reader.GetString(1), // itemName
                        reader.GetInt32(0), // itemID
                        itemType, // typeID
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
                        new AttributeList(
                            this.mItemFactory,
                            itemType,
                            this.LoadAttributesForItem(reader.GetInt32(0))
                        ),
                        this.mItemFactory
                    );

                    items[newItem.ID] = newItem;
                }

                return items;
            }
        }

        public Dictionary<int, ItemCategory> LoadItemCategories()
        {
            MySqlConnection connection = null;

            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT categoryID, categoryName, description, graphicID, published FROM invCategories"
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, ItemCategory> itemCategories = new Dictionary<int, ItemCategory>();

                while (reader.Read())
                {
                    ItemCategory itemCategory = new ItemCategory(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        reader.GetBoolean(4)
                    );

                    itemCategories[itemCategory.ID] = itemCategory;
                }

                return itemCategories;
            }
        }

        public Dictionary<int, ItemType> LoadItemTypes()
        {
            MySqlConnection connection = null;

            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating FROM invTypes"
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, ItemType> itemTypes = new Dictionary<int, ItemType>();

                while (reader.Read())
                {
                    int typeID = reader.GetInt32(0);
                    
                    Dictionary<int, ItemAttribute> defaultAttributes = null;

                    if (this.mItemFactory.AttributeManager.DefaultAttributes.ContainsKey(typeID) == true)
                        defaultAttributes = this.mItemFactory.AttributeManager.DefaultAttributes[typeID];
                    else
                        defaultAttributes = new Dictionary<int, ItemAttribute>();
                    
                    ItemType type = new ItemType(
                        typeID,
                        this.mItemFactory.GroupManager[reader.GetInt32(1)],
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
                        reader.GetDouble(14),
                        defaultAttributes
                    );

                    itemTypes[type.ID] = type;
                }

                return itemTypes;
            }
        }

        public Dictionary<int, ItemGroup> LoadItemGroups()
        {
            MySqlConnection connection = null;

            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT groupID, categoryID, groupName, description, graphicID, useBasePrice, allowManufacture, allowRecycler, anchored, anchorable, fittableNonSingleton, published FROM invGroups"
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, ItemGroup> itemGroups = new Dictionary<int, ItemGroup>();

                while (reader.Read() == true)
                {
                    ItemGroup group = new ItemGroup(
                        reader.GetInt32(0),
                        this.mItemFactory.CategoryManager[reader.GetInt32(1)],
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        reader.GetBoolean(5),
                        reader.GetBoolean(6),
                        reader.GetBoolean(7),
                        reader.GetBoolean(8),
                        reader.GetBoolean(9),
                        reader.GetBoolean(10),
                        reader.GetBoolean(11)
                    );

                    itemGroups[group.ID] = group;
                }

                return itemGroups;
            }
        }

        public Dictionary<int, AttributeInfo> LoadAttributesInformation()
        {
            MySqlConnection connection = null;

            // sort the attributes by maxAttributeID so the simple attributes are loaded first
            // and then the complex ones that are related to other attributes
            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT attributeID, attributeName, attributeCategory, description, maxAttributeID, attributeIdx, graphicID, chargeRechargeTimeID, defaultValue, published, displayName, unitID, stackable, highIsGood, categoryID FROM dgmAttributeTypes ORDER BY maxAttributeID ASC"
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, AttributeInfo> attributes = new Dictionary<int, AttributeInfo>();

                while (reader.Read())
                {
                    AttributeInfo info = new AttributeInfo(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetString(3),
                        reader.IsDBNull(4) ? null : attributes[reader.GetInt32(4)],
                        reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                        reader.GetDouble(8),
                        reader.GetInt32(9),
                        reader.GetString(10),
                        reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        reader.GetInt32(12),
                        reader.GetInt32(13),
                        reader.IsDBNull(14) ? 0 : reader.GetInt32(14)
                    );

                    attributes[info.ID] = info;
                }

                return attributes;
            }
        }

        public Dictionary<int, Dictionary<int, ItemAttribute>> LoadDefaultAttributes()
        {
            MySqlConnection connection = null;
            
            MySqlDataReader reader = Database.Query(ref connection, 
                "SELECT typeID, attributeID, valueInt, valueFloat FROM dgmTypeAttributes"
            );
            
            using(connection)
            using (reader)
            {
                Dictionary<int, Dictionary<int, ItemAttribute>> attributes = new Dictionary<int, Dictionary<int, ItemAttribute>>();

                while (reader.Read() == true)
                {
                    int typeID = reader.GetInt32(0);
                    
                    if(attributes.ContainsKey(typeID) == false)
                        attributes[typeID] = new Dictionary<int, ItemAttribute>();
                    
                    ItemAttribute attribute = null;
                    
                    if (reader.IsDBNull(2) == false)
                    {
                        attribute = new ItemAttribute(
                            this.mItemFactory.AttributeManager[reader.GetInt32(1)],
                            reader.GetInt32(2)
                        );
                    }
                    else
                    {
                        attribute = new ItemAttribute(
                            this.mItemFactory.AttributeManager[reader.GetInt32(1)],
                            reader.GetInt32(3)
                        );
                    }

                    attributes[typeID][attribute.Info.ID] = attribute;
                }

                return attributes;
            }
        }

        public Entity LoadItem(int itemID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM entity WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );

            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return null;

                ItemType itemType = this.mItemFactory.TypeManager[reader.GetInt32(2)];
                
                Entity newItem = new Entity(reader.GetString(1), // itemName
                    reader.GetInt32(0), // itemID
                    itemType, // typeID
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
                    new AttributeList(
                        this.mItemFactory,
                        itemType,
                        this.LoadAttributesForItem(reader.GetInt32(0))
                    ), 
                    this.mItemFactory
                );

                // Update the database information
                Database.PrepareQuery(
                    "UPDATE entity SET nodeID = @nodeID WHERE itemID = @itemID",
                    new Dictionary<string, object>()
                    {
                        {"@nodeID", Program.NodeID},
                        {"@itemID", itemID}
                    }
                );

                return newItem;
            }
        }

        public List<int> GetInventoryItems(int inventoryID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemID FROM entity WHERE locationID = @inventoryID",
                new Dictionary<string, object>()
                {
                    {"@inventoryID", inventoryID}
                }
            );

            using (connection)
            using (reader)
            {
                List<int> itemList = new List<int>();

                while (reader.Read()) itemList.Add(reader.GetInt32(0));

                return itemList;
            }
        }
        
        public int GetCategoryID(int typeID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT invCategories.categoryID FROM invCategories LEFT JOIN invTypes ON invTypes.typeID = @typeID LEFT JOIN invGroups ON invGroups.groupID = invTypes.groupID WHERE invCategories.categoryID = invGroups.groupID",
                new Dictionary<string, object>()
                {
                    {"@typeID",typeID}
                }
            );

            using (connection)
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
            Database.PrepareQuery(
                "UPDATE invBlueprints SET copy = @copy, materialLevel = @materialLevel, productivityLevel = @productivityLevel, licensedProductionRunsRemaining = @licensedProductionRunsRemaining WHERE blueprintID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID},
                    {"@copy", copy},
                    {"@materialLevel", materialLevel},
                    {"@productivityLevel", productivityLevel},
                    {"@licensedProductionRunsRemaining", licensedProductionRunsRemaining}
                }
            );
        }

        public Blueprint LoadBlueprint(int itemID)
        {
            Entity item = LoadItem(itemID);

            if (item == null)
                return null;

            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT copy, materialLevel, productivityLevel, licensedProductionRunsRemaining FROM invBlueprints WHERE blueprintID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );

            using (connection)
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
            Database.PrepareQuery("UPDATE entity SET customInfo = @customInfo WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@itemID", itemID},
                {"@customInfo", customInfo}
            });
        }

        public void SetQuantity(int itemID, int quantity)
        {
            Database.PrepareQuery("UPDATE entity SET quantity = @quantity WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@quantity", quantity},
                {"@itemID", itemID}
            });
        }

        public void SetSingleton(int itemID, bool singleton)
        {
            Database.PrepareQuery("UPDATE entity SET singleton = @singleton WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@singleton", singleton},
                {"@itemID", itemID}
            });
        }

        public void SetItemName(int itemID, string name)
        {
            Database.PrepareQuery("UPDATE entity SET itemName = @itemName WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@itemName", name},
                {"@itemID", itemID}
            });
        }

        public void SetItemFlag(int itemID, int flag)
        {
            Database.PrepareQuery("UPDATE entity SET flag = @flag WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@flag", flag},
                {"@itemID", itemID}
            });
        }

        public void SetLocation(int itemID, int locationID)
        {
            Database.PrepareQuery("UPDATE entity SET locationID = @locationID WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@locationID", locationID},
                {"@itemID", itemID}
            });
        }

        public void SetOwner(int itemID, int ownerID)
        {
            Database.PrepareQuery("UPDATE entity SET ownerID = @ownerID WHERE itemID = @itemID", new Dictionary<string, object>()
            {
                {"@ownerID", ownerID},
                {"@itemID", itemID}
            });
        }

        public ulong CreateItem(string itemName, int typeID, int ownerID, int locationID, int flag, bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO entity(itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo)VALUES(NULL, @itemName, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @x, @y, @z, @customInfo)",
                new Dictionary<string, object>()
                {
                    {"@itemName", itemName},
                    {"@typeID", typeID},
                    {"@ownerID", ownerID},
                    {"@locationID", locationID},
                    {"@flag", flag},
                    {"@contraband", contraband},
                    {"@singleton", singleton},
                    {"@quantity", quantity},
                    {"@x", x},
                    {"@y", y},
                    {"@z", z},
                    {"@customInfo", customInfo}
                }
            );
        }

        public List<Entity> GetItemsLocatedAt(int locationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo FROM entity WHERE locationID = @locationID",
                new Dictionary<string, object>()
                {
                    {"@locationID", locationID}
                }
            );

            using (connection)
            using (reader)
            {
                List<Entity> items = new List<Entity>();

                while (reader.Read())
                {
                    ItemType itemType = this.mItemFactory.TypeManager[reader.GetInt32(1)];
                    
                    Entity newItem = new Entity(reader.GetString(1), // itemName
                        reader.GetInt32(0), // itemID
                        itemType, // typeID
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
                        new AttributeList(
                            this.mItemFactory,
                            itemType,
                            this.LoadAttributesForItem(reader.GetInt32(0))
                        ), 
                        this.mItemFactory
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
                MySqlConnection connection = null;
                MySqlDataReader reader = Database.PrepareQuery(ref connection,
                    "SELECT regionID, constellationID, x, y, z, xMin, yMin, zMin, xMax, yMax, zMax, luminosity, border, fringe, corridor, hub, international, regional, constellation, security, factionID, radius, sunTypeID, securityClass FROM mapSolarSystems WHERE solarSystemID = @solarSystemID",
                    new Dictionary<string, object>()
                    {
                        {"@solarSystemID", solarSystemID}
                    }
                );

                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
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

        public void UnloadItem(int itemID)
        {
            Database.PrepareQuery("UPDATE entity SET nodeID = 0 WHERE itemID = @itemID", new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );
        }

        public Dictionary<int, ItemAttribute> LoadAttributesForItem(int itemID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT attributeID, valueInt, valueFloat FROM entity_attributes WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );

            using (connection)
            using (reader)
            {
                Dictionary<int, ItemAttribute> result = new Dictionary<int, ItemAttribute>();

                while (reader.Read())
                {
                    ItemAttribute attribute = null;

                    if (reader.IsDBNull(1) == true)
                    {
                        attribute = new ItemAttribute(
                            this.mItemFactory.AttributeManager[reader.GetInt32(0)],
                            reader.GetDouble(2)
                        );
                    }
                    else
                    {
                        attribute = new ItemAttribute(
                            this.mItemFactory.AttributeManager[reader.GetInt32(0)],
                            reader.GetInt32(2)
                        );
                    }

                    result[attribute.Info.ID] = attribute;
                }

                return result;
            }
        }

        /// <summary>
        /// Saves an entity to the database
        /// </summary>
        /// <param name="entity"></param>
        public void PersistEntity(Entity entity)
        {
            Database.PrepareQuery(
                "UPDATE entity SET itemName = @itemName, ownerID = @ownerID, locationID = @locationID, flag = @flag, contraband = @contraband, singleton = @singleton, quantity = @quantity, x = @x, y = @y, z = @z, customInfo = @customInfo WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemName", entity.Name},
                    {"@ownerID", entity.OwnerID},
                    {"@locationID", entity.LocationID},
                    {"@flag", entity.Flag},
                    {"@contraband", entity.Contraband},
                    {"@singleton", entity.Singleton},
                    {"@quantity", entity.Quantity},
                    {"@x", entity.X},
                    {"@y", entity.Y},
                    {"@z", entity.Z},
                    {"@customInfo", entity.CustomInfo},
                    {"@itemID", entity.ID}
                }
            );
        }

        public void PersistAttributeList(Entity item, AttributeList list)
        {
            MySqlConnection connection = null;
            MySqlCommand update = Database.PrepareQuery(
                ref connection,
                "UPDATE entity_attributes SET valueInt = @valueInt, valueFloat = @valueFloat WHERE itemID = @itemID AND attributeID = @attributeID"
            );
            MySqlCommand create = Database.PrepareQuery(
                ref connection,
                "INSERT INTO entity_attributes(itemID, attributeID, valueInt, valueFloat) VALUE (@itemID, @attributeID, @valueInt, @valueFloat)"
            );

            using (connection)
            {
                // set query parameters
                update.Parameters.AddWithValue("@itemID", item.ID);
                create.Parameters.AddWithValue("@itemID", item.ID);

                foreach (KeyValuePair<int, ItemAttribute> pair in list)
                {
                    // only update dirty records
                    if (pair.Value.Dirty == false)
                        continue;

                    // use the correct prepared statement based on whether the attribute is new or old
                    if (pair.Value.New == true)
                    {
                        create.Parameters.AddWithValue("@attributeID", pair.Value.Info.ID);

                        if (pair.Value.ValueType == ItemAttribute.ItemAttributeValueType.Integer)
                            create.Parameters.AddWithValue("@valueInt", pair.Value.Integer);
                        else
                            create.Parameters.AddWithValue("@valueInt", null);

                        if (pair.Value.ValueType == ItemAttribute.ItemAttributeValueType.Double)
                            create.Parameters.AddWithValue("@valueFloat", pair.Value.Float);
                        else
                            create.Parameters.AddWithValue("@valueFloat", null);

                        create.ExecuteNonQuery();
                    }
                    else
                    {
                        update.Parameters.AddWithValue("@attributeID", pair.Value.Info.ID);

                        if (pair.Value.ValueType == ItemAttribute.ItemAttributeValueType.Integer)
                            update.Parameters.AddWithValue("@valueInt", pair.Value.Integer);
                        else
                            update.Parameters.AddWithValue("@valueInt", null);

                        if (pair.Value.ValueType == ItemAttribute.ItemAttributeValueType.Double)
                            update.Parameters.AddWithValue("@valueFloat", pair.Value.Float);
                        else
                            update.Parameters.AddWithValue("@valueFloat", null);

                        update.ExecuteNonQuery();
                    }
                }
            }
        }

        public ItemDB(DatabaseConnection db, ItemFactory factory) : base(db)
        {
            this.mItemFactory = factory;
        }
    }
}