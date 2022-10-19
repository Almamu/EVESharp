DROP PROCEDURE IF EXISTS `CrpListMedals`;

DELIMITER //

CREATE PROCEDURE `CrpListMedals`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'List the medals created by the given corporation'
BEGIN
  SELECT medalID, title, description, date, creatorID, noRecepients FROM crpMedals WHERE corporationID = _corporationID;
END//

DELIMITER ;