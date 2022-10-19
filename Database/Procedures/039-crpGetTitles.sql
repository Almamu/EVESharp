DROP PROCEDURE IF EXISTS `CrpGetTitles`;

DELIMITER //

CREATE PROCEDURE `CrpGetTitles`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the titles for the given corporation'
BEGIN
  SELECT titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther FROM crpTitles WHERE corporationID = _corporationID;
END//

DELIMITER ;