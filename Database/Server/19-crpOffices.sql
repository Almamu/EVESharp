/*Table structure for table `crpOffices` */

DROP TABLE IF EXISTS `crpOffices`;

CREATE TABLE `crpOffices` (
  `corporationID` int(10) unsigned NOT NULL default '0',
  `stationID` int(10) unsigned NOT NULL default '0',
  `officeID` int(10) unsigned NOT NULL default '0',
  `typeID` int(10) unsigned NOT NULL default '0',
  `officeFolderID` int(10) unsigned NOT NULL default '0',
  PRIMARY KEY  (`corporationID`,`officeFolderID`),
  KEY `itemID` (`itemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `crpOffices` */