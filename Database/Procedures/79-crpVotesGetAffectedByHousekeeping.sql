DROP PROCEDURE IF EXISTS `CrpVotesGetAffectedByHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetAffectedByHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Returns a list of corporationIDs and VoteCases that will be affected by the next housekeeping (with the same date/time)'
BEGIN
	SELECT crpVotes.corporationID, voteCaseID
	FROM crpVoteOptions
	LEFT JOIN chrVotes USING (optionID)
	LEFT JOIN crpVotes USING (voteCaseID)
	LEFT JOIN crpShares sharesVotes ON chrVotes.characterID = sharesVotes.ownerID
	LEFT JOIN crpShares sharesTotal ON crpVotes.corporationID = sharesTotal.corporationID
	WHERE status = 0 AND (voteType = 2 OR voteType = 3 OR voteType = 6 OR voteType = 1) AND endDateTime < _currentTime AND parameter > 0
	GROUP BY voteCaseID
	HAVING COALESCE(SUM(sharesVotes.shares), 0) / COALESCE(SUM(sharesTotal.shares), 0) > 0.5;
END//

DELIMITER ;