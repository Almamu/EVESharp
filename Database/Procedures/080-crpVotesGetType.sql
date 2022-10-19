DROP PROCEDURE IF EXISTS `CrpVotesGetType`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetType`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the vote type from the given case id'
BEGIN
	SELECT voteType FROM crpVotes WHERE voteCaseID = _voteCaseID;
END//

DELIMITER ;