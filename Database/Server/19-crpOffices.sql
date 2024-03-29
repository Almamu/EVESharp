/*Table structure for table `crpOffices` */

DROP TABLE IF EXISTS `crpOffices`;

CREATE TABLE `crpOffices` (
  `corporationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `stationID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `officeID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `officeFolderID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `startDate` BIGINT(20) NOT NULL DEFAULT '0',
  `rentPeriodInDays` INT(11) NOT NULL DEFAULT '0',
  `periodCost` DOUBLE(22,0) NOT NULL DEFAULT '0',
  `balanceDueDate` BIGINT(20) NULL DEFAULT NULL,
  `impounded` TINYINT(1) NOT NULL DEFAULT '0',
  `nextBillID` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY  (`corporationID`,`stationID`),
  INDEX `officeFolderID` (`officeFolderID` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `crpOffices` */
