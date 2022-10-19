DROP PROCEDURE IF EXISTS `MktBillsCreate`;

DELIMITER //

CREATE PROCEDURE `MktBillsCreate`(
  IN `_billTypeID` int(10) unsigned,
  IN `_debtorID` int(10) unsigned,
  IN `_creditorID` int(10) unsigned,
  IN `_amount` double,
  IN `_dueDateTime` bigint(20),
  IN `_interest` double,
  IN `_externalID` int(11),
  IN `_externalID2` int(11)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new bill'
BEGIN
	INSERT INTO mktBills(billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2)VALUES(_billTypeID, _debtorID, _creditorID, _amount, _dueDateTime, _interest, _externalID, 0, _externalID2);
END//

DELIMITER ;