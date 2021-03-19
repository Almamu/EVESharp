/*
 * Disable auto value on 0 to properly create the required items
 */
SET SESSION sql_mode='NO_AUTO_VALUE_ON_ZERO';

/**
 * Tables for ingame entities
 */

DROP TABLE IF EXISTS `invItems`;

CREATE TABLE `invItems` (
  `itemID` int(10) unsigned NOT NULL auto_increment,
  `typeID` int(10) unsigned NOT NULL default '0',
  `ownerID` int(10) unsigned NOT NULL default '0',
  `locationID` int(10) unsigned NOT NULL default '0',
  `flag` int(10) unsigned NOT NULL default '0',
  `contraband` int(10) unsigned NOT NULL default '0',
  `singleton` int(10) unsigned NOT NULL default '0',
  `quantity` int(10) unsigned NOT NULL default '0',
  `customInfo` text,
  `nodeID` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY  (`itemID`),
  KEY `typeID` (`typeID`),
  KEY `locationID` (`locationID`),
  KEY `ownerID` (`ownerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `invPositions`;

CREATE TABLE `invPositions` (
  `itemID` int(10) unsigned NOT NULL,
  `x` double NOT NULL default '0',
  `y` double NOT NULL default '0',
  `z` double NOT NULL default '0',
  PRIMARY KEY (`itemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `invItemsAttributes`;

CREATE TABLE `invItemsAttributes` (
  `itemID` int(10) unsigned NOT NULL default '0',
  `attributeID` int(10) unsigned NOT NULL default '0',
  `valueInt` bigint unsigned default NULL,
  `valueFloat` double unsigned default NULL,
  PRIMARY KEY  (`itemID`,`attributeID`),
  KEY `attributeID` (`attributeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/**
 * Insert owner for the EVE System
 */
INSERT INTO invItems (itemID, singleton, quantity)
  VALUES (0, 1, 1);
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  VALUES (0, '(none)', 0, 0, 0);
/*
 * Static record of EVE System
 */
INSERT INTO invItems (itemID, singleton, quantity)
  VALUES (1, 1, 1);
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  VALUES (1, 'EVE System', 0, 0, 0);
/*
 * Static record for the EVE Market
 */
INSERT INTO invItems (itemID, singleton, quantity)
  VALUES (3, 1, 1);
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  VALUES (3, 'EVE Market', 0, 0, 0);
/**
 * Static record for the EVE Temp item
 */
INSERT INTO invItems (itemID, singleton, quantity)
  VALUES (5, 1, 1);
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  VALUES (5, 'EVE Temp', 0, 0, 0);
/*
 * Static record for Recycler
 */
INSERT INTO invItems (itemID, singleton, quantity)
  VALUES (6, 1, 1);
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  VALUES (6, 'EVE Recycler', 0, 0, 0);
/*
 * Static records for universes
 */
INSERT INTO invItems (itemID, typeID, singleton, quantity)
  SELECT universeID, 1, 1, 1
    FROM mapUniverse;
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  SELECT universeID, universeName, 1, 0, 0
    FROM mapUniverse;
INSERT INTO invPositions (itemID, x, y, z)
  SELECT universeID, x, y, z
    FROM mapUniverse;
/*
 * Insert factions
 */
INSERT INTO invItems (itemID, typeID, ownerID, locationID, singleton, quantity)
  SELECT factionID, 30, corporationID, solarSystemID, 1, 1
    FROM chrFactions;
/*
 * Insert any map item
 */
INSERT INTO invItems (itemID, typeID, ownerID, locationID, singleton, quantity)
  SELECT itemID, typeID, IF(staStations.corporationID IS NULL, 1, staStations.corporationID), IF(mapDenormalize.solarSystemID IS NULL, IF(mapDenormalize.constellationID IS NULL, IF(mapDenormalize.regionID IS NULL, 9, mapDenormalize.regionID), mapDenormalize.constellationID), mapDenormalize.solarSystemID), 1, 1
    FROM mapDenormalize
      LEFT JOIN staStations ON staStations.stationID = mapDenormalize.itemID;
/*
 * Insert missing names
 */
INSERT INTO eveNames (itemID, itemName, typeID, groupID, categoryID)
  SELECT itemID, itemName, typeID, groupID, categoryID
    FROM mapDenormalize LEFT JOIN invGroups USING (groupID) WHERE itemID IN (
      SELECT mapDenormalize.itemID FROM mapDenormalize LEFT JOIN eveNames ON eveNames.itemID = mapDenormalize.itemID WHERE eveNames.itemName IS NULL
    );
/*
 * Insert missing positions
 */
INSERT INTO invPositions (itemID, x, y, z)
  SELECT itemID, x, y, z
    FROM mapDenormalize WHERE itemID IN (
      SELECT mapDenormalize.itemID FROM mapDenormalize LEFT JOIN invPositions ON invPositions.itemID = mapDenormalize.itemID WHERE invPositions.itemID IS NULL
    );
/*
 * Insert static characters to invItems table
 */
INSERT INTO invItems (itemID, typeID, ownerID, locationID, singleton, quantity)
 SELECT characterID, typeID, 1, stationID, 1, 1
  FROM chrStatic;
/*
 * Insert corporations
 */
INSERT INTO invItems (itemID, typeID, ownerID, locationID, singleton, quantity)
  SELECT crp.corporationID, 2, npc.factionID, crp.stationID, 1, 1
    FROM crpStatic AS crp
    LEFT JOIN crpNPCCorporations AS npc USING (corporationID);
/*
 * Set the auto-increment lower bound
 */
ALTER TABLE invItems AUTO_INCREMENT = 100000000;

/*
 * Add default capacity attribute to all the items that have a capacity greater than 0
 */
INSERT INTO dgmTypeAttributes(typeID, attributeID, valueInt, valueFloat) SELECT typeID, 38 AS attributeID, NULL AS valueInt, capacity AS valueFloat FROM invTypes WHERE capacity > 0;
INSERT INTO dgmTypeAttributes(typeID, attributeID, valueInt, valueFloat) SELECT typeID, 4 AS attributeID, NULL AS valueInt, mass AS valueFloat FROM invTypes WHERE mass > 0;
INSERT INTO dgmTypeAttributes(typeID, attributeID, valueInt, valueFloat) SELECT typeID, 161 AS attributeID, NULL AS valueInt, volume AS valueFloat FROM invTypes WHERE volume > 0;
INSERT INTO dgmTypeAttributes(typeID, attributeID, valueInt, valueFloat) SELECT typeID, 162 AS attributeID, NULL AS valueInt, radius AS valueFloat FROM invTypes WHERE radius > 0;
/*
 * Create the invTypes entry for the universe item type
 */
INSERT INTO invTypes(typeID, groupID, typeName, description, radius, mass, volume, capacity, portionSize, basePrice, published, chanceOfDuplicating)VALUES(1, 0, "Universe", "EVE Online Universe that contains everything in-game", (SELECT radius FROM mapUniverse LIMIT 1), 0, 0, 0, 0, 0, 0, 0);
