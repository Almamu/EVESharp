/*Table structure for table `chrApplications` */

DROP TABLE IF EXISTS `crpApplications`;

CREATE TABLE `crpApplications` (
  `corporationID` int(10) unsigned NOT NULL,
  `allianceID` int(10) unsigned NOT NULL,
  `applicationText` text NOT NULL,
  `applicationDateTime` bigint(20) unsigned NOT NULL,
  `state` int(10) unsigned NOT NULL,
  PRIMARY KEY  (`corporationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `chrApplications` */