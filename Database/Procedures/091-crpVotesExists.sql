DROP PROCEDURE IF EXISTS `CrpVotesExists`;

DELIMITER //

CREATE PROCEDURE `CrpVotesExists`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Checks if the vote exists'
BEGIN
	SELECT COUNT(*) > 0 FROM crpVotes WHERE voteCaseID = _voteCaseID;
END//

DELIMITER ;