DROP PROCEDURE IF EXISTS `CrpVotesGetAsSanctionable`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetAsSanctionable`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Updates a voteCaseID and applies it'
BEGIN
	SELECT
		voteCaseID, voteType, corporationID, chrVotes.characterID,
		startDateTime, endDateTime, voteCaseText,
		description, COUNT(*) AS votes, parameter,
		parameter1, parameter2, actedUpon, inEffect,
		expires, timeRescended, timeActedUpon
	FROM crpVotes
	RIGHT JOIN crpVoteOptions USING(voteCaseID)
	RIGHT JOIN chrVotes USING(optionID)
	WHERE voteCaseID = _voteCaseID GROUP BY optionID ORDER BY votes DESC;
END//

DELIMITER ;