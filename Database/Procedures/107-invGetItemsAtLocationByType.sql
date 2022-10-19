DROP PROCEDURE IF EXISTS `InvItemsGetAtLocationByType`;

DELIMITER //

CREATE PROCEDURE `InvItemsGetAtLocationByType`(
	IN _locationID INT,
	IN _ownerID INT,
	IN _typeID INT,
	IN _flag INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the items that match the given criteria'
BEGIN
	SELECT itemID, quantity, singleton, contraband FROM invItems WHERE typeID = _typeID AND locationID = _locationID AND flag = _flag;
END//

DELIMITER ;