DROP PROCEDURE IF EXISTS `CrpGetTitlesTemplate`;

DELIMITER //

CREATE PROCEDURE `CrpGetTitlesTemplate`()
SQL SECURITY INVOKER
COMMENT 'Gets the default titles for new corporations'
BEGIN
  SELECT titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther FROM crpTitlesTemplate;
END//

DELIMITER ;