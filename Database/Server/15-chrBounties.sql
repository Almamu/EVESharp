DROP TABLE IF EXISTS `chrBounties`;

CREATE TABLE `chrBounties` (
	`characterID` INT UNSIGNED NOT NULL,
	`ownerID` INT UNSIGNED NOT NULL,
	`bounty` DOUBLE NOT NULL,
	PRIMARY KEY (`characterID`, `ownerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;