DROP PROCEDURE IF EXISTS `CrpOfficesGetExpired`;

DELIMITER //

CREATE PROCEDURE `CrpOfficesGetExpired`(
  IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Returns all the expired offices that are not impounded yet for processing'
BEGIN
	SELECT stationID, corporationID, officeFolderID FROM crpOffices WHERE balanceDueDate < _currentTime AND impounded = 0;
END//

DELIMITER ;