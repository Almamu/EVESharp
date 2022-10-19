DROP PROCEDURE IF EXISTS `ConSaveInfo`;

DELIMITER //

CREATE PROCEDURE `ConSaveInfo`(
	IN _contractID INT,
	IN _crateID INT,
	IN _status INT,
	IN _volume DOUBLE,
	IN _acceptorID INT,
	IN _dateAccepted BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Updates the specified contract with the given information'
BEGIN
	UPDATE conContracts SET volume = _volume, `status` = _status, crateID = _crateID, acceptorID = _acceptorID, dateAccepted = _dateAccepted WHERE contractID = _contractID;
END//

DELIMITER ;