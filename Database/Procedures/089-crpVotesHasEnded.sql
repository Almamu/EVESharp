DROP PROCEDURE IF EXISTS `CrpVotesHasEnded`;

DELIMITER //

CREATE PROCEDURE `CrpVotesHasEnded`(
	IN _voteCaseID INT,
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Checks if a vote is expired'
BEGIN
	SELECT endDateTime < _currentTime FROM crpVotes WHERE voteCaseID = _voteCaseID;
END//

DELIMITER ;