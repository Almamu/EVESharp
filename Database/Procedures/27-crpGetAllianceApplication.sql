DROP PROCEDURE IF EXISTS `CrpGetAllianceApplications`;

DELIMITER //

CREATE PROCEDURE `CrpGetAllianceApplications`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'Get current applications for the corporation'
BEGIN
	SELECT allianceID, corporationID, applicationText, applicationDateTime, state FROM crpApplications WHERE corporationID = _corporationID;
END//

DELIMITER ;