DROP PROCEDURE IF EXISTS `CrpAlliancesList`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesList`()
SQL SECURITY INVOKER
COMMENT 'Lists all the alliance information'
BEGIN
	-- TODO: CALCULATE MEMBER COUNT
	SELECT allianceID, itemName AS allianceName, shortName, 0 AS memberCount, executorCorpID, creatorCorpID, creatorCharID, dictatorial, startDate, deleted FROM crpAlliances LEFT JOIN eveNames ON itemID = allianceID;
END//

DELIMITER ;