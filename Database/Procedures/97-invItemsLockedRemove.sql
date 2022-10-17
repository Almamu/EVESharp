DROP PROCEDURE IF EXISTS `InvItemsLockedRemove`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedRemove`(
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Adds an item to the locked list'
BEGIN
	DECLARE _itemID INT;
	DECLARE _corporationID INT;
	DECLARE _stationID INT;

	SELECT itemID, corporationID, stationID INTO _itemID, _corporationID, _stationID FROM invItemsLocked WHERE voteCaseID = _voteCaseID;

	DELETE FROM invItemsLocked WHERE voteCaseID = _voteCaseID;

	SELECT _itemID, _corporationID, _stationID;
END//

DELIMITER ;