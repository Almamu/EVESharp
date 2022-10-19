DROP PROCEDURE IF EXISTS `InvItemsLockedAdd`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedAdd`(
	IN _itemID INT,
	IN _stationID INT,
	IN _corporationID INT,
	IN _voteCaseID INT
)
SQL SECURITY INVOKER
COMMENT 'Adds an item to the locked list'
BEGIN
	INSERT INTO invItemsLocked(itemID, corporationID, stationID, voteCaseID)VALUES(_itemID, _corporationID, _stationID, _voteCaseID);
END//

DELIMITER ;