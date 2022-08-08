DROP PROCEDURE IF EXISTS `MapCalculateJumps`;

DELIMITER //

CREATE PROCEDURE `MapCalculateJumps`(IN _fromSolarSystemID INT(10), IN _toSolarSystemID INT(10))
SQL SECURITY INVOKER
COMMENT 'Fetches the distance in jumps between two solar systems'
BEGIN
	SELECT jumps FROM mapPrecalculatedSolarSystemJumps WHERE fromSolarSystemID = _fromSolarSystemID AND toSolarSystemID = _toSolarSystemID;
END//

DELIMITER ;