DROP PROCEDURE IF EXISTS `CrpListFactionStationCount`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionStationCount`()
SQL SECURITY INVOKER
COMMENT 'List factions station count'
BEGIN
	SELECT factionID, COUNT(stationID) FROM crpNPCCorporations LEFT JOIN staStations USING (corporationID) GROUP BY factionID;
END//

DELIMITER ;