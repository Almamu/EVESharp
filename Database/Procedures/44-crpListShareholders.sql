DROP PROCEDURE IF EXISTS `CrpListShareholders`;

DELIMITER //

CREATE PROCEDURE `CrpListShareholders`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'List the shareholders for the given corporation'
BEGIN
  SELECT ownerID FROM crpShares LEFT JOIN chrInformation ON ownerID = characterID WHERE crpShares.corporationID = _corporationID;
END//

DELIMITER ;