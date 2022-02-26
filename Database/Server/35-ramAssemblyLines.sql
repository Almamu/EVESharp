/* Update assembly lines so the nextFreeTime field is a simple bigint instead of a datetime */
ALTER TABLE `ramAssemblyLines`
	CHANGE COLUMN `nextFreeTime` `nextFreeTime` BIGINT NULL DEFAULT 0 AFTER `containerID`;