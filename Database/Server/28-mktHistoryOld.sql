
/*Table structure for table `mktHistoryOld` */

DROP TABLE IF EXISTS `mktHistoryOld`;

CREATE TABLE `mktHistoryOld` (
  `regionID` int(10) unsigned NOT NULL,
  `typeID` int(10) unsigned NOT NULL,
  `historyDate` bigint(20) unsigned NOT NULL,
  `lowPrice` double NOT NULL,
  `highPrice` double NOT NULL,
  `avgPrice` double NOT NULL,
  `volume` int(10) unsigned NOT NULL,
  `orders` int(10) unsigned NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktHistoryOld` */