DROP TABLE IF EXISTS `allRelationships`;

CREATE TABLE `allRelationships` (
	`fromID` INT NOT NULL,
	`toID` INT NOT NULL,
	`relationship` INT NOT NULL,
	PRIMARY KEY (`fromID`, `toID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;