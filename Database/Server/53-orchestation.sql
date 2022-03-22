DROP TABLE IF EXISTS `cluster`;

CREATE TABLE `cluster` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ip` char(15) COLLATE utf8mb4_unicode_ci NOT NULL,
  `address` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `port` int(11) NOT NULL,
  `role` char(6) NOT NULL,
  `load` double NOT NULL DEFAULT 0,
  `lastHeartBeat` bigint(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;