DROP PROCEDURE IF EXISTS `ConPlaceBid`;

DELIMITER //

CREATE PROCEDURE `ConPlaceBid`(
	IN _contractID INT,
	IN _bidderID INT,
	IN _forCorp TINYINT(1),
	IN _amount INT,
	IN _walletKey INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the users that we\'re going to outbid'
BEGIN
	INSERT INTO conBids(contractID, bidderID, forCorp, amount, walletKey)VALUES(_contractID, _bidderID, _forCorp, _amount, _walletKey);
END//

DELIMITER ;