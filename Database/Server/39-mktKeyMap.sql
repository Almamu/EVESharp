/*Table structure for table `mktKeyMap` */

DROP TABLE IF EXISTS `mktKeyMap`;

CREATE TABLE `mktKeyMap` (
  `accountKey` int(10) unsigned NOT NULL default '0',
  `accountType` varchar(100) NOT NULL default '',
  `accountName` varchar(100) NOT NULL default '',
  `description` varchar(100) NOT NULL default '',
  PRIMARY KEY  (`accountKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `mktKeyMap` */

insert  into `mktKeyMap`(`accountKey`,`accountType`,`accountName`,`description`) values
	(1000,'A','cash',''),
	(1001,'A','cash2',''),
	(1002,'A','cash3',''),
	(1003,'A','cash4',''),
	(1004,'A','cash5',''),
	(1005,'A','cash6',''),
	(1006,'A','cash7',''),
	(1100,'A','property',''),
	(1500,'A','escrow',''),
	(1800,'A','receivables',''),
	(2000,'L','payables',''),
	(2010,'L','gold',''),
	(2900,'L','equity',''),
	(3000,'R','sales',''),
	(4000,'C','purchases','');
