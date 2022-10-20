DROP PROCEDURE IF EXISTS `InvItemsLockedAnyAtStation`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedAnyAtStation`(
	IN _stationID INT,
	IN _corporationID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the office available'
BEGIN
	SELECT COUNT(*) FROM invItemsLocked WHERE stationID = _stationID AND corporationID = _corporationID;
END//

DELIMITER ;