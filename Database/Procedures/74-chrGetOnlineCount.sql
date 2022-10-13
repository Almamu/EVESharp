DROP PROCEDURE IF EXISTS `ChrGetOnlineCount`;

DELIMITER //

CREATE PROCEDURE `ChrGetOnlineCount`()
SQL SECURITY INVOKER
COMMENT 'Returns the number of players connected to the server'
BEGIN
	SELECT COUNT(*) AS playerCount FROM chrInformation WHERE `online` = 1;
END//

DELIMITER ;