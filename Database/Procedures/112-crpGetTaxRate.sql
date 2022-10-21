DROP PROCEDURE IF EXISTS `CrpGetTaxRate`;

DELIMITER //

CREATE PROCEDURE `CrpGetTaxRate`(
	IN _corporationID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets the tax rate for the given corporation'
BEGIN
	SELECT taxRate FROM corporation WHERE corporationID = _corporationID;
END//

DELIMITER ;