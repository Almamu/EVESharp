DROP PROCEDURE IF EXISTS `MktRecordTransaction`;

DELIMITER //

CREATE PROCEDURE `MktRecordTransaction`(
  IN `_transactionDateTime` bigint(20),
  IN `_typeID` int(10),
  IN `_quantity` int(10),
  IN `_price` double(22,0),
  IN `_transactionType` int(10),
  IN `_characterID` int(10),
  IN `_clientID` int(10),
  IN `_stationID` int(10),
  IN `_accountKey` int(10),
  IN `_entityID` int(11)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new transaction entry with the given data'
BEGIN
  INSERT INTO mktTransactions(
    transactionDateTime, typeID, quantity, price, transactionType, characterID, clientID, stationID, accountKey, entityID
  )VALUE(
    _transactionDateTime, _typeID, _quantity, _price, _transactionType, _characterID, _clientID, _stationID, _accountKey, _entityID
  );
END//

DELIMITER ;