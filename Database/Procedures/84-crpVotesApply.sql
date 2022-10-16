DROP PROCEDURE IF EXISTS `CrpVotesApply`;

DELIMITER //

CREATE PROCEDURE `CrpVotesApply`(
	IN _voteCaseID INT,
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Updates a voteCaseID and applies it'
BEGIN
	UPDATE crpVotes SET timeActedUpon = _currentTime, inEffect = 1, actedUpon = 1, status = 1 WHERE voteCaseID = _voteCaseID AND expires < _currentTime;
END//

DELIMITER ;