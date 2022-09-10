using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.EVE.Database;

namespace EVESharp.Database.Inventory;

public static class InventoryItemsDB
{
    public static Dictionary <int, Category> InvLoadItemCategories (this IDatabaseConnection Database)
    {
        IDbConnection connection = null;

        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT categoryID, categoryName, description, graphicID, published FROM invCategories"
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, Category> itemCategories = new Dictionary <int, Category> ();

            while (reader.Read ())
            {
                Category itemCategory = new Category (
                    reader.GetInt32 (0),
                    reader.GetString (1),
                    reader.GetString (2),
                    reader.GetInt32OrDefault (3),
                    reader.GetBoolean (4)
                );

                itemCategories [itemCategory.ID] = itemCategory;
            }

            return itemCategories;
        }
    }

    public static Dictionary <int, Group> InvLoadItemGroups (this IDatabaseConnection Database, ICategories categories)
    {
        IDbConnection connection = null;

        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT groupID, categoryID, groupName, description, graphicID, useBasePrice, allowManufacture, allowRecycler, anchored, anchorable, fittableNonSingleton, published FROM invGroups"
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, Group> itemGroups = new Dictionary <int, Group> ();

            while (reader.Read ())
            {
                Group group = new Group (
                    reader.GetInt32 (0),
                    categories [reader.GetInt32 (1)],
                    reader.GetString (2),
                    reader.GetString (3),
                    reader.GetInt32OrDefault (4),
                    reader.GetBoolean (5),
                    reader.GetBoolean (6),
                    reader.GetBoolean (7),
                    reader.GetBoolean (8),
                    reader.GetBoolean (9),
                    reader.GetBoolean (10),
                    reader.GetBoolean (11)
                );

                itemGroups [group.ID] = group;
            }

            return itemGroups;
        }
    }

    public static Dictionary <int, Type> InvLoadItemTypes (this IDatabaseConnection Database, IAttributes attributes, IDefaultAttributes defaultAttributes, IGroups groups, IExpressions expressions)
    {
        // item effects should be loaded before as they're needed for the types instantiation
        Dictionary <int, Dictionary <int, Effect>> effects = Database.InvLoadItemEffects (expressions);

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating FROM invTypes"
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, Type> itemTypes = new Dictionary <int, Type> ();

            while (reader.Read ())
            {
                int typeID = reader.GetInt32 (0);

                if (defaultAttributes.TryGetValue (typeID, out Dictionary <int, Attribute> typeAttributes) == false)
                    typeAttributes = new Dictionary <int, Attribute> ();

                if (effects.TryGetValue (typeID, out Dictionary <int, Effect> typeEffects) == false)
                    typeEffects = new Dictionary <int, Effect> ();

                Type type = new Type (
                    typeID,
                    groups [reader.GetInt32 (1)],
                    reader.GetString (2),
                    reader.GetString (3),
                    reader.GetInt32OrDefault (4),
                    reader.GetDouble (5),
                    reader.GetDouble (6),
                    reader.GetDouble (7),
                    reader.GetDouble (8),
                    reader.GetInt32 (9),
                    reader.GetInt32OrDefault (10),
                    reader.GetDouble (11),
                    reader.GetBoolean (12),
                    reader.GetInt32OrDefault (13),
                    reader.GetDouble (14),
                    typeAttributes,
                    typeEffects
                );

                itemTypes [type.ID] = type;
            }

            return itemTypes;
        }
    }

    public static Dictionary <int, Dictionary <int, Effect>> InvLoadItemEffects (this IDatabaseConnection Database, IExpressions expressionManager)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT effectID, effectName, effectCategory, preExpression, postExpression, description, guid, graphicID, isOffensive, isAssistance, durationAttributeID, trackingSpeedAttributeID, dischargeAttributeID, rangeAttributeID, falloffAttributeID, disallowAutoRepeat, published, displayName, isWarpSafe, rangeChance, electronicChance, propulsionChance, distribution, sfxName, npcUsageChanceAttributeID, npcActivationChanceAttributeID, fittingUsageChanceAttributeID FROM dgmEffects"
        );

        Dictionary <int, Effect> effects = new Dictionary <int, Effect> ();

        using (connection)
        {
            using (reader)
            {
                while (reader.Read ())
                    effects [reader.GetInt32 (0)] = new Effect (
                        reader.GetInt32 (0),
                        reader.GetString (1),
                        (EffectCategory) reader.GetInt32 (2),
                        expressionManager [reader.GetInt32 (3)],
                        expressionManager [reader.GetInt32 (4)],
                        reader.GetString (5),
                        reader.GetStringOrNull (6),
                        reader.GetInt32OrNull (7),
                        reader.GetBoolean (8),
                        reader.GetBoolean (9),
                        reader.GetInt32OrNull (10),
                        reader.GetInt32OrNull (11),
                        reader.GetInt32OrNull (12),
                        reader.GetInt32OrNull (13),
                        reader.GetInt32OrNull (14),
                        reader.GetBoolean (15),
                        reader.GetBoolean (16),
                        reader.GetString (17),
                        reader.GetBoolean (18),
                        reader.GetBoolean (19),
                        reader.GetBoolean (20),
                        reader.GetBoolean (21),
                        reader.GetInt32OrNull (22),
                        reader.GetStringOrNull (23),
                        reader.GetInt32OrNull (24),
                        reader.GetInt32OrNull (25),
                        reader.GetInt32OrNull (26)
                    );
            }

            // disable assignation warning as the connection is not null anymore
#pragma warning disable CS0728
            reader = Database.Select (ref connection, "SELECT typeID, effectID FROM dgmTypeEffects");
#pragma warning restore CS0728

            using (reader)
            {
                Dictionary <int, Dictionary <int, Effect>> typeEffects = new Dictionary <int, Dictionary <int, Effect>> ();

                while (reader.Read ())
                {
                    int typeID   = reader.GetInt32 (0);
                    int effectID = reader.GetInt32 (1);

                    // ignore effects that were not loaded
                    if (typeEffects.TryGetValue (typeID, out Dictionary <int, Effect> typeEffect) == false)
                        typeEffect = typeEffects [typeID] = new Dictionary <int, Effect> ();

                    typeEffect [effectID] = effects [effectID];
                }

                return typeEffects;
            }
        }
    }
}