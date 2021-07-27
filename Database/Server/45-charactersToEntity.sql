/*
 * Insert static characters to invItems table
 */
REPLACE INTO invItems (itemID, typeID, ownerID, locationID, singleton, quantity)
 SELECT itemID, typeID, 1, IF(stationID IS NOT NULL, stationID, 0), 1, 1
  FROM eveNames
  LEFT JOIN chrInformation ON chrInformation.characterID = itemID
  WHERE itemID < 100000000 AND groupID = 1;