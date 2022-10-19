DROP PROCEDURE IF EXISTS `InvItemsLockedRemoveByID`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedRemoveByID`(
	IN _itemID INT
)
SQL SECURITY INVOKER
COMMENT 'Adds an item to the locked list'
BEGIN
	DECLARE _corporationID INT;
	DECLARE _stationID INT;

	SELECT corporationID, stationID INTO _corporationID, _stationID FROM invItemsLocked WHERE itemID = _itemID;

	DELETE FROM invItemsLocked WHERE itemID = _itemID;

	SELECT _corporationID, _stationID;
END//

DELIMITER ;