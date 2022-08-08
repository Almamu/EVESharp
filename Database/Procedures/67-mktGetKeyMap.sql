DROP PROCEDURE IF EXISTS `MktGetKeyMap`;

DELIMITER //

CREATE PROCEDURE `MktGetKeyMap`()
SQL SECURITY INVOKER
COMMENT 'Returns the full KeyMap for wallets'
BEGIN
	SELECT accountKey AS keyID, accountType AS keyType, accountName AS keyName, description FROM mktKeyMap;
END//

DELIMITER ;