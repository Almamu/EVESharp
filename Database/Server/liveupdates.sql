-- MariaDB dump 10.17  Distrib 10.4.12-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: evesharp
-- ------------------------------------------------------
-- Server version	10.4.12-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `liveupdates`
--

DROP TABLE IF EXISTS `liveupdates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `liveupdates` (
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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `liveupdates`
--

LOCK TABLES `liveupdates` WRITE;
/*!40000 ALTER TABLE `liveupdates` DISABLE KEYS */;
INSERT INTO `liveupdates` VALUES (1,'tutorial','Disables tutorials whenever accessed, inspired from EVEmuDisableTutorial.py from Groove\'s evegen',219,219,101786,101786,'GetTutorials','svc.tutorial::TutorialSvc','globalClassMethod','c\0\0\0\0\0\0\0\0\0C\0\0\0sa\0\0\0d\0d\0\0k\0\0}\0t\0i\0d\0h\0\0d\0d\0<ƒ\0|\0i\0i\0d\0ƒ\0i\0d\0d\0d\0ƒ\0t\0i\0d\0h\0\0d	\0d\0<ƒ\0h\0\0S(\n\0\0\0Niÿÿÿÿt\0\0\0CustomNotifys\0\0\0Disabling tutorialst\0\0\0notifyt\0\0\0chart\0\0\0uit\r\0\0\0showTutorialsi\0\0\0\0s\0\0\0Tutorials disabled!(\0\0\0t\0\0\0__builtin__t\0\0\0evet\0\0\0Messaget\0\0\0settingst\0\0\0Gett\0\0\0Set(\0\0\0t\0\0\0selfR\0\0\0(\0\0\0\0(\0\0\0\0s:\0\0\0C:\\Program Files (x86)\\CCP\\EVE\\lib\\corelib.ccp\\autoexec.pyt\0\0\0GetTutorials\0\0\0s\n\0\0\0\0');
/*!40000 ALTER TABLE `liveupdates` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-02-18 15:53:12
