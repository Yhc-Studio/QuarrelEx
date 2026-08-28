@echo off
if "%~1"=="" (
  echo Usage: prepare_32k_base.bat "Battle City (J).nes"
  exit /b 2
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0prepare_32k_base.ps1" -InputRom "%~1"
