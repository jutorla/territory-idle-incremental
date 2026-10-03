@echo off
title Territory Idle QoL mod - uninstall
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Install-Mod.ps1" -Uninstall %*
echo.
pause
