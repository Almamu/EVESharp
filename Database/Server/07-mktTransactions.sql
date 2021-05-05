
/*Table structure for table `mktTransactions` */

DROP TABLE IF EXISTS `mktTransactions`;

CREATE TABLE `mktTransactions` (
  `transactionID` int(10) unsigned NOT NULL auto_increment,
  `transactionDateTime` bigint(20) unsigned NOT NULL default '0',
  `typeID` int(10) unsigned NOT NULL default '0',
  `quantity` int(10) unsigned NOT NULL default '0',
  `price` double NOT NULL default '0',
  `transactionType` int(10) unsigned NOT NULL default '0',
  `characterID` int(10) unsigned NOT NULL default '0',
  `clientID` int(10) unsigned default NULL,
  `stationID` int(10) unsigned NOT NULL default '0',
  `accountKey` int(10) unsigned NOT NULL default '1000',
  PRIMARY KEY  (`transactionID`),
  KEY `stationID` (`stationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktTransactions` */