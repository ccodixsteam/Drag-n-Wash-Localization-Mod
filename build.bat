@echo off
setlocal enabledelayedexpansion

echo Building Drag'n Wash Localization Mod...

where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Error: .NET SDK is not installed or not in PATH.
    pause
    exit /b 1
)

dotnet build src\DragNWashLocalization.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo Error: Build failed.
    pause
    exit /b 1
)

if not exist dist\LocalizationMod\data mkdir dist\LocalizationMod\data

copy /y src\bin\Release\netstandard2.1\DragNWashLocalization.dll dist\LocalizationMod\ >nul
copy /y src\bin\Release\netstandard2.1\0Harmony.dll dist\LocalizationMod\ >nul
xcopy /s /e /y /i data dist\LocalizationMod\data >nul
copy /y doorstop\winhttp.dll dist\ >nul
copy /y doorstop\doorstop_config.ini dist\ >nul

echo Build complete. Package files assembled in dist\
