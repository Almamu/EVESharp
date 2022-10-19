DROP PROCEDURE IF EXISTS `ConGetOutbids`;

DELIMITER //

CREATE PROCEDURE `ConGetOutbids`(
	IN _contractID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the users that we\'re going to outbid'
BEGIN
	SELECT bidderID FROM conBids WHERE contractID = _contractID GROUP BY bidderID;
END//

DELIMITER ;