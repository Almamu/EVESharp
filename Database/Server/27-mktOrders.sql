/*Table structure for table `market_orders` */

DROP TABLE IF EXISTS `mktOrders`;

CREATE TABLE `mktOrders` (
  `orderID` int(10) unsigned NOT NULL auto_increment,
  `typeID` int(10) unsigned NOT NULL default '0',
  `charID` int(10) unsigned NOT NULL default '0',
  `regionID` int(10) unsigned NOT NULL default '0',
  `stationID` int(10) unsigned NOT NULL default '0',
  `range` int(10) unsigned NOT NULL default '0',
  `bid` tinyint(3) unsigned NOT NULL default '0',
  `price` double NOT NULL default '0',
  `volEntered` int(10) unsigned NOT NULL default '0',
  `volRemaining` int(10) unsigned NOT NULL default '0',
  `issued` bigint(20) unsigned NOT NULL default '0',
  `orderState` int(10) unsigned NOT NULL default '0',
  `minVolume` int(10) unsigned NOT NULL default '0',
  `contraband` tinyint(3) unsigned NOT NULL default '0',
  `accountID` int(10) unsigned NOT NULL default '0',
  `duration` int(10) unsigned NOT NULL default '0',
  `isCorp` tinyint(3) unsigned NOT NULL default '0',
  `solarSystemID` int(11) NOT NULL default '0',
  `escrow` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`orderID`),
  KEY `typeID` (`typeID`),
  KEY `regionID` (`regionID`),
  KEY `stationID` (`stationID`),
  KEY `solarSystemID` (`solarSystemID`),
  KEY `regionID_2` (`regionID`),
  KEY `typeID_2` (`typeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktOrders` */