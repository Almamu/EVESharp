DROP PROCEDURE IF EXISTS `CrpListFactionCorporations`;

DELIMITER //

CREATE PROCEDURE `CrpListFactionCorporations`()
SQL SECURITY INVOKER
COMMENT 'List faction corporations'
BEGIN
	SELECT corporationID, factionID from crpNPCCorporations;
END//

DELIMITER ;