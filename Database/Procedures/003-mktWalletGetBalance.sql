DROP PROCEDURE IF EXISTS `MktWalletGetBalance`;

DELIMITER //

CREATE PROCEDURE `MktWalletGetBalance`(IN _ownerID INT(11), IN _walletKey INT(11))
SQL SECURITY INVOKER
COMMENT 'Gets the balance of a wallet'
BEGIN
	SELECT balance FROM mktWallets WHERE ownerID = _ownerID AND `key` = _walletKey;
END//

DELIMITER ;