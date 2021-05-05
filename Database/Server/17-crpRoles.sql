DROP TABLE IF EXISTS `crpRoles`;

CREATE TABLE IF NOT EXISTS `crpRoles` (
  `roleID` bigint(20) unsigned NOT NULL,
  `roleName` varchar(50) DEFAULT NULL,
  `description` varchar(255) DEFAULT '',
  `shortDescription` varchar(255) DEFAULT '',
  `roleIID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`roleID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1, 'corpRoleDirector', 'Can do anything like a CEO. Can assign any role.', 'Director', 1);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (128, 'corpRolePersonnelManager', 'Can accept applications to join the corporation.', 'Personnel Manager', 2);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (256, 'corpRoleAccountant', 'Can view/use corporation accountancy info.', 'Accountant', 3);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (512, 'corpRoleSecurityOfficer', 'Can view the content of others hangars', 'Security Officer', 4);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1024, 'corpRoleFactoryManager', 'Can perform factory management tasks.', 'Factory Manager', 5);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (2048, 'corpRoleStationManager', 'Can perform station management for a corporation.', 'Station Manager', 6);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (4096, 'corpRoleAuditor', 'Can perform audits on corporation security event logs.', 'Auditor', 7);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (8192, 'corpRoleHangarCanTake1', 'Can take items from this divisions hangar', 'Hangar Take [1]', 8);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (16384, 'corpRoleHangarCanTake2', 'Can take items from this divisions hangar', 'Hangar Take [2]', 9);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (32768, 'corpRoleHangarCanTake3', 'Can take items from this divisions hangar', 'Hangar Take [3]', 10);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (65536, 'corpRoleHangarCanTake4', 'Can take items from this divisions hangar', 'Hangar Take [4]', 11);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (131072, 'corpRoleHangarCanTake5', 'Can take items from this divisions hangar', 'Hangar Take [5]', 12);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (262144, 'corpRoleHangarCanTake6', 'Can take items from this divisions hangar', 'Hangar Take [6]', 13);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (524288, 'corpRoleHangarCanTake7', 'Can take items from this divisions hangar', 'Hangar Take [7]', 14);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1048576, 'corpRoleHangarCanQuery1', 'Can query the content of this divisions hangar', 'Hangar Query [1]', 15);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (2097152, 'corpRoleHangarCanQuery2', 'Can query the content of this divisions hangar', 'Hangar Query [2]', 16);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (4194304, 'corpRoleHangarCanQuery3', 'Can query the content of this divisions hangar', 'Hangar Query [3]', 17);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (8388608, 'corpRoleHangarCanQuery4', 'Can query the content of this divisions hangar', 'Hangar Query [4]', 18);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (16777216, 'corpRoleHangarCanQuery5', 'Can query the content of this divisions hangar', 'Hangar Query [5]', 19);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (33554432, 'corpRoleHangarCanQuery6', 'Can query the content of this divisions hangar', 'Hangar Query [6]', 20);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (67108864, 'corpRoleHangarCanQuery7', 'Can query the content of this divisions hangar', 'Hangar Query [7]', 21);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (134217728, 'corpRoleAccountCanTake1', 'Can take funds from this divisions account', 'Account Take [1]', 22);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (268435456, 'corpRoleAccountCanTake2', 'Can take funds from this divisions account', 'Account Take [2]', 23);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (536870912, 'corpRoleAccountCanTake3', 'Can take funds from this divisions account', 'Account Take [3]', 24);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1073741824, 'corpRoleAccountCanTake4', 'Can take funds from this divisions account', 'Account Take [4]', 25);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (2147483648, 'corpRoleAccountCanTake5', 'Can take funds from this divisions account', 'Account Take [5]', 26);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (4294967296, 'corpRoleAccountCanTake6', 'Can take funds from this divisions account', 'Account Take [6]', 27);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (8589934592, 'corpRoleAccountCanTake7', 'Can take funds from this divisions account', 'Account Take [7]', 28);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (17179869184, 'corpRoleAccountCanQuery1', 'Can query funds from this divisions account', 'Account Query [1]', 29);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (34359738368, 'corpRoleAccountCanQuery2', 'Can query funds from this divisions account', 'Account Query [2]', 30);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (68719476736, 'corpRoleAccountCanQuery3', 'Can query funds from this divisions account', 'Account Query [3]', 31);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (137438953472, 'corpRoleAccountCanQuery4', 'Can query funds from this divisions account', 'Account Query [4]', 32);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (274877906944, 'corpRoleAccountCanQuery5', 'Can query funds from this divisions account', 'Account Query [5]', 33);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (549755813888, 'corpRoleAccountCanQuery6', 'Can query funds from this divisions account', 'Account Query [6]', 34);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1099511627776, 'corpRoleAccountCanQuery7', 'Can query funds from this divisions account', 'Account Query [7]', 35);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (2199023255552, 'corpRoleEquipmentConfig', 'Can deploy and configure equipment in space.', '', 36);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (4398046511104, 'corpRoleContainerCanTake1', 'Can take containers from this divisional hangar', 'Container Take [1]', 37);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (8796093022208, 'corpRoleContainerCanTake2', 'Can take containers from this divisional hangar', 'Container Take [2]', 38);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (17592186044416, 'corpRoleContainerCanTake3', 'Can take containers from this divisional hangar', 'Container Take [3]', 39);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (35184372088832, 'corpRoleContainerCanTake4', 'Can take containers from this divisional hangar', 'Container Take [4]', 40);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (70368744177664, 'corpRoleContainerCanTake5', 'Can take containers from this divisional hangar', 'Container Take [5]', 41);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (140737488355328, 'corpRoleContainerCanTake6', 'Can take containers from this divisional hangar', 'Container Take [6]', 42);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (281474976710656, 'corpRoleContainerCanTake7', 'Can take containers from this divisional hangar', 'Container Take [7]', 43);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (562949953421312, 'corpRoleCanRentOffice', 'When assigned to a member, the member can rent offices on behalf of the corporation', 'Rent Office', 44);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (1125899906842624, 'corpRoleCanRentFactorySlot', 'When assigned to a member, the member can rent factory slots on behalf of the corporation', 'Rent Factory Facility', 45);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (2251799813685248, 'corpRoleCanRentResearchSlot', 'When assigned to a member, the member can rent research facilities on behalf of the corporation', 'Rent Research Facility', 46);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (4503599627370496, 'corpRoleJuniorAccountant', 'Can view corporation accountancy info.', 'Junior Accountant', 47);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (9007199254740992, 'corpRoleStarbaseConfig', 'Can deploy and configure starbase structures in space.', 'Config Starbase Equipment', 48);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (18014398509481984, 'corpRoleTrader', 'Can buy and sell things for the corporation', 'Trader', 49);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (36028797018963968, 'corpRoleChatManager', 'Can moderate corporation/alliance communications channels.', 'Communications Officer', 50);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (72057594037927936, 'corpRoleContractManager', 'Can create, edit and oversee all contracts made on behalf of the corporation as well as accept contracts on behalf of the corporation', 'Contract Manager', 51);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (144115188075855872, 'corpRoleInfrastructureTacticalOfficer', 'Can operate defensive starbase structures', 'Starbase Defensive Operator', 52);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (288230376151711744, 'corpRoleStarbaseCaretaker', 'Can refuel starbases and take from silo bins.', 'Starbase Fuel Technician', 53);
REPLACE INTO `crpRoles` (`roleID`, `roleName`, `description`, `shortDescription`, `roleIID`) VALUES (576460752303423488, 'corpRoleFittingManager', 'Can add and delete fittings.', 'Fitting Manager', 54);
