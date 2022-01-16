DROP PROCEDURE IF EXISTS `CrpAlliancesListApplications`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesListApplications`(IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'List applications to the given alliance'
BEGIN
	SELECT allianceID, corporationID, applicationText, applicationDateTime, state FROM crpApplications WHERE allianceID = _allianceID;
END//

DELIMITER ;