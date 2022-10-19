DROP PROCEDURE IF EXISTS `CrtGetRelationships`;

DELIMITER //

CREATE PROCEDURE `CrtGetRelationships`()
SQL SECURITY INVOKER
COMMENT 'Returns all the certificate relationships available'
BEGIN
	SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships;
END//

DELIMITER ;