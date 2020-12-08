DROP TABLE IF EXISTS `eveLiveUpdates`;

CREATE TABLE `eveLiveUpdates` (
  `updateID` int(11) NOT NULL AUTO_INCREMENT,
  `updateName` varchar(100) DEFAULT NULL,
  `description` varchar(100) DEFAULT NULL,
  `machoVersionMin` int(11) DEFAULT NULL,
  `machoVersionMax` int(11) DEFAULT NULL,
  `buildNumberMin` int(11) DEFAULT NULL,
  `buildNumberMax` int(11) DEFAULT NULL,
  `methodName` varchar(100) DEFAULT NULL,
  `objectID` varchar(100) DEFAULT NULL,
  `codeType` varchar(100) DEFAULT NULL,
  `code` blob DEFAULT NULL,
  PRIMARY KEY (`updateID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

INSERT INTO `eveLiveUpdates` VALUES (1,'tutorial','Disables tutorials whenever accessed, inspired from EVEmuDisableTutorial.py from Groove\'s evegen',219,219,101786,101786,'GetTutorials','svc.tutorial::TutorialSvc','globalClassMethod','c\0\0\0\0\0\0\0\0\0C\0\0\0s[\0\0\0d\0d\0\0k\0\0}\0t\0i\0d\0h\0\0d\0d\0<ƒ\0|\0i\0i\0i\0i\0d\0d\0ƒ\0t\0i\0d\0h\0\0d\0d\0<ƒ\0h\0\0S(\0\0\0Niÿÿÿÿt\0\0\0CustomNotifys\0\0\0Disabling tutorialst\0\0\0notifyt\r\0\0\0showTutorialsi\0\0\0\0s\0\0\0Tutorials disabled!(\0\0\0t\0\0\0__builtin__t\0\0\0evet\0\0\0Messaget\0\0\0settingst\0\0\0chart\0\0\0uit\0\0\0Set(\0\0\0t\0\0\0selfR\0\0\0(\0\0\0\0(\0\0\0\0s:\0\0\0C:\\Program Files (x86)\\CCP\\EVE\\lib\\corelib.ccp\\autoexec.pyt\0\0\0GetTutorials\0\0\0s\n\0\0\0\0');
