DROP PROCEDURE IF EXISTS `CrpAlliancesGetMembersPrivate`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesGetMembersPrivate`(IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the members information of an alliance'
BEGIN
	SELECT corporationID, allianceID, chosenExecutorID FROM corporation WHERE allianceID = _allianceID;
END//

DELIMITER ;