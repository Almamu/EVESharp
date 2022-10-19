DROP PROCEDURE IF EXISTS `ConDestroy`;

DELIMITER //

CREATE PROCEDURE `ConDestroy`(
	IN _contractID INT
)
SQL SECURITY INVOKER
COMMENT 'Destroys the given contract'
BEGIN
	DELETE FROM conContracts WHERE contractID = _contractID;
	DELETE FROM conItems WHERE contractID = _contractID;
	DELETE FROM conBids WHERE contractID = _contractID;
	
END//

DELIMITER ;