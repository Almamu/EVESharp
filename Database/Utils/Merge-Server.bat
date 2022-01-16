@echo off

set SQL_ROOT=..

set INPUT_FILE=%SQL_ROOT%\Server\*.sql
set PROCEDURES_INPUT_FILE=%SQL_ROOT%\Procedures\*.sql
set OUTPUT_FILE=%SQL_ROOT%\Server.sql
set TEMP_FILE=_merge_temp_

if exist %OUTPUT_FILE% del %OUTPUT_FILE%
type %INPUT_FILE% > %TEMP_FILE%
type %PROCEDURES_INPUT_FILE% >> %TEMP_FILE%
move %TEMP_FILE% %OUTPUT_FILE%

echo File %OUTPUT_FILE% has been successfully created.
