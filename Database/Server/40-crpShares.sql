
/*Table structure for table `crpCharShares` */

DROP TABLE IF EXISTS `crpShares`;

CREATE TABLE `crpShares` (
  `ownerID` int(10) unsigned NOT NULL default '0',
  `corporationID` int(10) unsigned NOT NULL default '0',
  `shares` int(10) unsigned NOT NULL default '0',
  PRIMARY KEY  (`ownerID`,`corporationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `crpCharShares` */