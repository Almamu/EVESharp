
/*Table structure for table `channels` */

DROP TABLE IF EXISTS `channels`;

CREATE TABLE `channels` (
  `channelID` int(10) unsigned NOT NULL auto_increment,
  `ownerID` int(10) unsigned NOT NULL default '0',
  `relatedEntityID` int(10) unsigned default NULL,
  `displayName` varchar(85) default NULL,
  `motd` text,
  `comparisonKey` varchar(11) default NULL,
  `memberless` tinyint(4) NOT NULL default '0',
  `password` varchar(100) default NULL,
  `mailingList` tinyint(4) NOT NULL default '0',
  `cspa` tinyint(4) NOT NULL default '0',
  `temporary` tinyint(4) NOT NULL default '0',
  `estimatedMemberCount` int(10) unsigned NOT NULL default '0',
  PRIMARY KEY  (`channelID`),
  KEY `FK_CHANNELS_RELEATEDENTITY` (`relatedEntityID`),
  KEY `FK_CHANNELS_OWNER` (`ownerID`)
) ENGINE=InnoDB AUTO_INCREMENT=1000 DEFAULT CHARSET=utf8;

/*Data for the table `channels` */

INSERT INTO `channels`(`channelID`,`ownerID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`) VALUES (1,1,'Help\\Rookie Help','rookie MOTD','rookiehelp',1,NULL,0,100,0,0),(2,1,'Help\\Help','help MOTD','help',1,NULL,0,100,0,0);

/* Insert solar systems into the channels table */
INSERT INTO `channels`(`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT 1 AS ownerID, solarSystemID as relatedEntityID, "System Channels\\Local" AS displayName, solarSystemName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 0 AS mailingList, 100 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapSolarSystems;

/* Insert constellations into the channels table */
INSERT INTO `channels`(`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT 1 AS ownerID, constellationID as relatedEntityID, "System Channels\\Constellation" AS displayName, constellationName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 0 AS mailingList, 100 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapConstellations;

/* Insert regions into the channels table */
INSERT INTO `channels`(`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT 1 AS ownerID, regionID as relatedEntityID, "System Channels\\Region" AS displayName, regionName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 0 AS mailingList, 100 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapRegions;

/* Insert NPC corporations into the channels table */
INSERT INTO `channels`(`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT corporationID AS ownerID, corporationID as relatedEntityID, "System Channels\\Corp" AS displayName, corporationName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 0 AS mailingList, 100 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM corporation;

/*Table structure for table `channelChars` */

DROP TABLE IF EXISTS `channelChars`;

CREATE TABLE `channelChars` (
  `channelID` int(10) unsigned NOT NULL default '0',
  `charID` int(10) unsigned NOT NULL default '0',
  `role` int(10) unsigned NOT NULL default '0',
  `extra` int(10) unsigned NOT NULL default '0',
  PRIMARY KEY  (`channelID`,`charID`),
  KEY `FK_CHANNELCHARS_CHARACTER` (`charID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `channelChars` */

/*Table structure for table `channelMods` */

DROP TABLE IF EXISTS `channelMods`;

CREATE TABLE `channelMods` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `channelID` int(10) unsigned NOT NULL default '0',
  `accessor` int(10) unsigned default NULL,
  `mode` int(10) unsigned NOT NULL default '0',
  `untilWhen` bigint(20) unsigned default NULL,
  `originalMode` int(10) unsigned default NULL,
  `admin` varchar(85) default NULL,
  `reason` text,
  PRIMARY KEY  (`id`),
  KEY `FK_CHANNELMODS_CHANNELS` (`channelID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `channelMods` */
