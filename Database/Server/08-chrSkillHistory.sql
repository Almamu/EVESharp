DROP TABLE IF EXISTS `chrSkillHistory`;

CREATE TABLE `chrSkillhistory` (
	`characterID` INT(10) UNSIGNED NOT NULL,
	`skillTypeID` INT(10) UNSIGNED NOT NULL,
	`eventID` INT(10) NOT NULL,
	`logDateTime` BIGINT(20) NOT NULL,
	`absolutePoints` DOUBLE NOT NULL,
	PRIMARY KEY (`characterID`, `skillTypeID`, `eventID`, `logDateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;