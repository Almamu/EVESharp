DROP PROCEDURE IF EXISTS `CrpListFactionRegions`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionRegions`()
SQL SECURITY INVOKER
COMMENT 'List faction regions'
BEGIN
	SELECT factionID, regionID FROM mapRegions WHERE factionID IS NOT NULL ORDER BY factionID;
END//

DELIMITER ;