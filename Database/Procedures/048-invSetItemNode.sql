DROP PROCEDURE IF EXISTS `InvSetItemNode`;

DELIMITER //

CREATE PROCEDURE `InvSetItemNode`(IN _itemID INT(10), IN _nodeID INT(10))
SQL SECURITY INVOKER
COMMENT 'Updates the nodeID where the item should be loaded'
BEGIN
	UPDATE invItems SET nodeID = _nodeID WHERE itemID = _itemID;
END//

DELIMITER ;