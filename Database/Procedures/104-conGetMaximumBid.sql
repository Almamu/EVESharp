DROP PROCEDURE IF EXISTS `ConGetMaximumBid`;

DELIMITER //

CREATE PROCEDURE `ConGetMaximumBid`(
	IN _contractID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the maximum bidder for the specified contract'
BEGIN
	SELECT MAX(amount), bidderID, walletKey FROM conBids WHERE contractID = _contractID;
END//

DELIMITER ;