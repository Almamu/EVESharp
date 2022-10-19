DROP PROCEDURE IF EXISTS `CluRegisterClientAddress`;

DELIMITER //

CREATE PROCEDURE `CluRegisterClientAddress`(IN _clientID INT(11), IN _proxyNodeID INT(11))
SQL SECURITY INVOKER
COMMENT 'Registers a client under the specified proxyNodeID'
BEGIN
	UPDATE account SET proxyNodeID = _proxyNodeID WHERE accountID = _clientID;
END//

DELIMITER ;