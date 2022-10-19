DROP PROCEDURE IF EXISTS `CrpListFactionConstellations`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionConstellations`()
SQL SECURITY INVOKER
COMMENT 'List faction constellations'
BEGIN
	SELECT factionID, constellationID FROM mapConstellations WHERE factionID IS NOT NULL ORDER BY factionID;
END//

DELIMITER ;