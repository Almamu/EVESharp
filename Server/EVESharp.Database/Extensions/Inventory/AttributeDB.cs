using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.MySql;

namespace EVESharp.Database.Extensions.Inventory;

public static class AttributeDB
{
    public static Dictionary <int, Dictionary <int, Attribute>> InvDgmLoadDefaultAttributes (this IDatabase Database, IAttributes attributes)
    {
        using (DbDataReader reader = Database.Select ("SELECT typeID, attributeID, valueInt, valueFloat FROM dgmTypeAttributes"))
        {
            Dictionary <int, Dictionary <int, Attribute>> result = new Dictionary <int, Dictionary <int, Attribute>> ();

            while (reader.Read ())
            {
                int typeID = reader.GetInt32 (0);

                if (result.ContainsKey (typeID) == false)
                    result [typeID] = new Dictionary <int, Attribute> ();

                Attribute attribute = null;

                if (reader.IsDBNull (2) == false)
                    attribute = new Attribute (
                        attributes [reader.GetInt32 (1)],
                        reader.GetInt32 (2)
                    );
                else
                    attribute = new Attribute (
                        attributes [reader.GetInt32 (1)],
                        reader.GetDouble (3)
                    );

                result [typeID] [attribute.ID] = attribute;
            }

            return result;
        }
    }

    public static Dictionary <int, AttributeType> InvDgmLoadAttributes (this IDatabase Database)
    {
        // sort the attributes by maxAttributeID so the simple attributes are loaded first
        // and then the complex ones that are related to other attributes
        using (DbDataReader reader = Database.Select (
                   "SELECT attributeID, attributeName, attributeCategory, description, maxAttributeID, attributeIdx, graphicID, chargeRechargeTimeID, defaultValue, published, displayName, unitID, stackable, highIsGood, categoryID FROM dgmAttributeTypes ORDER BY maxAttributeID ASC"
               )
              )
        {
            Dictionary <int, AttributeType> result = new Dictionary <int, AttributeType> ();

            while (reader.Read ())
            {
                AttributeType info = new AttributeType (
                    reader.GetInt32 (0),
                    reader.GetString (1),
                    reader.GetInt32 (2),
                    reader.GetString (3),
                    reader.IsDBNull (4) ? null : result [reader.GetInt32 (4)],
                    reader.GetInt32OrDefault (5),
                    reader.GetInt32OrDefault (6),
                    reader.GetInt32OrDefault (7),
                    reader.GetDouble (8),
                    reader.GetInt32 (9),
                    reader.GetString (10),
                    reader.GetInt32OrDefault (11),
                    reader.GetInt32 (12),
                    reader.GetInt32 (13),
                    reader.GetInt32OrDefault (14)
                );

                result [info.ID] = info;
            }

            return result;
        }
    }

    public static Dictionary <int, Attribute> InvDgmLoadAttributesForEntity (this IDatabase Database, int itemID, IAttributes attributes)
    {
        using (
            DbDataReader reader = Database.Select (
                "SELECT attributeID, valueInt, valueFloat FROM invItemsAttributes WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", itemID}}
            )
        )
        {
            Dictionary <int, Attribute> result = new Dictionary <int, Attribute> ();

            while (reader.Read ())
            {
                Attribute attribute = null;

                if (reader.IsDBNull (1))
                    attribute = new Attribute (
                        attributes [reader.GetInt32 (0)],
                        reader.GetDouble (2)
                    );
                else
                    attribute = new Attribute (
                        attributes [reader.GetInt32 (0)],
                        reader.GetInt64 (1)
                    );

                result [attribute.ID] = attribute;
            }

            return result;
        }
    }

    public static void InvDgmPersistEntityAttributes (this IDatabase Database, int itemID, AttributeList attributes)
    {
        using (IDbConnection connection = Database.OpenConnection ())
        {
            MySqlCommand command = (MySqlCommand) Database.Prepare (
                connection,
                "REPLACE INTO invItemsAttributes(itemID, attributeID, valueInt, valueFloat) VALUE (@itemID, @attributeID, @valueInt, @valueFloat)"
            );

            using (command)
            {
                foreach (KeyValuePair <int, Attribute> pair in attributes)
                {
                    // only update dirty records
                    if (pair.Value.Dirty == false && pair.Value.New == false)
                        continue;

                    command.Parameters.Clear ();

                    command.Parameters.AddWithValue ("@itemID",      itemID);
                    command.Parameters.AddWithValue ("@attributeID", pair.Value.ID);

                    if (pair.Value.ValueType == Attribute.ItemAttributeValueType.Integer)
                        command.Parameters.AddWithValue ("@valueInt", pair.Value.Integer);
                    else
                        command.Parameters.AddWithValue ("@valueInt", null);

                    if (pair.Value.ValueType == Attribute.ItemAttributeValueType.Double)
                        command.Parameters.AddWithValue ("@valueFloat", pair.Value.Float);
                    else
                        command.Parameters.AddWithValue ("@valueFloat", null);

                    command.ExecuteNonQuery ();

                    pair.Value.New   = false;
                    pair.Value.Dirty = false;
                }
            }
        }
    }
}