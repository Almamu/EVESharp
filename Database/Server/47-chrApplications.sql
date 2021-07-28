/*Table structure for table `chrApplications` */

DROP TABLE IF EXISTS `chrApplications`;

CREATE TABLE `chrApplications` (
  `corporationID` int(10) unsigned NOT NULL,
  `characterID` int(10) unsigned NOT NULL,
  `applicationText` text NOT NULL,
  `applicationDateTime` bigint(20) unsigned NOT NULL,
  PRIMARY KEY  (`corporationID`,`characterID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `chrApplications` */