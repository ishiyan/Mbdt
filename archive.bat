@echo off
rem Must run under admin account, otherwise we get a confirmation popup from the regedit
cd ..
set prefix=Mbdt
set sevenz=%prefix%\7z.exe

rem -------------------------
rem select a type of archive:

set archiveType=zip

rem set archiveType=7z
rem set sevenzOptions=-mx9 -mmt=on
rem -------------------------

set tmpfile=%TEMP%%\prefix%.tmp
if exist "%tmpfile%" (
 del "%tmpfile%"
)

rem Export registry settings to a temporary file
start /W regedit /E %tmpfile% "HKEY_CURRENT_USER\Control Panel\International"

rem Read the exported data
for /f "tokens=1* delims==" %%i in ('find/I "iDate" "%tmpfile%"') do set iDate=%%j
for /f "tokens=1* delims==" %%i in ('find/I "sDate" "%tmpfile%"') do set sDate=%%j

rem Remove quotes
set iDate=%iDate:"=%
set sDate=%sDate:"=%
rem echo debug: iDate = %iDate%, sDate = %sDate%

rem Parse today's date depending on registry's date format settings
if %iDate%==0 for /f "tokens=1-4* delims=%sDate%" %%i in ('date/T') do (
    set Year=%%k
    set Month=%%i
    set Day=%%j
)
if %iDate%==1 for /f "tokens=1-4* delims=%sDate%" %%i in ('date/T') do (
    set Year=%%k
    set Month=%%j
    set Day=%%i
)
if %iDate%==2 for /f "tokens=1-4* delims=%sDate%" %%i in ('date/T') do (
    set Year=%%i
    set Month=%%j
    set Day=%%k
)
rem Remove the day of week if applicable
for %%a in (%Year%)  do set Year=%%a
for %%a in (%Month%) do set Month=%%a
for %%a in (%Day%)   do set Day=%%a

rem Today's date in YYYYMMDD format
set logfile=%prefix%_%Year%%Month%%Day%.log
set archive=%prefix%_%Year%%Month%%Day%.%archiveType%

if exist %archive% (
 del %archive%
)
if exist %logfile% (
 del %logfile%
)
if exist "%tmpfile%" (
 del "%tmpfile%"
)

call "%sevenz%" a %archive% %prefix% -t%archiveType% %sevenzOptions% -xr!obj >%logfile%
