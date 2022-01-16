DROP PROCEDURE IF EXISTS `CrpAlliancesUpdate`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesUpdate`(
	IN _allianceID INT(11),
	IN _description TEXT,
	IN _url VARCHAR(255),
	IN _executorCorpID INT(11)
)
SQL SECURITY INVOKER
COMMENT 'Updates the given alliance information'
BEGIN
	UPDATE crpAlliances SET description = _description, url = _url, executorCorpID = _executorCorpID WHERE allianceID = _allianceID;
END//

DELIMITER ;