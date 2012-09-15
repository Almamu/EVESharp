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
using Marshal;
using Common;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Common.Packets;
using Common.Utils;

namespace EVESharp
{
    public static class Cache
    {
        public static string[] LoginCacheTable = new string[]
        {
        	"config.BulkData.ramactivities",
	        "config.BulkData.billtypes",
	        "config.Bloodlines",
	        "config.Units",
	        "config.BulkData.tickernames",
	        "config.BulkData.ramtyperequirements",
	        "config.BulkData.ramaltypesdetailpergroup",
	        "config.BulkData.ramaltypes",
	        "config.BulkData.allianceshortnames",
	        "config.BulkData.ramcompletedstatuses",
	        "config.BulkData.categories",
	        "config.BulkData.invtypereactions",
	        "config.BulkData.dgmtypeeffects",
	        "config.BulkData.metagroups",
	        "config.BulkData.ramtypematerials",
	        "config.BulkData.ramaltypesdetailpercategory",
	        "config.BulkData.owners",
	        "config.StaticOwners",
	        "config.Races",
	        "config.Attributes",
	        "config.BulkData.dgmtypeattribs",
	        "config.BulkData.locations",
	        "config.BulkData.locationwormholeclasses",
	        "config.BulkData.groups",
	        "config.BulkData.shiptypes",
	        "config.BulkData.dgmattribs",
	        "config.Flags",
	        "config.BulkData.bptypes",
	        "config.BulkData.graphics",
	        "config.BulkData.mapcelestialdescriptions",
	        "config.BulkData.certificates",
	        "config.StaticLocations",
	        "config.InvContrabandTypes",
	        "config.BulkData.certificaterelationships",
	        "config.BulkData.units",
	        "config.BulkData.dgmeffects",
	        "config.BulkData.types",
	        "config.BulkData.invmetatypes"
        };

        public static int LoginCacheTableSize = LoginCacheTable.Length;

        public static string[] CreateCharacterCacheTable = new string[] 
        {
          	"charCreationInfo.bl_eyebrows",
	        "charCreationInfo.bl_eyes",
	        "charCreationInfo.bl_decos",
	        "charCreationInfo.bloodlines",
	        "charCreationInfo.bl_hairs",
	        "charCreationInfo.bl_backgrounds",
	        "charCreationInfo.bl_accessories",
	        "charCreationInfo.bl_costumes",
	        "charCreationInfo.bl_lights",
	        "charCreationInfo.races",
	        "charCreationInfo.ancestries",
	        "charCreationInfo.schools",
	        "charCreationInfo.attributes",
	        "charCreationInfo.bl_beards",
	        "charCreationInfo.bl_skins",
	        "charCreationInfo.bl_lipsticks",
	        "charCreationInfo.bl_makeups"
        };

        public static int CreateCharacterCacheTableSize = CreateCharacterCacheTable.Length;

        public static string[] CharacterAppearanceCacheTable = new string[]
        {
           	"charCreationInfo.eyebrows",
	        "charCreationInfo.eyes",
	        "charCreationInfo.decos",
	        "charCreationInfo.hairs",
	        "charCreationInfo.backgrounds",
	        "charCreationInfo.accessories",
	        "charCreationInfo.costumes",
	        "charCreationInfo.lights",
	        "charCreationInfo.makeups",
	        "charCreationInfo.beards",
	        "charCreationInfo.skins",
	        "charCreationInfo.lipsticks"
        };

        public static int CharacterAppearanceCacheTableSize = CharacterAppearanceCacheTable.Length;

        private static Dictionary<string, PyTuple> cacheData = new Dictionary<string, PyTuple>();
        private static Dictionary<int, Dictionary<string, PyTuple>> userCacheData = new Dictionary<int, Dictionary<string, PyTuple>>();

        /*
         *  User cache 
         */
        public static bool LoadUserCacheFor(int user, string name)
        {
            return LoadUserCacheFor(user, name, false);
        }

        public static bool UpdateUserCache(int user, string name)
        {
            return LoadUserCacheFor(user, name, true);
        }

