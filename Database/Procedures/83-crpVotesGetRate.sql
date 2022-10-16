DROP PROCEDURE IF EXISTS `CrpVotesGetRate`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetRate`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Returns the voting rate of the given voteCaseID'
BEGIN
	SELECT CAST(COALESCE(SUM(sharesVotes.shares), 0) / COALESCE(SUM(sharesTotal.shares), 0) AS DOUBLE) AS rate, parameter
	FROM crpVoteOptions
	LEFT JOIN chrVotes USING (optionID)
	LEFT JOIN crpVotes USING (voteCaseID)
	LEFT JOIN crpShares sharesVotes ON chrVotes.characterID = sharesVotes.ownerID
	LEFT JOIN crpShares sharesTotal ON crpVotes.corporationID = sharesTotal.corporationID
	WHERE voteCaseID = _voteCaseID;
END//

DELIMITER ;