DROP TABLE IF EXISTS `chrInformation`;

/**
 * Character information
 */
CREATE TABLE `chrInformation` (
  `characterID` int(10) unsigned NOT NULL default '0',
  `accountID` int(10) unsigned default NULL,
  `activeCloneID` int(10) unsigned default NULL,
  `timeLastJump` BIGINT(20) NOT NULL DEFAULT '0',
  `title` varchar(85) NOT NULL default '',
  `description` text NOT NULL,
  `bounty` double NOT NULL default '0',
  `balance` double NOT NULL default '0',
  `securityRating` double NOT NULL default '0',
  `petitionMessage` varchar(85) NOT NULL default '',
  `logonMinutes` int(10) unsigned NOT NULL default '0',
  `corporationID` int(10) unsigned NOT NULL default '0',
  `corpRole` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `rolesAtAll` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `rolesAtBase` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `rolesAtHQ` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `rolesAtOther` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `corporationDateTime` bigint(20) unsigned NOT NULL default '0',
  `startDateTime` bigint(20) unsigned NOT NULL default '0',
  `createDateTime` bigint(20) unsigned NOT NULL default '0',
  `ancestryID` int(10) unsigned NOT NULL default '0',
  `careerID` int(10) unsigned NOT NULL default '0',
  `schoolID` int(10) unsigned NOT NULL default '0',
  `careerSpecialityID` int(10) unsigned NOT NULL default '0',
  `gender` tinyint(4) NOT NULL default '0',
  `accessoryID` int(10) unsigned default NULL,
  `beardID` int(10) unsigned default NULL,
  `costumeID` int(10) unsigned NOT NULL default '0',
  `decoID` int(10) unsigned default NULL,
  `eyebrowsID` int(10) unsigned NOT NULL default '0',
  `eyesID` int(10) unsigned NOT NULL default '0',
  `hairID` int(10) unsigned NOT NULL default '0',
  `lipstickID` int(10) unsigned default NULL,
  `makeupID` int(10) unsigned default NULL,
  `skinID` int(10) unsigned NOT NULL default '0',
  `backgroundID` int(10) unsigned NOT NULL default '0',
  `lightID` int(10) unsigned NOT NULL default '0',
  `headRotation1` double NOT NULL default '0',
  `headRotation2` double NOT NULL default '0',
  `headRotation3` double NOT NULL default '0',
  `eyeRotation1` double NOT NULL default '0',
  `eyeRotation2` double NOT NULL default '0',
  `eyeRotation3` double NOT NULL default '0',
  `camPos1` double NOT NULL default '0',
  `camPos2` double NOT NULL default '0',
  `camPos3` double NOT NULL default '0',
  `morph1e` double default NULL,
  `morph1n` double default NULL,
  `morph1s` double default NULL,
  `morph1w` double default NULL,
  `morph2e` double default NULL,
  `morph2n` double default NULL,
  `morph2s` double default NULL,
  `morph2w` double default NULL,
  `morph3e` double default NULL,
  `morph3n` double default NULL,
  `morph3s` double default NULL,
  `morph3w` double default NULL,
  `morph4e` double default NULL,
  `morph4n` double default NULL,
  `morph4s` double default NULL,
  `morph4w` double default NULL,
  `stationID` int(10) unsigned NOT NULL default '0',
  `solarSystemID` int(10) unsigned NOT NULL default '0',
  `constellationID` int(10) unsigned NOT NULL default '0',
  `regionID` int(10) unsigned NOT NULL default '0',
  `online` tinyint(1) NOT NULL default '0',
  `nextRespecTime` BIGINT(20) NOT NULL DEFAULT '0',
  `freeRespecs` INT(11) NOT NULL DEFAULT '2',
  PRIMARY KEY  (`characterID`),
  KEY `FK_CHARACTER__ACCOUNTS` (`accountID`),
  KEY `FK_CHARACTER__CHRACCESSORIES` (`accessoryID`),
  KEY `FK_CHARACTER__CHRANCESTRIES` (`ancestryID`),
  KEY `FK_CHARACTER__CHRBEARDS` (`beardID`),
  KEY `FK_CHARACTER__CHRCAREERS` (`careerID`),
  KEY `FK_CHARACTER__CHRCAREERSPECIALITIES` (`careerSpecialityID`),
  KEY `FK_CHARACTER__CHRCOSTUMES` (`costumeID`),
  KEY `FK_CHARACTER__CHRDECOS` (`decoID`),
  KEY `FK_CHARACTER__CHREYEBROWS` (`eyebrowsID`),
  KEY `FK_CHARACTER__CHREYES` (`eyesID`),
  KEY `FK_CHARACTER__CHRHAIRS` (`hairID`),
  KEY `FK_CHARACTER__CHRLIPSTICKS` (`lipstickID`),
  KEY `FK_CHARACTER__CHRMAKEUPS` (`makeupID`),
  KEY `FK_CHARACTER__CHRSCHOOLS` (`schoolID`),
  KEY `FK_CHARACTER__CHRSKINS` (`skinID`),
  KEY `FK_CHARACTER__CHRBACKGROUNDS` (`backgroundID`),
  KEY `FK_CHARACTER__CHRLIGHTS` (`lightID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*
 * Insert agents information into the chrInformation table
 */
INSERT INTO chrInformation
 SELECT
  characterID,accountID,title,description,bounty,balance,securityRating,petitionMessage,logonMinutes,
  corporationID,0 AS corpRole,0 AS rolesAtAll,0 AS rolesAtBase,0 AS rolesAtHQ,0 AS rolesAtOther,
  corporationDateTime,startDateTime,createDateTime,
  ancestryID,careerID,schoolID,careerSpecialityID,gender,
  accessoryID,beardID,costumeID,decoID,eyebrowsID,eyesID,hairID,lipstickID,makeupID,skinID,backgroundID,lightID,
  headRotation1,headRotation2,headRotation3,
  eyeRotation1,eyeRotation2,eyeRotation3,
  camPos1,camPos2,camPos3,
  morph1e,morph1n,morph1s,morph1w,
  morph2e,morph2n,morph2s,morph2w,
  morph3e,morph3n,morph3s,morph3w,
  morph4e,morph4n,morph4s,morph4w,
  stationID,solarSystemID,constellationID,regionID,
  0 AS online
 FROM chrStatic;

DROP TABLE IF EXISTS `chrSkillQueue`;

/**
 * Create table for skill queue
 */
CREATE TABLE `chrSkillQueue` (
  `orderIndex` int(10) unsigned NOT NULL,
  `characterID` int(10) unsigned NOT NULL,
  `skillItemID` int(10) unsigned NOT NULL,
  `level` int(10) unsigned NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
