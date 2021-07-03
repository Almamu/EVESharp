/*Table structure for table `crpOffices` */

DROP TABLE IF EXISTS `crpOffices`;

CREATE TABLE `crpOffices` (
  `corporationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `stationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `officeID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `typeID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `officeFolderID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `startDate` BIGINT(20) NOT NULL DEFAULT '0',
  `rentPeriodInDays` INT(11) NOT NULL DEFAULT '0',
  `periodCost` DOUBLE(22,0) NOT NULL DEFAULT '0',
  `balanceDueDate` DOUBLE(22,0) NULL DEFAULT NULL,
  `nextBillID` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY  (`corporationID`,`officeFolderID`),
  KEY `itemID` (`itemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `crpOffices` */