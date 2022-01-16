DROP PROCEDURE IF EXISTS `MktWalletGet`;

DELIMITER //

CREATE PROCEDURE `MktWalletGet`(IN _ownerID INT(11), IN _walletKeyKeys TEXT)
SQL SECURITY INVOKER
COMMENT 'Get the specified wallet divisions for the ownerID'
BEGIN
	SELECT `key`, balance FROM mktWallets WHERE ownerID = _ownerID AND FIND_IN_SET(`key`, _walletKeyKeys);
END//

DELIMITER ;