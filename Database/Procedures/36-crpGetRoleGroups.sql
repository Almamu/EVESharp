DROP PROCEDURE IF EXISTS `CrpGetRoleGroups`;

DELIMITER //

CREATE PROCEDURE `CrpGetRoleGroups`()
SQL SECURITY INVOKER
COMMENT 'List corporation role groups'
BEGIN
  SELECT roleGroupID, roleMask, roleGroupName, isDivisional, appliesTo, appliesToGrantable FROM crpRoleGroups;
END//

DELIMITER ;