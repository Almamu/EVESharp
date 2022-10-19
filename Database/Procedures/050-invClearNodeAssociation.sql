DROP PROCEDURE IF EXISTS `InvClearNodeAssociation`;

DELIMITER //

CREATE PROCEDURE `InvClearNodeAssociation`()
SQL SECURITY INVOKER
COMMENT 'Clears the nodeID assigned to all items'
BEGIN
	UPDATE invItems SET nodeID = 0;
END//

DELIMITER ;