DROP TABLE IF EXISTS `chrMedals`;

CREATE TABLE `chrMedals` (
	`characterID` INT UNSIGNED NOT NULL,
	`medalID` INT UNSIGNED NOT NULL,
	`title` VARCHAR(255) NOT NULL,
	`description` TEXT NOT NULL,
	`ownerID` INT UNSIGNED NOT NULL,
	`issuerID` INT UNSIGNED NOT NULL,
	`date` BIGINT NOT NULL,
	`reason` INT NOT NULL,
	`status` INT NOT NULL,
	PRIMARY KEY (`characterID`, `medalID`),
	INDEX `ownerID` (`ownerID`),
	INDEX `issuerID` (`issuerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;