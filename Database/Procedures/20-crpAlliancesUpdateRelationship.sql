DROP PROCEDURE IF EXISTS `CrpAlliancesUpdateRelationship`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesUpdateRelationship`(IN _fromID INT(11), IN _toID INT(11), IN _relationship INT(11))
SQL SECURITY INVOKER
COMMENT 'Updates the alliances relationship'
BEGIN
	REPLACE INTO allRelationships(fromID, toID, relationship)VALUES(_fromID, _toID, _relationship);
END//

DELIMITER ;