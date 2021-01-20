DROP TABLE IF EXISTS `crpRecruitmentAds`;

CREATE TABLE `crpRecruitmentAds` (
	`adID` INT(11) NOT NULL AUTO_INCREMENT,
	`expiryDateTime` BIGINT(20) NOT NULL DEFAULT '0',
	`createDateTime` BIGINT(20) NOT NULL DEFAULT '0',
	`corporationID` INT(11) NOT NULL DEFAULT '0',
	`typeMask` INT(11) NOT NULL DEFAULT '0',
	`description` TEXT NOT NULL DEFAULT '0',
	`minimumSkillPoints` DOUBLE NOT NULL DEFAULT '0',
	`stationID` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`adID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;