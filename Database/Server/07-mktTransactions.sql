
/*Table structure for table `mktTransactions` */

DROP TABLE IF EXISTS `mktTransactions`;

CREATE TABLE `mktTransactions` (
  `transactionID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `transactionDateTime` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
  `typeID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `quantity` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `price` DOUBLE(22,0) NOT NULL DEFAULT '0',
  `transactionType` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `characterID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `clientID` INT(10) UNSIGNED NULL DEFAULT NULL,
  `stationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `accountKey` INT(10) UNSIGNED NOT NULL DEFAULT '1000',
  `entityID` INT(11) NOT NULL DEFAULT '0',
  PRIMARY KEY  (`transactionID`),
  KEY `stationID` (`stationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktTransactions` */