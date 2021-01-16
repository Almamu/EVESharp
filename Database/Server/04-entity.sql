/*
 * Disable auto value on 0 to properly create the required items
 */
SET SESSION sql_mode='NO_AUTO_VALUE_ON_ZERO';

/**
 * Tables for ingame entities
 */

DROP TABLE IF EXISTS `entity`;

CREATE TABLE `entity` (
  `itemID` int(10) unsigned NOT NULL auto_increment,
  `itemName` varchar(85) NOT NULL default '',
  `typeID` int(10) unsigned NOT NULL default '0',
  `ownerID` int(10) unsigned NOT NULL default '0',
  `locationID` int(10) unsigned NOT NULL default '0',
  `flag` int(10) unsigned NOT NULL default '0',
  `contraband` int(10) unsigned NOT NULL default '0',
  `singleton` int(10) unsigned NOT NULL default '0',
  `quantity` int(10) unsigned NOT NULL default '0',
  `x` double NOT NULL default '0',
  `y` double NOT NULL default '0',
  `z` double NOT NULL default '0',
  `customInfo` text,
  `nodeID` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY  (`itemID`),
  KEY `typeID` (`typeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `entity_attributes`;

CREATE TABLE `entity_attributes` (
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
INSERT INTO entity (itemID, itemName, singleton, quantity)
  VALUES (0, '(none)', 1, 1);
/*
 * Static record of EVE System
 */
INSERT INTO entity (itemID, itemName, singleton, quantity)
  VALUES (1, 'EVE System', 1, 1);
/*
 * Insert factions
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, singleton, quantity)
  SELECT factionID, factionName, 30, corporationID, solarSystemID, 1, 1
    FROM chrFactions;
/*
 * Insert regions
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, x, y, z, singleton, quantity)
  SELECT regionID, regionName, 3, 1, 1, x, y, z, 1, 1
    FROM mapRegions;
/*
 * Insert constellations
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, x, y, z, singleton, quantity)
  SELECT constellationID, constellationName, 4, 1, regionID, x, y, z, 1, 1
    FROM mapConstellations;
/*
 * Insert solar systems
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, singleton, quantity, x, y, z)
 SELECT solarSystemID, solarSystemName, 5, 1, constellationID, 1, 1, x, y, z
 FROM mapSolarSystems;
/*
 * Insert stations
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, singleton, quantity, x, y, z)
 SELECT stationID, stationName, stationTypeID, corporationID, solarSystemID, 1, 1, x, y, z
 FROM staStations;
/*
 * Insert static characters to entity table
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, singleton, quantity)
 SELECT characterID, characterName, typeID, 1, stationID, 1, 1
  FROM chrStatic;
/*
 * Insert corporations
 */
INSERT INTO entity (itemID, itemName, typeID, ownerID, locationID, singleton, quantity)
  SELECT corporationID, corporationName, 2, ceoID, stationID, 1, 1
    FROM crpStatic;
/*
 * Set the auto-increment lower bound
 */
ALTER TABLE entity AUTO_INCREMENT = 100000000;