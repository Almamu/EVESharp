DROP PROCEDURE IF EXISTS `InvItemsLockedGetAtLocation`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedGetAtLocation`(
	IN _corporationID INT,
	IN _stationID INT
)
SQL SECURITY INVOKER
COMMENT 'Returns all the locked items in the given station for the corporation'
BEGIN
	SELECT
		itemID, typeID, ownerID
	FROM invItemsLocked
	LEFT JOIN invItems USING (itemID)
	WHERE corporationID = _corporationID AND stationID = _stationID;
END//

DELIMITER ;