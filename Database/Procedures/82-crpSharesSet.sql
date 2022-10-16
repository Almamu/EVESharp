DROP PROCEDURE IF EXISTS `CrpSharesSet`;

DELIMITER //

CREATE PROCEDURE `CrpSharesSet`(
	IN _ownerID INT,
	IN _corporationID INT,
	IN _shares INT
)
SQL SECURITY INVOKER
COMMENT 'Updates the current shares owned for the given corporationID'
BEGIN
	REPLACE INTO crpShares(ownerID, corporationID, shares) VALUES (_ownerID, _corporationID, _shares);
END//

DELIMITER ;