DROP PROCEDURE IF EXISTS `CrpVoteHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpVoteHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Performs timed events related to corporations like changing vote status when they\'re done'
BEGIN
	UPDATE crpVotes SET status = 2 WHERE endDateTime < _currentTime AND status = 0;
END//

DELIMITER ;