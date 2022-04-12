DROP PROCEDURE IF EXISTS `CluResolveCharacter`;

DELIMITER //

CREATE PROCEDURE `CluResolveCharacter`(IN _characterID INT(11))
SQL SECURITY INVOKER
COMMENT 'Resolves the characterID to a clientID'
BEGIN
	SELECT accountID FROM chrInformation WHERE characterID = _characterID;
END//

DELIMITER ;