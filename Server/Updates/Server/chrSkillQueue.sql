DROP TABLE IF EXISTS `chrSkillQueue`;

CREATE TABLE `chrSkillQueue` (
	`id` int(10) unsigned NOT NULL auto_increment,
	`itemID` int(10) unsigned NOT NULL,
	`typeID` int(10) unsigned NOT NULL,
	`level` int(10) unsigned NOT NULL,
	UNIQUE KEY `id` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 ROW_FORMAT=FIXED;

ALTER TABLE `chrSkillQueue`
 CHANGE `id` `orderIndex` INT(10) UNSIGNED NOT NULL AFTER `characterID`,
 CHANGE `itemID` `characterID` INT(10) UNSIGNED NOT NULL,
 DROP KEY `id`;
