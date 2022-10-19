DROP PROCEDURE IF EXISTS `CrpVotesIsExpired`;

DELIMITER //

CREATE PROCEDURE `CrpVotesIsExpired`(
	IN _voteCaseID INT,
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Checks if a vote is expired'
BEGIN
	DECLARE _expires BIGINT(20);

	SELECT expires INTO _expires FROM crpVotes WHERE voteCaseID = _voteCaseID;

	SELECT _expires < _currentTime;
END//

DELIMITER ;