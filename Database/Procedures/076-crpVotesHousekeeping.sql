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
		FROM
		(
				SELECT voteCaseID, optionsByShares.shares / (SELECT COALESCE(SUM(shares), 0) FROM crpShares WHERE corporationID = optionsByShares.corporationID) AS rate
				FROM
					(
						SELECT
							voteCaseID, optionID, parameter, voteType, crpVotes.corporationID, COALESCE(SUM(shares), 0) AS shares
						FROM crpVoteOptions vt
						LEFT JOIN crpShares ON ownerID = (SELECT characterID FROM chrVotes WHERE optionID = vt.optionID)
				        LEFT JOIN crpVotes USING (voteCaseID)
						WHERE status = 0 AND endDateTime < _currentTime
						GROUP BY optionID
				        ORDER BY shares DESC
				    ) optionsByShares
				LEFT JOIN crpShares USING (corporationID)
				WHERE voteType = 2 OR voteType = 3 OR voteType = 6 OR voteType = 1
				GROUP BY voteCaseID
				HAVING rate > 0.5
		) voteCasesAffected
	);

	-- UPDATE crpVotes SET status = 3 WHERE endDateTime < _currentTime AND status = 0;
END//

DELIMITER ;