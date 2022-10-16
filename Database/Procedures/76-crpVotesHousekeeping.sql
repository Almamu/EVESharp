DROP PROCEDURE IF EXISTS `CrpVoteHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpVoteHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Performs timed events related to corporations like changing vote status when they\'re done (only affects votes that need to be signed off by the CEO)'
BEGIN
	UPDATE crpVotes SET status = 2 WHERE voteCaseID IN (
		SELECT voteCaseID
		FROM crpVoteOptions
		LEFT JOIN chrVotes USING (optionID)
		LEFT JOIN crpVotes USING (voteCaseID)
		LEFT JOIN crpShares sharesVotes ON chrVotes.characterID = sharesVotes.ownerID
		LEFT JOIN crpShares sharesTotal ON crpVotes.corporationID = sharesTotal.corporationID
		WHERE status = 2 AND (voteType = 2 OR voteType = 3 OR voteType = 6 OR voteType = 1) AND endDateTime < _currentTime AND parameter > 0
		GROUP BY voteCaseID
		HAVING COALESCE(SUM(sharesVotes.shares), 0) / COALESCE(SUM(sharesTotal.shares), 0) > 0.5
	);
END//

DELIMITER ;