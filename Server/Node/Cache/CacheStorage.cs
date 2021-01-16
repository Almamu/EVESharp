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
using Common.Logging;
using MySql.Data.MySqlClient;
using Node.Inventory;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class CacheStorage : DatabaseAccessor
    {
        public enum CacheObjectType
        {
            TupleSet = 0,
            Rowset = 1,
            CRowset = 2,
            PackedRowList = 3,
            IntIntDict = 4,
            IndexRowset = 5
        };

        public static Dictionary<string, string> LoginCacheTable = new Dictionary<string, string>()
        {
            {"config.BulkData.ramactivities", "config.BulkData.ramactivities"},
            {"config.BulkData.billtypes", "config.BulkData.billtypes"},
            {"config.Bloodlines", "config.Bloodlines"},
            {"config.Units", "config.Units"},
            {"config.BulkData.tickernames", "config.BulkData.tickernames"},
            {"config.BulkData.ramtyperequirements", "config.BulkData.ramtyperequirements"},
            {"config.BulkData.ramaltypesdetailpergroup", "config.BulkData.ramaltypesdetailpergroup"},
            {"config.BulkData.ramaltypes", "config.BulkData.ramaltypes"},
            {"config.BulkData.allianceshortnames", "config.BulkData.allianceshortnames"},
            {"config.BulkData.ramcompletedstatuses", "config.BulkData.ramcompletedstatuses"},
            {"config.BulkData.categories", "config.BulkData.categories"},
            {"config.BulkData.invtypereactions", "config.BulkData.invtypereactions"},
            {"config.BulkData.dgmtypeeffects", "config.BulkData.dgmtypeeffects"},
            {"config.BulkData.metagroups", "config.BulkData.metagroups"},
            {"config.BulkData.ramtypematerials", "config.BulkData.ramtypematerials"},
            {"config.BulkData.ramaltypesdetailpercategory", "config.BulkData.ramaltypesdetailpercategory"},
            {"config.BulkData.owners", "config.BulkData.owners"},
            {"config.StaticOwners", "config.StaticOwners"},
            {"config.Races", "config.Races"},
            {"config.Attributes", "config.Attributes"},
            {"config.BulkData.dgmtypeattribs", "config.BulkData.dgmtypeattribs"},
            {"config.BulkData.locations", "config.BulkData.locations"},
            {"config.BulkData.locationwormholeclasses", "config.BulkData.locationwormholeclasses"},
            {"config.BulkData.groups", "config.BulkData.groups"},
            {"config.BulkData.shiptypes", "config.BulkData.shiptypes"},
            {"config.BulkData.dgmattribs", "config.BulkData.dgmattribs"},
            {"config.Flags", "config.Flags"},
            {"config.BulkData.bptypes", "config.BulkData.bptypes"},
            {"config.BulkData.graphics", "config.BulkData.graphics"},
            {"config.BulkData.mapcelestialdescriptions", "config.BulkData.mapcelestialdescriptions"},
            {"config.BulkData.certificates", "config.BulkData.certificates"},
            {"config.StaticLocations", "config.StaticLocations"},
            {"config.InvContrabandTypes", "config.InvContrabandTypes"},
            {"config.BulkData.certificaterelationships", "config.BulkData.certificaterelationships"},
            {"config.BulkData.units", "config.BulkData.units"},
            {"config.BulkData.dgmeffects", "config.BulkData.dgmeffects"},
            {"config.BulkData.types", "config.BulkData.types"},
            {"config.BulkData.invmetatypes", "config.BulkData.invmetatypes"},
        };

        public static string[] LoginCacheQueries =
        {
            "SELECT activityID, activityName, iconNo, description, published FROM ramActivities",
            "SELECT billTypeID, billTypeName, description FROM billTypes",
            "SELECT bloodlineID, bloodlineName, raceID, description, maleDescription, femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription, dataID FROM chrBloodlines",
            "SELECT unitID, unitName, displayName FROM eveUnits",
            "SELECT corporationID, tickerName, shape1, shape2, shape3, color1, color2, color3 FROM corporation WHERE hasPlayerPersonnelManager = 0",
            "SELECT typeID, activityID, requiredTypeID, quantity, damagePerJob, recycle FROM typeActivityMaterials WHERE damagePerJob != 1.0 OR recycle = 1",
            "SELECT a.assemblyLineTypeID, b.activityID, a.groupID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerGroup AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID",
            "SELECT assemblyLineTypeID, assemblyLineTypeName, assemblyLineTypeName AS typeName, description, activityID, baseTimeMultiplier, baseMaterialMultiplier, volume FROM ramAssemblyLineTypes",
            "SELECT allianceID, shortName FROM alliance_shortnames",
            "SELECT completedStatusID, completedStatusName, completedStatusText FROM ramCompletedStatuses",
            "SELECT categoryID, categoryName, description, graphicID, published, 0 AS dataID FROM invCategories",
            "SELECT reactionTypeID, input, typeID, quantity FROM invTypeReactions",
            "SELECT typeID, effectID, isDefault FROM dgmTypeEffects",
            "SELECT metaGroupID, metaGroupName, description, graphicID, 0 AS dataID FROM invMetaGroups",
            "SELECT typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials WHERE activityID = 6 AND damagePerJob = 1.0 UNION SELECT productTypeID AS typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE activityID = 1 AND damagePerJob = 1.0",
            "SELECT a.assemblyLineTypeID, b.activityID, a.categoryID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerCategory AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID",
            "SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM entity WHERE itemID = 0",
            $"SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM entity LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) WHERE categoryID = 1 AND itemID < {ItemManager.USERGENERATED_ID_MIN}",
            "SELECT raceID, raceName, description, graphicID, shortDescription, 0 AS dataID FROM chrRaces",
            "SELECT attributeID, attributeName, description, graphicID FROM chrAttributes",
            "SELECT	typeID, attributeID, IF(valueInt IS NULL, valueFloat, valueInt) AS value FROM dgmTypeAttributes",
            "SELECT locationID, locationName, x, y, z FROM cacheLocations",
            "SELECT locationID, wormholeClassID FROM mapLocationWormholeClasses",
            "SELECT groupID, categoryID, groupName, description, graphicID, useBasePrice, allowManufacture, allowRecycler, anchored, anchorable, fittableNonSingleton, 1 AS published, 0 AS dataID FROM invGroups",
            "SELECT shipTypeID,weaponTypeID,miningTypeID,skillTypeID FROM invShipTypes",
            "SELECT attributeID, attributeName, attributeCategory, description, maxAttributeID, attributeIdx, graphicID, chargeRechargeTimeID, defaultValue, published, displayName, unitID, stackable, highIsGood, categoryID, 0 AS dataID FROM dgmAttributeTypes",
            "SELECT flagID, flagName, flagText, flagType, orderID FROM invFlags",
            "SELECT invTypes.typeName AS blueprintTypeName, invTypes.description, invTypes.graphicID, invTypes.basePrice, blueprintTypeID, parentBlueprintTypeID, productTypeID, productionTime, techLevel, researchProductivityTime, researchMaterialTime, researchCopyTime, researchTechTime, productivityModifier, materialModifier, wasteFactor, chanceOfReverseEngineering, maxProductionLimit FROM invBlueprintTypes, invTypes WHERE invBlueprintTypes.blueprintTypeID = invTypes.typeID",
            "SELECT graphicID, url3D, urlWeb, icon, urlSound, explosionID FROM eveGraphics",
            "SELECT celestialID, description FROM mapCelestialDescriptions",
            "SELECT certificateID, categoryID, classID, grade, iconID, corpID, description, 0 AS dataID FROM crtCertificates",
            $"SELECT itemID AS locationID, itemName as locationName, x, y, z FROM entity LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) WHERE (categoryID = 2 OR categoryID = 3) AND itemID < {ItemManager.USERGENERATED_ID_MIN}",
            "SELECT factionID, typeID, standingLoss, confiscateMinSec, fineByValue, attackMinSec FROM invContrabandTypes",
            "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships",
            "SELECT unitID,unitName,displayName FROM eveUnits",
            "SELECT effectID, effectName, effectCategory, preExpression, postExpression, description, guid, graphicID, isOffensive, isAssistance, durationAttributeID, trackingSpeedAttributeID, dischargeAttributeID, rangeAttributeID, falloffAttributeID, published, displayName, isWarpSafe, rangeChance, electronicChance, propulsionChance, distribution, sfxName, npcUsageChanceAttributeID, npcActivationChanceAttributeID, 0 AS fittingUsageChanceAttributeID, 0 AS dataID FROM dgmEffects",
            "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating, dataID FROM invTypes",
            "SELECT typeID, parentTypeID, metaGroupID FROM invMetaTypes"
        };

        public static CacheObjectType[] LoginCacheTypes =
        {
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.TupleSet,
            CacheObjectType.Rowset,
            CacheObjectType.CRowset,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.CRowset,
            CacheObjectType.Rowset,
            CacheObjectType.TupleSet,
            CacheObjectType.Rowset,
            CacheObjectType.CRowset,
            CacheObjectType.TupleSet,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.PackedRowList,
            CacheObjectType.TupleSet,
            CacheObjectType.CRowset,
            CacheObjectType.TupleSet,
            CacheObjectType.CRowset,
            CacheObjectType.TupleSet,
            CacheObjectType.Rowset,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.CRowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.CRowset,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.TupleSet,
            CacheObjectType.CRowset
        };

        public static Dictionary<string, string> CreateCharacterCacheTable = new Dictionary<string, string>()
        {
            {"charCreationInfo.bl_eyebrows", "eyebrows"},
            {"charCreationInfo.bl_eyes", "eyes"},
            {"charCreationInfo.bl_decos", "decos"},
            {"charCreationInfo.bloodlines", "bloodlines"},
            {"charCreationInfo.bl_hairs", "hairs"},
            {"charCreationInfo.bl_backgrounds", "backgrounds"},
            {"charCreationInfo.bl_accessories", "accessories"},
            {"charCreationInfo.bl_costumes", "costumes"},
            {"charCreationInfo.bl_lights", "lights"},
            {"charCreationInfo.races", "races"},
            {"charCreationInfo.ancestries", "ancestries"},
            {"charCreationInfo.schools", "schools"},
            {"charCreationInfo.attributes", "attributes"},
            {"charCreationInfo.bl_beards", "beards"},
            {"charCreationInfo.bl_skins", "skins"},
            {"charCreationInfo.bl_lipsticks", "lipsticks"},
            {"charCreationInfo.bl_makeups", "makeups"}
        };

        public static string[] CreateCharacterCacheQueries = new string[]
        {
            "SELECT bloodlineID, gender, eyebrowsID, npc FROM chrBLEyebrows",
            "SELECT bloodlineID, gender, eyesID, npc FROM chrBLEyes",
            "SELECT bloodlineID, gender, decoID, npc FROM chrBLDecos",
            "SELECT bloodlineID, bloodlineName, raceID, description, maleDescription, femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription, 0 AS dataID FROM chrBloodlines",
            "SELECT bloodlineID, gender, hairID, npc FROM chrBLHairs",
            "SELECT backgroundID, backgroundName FROM chrBLBackgrounds",
            "SELECT bloodlineID, gender, accessoryID, npc FROM chrBLAccessories",
            "SELECT bloodlineID, gender, costumeID, npc FROM chrBLCostumes",
            "SELECT lightID, lightName FROM chrBLLights",
            "SELECT raceID, raceName, description, graphicID, shortDescription, 0 AS dataID FROM chrRaces",
            "SELECT ancestryID, ancestryName, bloodlineID, description, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, 0 AS dataID FROM chrAncestries",
            "SELECT raceID, schoolID, schoolName, description, graphicID, corporationID, agentID, newAgentID FROM chrSchools LEFT JOIN agtAgents USING (corporationID) GROUP BY schoolID",
            "SELECT attributeID, attributeName, description, graphicID FROM chrAttributes",
            "SELECT bloodlineID, gender, beardID, npc FROM chrBLBeards",
            "SELECT bloodlineID, gender, skinID, npc FROM chrBLSkins",
            "SELECT bloodlineID, gender, lipstickID, npc FROM chrBLLipsticks",
            "SELECT bloodlineID, gender, makeupID, npc FROM chrBLMakeups"
        };

        public static CacheObjectType[] CreateCharacterCacheTypes = new CacheObjectType[]
        {
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.CRowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.CRowset,
            CacheObjectType.CRowset,
            CacheObjectType.CRowset,
            CacheObjectType.CRowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset
        };

        public static Dictionary<string, string> CharacterAppearanceCacheTable = new Dictionary<string, string>()
        {
            {"charCreationInfo.eyebrows", "eyebrows"},
            {"charCreationInfo.eyes", "eyes"},
            {"charCreationInfo.decos", "decos"},
            {"charCreationInfo.hairs", "hairs"},
            {"charCreationInfo.backgrounds", "backgrounds"},
            {"charCreationInfo.accessories", "accessories"},
            {"charCreationInfo.costumes", "costumes"},
            {"charCreationInfo.lights", "lights"},
            {"charCreationInfo.makeups", "makeups"},
            {"charCreationInfo.beards", "beards"},
            {"charCreationInfo.skins", "skins"},
            {"charCreationInfo.lipsticks", "lipsticks"}
        };

        public static string[] CharacterAppearanceCacheQueries = new string[]
        {
            "SELECT eyebrowsID, eyebrowsName FROM chrEyebrows",
            "SELECT eyesID, eyesName FROM chrEyes",
            "SELECT decoID, decoName FROM chrDecos",
            "SELECT hairID, hairName FROM chrHairs",
            "SELECT backgroundID, backgroundName FROM chrBackgrounds",
            "SELECT accessoryID, accessoryName FROM chrAccessories",
            "SELECT costumeID, costumeName FROM chrCostumes",
            "SELECT lightID, lightName FROM chrLights",
            "SELECT makeupID, makeupName FROM chrMakeups",
            "SELECT beardID, beardName FROM chrBeards",
            "SELECT skinID, skinName FROM chrSkins",
            "SELECT lipstickID, lipstickName FROM chrLipsticks"
        };

        public static CacheObjectType[] CharacterAppearanceCacheTypes = new CacheObjectType[]
        {
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset,
            CacheObjectType.Rowset
        };

        private readonly Dictionary<string, PyDataType> mCacheData = new Dictionary<string, PyDataType>();
        private readonly PyDictionary mCacheHints = new PyDictionary();
        private readonly NodeContainer mContainer = null;

        private Channel Log { get; set; }

        public bool Exists(string name)
        {
            return this.mCacheData.ContainsKey(name);
        }

        public PyDataType GenerateObjectIDForCall(string service, string method)
        {
            return new PyTuple(new PyDataType []
            {
                "Method Call", "server", new PyTuple (new PyDataType []
                {
                    service, method
                })
            });
        }

        public PyDataType Get(string name)
        {
            return this.mCacheData[name];
        }

        public PyDataType Get(string service, string method)
        {
            return this.mCacheData[$"{service}::{method}"];
        }

        public void Store(string name, PyDataType data, long timestamp)
        {
            PyCacheHint hint = PyCacheHint.FromPyObject(name, data, timestamp, this.mContainer.NodeID);

            // save cache hint
            this.mCacheHints[name] = hint;
            // save cache object
            this.mCacheData[name] = PyCachedObject.FromCacheHint(hint, data);
        }

        public void StoreCall(string service, string method, PyDataType data, long timestamp)
        {
            string index = $"{service}::{method}";
            PyDataType objectID = this.GenerateObjectIDForCall(service, method);
            PyCacheHint hint = PyCacheHint.FromPyObject(objectID, data, timestamp, this.mContainer.NodeID);
            
            // save cache hint
            this.mCacheHints[index] = hint;
            // save cache object
            this.mCacheData[index] = PyCachedObject.FromCacheHint(hint, data);
        }

        public PyDictionary GetHints(Dictionary<string, string> list)
        {
            PyDictionary hints = new PyDictionary();

            foreach (KeyValuePair<string, string> pair in list)
                hints[pair.Value] = this.GetHint(pair.Key);

            return hints;
        }

        public PyDataType GetHint(string name)
        {
            return this.mCacheHints[name];
        }

        public PyDataType GetHint(string service, string method)
        {
            return this.mCacheHints[$"{service}::{method}"];
        }

        private void Load(string name, string query, CacheObjectType type)
        {
            Log.Debug($"Loading cache data for {name} of type {type}");

            try
            {
                MySqlConnection connection = null;
                MySqlDataReader reader = Database.Query(ref connection, query);
                PyDataType cacheObject = null;

                using(connection)
                using (reader)
                {
                    switch (type)
                    {
                        case CacheObjectType.Rowset:
                            cacheObject = Rowset.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.CRowset:
                            cacheObject = CRowset.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.TupleSet:
                            cacheObject = TupleSet.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.PackedRowList:
                            cacheObject = PyPackedRowList.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.IntIntDict:
                            cacheObject = IntIntDictionary.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.IndexRowset:
                            cacheObject = IndexRowset.FromMySqlDataReader(reader, 0);
                            break;
                    }

                    Store(name, cacheObject, DateTime.UtcNow.ToFileTimeUtc());
                }
            }
            catch (Exception)
            {
                Log.Error($"Cannot generate cache data for {name}");
                throw;
            }
        }
        
        public void Load(string service, string method, string query, CacheObjectType type)
        {
            Log.Debug($"Loading cache data for {service}::{method} of type {type}");

            try
            {
                MySqlConnection connection = null;
                MySqlDataReader reader = Database.Query(ref connection, query);
                PyDataType cacheObject = null;

                using(connection)
                using (reader)
                {
                    switch (type)
                    {
                        case CacheObjectType.Rowset:
                            cacheObject = Rowset.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.CRowset:
                            cacheObject = CRowset.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.TupleSet:
                            cacheObject = TupleSet.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.PackedRowList:
                            cacheObject = PyPackedRowList.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.IntIntDict:
                            cacheObject = IntIntDictionary.FromMySqlDataReader(reader);
                            break;
                        case CacheObjectType.IndexRowset:
                            cacheObject = IndexRowset.FromMySqlDataReader(reader, 0);
                            break;
                    }

                    StoreCall(service, method, cacheObject, DateTime.UtcNow.ToFileTimeUtc());
                }
            }
            catch (Exception)
            {
                Log.Error($"Cannot generate cache data for {service}::{method}");
                throw;
            }
        }

        public void Load(Dictionary<string, string> names, string[] queries, CacheObjectType[] types)
        {
            if (names.Count != queries.Length || names.Count != types.Length)
                throw new ArgumentOutOfRangeException("names", "names, queries and types do not match in size");

            int i = 0;

            foreach (KeyValuePair<string, string> pair in names)
            {
                Load(pair.Key, queries[i], types[i]);
                i++;
            }
        }

        public CacheStorage(NodeContainer container, DatabaseConnection db, Logger logger) : base(db)
        {
            this.Log = logger.CreateLogChannel("CacheStorage");
            this.mContainer = container;
        }
    }
}