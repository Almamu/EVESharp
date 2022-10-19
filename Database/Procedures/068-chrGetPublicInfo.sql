DROP PROCEDURE IF EXISTS `ChrGetPublicInfo`;

DELIMITER //

CREATE PROCEDURE `ChrGetPublicInfo`(IN _characterID INT(10))
SQL SECURITY INVOKER
COMMENT 'Obtains the public info of the given character'
BEGIN
            SELECT
            	chrInformation.corporationID, raceID, bloodlineID,
            	ancestryID, careerID, schoolID, careerSpecialityID,
            	createDateTime, gender 
            FROM chrInformation 
            LEFT JOIN chrAncestries USING (ancestryID) 
            LEFT JOIN chrBloodlines USING (bloodlineID) 
            WHERE characterID = _characterID;
END//

DELIMITER ;