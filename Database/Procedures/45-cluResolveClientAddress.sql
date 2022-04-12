DROP PROCEDURE IF EXISTS `CluResolveClientAddress`;

DELIMITER //

CREATE PROCEDURE `CluResolveClientAddress`(IN _clientID INT(11))
SQL SECURITY INVOKER
COMMENT 'Resolves the clientID to a proxyNodeID'
BEGIN
	SELECT proxyNodeID FROM account WHERE accountID = _clientID;
END//

DELIMITER ;