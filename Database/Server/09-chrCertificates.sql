DROP TABLE IF EXISTS `chrCertificates`;

CREATE TABLE `chrCertificates` (
	`characterID` INT(10) UNSIGNED NOT NULL,
	`certificateID` INT(10) UNSIGNED NOT NULL,
	`grantDate` BIGINT(20) NOT NULL,
	`visibilityFlags` TINYINT(4) NOT NULL,
	PRIMARY KEY (`characterID`, `certificateID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
