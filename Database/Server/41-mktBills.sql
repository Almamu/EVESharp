/*Table structure for table `mktBills` */

DROP TABLE IF EXISTS `mktBills`;

CREATE TABLE `mktBills` (
  `billID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `billTypeID` INT(10) UNSIGNED NULL DEFAULT NULL,
  `debtorID` INT(10) UNSIGNED NULL DEFAULT NULL,
  `creditorID` INT(10) UNSIGNED NULL DEFAULT NULL,
  `amount` DOUBLE(22,0) NOT NULL DEFAULT '0',
  `dueDateTime` BIGINT(20) NOT NULL DEFAULT '0',
  `interest` DOUBLE(22,0) NOT NULL DEFAULT '0',
  `externalID` INT(11) NOT NULL DEFAULT -1,
  `paid` TINYINT(4) NOT NULL DEFAULT '0',
  `externalID2` INT(11) NOT NULL DEFAULT -1,
  PRIMARY KEY  (`billID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktBills` */