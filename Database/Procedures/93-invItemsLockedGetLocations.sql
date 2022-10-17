DROP PROCEDURE IF EXISTS `InvItemsLockedGetLocations`;

DELIMITER //

CREATE PROCEDURE `InvItemsLockedGetLocations`(
	IN _corporationID INT
)
SQL SECURITY INVOKER
COMMENT 'Returns all the stations where there\'s items for this corporation'
BEGIN
	SELECT DISTINCT stationID FROM invItemsLocked WHERE corporationID = _corporationID;
END//

DELIMITER ;