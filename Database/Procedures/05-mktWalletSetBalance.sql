DROP PROCEDURE IF EXISTS `MktWalletSetBalance`;

DELIMITER //

CREATE PROCEDURE `MktWalletSetBalance`(IN _ownerID INT(11), IN _walletKey INT(11), IN _balance DOUBLE)
SQL SECURITY INVOKER
COMMENT 'Sets the balance of a wallet'
BEGIN
	REPLACE INTO mktWallets(`key`, ownerID, balance)VALUES(_walletKey, _ownerID, _balance);
END//

DELIMITER ;