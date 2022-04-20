DROP TABLE IF EXISTS `cluAddresses`;

CREATE TABLE `cluAddresses` (
  `type` char(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `objectID` int(11) NOT NULL,
  `nodeID` bigint(20) NOT NULL,
  PRIMARY KEY (`type`, `objectID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;