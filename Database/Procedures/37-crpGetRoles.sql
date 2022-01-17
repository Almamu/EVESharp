DROP PROCEDURE IF EXISTS `CrpGetRoles`;

DELIMITER //

CREATE PROCEDURE `CrpGetRoles`()
SQL SECURITY INVOKER
COMMENT 'List corporation roles'
BEGIN
  SELECT roleID, roleName, shortDescription, description, roleIID FROM crpRoles;
END//

DELIMITER ;