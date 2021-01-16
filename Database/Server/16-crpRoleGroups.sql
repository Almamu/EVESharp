DROP TABLE IF EXISTS `crpRoleGroups`;

CREATE TABLE IF NOT EXISTS `crprolegroups` (
  `roleGroupID` int(11) NOT NULL,
  `roleMask` bigint(20) DEFAULT NULL,
  `roleGroupName` varchar(255) NOT NULL,
  `appliesTo` varchar(50) NOT NULL,
  `appliesToGrantable` varchar(50) NOT NULL,
  `isDivisional` tinyint(4) NOT NULL,
  PRIMARY KEY (`roleGroupID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `crprolegroups` (`roleGroupID`, `roleMask`, `roleGroupName`, `appliesTo`, `appliesToGrantable`, `isDivisional`) VALUES
(1, 1130405705493254529, 'General', 'roles', 'grantableRoles', 0),
(2, 21955048183434752, 'Station Service', 'roles', 'grantableRoles', 0),
(3, 2198889037824, 'Accounting (Divisional)', 'roles', 'grantableRoles', 1),
(4, 134209536, 'Hangar Access (Headquarters)', 'rolesAtHQ', 'grantableRolesAtHQ', 1),
(5, 558551906910208, 'Container Access (Headquarters)', 'rolesAtHQ', 'grantableRolesAtHQ', 1),
(6, 134209536, 'Hangar Access (Based at)', 'rolesAtBase', 'grantableRolesAtBase', 1),
(7, 558551906910208, 'Container Access (Based At)', 'rolesAtBase', 'grantableRolesAtBase', 1),
(8, 134209536, 'Hangar Access (Other)', 'rolesAtOther', 'grantableRolesAtOther', 1),
(9, 558551906910208, 'Container Access (Other)', 'rolesAtOther', 'grantableRolesAtOther', 1);