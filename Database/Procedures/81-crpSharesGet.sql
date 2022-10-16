DROP PROCEDURE IF EXISTS `CrpSharesGet`;

DELIMITER //

CREATE PROCEDURE `CrpSharesGet`(
	IN _ownerID INT,
	IN _corporationID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the current shares owned for the given corporationID'
BEGIN
	DECLARE _shares INT(10) UNSIGNED DEFAULT 0;

	SELECT shares INTO _shares FROM crpShares WHERE ownerID = _ownerID AND corporationID = _corporationID;

	SELECT _shares;
END//

DELIMITER ;