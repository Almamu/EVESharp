DROP PROCEDURE IF EXISTS `CrpAlliancesHousekeepApplications`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesHousekeepApplications`(IN _limit BIGINT(20))
SQL SECURITY INVOKER
COMMENT 'Removes accepted applications older than X time'
BEGIN
	DELETE FROM crpApplications WHERE state = (SELECT constantValue FROM eveConstants WHERE constantID LIKE 'allianceApplicationAccepted') AND applicationUpdateTime < _limit;
END//

DELIMITER ;