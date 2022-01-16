DROP PROCEDURE IF EXISTS `MktBillsGetReceivable`;

DELIMITER //

CREATE PROCEDURE `MktBillsGetReceivable`(IN _creditorID INT(11))
SQL SECURITY INVOKER
COMMENT 'Returns the receivable bills for the specified creditorID'
BEGIN
	SELECT billID, billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2 FROM mktBills WHERE creditorID = _creditorID;
END//

DELIMITER ;