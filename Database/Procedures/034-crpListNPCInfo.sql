DROP PROCEDURE IF EXISTS `CrpListNPCInfo`;

DELIMITER //

CREATE PROCEDURE `CrpListNPCInfo`()
SQL SECURITY INVOKER
COMMENT 'List NPC corporation info'
BEGIN
	SELECT 
       corporationID,
       corporationName, mainActivityID, secondaryActivityID,
       size, extent, solarSystemID, investorID1, investorShares1,
       investorID2, investorShares2, investorID3, investorShares3,
       investorID4, investorShares4,
       friendID, enemyID, publicShares, initialPrice,
       minSecurity, scattered, fringe, corridor, hub, border,
       factionID, sizeFactor, stationCount, stationSystemCount,
       stationID, ceoID, eveNames.itemName AS ceoName
     FROM crpNPCCorporations
     JOIN corporation USING (corporationID)
       LEFT JOIN eveNames ON ceoID = eveNames.itemID;
END//

DELIMITER ;