using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database.Inventory;

public static class AttributeDB
{
    public static Dictionary <int, Dictionary <int, Attribute>> InvDgmLoadDefaultAttributes (this IDatabaseConnection Database, IAttributes attributes)
    {
        IDbConnection connection = null;

        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT typeID, attributeID, valueInt, valueFloat FROM dgmTypeAttributes"
        );

        using (connection)
        using (reader)
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

    public static Dictionary <int, AttributeType> InvDgmLoadAttributes (this IDatabaseConnection Database)
    {
        IDbConnection connection = null;

        // sort the attributes by maxAttributeID so the simple attributes are loaded first
        // and then the complex ones that are related to other attributes
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT attributeID, attributeName, attributeCategory, description, maxAttributeID, attributeIdx, graphicID, chargeRechargeTimeID, defaultValue, published, displayName, unitID, stackable, highIsGood, categoryID FROM dgmAttributeTypes ORDER BY maxAttributeID ASC"
        );

        using (connection)
        using (reader)
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
}