using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Network.Caching;

namespace EVESharp.EVE.Data;

public static class Cache
{
    /// <summary>
    /// BulkData fetched by the EVE client on login
    /// </summary>
    public static readonly Dictionary <string, string> LoginCacheTable = new Dictionary <string, string>
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
        {"config.BulkData.invmetatypes", "config.BulkData.invmetatypes"}
    };

    /// <summary>
    /// Queries to populate the BulkData for the EVE Client on login
    /// </summary>
    public static readonly string [] LoginCacheQueries =
    {
        "SELECT activityID, activityName, iconNo, description, published FROM ramActivities",
        "SELECT billTypeID, billTypeName, description FROM billTypes",
        "SELECT bloodlineID, bloodlineName, raceID, description, maleDescription, femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription, dataID FROM chrBloodlines",
        "SELECT unitID, unitName, displayName FROM eveUnits",
        "SELECT corporationID, tickerName, shape1, shape2, shape3, color1, color2, color3 FROM corporation WHERE hasPlayerPersonnelManager = 0",
        "SELECT typeID, activityID, requiredTypeID, quantity, damagePerJob, recycle FROM typeActivityMaterials WHERE damagePerJob != 1.0 OR recycle = 1",
        "SELECT a.assemblyLineTypeID, b.activityID, a.groupID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerGroup AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID",
        "SELECT assemblyLineTypeID, assemblyLineTypeName, assemblyLineTypeName AS typeName, description, activityID, baseTimeMultiplier, baseMaterialMultiplier, volume FROM ramAssemblyLineTypes",
        "SELECT allianceID, shortName FROM crpAlliances",
        "SELECT completedStatusID, completedStatusName, completedStatusText FROM ramCompletedStatuses",
        "SELECT categoryID, categoryName, description, graphicID, published, 0 AS dataID FROM invCategories",
        "SELECT reactionTypeID, input, typeID, quantity FROM invTypeReactions",
        "SELECT typeID, effectID, isDefault FROM dgmTypeEffects",
        "SELECT metaGroupID, metaGroupName, description, graphicID, 0 AS dataID FROM invMetaGroups",
        "SELECT typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials WHERE activityID = 6 AND damagePerJob = 1.0 UNION SELECT productTypeID AS typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE activityID = 1 AND damagePerJob = 1.0",
        "SELECT a.assemblyLineTypeID, b.activityID, a.categoryID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerCategory AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID",
        "SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM eveNames WHERE itemID = 0",
        $"SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM eveNames WHERE categoryID = 1 AND itemID < {ItemRanges.UserGenerated.MIN}",
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
#if DEBUG
        "SELECT graphicID, url3D, urlWeb, icon, urlSound, description, explosionID FROM eveGraphics", // include description on debug builds so the developers can search by description value
#else
            "SELECT graphicID, url3D, urlWeb, icon, urlSound, explosionID FROM eveGraphics",
#endif
        "SELECT celestialID, description FROM mapCelestialDescriptions",
        "SELECT certificateID, categoryID, classID, grade, iconID, corpID, description, 0 AS dataID FROM crtCertificates",
        $"SELECT itemID AS locationID, itemName as locationName, x, y, z FROM invItems LEFT JOIN eveNames USING (itemID) LEFT JOIN invPositions USING (itemID) WHERE (groupID = {(int) GroupID.Station} OR groupID = {(int) GroupID.Constellation} OR groupID = {(int) GroupID.SolarSystem} OR groupID = {(int) GroupID.Region}) AND itemID < {ItemRanges.UserGenerated.MIN}",
        "SELECT factionID, typeID, standingLoss, confiscateMinSec, fineByValue, attackMinSec FROM invContrabandTypes",
        "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships",
        "SELECT unitID,unitName,displayName FROM eveUnits",
        "SELECT effectID, effectName, effectCategory, preExpression, postExpression, description, guid, graphicID, isOffensive, isAssistance, durationAttributeID, trackingSpeedAttributeID, dischargeAttributeID, rangeAttributeID, falloffAttributeID, published, displayName, isWarpSafe, rangeChance, electronicChance, propulsionChance, distribution, sfxName, npcUsageChanceAttributeID, npcActivationChanceAttributeID, 0 AS fittingUsageChanceAttributeID, 0 AS dataID FROM dgmEffects",
        "SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating, dataID FROM invTypes",
        "SELECT typeID, parentTypeID, metaGroupID FROM invMetaTypes"
    };

    /// <summary>
    /// How the BulkData for the EVE Client should be stored
    /// </summary>
    public static readonly CacheObjectType [] LoginCacheTypes =
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

    /// <summary>
    /// Cache entries for the character creation information
    /// </summary>
    public static readonly Dictionary <string, string> CreateCharacterCacheTable = new Dictionary <string, string>
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

    /// <summary>
    /// Queries to populate the character creation cache
    /// </summary>
    public static readonly string [] CreateCharacterCacheQueries =
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

    /// <summary>
    /// How the character creation caches will be stored
    /// </summary>
    public static readonly CacheObjectType [] CreateCharacterCacheTypes =
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

    public static readonly Dictionary <string, string> CharacterAppearanceCacheTable = new Dictionary <string, string>
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

    public static readonly string [] CharacterAppearanceCacheQueries =
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

    public static readonly CacheObjectType [] CharacterAppearanceCacheTypes =
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
}