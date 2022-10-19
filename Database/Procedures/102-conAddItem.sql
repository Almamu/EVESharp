DROP PROCEDURE IF EXISTS `ConAddItem`;

DELIMITER //

CREATE PROCEDURE `ConAddItem`(
	IN _contractID INT,
	IN _typeID INT,
	IN _quantity INT,
	IN _inCrate INT,
	IN _itemID INT
)
SQL SECURITY INVOKER
COMMENT 'Finds the given item extracting the relevant information for a contract creation'
BEGIN
	INSERT INTO conItems(contractID, itemTypeID, quantity, inCrate, itemID)VALUES(_contractID, _typeID, _quantity, _inCrate, _itemID);
END//

DELIMITER ;