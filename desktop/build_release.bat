@echo off
setlocal
cd /d "%~dp0"
dotnet build QuarrelEx.sln -c Release
pause
