
/*Table structure for table `eveMailMimeType` */
DROP TABLE IF EXISTS `eveMail`;

CREATE TABLE `eveMail` (
	`channelID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`messageID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`senderID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`subject` VARCHAR(255) NOT NULL DEFAULT '',
	`body` LONGTEXT NOT NULL DEFAULT '0',
	`mimeTypeID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`created` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
	`read` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0',
	PRIMARY KEY (`messageID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `eveMailMimeType`;

CREATE TABLE `eveMailMimeType` (
  `mimeTypeID` int(10) unsigned NOT NULL auto_increment,
  `mimeType` text NOT NULL,
  `binary` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`mimeTypeID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

/*Data for the table `eveMailMimeType` */

INSERT INTO `eveMailMimeType`(`mimeTypeID`,`mimeType`,`binary`)VALUES(1,'text/plain',0),(2, 'text/html', 0);