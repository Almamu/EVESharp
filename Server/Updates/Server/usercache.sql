-- phpMyAdmin SQL Dump
-- version 3.3.9
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Mar 26, 2012 at 05:32 
-- Server version: 5.5.8
-- PHP Version: 5.3.5

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `eve-node`
--

-- --------------------------------------------------------

--
-- Table structure for table `usercache`
--

CREATE TABLE IF NOT EXISTS `usercache` (
  `cacheID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `cacheType` varchar(48) CHARACTER SET utf8 NOT NULL,
  `cacheOwner` int(10) unsigned NOT NULL,
  `cacheOwnerName` varchar(48) CHARACTER SET utf8 NOT NULL,
  `cacheOwnerType` int(10) unsigned NOT NULL,
  `cacheData` blob NOT NULL,
  `cacheTime` bigint(20) NOT NULL,
  PRIMARY KEY (`cacheID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

--
-- Dumping data for table `usercache`
--

