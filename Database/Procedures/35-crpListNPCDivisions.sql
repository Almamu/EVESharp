DROP PROCEDURE IF EXISTS `CrpListNPCDivisions`;

DELIMITER //

CREATE PROCEDURE `CrpListNPCDivisions`()
SQL SECURITY INVOKER
COMMENT 'List NPC corporation divisions'
BEGIN
  SELECT divisionID, divisionName, description, leaderType from crpNPCDivisions;
END//

DELIMITER ;