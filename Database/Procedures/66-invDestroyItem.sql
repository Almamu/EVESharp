DROP PROCEDURE IF EXISTS `InvDestroyItem`;

DELIMITER //

CREATE PROCEDURE `InvDestroyItem`(IN _itemID INT(10))
SQL SECURITY INVOKER
COMMENT 'Destroys the given itemID from the database'
BEGIN
	DELETE FROM invItems WHERE itemID = _itemID;
	DELETE FROM eveNames WHERE itemID = _itemID;
	DELETE FROM invItemsAttributes WHERE itemID = _itemID;
END//

DELIMITER ;