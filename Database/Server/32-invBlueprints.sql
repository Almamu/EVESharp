DROP TABLE IF EXISTS `invBlueprints`;

CREATE TABLE `invBlueprints` (
  `itemID` int(10) unsigned NOT NULL,
  `copy` tinyint(1) unsigned NOT NULL default '0',
  `materialLevel` int(10) unsigned NOT NULL default '0',
  `productivityLevel` int(10) unsigned NOT NULL default '0',
  `licensedProductionRunsRemaining` int(10) NOT NULL default '-1',
  PRIMARY KEY  (`itemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;