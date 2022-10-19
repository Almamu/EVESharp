DROP PROCEDURE IF EXISTS `ConGetItemPreloadValues`;

DELIMITER //

CREATE PROCEDURE `ConGetItemPreloadValues`(
	IN _itemID INT,
	IN _stationID INT,
	IN _ownerID INT,
	IN _volume INT,
	IN _damage INT
)
SQL SECURITY INVOKER
COMMENT 'Finds the given item extracting the relevant information for a contract creation'
BEGIN
	SELECT
		quantity, COALESCE(dmg.valueFloat, dmg.valueInt) AS damage, invItems.typeID, categoryID, singleton, contraband,
		IF(vol.attributeID IS NULL, COALESCE(vold.valueFloat, vold.valueInt), COALESCE(vol.valueFloat, vol.valueInt)) AS volume
	FROM invItems
	LEFT JOIN invTypes USING(typeID)
	LEFT JOIN invGroups USING(groupID)
	LEFT JOIN invItemsAttributes dmg ON invItems.itemID = dmg.itemID AND dmg.attributeID = _damage
	LEFT JOIN invItemsAttributes vol ON vol.itemID = invItems.itemID AND vol.attributeID = _volume
	LEFT JOIN dgmTypeAttributes vold ON vold.typeID = invItems.typeID AND vold.attributeID = _volume
	WHERE invItems.itemID = _itemID AND locationID = _stationID AND ownerID = _ownerID;
END//

DELIMITER ;