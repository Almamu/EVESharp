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
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;

namespace Node.Database
{
    public class ItemDB : DatabaseAccessor
    {
        private ItemFactory mItemFactory = null;

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

        public Item LoadItem(int itemID)
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
                ItemEntity owner = null, location = null;
                
                if (reader.IsDBNull(3) == false)
                    owner = this.mItemFactory.ItemManager.LoadItem(reader.GetInt32(3));
                
                if (reader.IsDBNull(4) == false)
                    location = this.mItemFactory.ItemManager.LoadItem(reader.GetInt32(4));
                
                Item newItem = new Item(
                    reader.GetString(1), // itemName
                    reader.GetInt32(0), // itemID
                    itemType, // typeID
                    owner, // ownerID
                    location, // locationID
                    (ItemFlags) reader.GetInt32(5), // flag
                    reader.GetBoolean(6), // contraband
                    reader.GetBoolean(7), // singleton
                    reader.GetInt32(8), // quantity
                    reader.GetDouble(9), // x
                    reader.GetDouble(10), // y
                    reader.GetDouble(11), // z
                    reader.IsDBNull(12) ? null : reader.GetString(12), // customInfo
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

                return reader.GetInt32(0);
            }
        }

        public Blueprint LoadBlueprint(int itemID)
        {
            Item item = LoadItem(itemID);

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

                return new Blueprint(item, reader.GetBoolean(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
            }
        }

        public Character LoadCharacter(int itemID)
        {
            Item item = LoadItem(itemID);

            if (item == null)
                return null;

            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT characterID, accountID, title, description, bounty, balance, securityRating," +
                " petitionMessage, logonMinutes, corporationID, corpRole, rolesAtAll, rolesAtBase, rolesAtHQ," +
                " rolesAtOther, corporationDateTime, startDateTime, createDateTime, ancestryID, careerID, schoolID," +
                " careerSpecialityID, gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
                " lipstickID, makeupID, skinID, backgroundID, lightID, headRotation1, headRotation2, headRotation3," +
                " eyeRotation1, eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3, morph1e, morph1n, morph1s," +
                " morph1w, morph2e, morph2n, morph2s, morph2w, morph3e, morph3n, morph3s, morph3w, morph4e, morph4n," +
                " morph4s, morph4w, stationID, solarSystemID, constellationID, regionID, online" +
                " FROM character_ WHERE characterID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID}
                }
            );
            
