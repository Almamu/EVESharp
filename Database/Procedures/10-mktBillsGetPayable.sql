DROP PROCEDURE IF EXISTS `MktBillsGetPayable`;

DELIMITER //

CREATE PROCEDURE `MktBillsGetPayable`(IN _debtorID INT(11))
SQL SECURITY INVOKER
COMMENT 'Returns the payable bills for the specified ownerID'
BEGIN
	SELECT billID, billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2 FROM mktBills WHERE debtorID = _debtorID AND paid = 0;
END//

DELIMITER ;