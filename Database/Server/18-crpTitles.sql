DROP TABLE IF EXISTS `crpTitlesTemplate`;
DROP TABLE IF EXISTS `crpTitles`;

CREATE TABLE `crpTitlesTemplate` (
    `titleID` INT UNSIGNED NOT NULL,
    `titleName` VARCHAR(50) NULL,
    `roles` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRoles` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtHQ` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtHQ` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtBase` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtBase` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtOther` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtOther` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    PRIMARY KEY(`titleID`)
) ENGINE=InnoDB CHARSET=utf8;

CREATE TABLE `crpTitles` (
    `corporationID` INT UNSIGNED NOT NULL,
    `titleID` INT UNSIGNED NOT NULL,
    `titleName` VARCHAR(50) NULL,
    `roles` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRoles` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtHQ` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtHQ` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtBase` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtBase` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `rolesAtOther` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    `grantableRolesAtOther` BIGINT UNSIGNED NOT NULL DEFAULT '0',
    PRIMARY KEY(`corporationID`, `titleID`)
) ENGINE=InnoDB CHARSET=utf8;

/*
 * Insert the templates for the titles
 */
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (1, 'Untitled 1', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (2, 'Untitled 2', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (4, 'Untitled 3', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (8, 'Untitled 4', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (16, 'Untitled 5', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (32, 'Untitled 6', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (64, 'Untitled 7', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (128, 'Untitled 8', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (256, 'Untitled 9', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (512, 'Untitled 10', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (1024, 'Untitled 11', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (2048, 'Untitled 12', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (4096, 'Untitled 13', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (8192, 'Untitled 14', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (16384, 'Untitled 15', 0, 0, 0, 0, 0, 0, 0, 0);
REPLACE INTO `crpTitlesTemplate` (`titleID`, `titleName`, `roles`, `grantableRoles`, `rolesAtHQ`, `grantableRolesAtHQ`, `rolesAtBase`, `grantableRolesAtBase`, `rolesAtOther`, `grantableRolesAtOther`) VALUES (32768, 'Untitled 16', 0, 0, 0, 0, 0, 0, 0, 0);
