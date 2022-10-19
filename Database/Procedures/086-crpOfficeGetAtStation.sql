DROP PROCEDURE IF EXISTS `CrpOfficeGetAtStation`;

DELIMITER //

CREATE PROCEDURE `CrpOfficeGetAtStation`(
	IN _corporationID INT,
	IN _stationID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the officeFolderID of the office at the given station (if any)'
BEGIN
	DECLARE _officeFolderID INT(10) UNSIGNED;

	SELECT officeFolderID INTO _officeFolderID FROM crpOffices WHERE corporationID = _corporationID AND stationID = _stationID AND impounded = 0;

	SELECT _officeFolderID;
END//

DELIMITER ;