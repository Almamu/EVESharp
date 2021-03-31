ALTER TABLE `eveGraphics`
	CHANGE COLUMN `url3D` `url3D` VARCHAR(100) NULL DEFAULT NULL COLLATE 'ascii_general_ci' AFTER `graphicID`,
	CHANGE COLUMN `urlWeb` `urlWeb` VARCHAR(100) NULL DEFAULT NULL COLLATE 'ascii_general_ci' AFTER `url3D`,
	CHANGE COLUMN `icon` `icon` VARCHAR(100) NULL DEFAULT NULL COLLATE 'ascii_general_ci' AFTER `obsolete`,
	CHANGE COLUMN `urlSound` `urlSound` VARCHAR(100) NULL DEFAULT NULL COLLATE 'ascii_general_ci' AFTER `icon`;