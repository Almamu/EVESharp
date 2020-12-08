/**
 * Base tables for corporations
 */
DROP TABLE IF EXISTS `corporation`;

CREATE TABLE `corporation` (
  `corporationID` int(10) unsigned NOT NULL,
  `corporationName` varchar(100) NOT NULL default '',
  `description` mediumtext NOT NULL,
  `tickerName` varchar(8) NOT NULL default '',
  `url` mediumtext NOT NULL,
  `taxRate` double NOT NULL default '0',
  `minimumJoinStanding` double NOT NULL default '0',
  `corporationType` int(10) unsigned NOT NULL default '0',
  `hasPlayerPersonnelManager` tinyint(3) unsigned NOT NULL default '0',
  `sendCharTerminationMessage` tinyint(3) unsigned NOT NULL default '1',
  `creatorID` int(10) unsigned NOT NULL default '0',
  `ceoID` int(10) unsigned NOT NULL default '0',
  `stationID` int(10) unsigned NOT NULL default '0',
  `raceID` int(10) unsigned default NULL,
  `allianceID` int(10) unsigned NOT NULL default '0',
  `shares` bigint(20) unsigned NOT NULL default '1000',
  `memberCount` int(10) unsigned NOT NULL default '0',
  `memberLimit` int(10) unsigned NOT NULL default '10',
  `allowedMemberRaceIDs` int(10) unsigned NOT NULL default '0',
  `graphicID` int(10) unsigned NOT NULL default '0',
  `shape1` int(10) unsigned default NULL,
  `shape2` int(10) unsigned default NULL,
  `shape3` int(10) unsigned default NULL,
  `color1` int(10) unsigned default NULL,
  `color2` int(10) unsigned default NULL,
  `color3` int(10) unsigned default NULL,
  `typeface` varchar(11) default NULL,
  `division1` varchar(100) default '1st division',
  `division2` varchar(100) default '2nd division',
  `division3` varchar(100) default '3rd division',
  `division4` varchar(100) default '4th division',
  `division5` varchar(100) default '5th division',
  `division6` varchar(100) default '6th division',
  `division7` varchar(100) default '7th division',
  `balance` double NOT NULL default '0',
  `deleted` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`corporationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*
 * Copy over the static corporation info
 */
INSERT INTO corporation
 SELECT * FROM crpStatic;