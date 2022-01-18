DROP PROCEDURE IF EXISTS `CrpGetRecruitmentAdTypes`;

DELIMITER //

CREATE PROCEDURE `CrpGetRecruitmentAdTypes`()
SQL SECURITY INVOKER
COMMENT 'Gets the types of recruitment ads for the client'
BEGIN
  SELECT
  	typeMask, typeName, description, groupID, groupName, crpRecruitmentAdTypes.dataID, crpRecruitmentAdGroups.dataID AS groupDataID
  FROM crpRecruitmentAdTypes
  LEFT JOIN crpRecruitmentAdGroups USING (groupID);
END//

DELIMITER ;