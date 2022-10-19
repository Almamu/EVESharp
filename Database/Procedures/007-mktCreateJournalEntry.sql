DROP PROCEDURE IF EXISTS `MktCreateJournalEntry`;

DELIMITER //

CREATE PROCEDURE `MktCreateJournalEntry`(
  IN `_transactionDate` bigint(20),
  IN `_entryTypeID` int(10),
  IN `_charID` int(10),
  IN `_ownerID1` int(10),
  IN `_ownerID2` int(10),
  IN `_referenceID` int(10),
  IN `_amount` double,
  IN `_balance` double,
  IN `_description` varchar(43),
  IN `_accountKey` int(10)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new journal entry with the given data'
BEGIN
	INSERT INTO mktJournal(
		transactionDate, entryTypeID, charID, ownerID1, ownerID2, referenceID, amount, balance, description, accountKey
	)VALUES(
		_transactionDate, _entryTypeID, _charID, _ownerID1, _ownerID2, _referenceID, _amount, _balance, _description, _accountKey
	);
END//

DELIMITER ;