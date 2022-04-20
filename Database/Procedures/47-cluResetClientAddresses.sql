DROP PROCEDURE IF EXISTS `CluResetClientAddresses`;

DELIMITER //

CREATE PROCEDURE `CluResetClientAddresses`()
SQL SECURITY INVOKER
COMMENT 'Clears any account associated with any node'
BEGIN
	UPDATE account SET proxyNodeID = 0;
END//

DELIMITER ;