DROP PROCEDURE IF EXISTS `CrpAlliancesGetRelationships`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesGetRelationships`(IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the relationships from this alliance outwards'
BEGIN
	SELECT toID, relationship FROM allRelationships WHERE fromID = _allianceID;
END//

DELIMITER ;