            using (connection)
            using (reader)
            {
                return new Character(
                    item,
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetString(7),
                    reader.GetInt32(8),
                    reader.GetInt32(9),
                    reader.GetInt32(10),
                    reader.GetInt32(11),
                    reader.GetInt32(12),
                    reader.GetInt32(13),
                    reader.GetInt32(14),
                    reader.GetInt64(15),
                    reader.GetInt64(16),
                    reader.GetInt64(17),
                    reader.GetInt32(18),
                    reader.GetInt32(19),
                    reader.GetInt32(20),
                    reader.GetInt32(21),
                    reader.GetInt32(22),
                    reader.GetInt32(23),
                    reader.GetInt32(24),
                    reader.GetInt32(25),
                    reader.GetInt32(26),
                    reader.GetInt32(27),
                    reader.GetInt32(28),
                    reader.GetInt32(29),
                    reader.GetInt32(30),
                    reader.GetInt32(31),
                    reader.GetInt32(32),
                    reader.GetInt32(33),
                    reader.GetInt32(34),
                    reader.GetDouble(35),
                    reader.GetDouble(36),
                    reader.GetDouble(37),
                    reader.GetDouble(38),
                    reader.GetDouble(39),
                    reader.GetDouble(40),
                    reader.GetDouble(41),
                    reader.GetDouble(42),
                    reader.GetDouble(43),
                    reader.GetDouble(44),
                    reader.GetDouble(45),
                    reader.GetDouble(46),
                    reader.GetDouble(47),
                    reader.GetDouble(48),
                    reader.GetDouble(49),
                    reader.GetDouble(50),
                    reader.GetDouble(51),
                    reader.GetDouble(52),
                    reader.GetDouble(53),
                    reader.GetDouble(54),
                    reader.GetDouble(55),
                    reader.GetDouble(56),
                    reader.GetDouble(57),
                    reader.GetDouble(58),
                    reader.GetDouble(59),
                    reader.GetInt32(60),
                    reader.GetInt32(61),
                    reader.GetInt32(62),
                    reader.GetInt32(63),
                    reader.GetInt32(64)
                );
            }
        }

        public ulong CreateItem(string itemName, int typeID, ItemEntity owner, ItemEntity location, ItemFlags flag,
            bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO entity(itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo)VALUES(NULL, @itemName, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @x, @y, @z, @customInfo)",
                new Dictionary<string, object>()
                {
                    {"@itemName", itemName},
                    {"@typeID", typeID},
                    {"@ownerID", owner?.ID},
                    {"@locationID", location?.ID},
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

        public ulong CreateItem(string itemName, int typeID, int owner, int location, ItemFlags flag, bool contraband,
            bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO entity(itemID, itemName, typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo)VALUES(NULL, @itemName, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @x, @y, @z, @customInfo)",
                new Dictionary<string, object>()
                {
                    {"@itemName", itemName},
                    {"@typeID", typeID},
                    {"@ownerID", owner},
                    {"@locationID", location},
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

        public List<ItemEntity> GetItemsLocatedAt(int locationID)
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
                List<ItemEntity> items = new List<ItemEntity>();

                while (reader.Read())
                {
                    ItemType itemType = this.mItemFactory.TypeManager[reader.GetInt32(1)];
                    ItemEntity owner = null, location = null;
                
                    if (reader.IsDBNull(3) == false)
                        owner = this.mItemFactory.ItemManager.LoadItem(reader.GetInt32(3));
                
                    if (reader.IsDBNull(4) == false)
                        location = this.mItemFactory.ItemManager.LoadItem(reader.GetInt32(4));
                    
                    Item newItem = new Item(
                        reader.GetString(1), // itemName
                        reader.GetInt32(0), // itemID
                        itemType, // typeID
                        owner, // ownerID
                        location, // locationID
                        (ItemFlags) reader.GetInt32(5), // flag
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

        public SolarSystem LoadSolarSystem(int solarSystemID)
        {
            Item item = LoadItem(solarSystemID);

            if (item == null)
                return null;
            
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
                    return null;

                return new SolarSystem(item,
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetDouble(7),
                    reader.GetDouble(8),
                    reader.GetDouble(9),
                    reader.GetDouble(10),
                    reader.GetDouble(11),
                    reader.GetBoolean(12),
                    reader.GetBoolean(13),
                    reader.GetBoolean(14),
                    reader.GetBoolean(15),
                    reader.GetBoolean(16),
                    reader.GetBoolean(17),
                    reader.GetBoolean(18),
                    reader.GetDouble(19),
                    reader.GetInt32(20),
                    reader.GetDouble(21),
                    reader.GetInt32(22),
                    reader.GetString(23)
                );
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
        /// <param name="Item"></param>
        public void PersistEntity(ItemEntity Item)
        {
            Database.PrepareQuery(
                "UPDATE entity SET itemName = @itemName, ownerID = @ownerID, locationID = @locationID, flag = @flag, contraband = @contraband, singleton = @singleton, quantity = @quantity, x = @x, y = @y, z = @z, customInfo = @customInfo WHERE itemID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemName", Item.Name},
                    {"@ownerID", Item.Owner?.ID},
                    {"@locationID", Item.Location?.ID},
                    {"@flag", Item.Flag},
                    {"@contraband", Item.Contraband},
                    {"@singleton", Item.Singleton},
                    {"@quantity", Item.Quantity},
                    {"@x", Item.X},
                    {"@y", Item.Y},
                    {"@z", Item.Z},
                    {"@customInfo", Item.CustomInfo},
                    {"@itemID", Item.ID}
                }
            );
        }

        public void PersistAttributeList(ItemEntity item, AttributeList list)
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
                    if (pair.Value.Dirty == false && pair.Value.New == false)
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

        public void PersistBlueprint(int itemID, bool copy, int materialLevel, int productivityLevel, int licensedProductionRunsRemaining)
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

        public ItemDB(DatabaseConnection db, ItemFactory factory) : base(db)
        {
            this.mItemFactory = factory;
        }
    }
}