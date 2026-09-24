@echo off
setlocal enabledelayedexpansion

echo Drag'n Wash Localization Uninstaller
echo.

set "TARGET_DIR="

if exist "DragNWash.exe" (
    set "TARGET_DIR=%~dp0"
    goto found_target
)

if exist "D:\SteamLibrary\steamapps\common\Drag'n Wash\DragNWash.exe" (
    set "TARGET_DIR=D:\SteamLibrary\steamapps\common\Drag'n Wash"
    goto found_target
)

if exist "C:\Program Files (x86)\Steam\steamapps\common\Drag'n Wash\DragNWash.exe" (
    set "TARGET_DIR=C:\Program Files (x86)\Steam\steamapps\common\Drag'n Wash"
    goto found_target
)

for /f "tokens=2*" %%a in ('reg query "HKCU\Software\Valve\Steam" /v "SteamPath" 2^>nul') do (
    set "STEAM_PATH=%%b"
    set "STEAM_PATH=!STEAM_PATH:/=\!"
    if exist "!STEAM_PATH!\steamapps\common\Drag'n Wash\DragNWash.exe" (
        set "TARGET_DIR=!STEAM_PATH!\steamapps\common\Drag'n Wash"
        goto found_target
    )
)

echo Could not automatically detect Drag'n Wash directory.
set /p "TARGET_DIR=Enter path to Drag'n Wash folder: "

:found_target
if not exist "%TARGET_DIR%\DragNWash.exe" (
    echo Error: DragNWash.exe was not found in "%TARGET_DIR%".
    echo Please verify the folder path and try again.
    pause
    exit /b 1
)

echo Removing localization mod files...

if exist "%TARGET_DIR%\winhttp.dll" (
    del /f /q "%TARGET_DIR%\winhttp.dll" >nul 2>nul
)

if exist "%TARGET_DIR%\doorstop_config.ini" (
    del /f /q "%TARGET_DIR%\doorstop_config.ini" >nul 2>nul
)

if exist "%TARGET_DIR%\LocalizationMod" (
    rmdir /s /q "%TARGET_DIR%\LocalizationMod" >nul 2>nul
)

echo.
echo Uninstallation completed successfully.
echo Original game files restored.
echo.
pause