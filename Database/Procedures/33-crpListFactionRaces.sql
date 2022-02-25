DROP PROCEDURE IF EXISTS `CrpListFactionRaces`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionRaces`()
SQL SECURITY INVOKER
COMMENT 'List faction races'
BEGIN
	SELECT factionID, raceID FROM factionRaces WHERE factionID IS NOT NULL ORDER BY factionID;
END//

DELIMITER ;