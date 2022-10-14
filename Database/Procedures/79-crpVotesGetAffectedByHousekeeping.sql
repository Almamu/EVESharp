DROP PROCEDURE IF EXISTS `CrpVotesGetAffectedByHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetAffectedByHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Returns a list of corporationIDs and VoteCases that will be affected by the next housekeeping (with the same date/time)'
BEGIN
	SELECT corporationID, voteCaseID FROM crpVotes WHERE endDateTime < _currentTime AND status = 0;
END//

DELIMITER ;