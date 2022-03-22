/**
 * user accounts table
 */
DROP TABLE IF EXISTS `account`;

CREATE TABLE IF NOT EXISTS `account` (
  `accountID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `accountName` varchar(48) CHARACTER SET utf8 NOT NULL,
  `password` blob NOT NULL,
  `role` bigint(20) unsigned NOT NULL,
  `online` tinyint(1) NOT NULL,
  `banned` tinyint(1) NOT NULL,
  `proxyNodeID` bigint(20) NOT NULL,
  PRIMARY KEY (`accountID`),
  UNIQUE KEY `accountName` (`accountName`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 ;

