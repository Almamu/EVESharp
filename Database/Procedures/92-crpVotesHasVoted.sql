DROP PROCEDURE IF EXISTS `CrpVotesHasVoted`;

DELIMITER //

CREATE PROCEDURE `CrpVotesHasVoted`(
	IN _voteCaseID INT,
	IN _characterID INT
)
SQL SECURITY INVOKER
COMMENT 'Checks if the character has already voted on this vote case'
BEGIN
	SELECT COUNT(*) > 0 FROM chrVotes LEFT JOIN crpVoteOptions USING (optionID) WHERE voteCaseID = _voteCaseID AND chrVotes.characterID = _characterID;
END//

DELIMITER ;