DROP PROCEDURE IF EXISTS `CrpVotesGetAffectedByHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetAffectedByHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Returns a list of corporationIDs and VoteCases that will be affected by the next housekeeping (with the same date/time)'
BEGIN
	SELECT corporationID, voteCaseID, parameter, voteType, optionsByShares.shares / (SELECT COALESCE(SUM(shares), 0) FROM crpShares WHERE corporationID = optionsByShares.corporationID) AS rate
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
	GROUP BY voteCaseID;
END//

DELIMITER ;