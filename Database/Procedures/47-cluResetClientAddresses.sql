DROP PROCEDURE IF EXISTS `CluResetClientAddress`;

DELIMITER //

CREATE PROCEDURE `CluResetClientAddress`()
SQL SECURITY INVOKER
COMMENT 'Clears any account associated with any node'
BEGIN
	UPDATE account SET proxyNodeID = 0;
END//

DELIMITER ;