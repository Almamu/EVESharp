DROP TABLE IF EXISTS `chrLPbalance`;

CREATE TABLE `chrLPbalance` (
	`characterID` INT UNSIGNED NOT NULL,
	`corporationID` INT UNSIGNED NOT NULL,
	`balance` DOUBLE NOT NULL,
	PRIMARY KEY (`characterID`, `corporationID`)
) ENGINE=InnoDB CHARSET=utf8;