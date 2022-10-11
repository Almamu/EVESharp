DROP TABLE IF EXISTS `crpMedals`;

CREATE TABLE `crpMedals` (
	`medalID` INT NOT NULL AUTO_INCREMENT,
	`corporationID` INT NOT NULL,
	`title` VARCHAR(255) NULL,
	`description` TEXT NULL,
	`date` BIGINT NULL,
	`creatorID` INT NULL,
	`noRecepients` INT NULL,
	PRIMARY KEY (`medalID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `crpMedalParts`;

CREATE TABLE `crpMedalParts` (
	`medalID` INT(11) NOT NULL,
	`index` INT(11) NOT NULL,
	`part` INT(11) NOT NULL,
	`graphic` VARCHAR(50) NULL DEFAULT NULL,
	`color` VARCHAR(50) NULL DEFAULT NULL,
	PRIMARY KEY (`medalID`, `index`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;