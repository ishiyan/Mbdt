@echo off

set exe=%1\bin\x64\Release\%1.exe

copy /Y "%exe%"         "%2"
copy /Y "%exe%.config"  "%2"
