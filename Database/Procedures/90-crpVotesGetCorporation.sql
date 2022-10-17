DROP PROCEDURE IF EXISTS `CrpVotesGetCorporation`;

DELIMITER //

CREATE PROCEDURE `CrpVotesGetCorporation`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the corporationID that this vote case belongs to'
BEGIN
	SELECT corporationID FROM crpVotes WHERE voteCaseID = _voteCaseID;
END//

DELIMITER ;