DROP PROCEDURE IF EXISTS `ConGet`;

DELIMITER //

CREATE PROCEDURE `ConGet`(
	IN _contractID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the contract information based on the contractID given'
BEGIN
	SELECT price, collateral, status, type, dateExpired, crateID, startStationID, issuerID, issuerCorpID, forCorp, reward, volume FROM conContracts WHERE contractID = _contractID;
END//

DELIMITER ;