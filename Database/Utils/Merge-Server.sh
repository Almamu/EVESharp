#!/bin/sh

SQL_ROOT=..

INPUT_FILE=$SQL_ROOT/Server/*.sql
PROCEDURES_INPUT_FILE=$SQL_ROOT/Procedures/*.sql
OUTPUT_FILE=$SQL_ROOT/Server.sql
TEMP_FILE=_merge_temp_

if [ -e $OUTPUT_FILE ]; then
	rm $OUTPUT_FILE
fi
cat $INPUT_FILE > $TEMP_FILE
cat $PROCEDURES_INPUT_FILE >> $TEMP_FILE
mv $TEMP_FILE $OUTPUT_FILE

echo File $OUTPUT_FILE has been successfully created.

