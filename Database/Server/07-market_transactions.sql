
/*Table structure for table `market_transactions` */

DROP TABLE IF EXISTS `market_transactions`;

CREATE TABLE `market_transactions` (
  `transactionID` int(10) unsigned NOT NULL auto_increment,
  `transactionDateTime` bigint(20) unsigned NOT NULL default '0',
  `typeID` int(10) unsigned NOT NULL default '0',
  `quantity` int(10) unsigned NOT NULL default '0',
  `price` double NOT NULL default '0',
  `transactionType` int(10) unsigned NOT NULL default '0',
  `characterID` int(10) unsigned NOT NULL default '0',
  `clientID` int(10) unsigned default NULL,
  `regionID` int(10) unsigned NOT NULL default '0',
  `stationID` int(10) unsigned NOT NULL default '0',
  `corpTransaction` int(10) unsigned NOT NULL default '0',
  PRIMARY KEY  (`transactionID`),
  KEY `regionID` (`regionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `market_transactions` */