        public static bool LoadUserCacheFor(int user, string name, bool force)
        {
            Dictionary<string, PyTuple> old = null;

            if (userCacheData.ContainsKey(user) == true)
            {
                if (userCacheData[user].ContainsKey(name) == true)
                {
                    if (force == false)
                    {
                        return true;
                    }

                    userCacheData[user].Remove(name);
                }

                if(force == false)
                {
                    return true;
                }

                // Store the actual cache data to reload only the required cacheName
                old = userCacheData[user];
                userCacheData.Remove(user);
            }

            // Get the cache data from the DB
            PyTuple data = Database.CacheDB.GetUserCacheData(user, name);

            if (data == null)
            {
                Log.Error("Cache", "Cannot load cache data for user " + user + " of type " + name);
                return false;
            }

            // If the data wasnt loaded yet create an empty dictionary
            if(old == null)
            {
                old = new Dictionary<string,PyTuple>();
            }

            // Add the cache loaded into the dictionary
            old.Add(name, data);

            // Load the dictionary with all the cache data into the userCacheData Dictionary
            userCacheData.Add(user, old);

            return true;
        }

        public static PyObject GetUserCache(int user, string name)
        {
            if (LoadUserCacheFor(user, name) == false)
            {
                return null;
            }

            // We can assume this will not throw any exception as we've just checked if it exists
            PyTuple info = userCacheData[user][name];

            PyCachedObject obj = new PyCachedObject();
            obj.nodeID = info.Items[2].As<PyIntegerVar>().Value;
            obj.objectID = new PyString(name);
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = info.Items[0].As<PyBuffer>();
            obj.timestamp = info.Items[1].As<PyLongLong>().Value;
            obj.version = info.Items[3].As<PyIntegerVar>().Value;

            return obj.Encode();
        }

        public static PyObject GetUserCacheData(int user, string name)
        {
            if (LoadUserCacheFor(user, name) == false)
            {
                return null;
            }

            PyTuple cache = userCacheData[user][name];

            if (cache == null)
            {
                return null;
            }

            CacheInfo data = new CacheInfo();
            data.objectID = new PyString(name);
            data.cacheTime = cache.Items[1].As<PyLongLong>().Value;
            data.nodeID = cache.Items[2].As<PyIntegerVar>().Value;
            data.version = cache.Items[3].As<PyIntegerVar>().Value; // This is a CRC of the buffer

            return data.Encode();
        }

        public static bool SaveUserCacheFor(int user, string name, PyObject data, long timestamp, int ownerType, string ownerName)
        {
            byte[] marshaled = Marshal.Marshal.Process(data);

            if (Database.CacheDB.SaveUserCacheData(user, name, marshaled, timestamp, ownerName, ownerType) == false)
            {
                Log.Error("Cache", "Cannot save usercache data for " + name);
                return false;
            }

            Log.Debug("Cache", "Saved usercache data for " + name);

            if (UpdateUserCache(user, name) == false)
            {
                Log.Error("Cache", "Cannot update local usercache info");
                return false;
            }

            return true;
        }

        /*
         * General purpose server cache
         */
        public static bool LoadCacheFor(string name)
        {
            return LoadCacheFor(name, false);
        }

        public static bool UpdateCache(string name)
        {
            return LoadCacheFor(name, true);
        }

        private static bool LoadCacheFor(string name, bool force)
        {
            // First search for the cache
            if (cacheData.ContainsKey(name) == true)
            {
                if (force == false)
                {
                    return true;
                }

                cacheData.Remove(name);
            }

            PyTuple data = Database.CacheDB.GetCacheData(name);

            if (data == null)
            {
                return false;
            }

            cacheData.Add(name, data);

            return true;
        }

        public static PyObject GetCache(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                return null;
            }

            PyTuple info = cacheData[name];

            PyCachedObject obj = new PyCachedObject();
            obj.nodeID = info.Items[2].As<PyIntegerVar>().Value;
            obj.objectID = new PyString(name);
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = info.Items[0].As<PyBuffer>();
            obj.timestamp = info.Items[1].As<PyLongLong>().Value;
            obj.version = info.Items[3].As<PyIntegerVar>().Value;

            return obj.Encode();
        }

        public static PyObject GetCacheData(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                Log.Error("Cache", "Cannot load cache data for cache " + name);
                return null;
            }

            PyTuple cache = cacheData[name];

            if (cache == null)
            {
                return null;
            }

            CacheInfo data = new CacheInfo();
            data.objectID = new PyString(name);
            data.cacheTime = cache.Items[1].As<PyLongLong>().Value;
            data.nodeID = cache.Items[2].As<PyIntegerVar>().Value;
            data.version = cache.Items[3].As<PyIntegerVar>().Value; // This is a CRC of the buffer

