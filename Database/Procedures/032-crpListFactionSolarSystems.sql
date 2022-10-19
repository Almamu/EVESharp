DROP PROCEDURE IF EXISTS `CrpListFactionSolarSystems`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionSolarSystems`()
SQL SECURITY INVOKER
COMMENT 'List faction solar system'
BEGIN
	SELECT factionID, solarSystemID FROM mapSolarSystems WHERE factionID IS NOT NULL ORDER BY factionID;
END//

DELIMITER ;