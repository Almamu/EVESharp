DROP TABLE IF EXISTS `crpRecruitmentAdTypes`;

CREATE TABLE `crpRecruitmentAdTypes` (
	`typeMask` INT(11) NOT NULL,
	`typeName` VARCHAR(50) NULL DEFAULT NULL,
	`description` VARCHAR(50) NULL DEFAULT NULL,
	`groupName` VARCHAR(50) NULL DEFAULT NULL,
	`dataID` INT(11) NOT NULL,
	`groupDataID` INT(11) NOT NULL,
	PRIMARY KEY (`typeMask`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (1, 'Mining', 'The corporation has a stake in the mining business', 'Operations', 2398853, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (2, 'Mission Running', 'The corporation carries out missions', 'Operations', 2398856, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (4, 'Fleet Operations', 'The corporation is involved in fleet operations', 'Operations', 2398859, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (8, 'Piraterie', 'The corporation is involved in piracy', 'Operations', 2398862, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (16, 'Research & Development', 'The corporation is involved in research and development', 'Operations', 2398865, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (32, 'Combat Warfare', 'The corporation is involved in small, stray gangs', 'Operations', 2398868, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (64, 'Mercenary', 'The corporation can be hired', 'Operations', 2398871, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (128, 'Manufacturing', 'The corporation is involved in manufacturing business', 'Operations', 2398883, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (256, 'Trading', 'The corporation is focused on training', 'Operations', 2398886, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (512, 'Role Play', 'The corporation is involved in role-playing', 'Operations', 2398888, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (1024, 'Logistics', 'The corporation is involved in logistics', 'Operations', 2398889, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (2048, 'Militia', 'Corporation participates in faction war', 'Operations', 2634612, 2398847);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (4096, 'High Security', NULL, 'Location', 2398877, 2398850);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (8192, 'Low Security', NULL, 'Location', 2398877, 2398850);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (16384, 'Null Security', NULL, 'Location', 2398880, 2398850);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (32768, '12:00 - 18:00', NULL, 'Time Zone', 2398891, 2398890);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (65536, '18:00 - 00:00', NULL, 'Time Zone', 2398892, 2398890);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (131072, '00:00 - 06:00', NULL, 'Time Zone', 2398893, 2398890);
INSERT INTO `crpRecruitmentAdTypes` (`typeMask`, `typeName`, `description`, `groupName`, `dataID`, `groupDataID`) VALUES (262144, '06:00 - 11:00', NULL, 'Time Zone', 2398894, 2398890);
