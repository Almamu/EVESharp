DROP PROCEDURE IF EXISTS `CrpAlliancesGet`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesGet`(IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the given alliance information'
BEGIN
	SELECT allianceID, shortName, description, url, executorCorpID, creatorCorpID, creatorCharID, dictatorial, deleted FROM crpAlliances WHERE allianceID = _allianceID;
END//

DELIMITER ;