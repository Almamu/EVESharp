
/**
 * Base tables for corporations
 */
DROP TABLE IF EXISTS `corporation`;

CREATE TABLE `corporation` (
	`corporationID` INT(10) UNSIGNED NOT NULL,
	`corporationName` VARCHAR(100) NOT NULL DEFAULT '',
	`description` MEDIUMTEXT NOT NULL,
	`tickerName` VARCHAR(8) NOT NULL DEFAULT '',
	`url` MEDIUMTEXT NOT NULL,
	`taxRate` DOUBLE NOT NULL DEFAULT '0',
	`minimumJoinStanding` DOUBLE NOT NULL DEFAULT '0',
	`corporationType` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`hasPlayerPersonnelManager` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0',
	`sendCharTerminationMessage` TINYINT(3) UNSIGNED NOT NULL DEFAULT '1',
	`creatorID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`ceoID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`stationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`raceID` INT(10) UNSIGNED NULL DEFAULT NULL,
	`allianceID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`shares` BIGINT(20) UNSIGNED NOT NULL DEFAULT '1000',
	`memberCount` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`memberLimit` INT(10) UNSIGNED NOT NULL DEFAULT '10',
	`allowedMemberRaceIDs` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`graphicID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`shape1` INT(10) UNSIGNED NULL DEFAULT NULL,
	`shape2` INT(10) UNSIGNED NULL DEFAULT NULL,
	`shape3` INT(10) UNSIGNED NULL DEFAULT NULL,
	`color1` INT(10) UNSIGNED NULL DEFAULT NULL,
	`color2` INT(10) UNSIGNED NULL DEFAULT NULL,
	`color3` INT(10) UNSIGNED NULL DEFAULT NULL,
	`typeface` VARCHAR(11) NULL DEFAULT NULL,
	`division1` VARCHAR(100) NULL DEFAULT '1st Division',
	`division2` VARCHAR(100) NULL DEFAULT '2nd Division',
	`division3` VARCHAR(100) NULL DEFAULT '3rd Division',
	`division4` VARCHAR(100) NULL DEFAULT '4th Division',
	`division5` VARCHAR(100) NULL DEFAULT '5th Division',
	`division6` VARCHAR(100) NULL DEFAULT '6th Division',
	`division7` VARCHAR(100) NULL DEFAULT '7th Division',
	`walletDivision1` VARCHAR(100) NULL DEFAULT 'Master wallet',
	`walletDivision2` VARCHAR(100) NULL DEFAULT '2nd Wallet Division',
	`walletDivision3` VARCHAR(100) NULL DEFAULT '3rd Wallet Division',
	`walletDivision4` VARCHAR(100) NULL DEFAULT '4th Wallet Division',
	`walletDivision5` VARCHAR(100) NULL DEFAULT '5th Wallet Division',
	`walletDivision6` VARCHAR(100) NULL DEFAULT '6th Wallet Division',
	`walletDivision7` VARCHAR(100) NULL DEFAULT '7th Wallet Division',
	`balance` DOUBLE NOT NULL DEFAULT '0',
	`deleted` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0',
	PRIMARY KEY (`corporationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*
 * Copy over the static corporation info
 */
INSERT INTO corporation
 SELECT
  corporationID, corporationName, description, tickerName, url, taxRate, minimumJoinStanding, corporationType, hasPlayerPersonnelManager, 
  sendCharTerminationMessage, creatorID, ceoID, stationID, raceID, allianceID, shares, memberCount, memberLimit, 
  allowedMemberRaceIDs, graphicID, shape1, shape2, shape3, color1, color2, color3, typeface, division1, division2, division3, 
  division4, division5, division6, division7, 'Master Wallet' AS walletDivision1, '2nd Wallet Division' AS walletDivision2, 
  '3rd Wallet Division' AS walletDivision3, '4th Wallet Division' AS walletDivision4, '5th Wallet Division' AS walletDivision5, 
  '6th Wallet Division' AS walletDivision6, '7th Wallet Division' AS walletDivision7, balance, deleted
 FROM crpStatic;

/*
 * Replace CEOs on the corporations to known characters, we don't know info about the CEOs
 */
UPDATE corporation SET ceoID = (SELECT characterID FROM chrInformation WHERE corporationID = corporationID LIMIT 1);