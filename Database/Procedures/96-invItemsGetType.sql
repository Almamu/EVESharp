DROP PROCEDURE IF EXISTS `InvItemsGetType`;

DELIMITER //

CREATE PROCEDURE `InvItemsGetType`(
	IN _itemID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the item type for the specified itemID'
BEGIN
	SELECT typeID FROM invItems WHERE itemID = _itemID;
END//

DELIMITER ;