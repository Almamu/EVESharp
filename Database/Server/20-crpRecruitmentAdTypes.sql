DROP TABLE IF EXISTS `crpRecruitmentAdTypes`;

CREATE TABLE `crpRecruitmentAdTypes` (
	`typeMask` INT(11) NOT NULL,
	`groupID` INT(11) NOT NULL,
	`typeName` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8_general_ci',
	`description` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8_general_ci',
	`dataID` INT(11) NOT NULL,
	PRIMARY KEY (`typeMask`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (1, 1, 'Mining', 'The corporation has a stake in the mining business', 2398853);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (2, 1, 'Mission Running', 'The corporation carries out missions', 2398856);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (4, 1, 'Fleet Operations', 'The corporation is involved in fleet operations', 2398859);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (8, 1, 'Piraterie', 'The corporation is involved in piracy', 2398862);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (16, 1, 'Research & Development', 'The corporation is involved in research and develo', 2398865);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (32, 1, 'Combat Warfare', 'The corporation is involved in small, stray gangs', 2398868);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (64, 1, 'Mercenary', 'The corporation can be hired', 2398871);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (128, 1, 'Manufacturing', 'The corporation is involved in manufacturing busin', 2398883);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (256, 1, 'Trading', 'The corporation is focused on training', 2398886);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (512, 1, 'Role Play', 'The corporation is involved in role-playing', 2398888);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (1024, 1, 'Logistics', 'The corporation is involved in logistics', 2398889);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (2048, 1, 'Militia', 'Corporation participates in faction war', 2634612);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (4096, 2, 'High Security', NULL, 2398877);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (8192, 2, 'Low Security', NULL, 2398877);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (16384, 2, 'Null Security', NULL, 2398880);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (32768, 3, '12:00 - 18:00', NULL, 2398891);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (65536, 3, '18:00 - 00:00', NULL, 2398892);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (131072, 3, '00:00 - 06:00', NULL, 2398893);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `groupID`, `typeName`, `description`, `dataID`) VALUES (262144, 3, '06:00 - 11:00', NULL, 2398894);

DROP TABLE IF EXISTS `crpRecruitmentAdGroups`;

CREATE TABLE `crpRecruitmentAdGroups` (
	`groupID` INT(11) NOT NULL AUTO_INCREMENT,
	`groupName` VARCHAR(50) NOT NULL DEFAULT '0' COLLATE 'utf8_general_ci',
	`dataID` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`groupID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `crpRecruitmentAdGroups` (`groupID`, `groupName`, `dataID`) VALUES (1, 'Operations', 2398847);
INSERT INTO `crpRecruitmentAdGroups` (`groupID`, `groupName`, `dataID`) VALUES (2, 'Location', 2398850);
INSERT INTO `crpRecruitmentAdGroups` (`groupID`, `groupName`, `dataID`) VALUES (3, 'Time Zone', 2398890);
