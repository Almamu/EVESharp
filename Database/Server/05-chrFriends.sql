CREATE TABLE `chrFriends` (
  `characterID` INT(10) UNSIGNED NOT NULL,
  `friendID` INT(10) UNSIGNED NOT NULL,
  `status` INT NULL,
  PRIMARY KEY (`characterID`, `friendID`),
  INDEX `fk_friendID_idx` (`friendID` ASC) VISIBLE,
  CONSTRAINT `fk_characterID`
    FOREIGN KEY (`characterID`)
    REFERENCES `evemu`.`chrInformation` (`characterID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_friendID`
    FOREIGN KEY (`friendID`)
    REFERENCES `evemu`.`chrInformation` (`characterID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
