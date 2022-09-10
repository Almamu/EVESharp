DROP PROCEDURE IF EXISTS `ChrGetPublicInfo3`;

DELIMITER //

CREATE PROCEDURE `ChrGetPublicInfo3`(IN _characterID INT(10))
SQL SECURITY INVOKER
COMMENT 'Obtains the public info of the given character'
BEGIN
            SELECT bounty, title, startDateTime, description, corporationID FROM chrInformation WHERE characterID = _characterID;
END//

DELIMITER ;