            return data.Encode();
        }

        public static bool SaveCacheFor(string name, PyObject data, long timestamp)
        {
            byte[] marshaled = Marshal.Marshal.Process(data);

            if (Database.CacheDB.SaveCacheData(name, marshaled, timestamp) == false)
            {
                Log.Error("Cache", "Cannot save cache dat for " + name);
                return false;
            }

            Log.Debug("Cache", "Saved cache data for " + name);

            if (UpdateCache(name) == false)
            {
                Log.Error("Cache", "Cannot update local cache info");
                return false;
            }

            return true;
        }

        public static PyDict GetCacheHints()
        {
            PyDict result = new PyDict();

            for(int i = 0; i < LoginCacheTableSize; i ++)
            {
                result.Set(LoginCacheTable[i], GetCacheData(LoginCacheTable[i]));
            }

            return result;
        }

        public static bool GenerateCache()
        {
            MySqlDataReader reader = null;

            if (Database.Database.Query(ref reader, "SELECT activityID, activityName, iconNo, description, published FROM ramactivities") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramactivities");
                return false;
            }

            SaveCacheFor(LoginCacheTable[0], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT billTypeID, billTypeName, description FROM billtypes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for billtypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[1], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT bloodlineID, bloodlineName, raceID, description, maleDescription, femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription, 0 AS dataID FROM chrBloodlines") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for bloodlines");
                return false;
            }

            SaveCacheFor(LoginCacheTable[2], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT unitID, unitName, displayName FROM eveUnits") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for units");
                return false;
            }

            SaveCacheFor(LoginCacheTable[3], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT corporationID, tickerName, shape1, shape2, shape3, color1, color2, color3 FROM corporation WHERE hasPlayerPersonnelManager = 0") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for tickernames");
                return false;
            }

            SaveCacheFor(LoginCacheTable[4], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT typeID, activityID, requiredTypeID, quantity, damagePerJob, recycle FROM typeActivityMaterials WHERE damagePerJob != 1.0 OR recycle = 1") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramtyperequirements");
                return false;
            }

            SaveCacheFor(LoginCacheTable[5], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT a.assemblyLineTypeID, b.activityID, a.groupID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerGroup AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypesdetailpergroup");
                return false;
            }

            SaveCacheFor(LoginCacheTable[6], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT assemblyLineTypeID, assemblyLineTypeName, assemblyLineTypeName AS typeName, description, activityID, baseTimeMultiplier, baseMaterialMultiplier, volume FROM ramAssemblyLineTypes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[7], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT allianceID, shortName FROM alliance_shortnames") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for alliance_shortnames");
                return false;
            }

            SaveCacheFor(LoginCacheTable[8], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT completedStatusID, completedStatusName, completedStatusText FROM ramCompletedStatuses") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramcompletedstatuses");
                return false;
            }

            SaveCacheFor(LoginCacheTable[8], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT categoryID, categoryName, description, graphicID, published, 0 AS dataID FROM invCategories") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invCategories");
                return false;
            }

            SaveCacheFor(LoginCacheTable[9], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT reactionTypeID, input, typeID, quantity FROM invTypeReactions") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invTypeReactions");
                return false;
            }
            SaveCacheFor(LoginCacheTable[10], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT typeID, effectID, isDefault FROM dgmTypeEffects") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for dgmTypeEffects");
                return false;
            }

            SaveCacheFor(LoginCacheTable[11], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT metaGroupID, metaGroupName, description, graphicID, 0 AS dataID FROM invMetaGroups") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invMetaGroups");
                return false;
            }

            SaveCacheFor(LoginCacheTable[12], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials WHERE activityID = 6 AND damagePerJob = 1.0 UNION SELECT productTypeID AS typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE activityID = 1 AND damagePerJob = 1.0") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramTypeMaterials");
                return false;
            }

            SaveCacheFor(LoginCacheTable[13], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT a.assemblyLineTypeID, b.activityID, a.categoryID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerCategory AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypesdetailpercategory");
                return false;
            }

            SaveCacheFor(LoginCacheTable[14], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT cacheOwner AS ownerID, cacheOwnerName AS ownerName, cacheOwnerType AS typeID FROM usercache") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for cacheowner");
                return false;
            }

            SaveCacheFor(LoginCacheTable[15], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT ownerID, ownerName, typeID FROM eveStaticOwners") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for staticowners");
                return false;
            }

            SaveCacheFor(LoginCacheTable[16], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT raceID, raceName, description, graphicID, shortDescription, 0 AS dataID FROM chrRaces") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for races");
                return false;
            }

            SaveCacheFor(LoginCacheTable[17], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT attributeID, attributeName, description, graphicID FROM chrAttributes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for attributes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[18], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT	dgmTypeAttributes.typeID,	dgmTypeAttributes.attributeID,	IF(valueInt IS NULL, valueFloat, valueInt) AS value FROM dgmTypeAttributes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for dgmtypeattribs");
                return false;
            }

            SaveCacheFor(LoginCacheTable[19], DBUtils.DBResultToPackedRowList(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT locationID, locationName, x, y, z FROM cacheLocations") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for locations");
                return false;
            }

            SaveCacheFor(LoginCacheTable[20], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT locationID, wormholeClassID FROM mapLocationWormholeClasses") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for locationwormholeclasses");
                return false;
            }

            SaveCacheFor(LoginCacheTable[21], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT groupID, categoryID, groupName, description, graphicID, useBasePrice, allowManufacture, allowRecycler, anchored, anchorable, fittableNonSingleton, 1 AS published, 0 AS dataID FROM invGroups") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for groups");
                return false;
            }

            SaveCacheFor(LoginCacheTable[22], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());
            
            if(Database.Database.Query(ref reader, "SELECT shipTypeID,weaponTypeID,miningTypeID,skillTypeID FROM invShipTypes") == false)
            {
                Log.Error("Cache", "Cannot create cache data for shiptypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[23], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT attributeID, attributeName, attributeCategory, description, maxAttributeID, attributeIdx, graphicID, chargeRechargeTimeID, defaultValue, published, displayName, unitID, stackable, highIsGood, categoryID, 0 AS dataID FROM dgmAttributeTypes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for dgmattribs");
                return false;
            }

            SaveCacheFor(LoginCacheTable[24], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT flagID, flagName, flagText, flagType, orderID FROM invFlags") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for flags");
                return false;
            }

            SaveCacheFor(LoginCacheTable[25], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT invTypes.typeName AS blueprintTypeName, invTypes.description, invTypes.graphicID, invTypes.basePrice, blueprintTypeID, parentBlueprintTypeID, productTypeID, productionTime, techLevel, researchProductivityTime, researchMaterialTime, researchCopyTime, researchTechTime, productivityModifier, materialModifier, wasteFactor, chanceOfReverseEngineering, maxProductionLimit FROM invBlueprintTypes, invTypes WHERE invBlueprintTypes.blueprintTypeID = invTypes.typeID") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for bptypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[26], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT graphicID, url3D, urlWeb, icon, urlSound, explosionID FROM eveGraphics") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for graphics");
                return false;
            }

            SaveCacheFor(LoginCacheTable[27], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT celestialID, description FROM mapCelestialDescriptions") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for mapcelestialdescriptions");
                return false;
            }

            SaveCacheFor(LoginCacheTable[28], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT certificateID, categoryID, classID, grade, iconID, corpID, description, 0 AS dataID FROM crtCertificates") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for certificates");
                return false;
            }

            SaveCacheFor(LoginCacheTable[29], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT locationID, locationName, x, y, z FROM eveStaticLocations") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for staticLocations");
                return false;
            }

            SaveCacheFor(LoginCacheTable[30], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT factionID, typeID, standingLoss, confiscateMinSec, fineByValue, attackMinSec FROM invContrabandTypes") == false)
            {
                Log.Error("Cache", "Cannot generate invContrabandTypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[31], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships") == false)
            {
                Log.Error("Cache", "Cannot generate certificateRelationships");
                return false;
            }

            SaveCacheFor(LoginCacheTable[32], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT unitID,unitName,displayName FROM eveUnits") == false)
            {
                Log.Error("Cache", "Cannot generate units");
                return false;
            }

            SaveCacheFor(LoginCacheTable[33], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT effectID, effectName, effectCategory, preExpression, postExpression, description, guid, graphicID, isOffensive, isAssistance, durationAttributeID, trackingSpeedAttributeID, dischargeAttributeID, rangeAttributeID, falloffAttributeID, published, displayName, isWarpSafe, rangeChance, electronicChance, propulsionChance, distribution, sfxName, npcUsageChanceAttributeID, npcActivationChanceAttributeID, 0 AS fittingUsageChanceAttributeID, 0 AS dataID FROM dgmEffects") == false)
            {
                Log.Error("Cache", "Cannot generate dgmeffects");
                return false;
            }

            SaveCacheFor(LoginCacheTable[34], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating, 0 AS dataID FROM invTypes") == false)
            {
                Log.Error("Cache", "Cannot generate types");
                return false;
            }

            SaveCacheFor(LoginCacheTable[35], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTimeUtc());

            if (Database.Database.Query(ref reader, "SELECT typeID, parentTypeID, metaGroupID FROM invMetaTypes") == false)
            {
                Log.Error("Cache", "Cannot generate invMetaTypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[36], DBUtils.DBResultToCRowset(ref reader), DateTime.Now.ToFileTimeUtc());

            Log.Debug("Cache", "Basic cache generated correctly");

            return true;
        }
    }
}
