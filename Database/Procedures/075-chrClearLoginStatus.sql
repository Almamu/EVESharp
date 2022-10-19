DROP PROCEDURE IF EXISTS `ChrClearLoginStatus`;

DELIMITER //

CREATE PROCEDURE `ChrClearLoginStatus`()
SQL SECURITY INVOKER
COMMENT 'Clears the online flag for all the characters'
BEGIN
	UPDATE chrInformation SET `online` = 0;
END//

DELIMITER ;