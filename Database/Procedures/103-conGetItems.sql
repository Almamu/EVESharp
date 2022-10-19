DROP PROCEDURE IF EXISTS `ConGetItems`;

DELIMITER //

CREATE PROCEDURE `ConGetItems`(
	IN _contractID INT,
	IN _inCrate INT
)
SQL SECURITY INVOKER
COMMENT 'Returns all the items in the given contract'
BEGIN
	SELECT itemTypeID, quantity, itemID FROM conItems WHERE contractID = _contractID AND inCrate = _inCrate;
END//

DELIMITER ;