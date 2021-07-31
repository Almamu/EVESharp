DROP TABLE IF EXISTS `crpAlliances`;

CREATE TABLE `crpAlliances` (
	`allianceID` INT(11) NOT NULL,
	`shortName` VARCHAR(50) NOT NULL DEFAULT '' COLLATE 'utf8_general_ci',
	`description` TEXT(65535) NOT NULL DEFAULT '0' COLLATE 'utf8_general_ci',
	`url` VARCHAR(255) NOT NULL DEFAULT '0' COLLATE 'utf8_general_ci',
	`executorCorpID` INT(11) NULL DEFAULT '0',
	`creatorCorpID` INT(11) NOT NULL,
	`creatorCharID` INT(11) NOT NULL,
	`dictatorial` TINYINT(4) NOT NULL DEFAULT '0',
	`dictatorial` TINYINT(4) NOT NULL DEFAULT '0',
	PRIMARY KEY (`allianceID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;