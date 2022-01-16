DROP PROCEDURE IF EXISTS `CrpAlliancesRemoveRelationship`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesRemoveRelationship`(IN _fromID INT(11), IN _toID INT(11))
SQL SECURITY INVOKER
COMMENT 'Removes the alliances relationship'
BEGIN
	DELETE FROM allRelationships WHERE fromID = _fromID AND toID = _toID;
END//

DELIMITER ;