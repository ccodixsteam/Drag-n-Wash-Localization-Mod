@echo off
setlocal enabledelayedexpansion

echo Drag'n Wash Multi-Language Localization Installer
echo Created by ccodix
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

echo Target game directory: %TARGET_DIR%
echo Installing localization mod...

set "SOURCE_DIR=%~dp0"
if exist "%SOURCE_DIR%dist\LocalizationMod" (
    set "MOD_SOURCE=%SOURCE_DIR%dist"
) else (
    set "MOD_SOURCE=%SOURCE_DIR%"
)

if not exist "%TARGET_DIR%\LocalizationMod\data" (
    mkdir "%TARGET_DIR%\LocalizationMod\data"
)

if exist "%MOD_SOURCE%\winhttp.dll" (
    copy /y "%MOD_SOURCE%\winhttp.dll" "%TARGET_DIR%\" >nul
) else if exist "%SOURCE_DIR%doorstop\winhttp.dll" (
    copy /y "%SOURCE_DIR%doorstop\winhttp.dll" "%TARGET_DIR%\" >nul
)

if exist "%MOD_SOURCE%\doorstop_config.ini" (
    copy /y "%MOD_SOURCE%\doorstop_config.ini" "%TARGET_DIR%\" >nul
) else if exist "%SOURCE_DIR%doorstop\doorstop_config.ini" (
    copy /y "%SOURCE_DIR%doorstop\doorstop_config.ini" "%TARGET_DIR%\" >nul
)

if exist "%TARGET_DIR%\LocalizationMod\DragNWashRussianLocalization.dll" (
    del /f /q "%TARGET_DIR%\LocalizationMod\DragNWashRussianLocalization.dll" >nul 2>nul
)

if exist "%MOD_SOURCE%\LocalizationMod\DragNWashLocalization.dll" (
    copy /y "%MOD_SOURCE%\LocalizationMod\DragNWashLocalization.dll" "%TARGET_DIR%\LocalizationMod\" >nul
) else if exist "%SOURCE_DIR%src\bin\Release\netstandard2.1\DragNWashLocalization.dll" (
    copy /y "%SOURCE_DIR%src\bin\Release\netstandard2.1\DragNWashLocalization.dll" "%TARGET_DIR%\LocalizationMod\" >nul
)

if exist "%MOD_SOURCE%\LocalizationMod\0Harmony.dll" (
    copy /y "%MOD_SOURCE%\LocalizationMod\0Harmony.dll" "%TARGET_DIR%\LocalizationMod\" >nul
) else if exist "%SOURCE_DIR%lib\0Harmony.dll" (
    copy /y "%SOURCE_DIR%lib\0Harmony.dll" "%TARGET_DIR%\LocalizationMod\" >nul
)

if exist "%MOD_SOURCE%\LocalizationMod\data" (
    xcopy /s /e /y /i "%MOD_SOURCE%\LocalizationMod\data" "%TARGET_DIR%\LocalizationMod\data" >nul
) else if exist "%SOURCE_DIR%data" (
    xcopy /s /e /y /i "%SOURCE_DIR%data" "%TARGET_DIR%\LocalizationMod\data" >nul
)

echo.
echo Installation completed successfully.
echo You can now run Drag'n Wash via Steam.
echo.
pause