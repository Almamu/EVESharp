DROP PROCEDURE IF EXISTS `ConGetRequestedItemsAtLocation`;

DELIMITER //

CREATE PROCEDURE `ConGetRequestedItemsAtLocation`(
	IN _locationID INT,
	IN _ownerID INT,
	IN _typeID INT,
	IN _flag INT,
	IN _damage INT
)
SQL SECURITY INVOKER
COMMENT 'Finds the given item types extracting the relevant information for the requested items'
BEGIN
	SELECT
		invItems.itemID, quantity, COALESCE(dmg.valueFloat, dmg.valueInt) AS damage, singleton, contraband
	FROM invItems
	LEFT JOIN invTypes USING(typeID)
	LEFT JOIN invGroups USING(groupID)
	LEFT JOIN invItemsAttributes dmg ON invItems.itemID = dmg.itemID AND dmg.attributeID = _damage
	WHERE flag = _flag AND typeID = _typeID AND locationID = _locationID AND ownerID = _ownerID;
END//

DELIMITER ;