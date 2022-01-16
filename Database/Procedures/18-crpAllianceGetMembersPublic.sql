DROP PROCEDURE IF EXISTS `CrpAlliancesGetMembersPublic`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesGetMembersPublic`(IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the public members information of an alliance'
BEGIN
	SELECT corporationID, startDate FROM corporation WHERE allianceID = _allianceID;
END//

DELIMITER ;