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
	`status` INT(11) NOT NULL DEFAULT '0',
	`actedUpon` TINYINT(1) NOT NULL DEFAULT '0',
	`inEffect` TINYINT(1) NOT NULL DEFAULT '0',
	`expires` BIGINT(20) NOT NULL,
	`timeRescended` BIGINT(20) NULL,
	`timeActedUpon` BIGINT(20) NULL,
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
	`optionID` INT(11) NOT NULL,
	`characterID` INT(11) NOT NULL,
	PRIMARY KEY (`optionID`, `characterID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;