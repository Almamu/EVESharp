DROP PROCEDURE IF EXISTS `CrpListMedalDetails`;

DELIMITER //

CREATE PROCEDURE `CrpListMedalDetails`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'List the medals created by the given corporation'
BEGIN
  SELECT crpMedals.medalID, part, graphic, color FROM crpMedalParts LEFT JOIN crpMedals USING(medalID) WHERE corporationID = _corporationID;
END//

DELIMITER ;