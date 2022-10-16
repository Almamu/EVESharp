DROP PROCEDURE IF EXISTS `CrpOfficeDestroyOrImpound`;

DELIMITER //

CREATE PROCEDURE `CrpOfficeDestroyOrImpound`(
	IN _officeFolderID INT
)
SQL SECURITY INVOKER
COMMENT 'Destroys any bill pending on the given officeFolderID and impounds anything in it' 
BEGIN
	DECLARE _itemCount BIGINT(20);

	DELETE FROM mktBills WHERE billID IN (SELECT nextBillID FROM crpOffices WHERE officeFolderID = _officeFolderID);
	UPDATE crpOffices SET nextBillID = 0 WHERE officeFolderID = _officeFolderID;

	SELECT COUNT(*) INTO _itemCount FROM invItems WHERE locationID = _officeFolderID;

	IF _itemCount = 0 THEN
		DELETE FROM crpOffices WHERE officeFolderID = _officeFolderID;
		DELETE FROM invItems WHERE itemID = _officeFolderID;
	ELSE
		UPDATE crpOffices SET impounded = 1 WHERE officeFolderID = _officeFolderID;
	END IF;
END//

DELIMITER ;