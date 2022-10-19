DROP PROCEDURE IF EXISTS `MktWalletCreate`;

DELIMITER //

CREATE PROCEDURE `MktWalletCreate`(IN _ownerID INT(11), IN _walletKey INT(11), IN _balance DOUBLE)
SQL SECURITY INVOKER
COMMENT 'Creates a new wallet'
BEGIN
	INSERT INTO mktWallets(`key`, ownerID, balance)VALUES(_walletKey, _ownerID, _balance);
END//

DELIMITER ;