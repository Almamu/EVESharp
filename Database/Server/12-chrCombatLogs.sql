DROP TABLE IF EXISTS `chrCombatLogs`;

CREATE TABLE `chrCombatLogs`(
	`killID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`solarSystemID` INT(10) UNSIGNED NOT NULL,
	`moonID` INT(10) UNSIGNED NOT NULL,
	`victimCharacterID` INT(10) UNSIGNED NOT NULL,
	`victimCorporationID` INT(10) UNSIGNED NOT NULL,
	`victimAllianceID` INT(10) UNSIGNED NULL DEFAULT NULL,
	`victimFactionID` INT(10) UNSIGNED NULL DEFAULT NULL,
	`victimShipTypeID` INT(10) UNSIGNED NOT NULL,
	`victimDamageTaken` DOUBLE UNSIGNED NOT NULL,
	`finalCharacterID` INT(10) UNSIGNED NOT NULL,
	`finalCorporationID` INT(10) UNSIGNED NOT NULL,
	`finalAllianceID` INT(10) UNSIGNED NULL DEFAULT NULL,
	`finalFactionID` INT(10) UNSIGNED NULL DEFAULT NULL,
	`finalDamageDone` DOUBLE UNSIGNED NOT NULL,
	`finalSecurityStatus` DOUBLE UNSIGNED NOT NULL,
	`finalShipTypeID` INT(10) UNSIGNED NOT NULL,
	`finalWeaponTypeID` INT(10) UNSIGNED NOT NULL,
	`killTime` BIGINT(20) NOT NULL,
	`killBlob` DOUBLE UNSIGNED NOT NULL,
	PRIMARY KEY (`killID`),
	INDEX `victimCharacterID` (`victimCharacterID`),
	INDEX `finalCharacterID` (`finalCharacterID`),
	INDEX `victimCorporationID` (`victimCorporationID`),
	INDEX `finalCorporationID` (`finalCorporationID`),
	INDEX `solarSystemID` (`solarSystemID`)
) ENGINE=InnoDB COLLATE=utf8;