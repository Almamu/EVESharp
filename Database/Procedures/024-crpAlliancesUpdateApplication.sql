DROP PROCEDURE IF EXISTS `CrpAlliancesUpdateApplication`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesUpdateApplication`(IN _allianceID INT(11), IN _corporationID INT(12), IN _newStatus INT(11), IN _currentTime BIGINT(20))
SQL SECURITY INVOKER
COMMENT 'Updates the given corporation\'s application to the alliance with the new status'
BEGIN
	UPDATE crpApplications SET state = _newStatus, applicationUpdateTime = _currentTime WHERE allianceID = _allianceID AND corporationID = _corporationID;
END//

DELIMITER ;