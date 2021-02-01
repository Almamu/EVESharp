
/*Table structure for table `lscGeneralChannels` */

DROP TABLE IF EXISTS `lscGeneralChannels`;

CREATE TABLE `lscGeneralChannels` (
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
) ENGINE=InnoDB AUTO_INCREMENT=2100000000 DEFAULT CHARSET=utf8;

CREATE TABLE `lscPrivateChannels` (
  `channelID` int(10) unsigned NOT NULL auto_increment,
  `ownerID` int(10) unsigned NOT NULL default '0',
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
  KEY `FK_CHANNELS_OWNER` (`ownerID`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;

/*Data for the table `lscGeneralChannels` */

/* Create the default channels */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`) VALUES
 (1, 1, 'Help\\Rookie Help', '', NULL, 0, NULL, 0, 0, 0, 0),
 (2, 1, 'Help\\Help', '', NULL, 0, NULL, 0, 0, 0, 0),
 (10, 1, 'Trade\\Other', '', NULL, 0, NULL, 0, 0, 0, 0),
 (11, 1, 'Trade\\Ships', '', NULL, 0, NULL, 0, 0, 0, 0),
 (12, 1, 'Trade\\Blueprints', '', NULL, 0, NULL, 0, 0, 0, 0),
 (13, 1, 'Trade\\Modules and Munitions', '', NULL, 0, NULL, 0, 0, 0, 0),
 (14, 1, 'Trade\\Minerals and Manufacturing', '', NULL, 0, NULL, 0, 0, 0, 0),
 (15, 1, 'Trade\\Real Estate', '', NULL, 0, NULL, 0, 0, 0, 0),
 (16, 1, 'Empires\\Caldari', '', NULL, 0, NULL, 0, 0, 0, 0),
 (17, 1, 'Empires\\Amarr', '', NULL, 0, NULL, 0, 0, 0, 0),
 (18, 1, 'Empires\\Minmatar', '', NULL, 0, NULL, 0, 0, 0, 0),
 (19, 1, 'Empires\\Gallente', '', NULL, 0, NULL, 0, 0, 0, 0),
 (20, 1, 'Empires\\Jove', '', NULL, 0, NULL, 0, 0, 0, 0),
 (21, 1, 'Alliances\\Smacktalk', '', NULL, 0, NULL, 0, 0, 0, 0),
 (22, 1, 'Alliances\\Rumour Mill', '', NULL, 0, NULL, 0, 0, 0, 0),
 (23, 1, 'Alliances\\Freelancer', '', NULL, 0, NULL, 0, 0, 0, 0),
 (24, 1, 'Corporate\\Recruitment', '', NULL, 0, NULL, 0, 0, 0, 0),
 (25, 1, 'Corporate\\CEO', '', NULL, 0, NULL, 0, 0, 0, 0),
 (26, 1, 'Languages\\English', '', NULL, 0, NULL, 0, 0, 0, 0),
 (27, 1, 'Languages\\German', '', NULL, 0, NULL, 0, 0, 0, 0),
 (28, 1, 'Languages\\French', '', NULL, 0, NULL, 0, 0, 0, 0),
 (29, 1, 'Languages\\Swedish', '', NULL, 0, NULL, 0, 0, 0, 0),
 (30, 1, 'Languages\\Danish', '', NULL, 0, NULL, 0, 0, 0, 0),
 (31, 1, 'Languages\\Japanese', '', NULL, 0, NULL, 0, 0, 0, 0),
 (32, 1, 'Languages\\Icelandic', '', NULL, 0, NULL, 0, 0, 0, 0),
 (33, 1, 'Languages\\Norwegian', '', NULL, 0, NULL, 0, 0, 0, 0),
 (34, 1, 'Languages\\Russian', '', NULL, 0, NULL, 0, 0, 0, 0),
 (35, 1, 'Languages\\Italian', '', NULL, 0, NULL, 0, 0, 0, 0),
 (36, 1, 'Languages\\Spanish', '', NULL, 0, NULL, 0, 0, 0, 0),
 (37, 1, 'Languages\\Portuguese', '', NULL, 0, NULL, 0, 0, 0, 0),
 (38, 1, 'Languages\\Albanian', '', NULL, 0, NULL, 0, 0, 0, 0),
 (40, 1, 'Help\\Hilfe', '', NULL, 0, NULL, 0, 0, 0, 0),
 (41, 1, 'Help\\Adoptions', '', NULL, 0, NULL, 0, 0, 0, 0),
 (42, 1, 'EVE Radio', '', NULL, 0, NULL, 0, 0, 0, 0),
 (43, 1, 'EVE Guardian News', '', NULL, 0, NULL, 0, 0, 0, 0),
 (44, 1, 'The Scope', '', NULL, 0, NULL, 0, 0, 0, 0),
 (45, 1, 'Events', '', NULL, 0, NULL, 0, 0, 0, 0),
 (46, 1, 'EVE Chat', '', NULL, 0, NULL, 0, 0, 0, 0),
 (47, 1, 'Missions', '', NULL, 0, NULL, 0, 0, 0, 0),
 (48, 1, 'Languages\\Dutch', '', NULL, 0, NULL, 0, 0, 0, 0),
 (49, 1, 'Languages\\Finnish', '', NULL, 0, NULL, 0, 0, 0, 0),
 (50, 1, 'Languages\\Polish', '', NULL, 0, NULL, 0, 0, 0, 0),
 (51, 1, 'Languages\\Slovenian', '', NULL, 0, NULL, 0, 0, 0, 0),
 (52, 1, 'Languages\\Chinese', '', NULL, 0, NULL, 0, 0, 0, 0),
 (53, 1, 'Languages\\Korean', '', NULL, 0, NULL, 0, 0, 0, 0),
 (55, 1, 'Combat Simulator Lobby', '', NULL, 0, NULL, 0, 0, 0, 0),
 (56, 1, 'Help\\\u30d8\u30eb\u30d7', '', NULL, 0, NULL, 0, 0, 0, 0),
 (901, 1, 'Help\\Rookie Help - Caldari - Deteis', '', NULL, 0, NULL, 0, 0, 0, 0),
 (902, 1, 'Help\\Rookie Help - Caldari - Civire', '', NULL, 0, NULL, 0, 0, 0, 0),
 (903, 1, 'Help\\Rookie Help - Minmatar - Sebiestor', '', NULL, 0, NULL, 0, 0, 0, 0),
 (904, 1, 'Help\\Rookie Help - Minmatar - Brutor', '', NULL, 0, NULL, 0, 0, 0, 0),
 (905, 1, 'Help\\Rookie Help - Amarr - Amarr', '', NULL, 0, NULL, 0, 0, 0, 0),
 (906, 1, 'Help\\Rookie Help - Amarr - Ni-Kunni', '', NULL, 0, NULL, 0, 0, 0, 0),
 (907, 1, 'Help\\Rookie Help - Gallente - Gallente', '', NULL, 0, NULL, 0, 0, 0, 0),
 (908, 1, 'Help\\Rookie Help - Gallente - Intaki', '', NULL, 0, NULL, 0, 0, 0, 0),
 (911, 1, 'Help\\Rookie Help - Caldari - Achura', '', NULL, 0, NULL, 0, 0, 0, 0),
 (912, 1, 'Help\\Rookie Help - Gallente - Jin-Mei', '', NULL, 0, NULL, 0, 0, 0, 0),
 (913, 1, 'Help\\Rookie Help - Amarr - Khanid', '', NULL, 0, NULL, 0, 0, 0, 0),
 (914, 1, 'Help\\Rookie Help - Minmatar - Vherokior', '', NULL, 0, NULL, 0, 0, 0, 0);

/* Insert solar systems into the lscGeneralChannels table */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT NULL AS channelID, 1 AS ownerID, solarSystemID as relatedEntityID, "System Channels\\Local" AS displayName, solarSystemName AS motd, NULL AS comparisonKey, 1 AS memberless, NULL AS password, 0 AS mailingList, 1 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapSolarSystems;

/* Insert constellations into the lscGeneralChannels table */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT NULL AS channelID, 1 AS ownerID, constellationID as relatedEntityID, "System Channels\\Constellation" AS displayName, constellationName AS motd, NULL AS comparisonKey, 1 AS memberless, NULL AS password, 0 AS mailingList, 1 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapConstellations;

/* Insert regions into the lscGeneralChannels table */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT NULL AS channelID, 1 AS ownerID, regionID as relatedEntityID, "System Channels\\Region" AS displayName, regionName AS motd, NULL AS comparisonKey, 1 AS memberless, NULL AS password, 0 AS mailingList, 1 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM mapRegions;

/* Insert NPC corporations into the lscGeneralChannels table */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT NULL AS channelID, corporationID AS ownerID, corporationID as relatedEntityID, "System Channels\\Corp" AS displayName, corporationName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 0 AS mailingList, 1 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM corporation;

/* Insert mailing lists for NPC corporations into the lscGeneralChannels table */
INSERT INTO `lscGeneralChannels`(`channelID`,`ownerID`,`relatedEntityID`,`displayName`,`motd`,`comparisonKey`,`memberless`,`password`,`mailingList`,`cspa`,`temporary`,`estimatedMemberCount`)
  SELECT corporationID AS channelID, corporationID AS ownerID, corporationID as relatedEntityID, "System Channels\\Corp" AS displayName, corporationName AS motd, NULL AS comparisonKey, 0 AS memberless, NULL AS password, 1 AS mailingList, 1 AS cspa, 0 AS temporary, 0 AS estimatedMemberCount FROM corporation;

/*Table structure for table `lscChannelPermissions` */

DROP TABLE IF EXISTS `lscChannelPermissions`;

CREATE TABLE `lscChannelPermissions` (
  `channelID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `accessor` INT(10) UNSIGNED NOT NULL,
  `mode` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `untilWhen` BIGINT(20) UNSIGNED NULL DEFAULT NULL,
  `originalMode` INT(10) UNSIGNED NULL DEFAULT NULL,
  `admin` VARCHAR(85) NULL DEFAULT NULL,
  `reason` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`channelID`, `accessor`),
  KEY `FK_CHANNELMODS_CHANNELS` (`channelID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `lscChannelPermissions` */
