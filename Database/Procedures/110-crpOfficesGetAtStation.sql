DROP PROCEDURE IF EXISTS `CrpOfficesGetAtStation`;

DELIMITER //

CREATE PROCEDURE `CrpOfficesGetAtStation`(
	IN _corporationID INT,
	IN _stationID INT,
	IN _impounded TINYINT(1)
)
SQL SECURITY INVOKER
COMMENT 'Gets the office available'
BEGIN
	SELECT officeFolderID FROM crpOffices WHERE corporationID = _corporationID AND stationID = _stationID AND impounded = _impounded;
END//

DELIMITER ;