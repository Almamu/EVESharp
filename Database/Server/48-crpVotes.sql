DROP TABLE IF EXISTS `crpVotes`;

CREATE TABLE `crpVotes` (
	`voteCaseID` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`voteType` INT NULL,
	`corporationID` INT NULL,
	`characterID` INT NULL,
	`startDateTime` BIGINT NULL,
	`endDateTime` BIGINT NULL,
	`voteCaseText` VARCHAR(255) NOT NULL DEFAULT '',
	`description` TEXT NOT NULL DEFAULT '',
	INDEX `voteType` (`voteType`),
	INDEX `corporationID` (`corporationID`),
	PRIMARY KEY (`voteCaseID`),
	INDEX `voteType_corporationID` (`voteType`, `corporationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `crpVoteOptions`;

CREATE TABLE `crpVoteOptions` (
	`optionID` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`voteCaseID` INT NULL,
	`optionText` TEXT NOT NULL DEFAULT '',
	`parameter` INT NOT NULL DEFAULT 0,
	`parameter1` INT NULL DEFAULT 0,
	`parameter2` INT NULL DEFAULT 0,
	PRIMARY KEY (`optionID`),
	INDEX `voteCaseID` (`voteCaseID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `chrVotes`;

CREATE TABLE `chrVotes` (
	`voteID` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`optionID` INT NULL,
	`characterID` INT NULL,
	INDEX `optionID` (`optionID`),
	PRIMARY KEY (`voteID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;