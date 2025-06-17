@echo off
chcp 65001 > nul
title JewelryStore Blazor UI

echo.
echo ===============================================
echo Starting JewelryStore Blazor UI
echo ===============================================
echo.

echo Checking API...
curl -s http://localhost:5257/health 2>nul
if errorlevel 1 (
    echo WARNING: API not responding on localhost:5257
    echo TIP: First start API with start-api.bat
    echo.
)

echo.
echo Building project...
dotnet build ../src/JewelryStore.BlazorUI
if errorlevel 1 (
    echo ERROR: Failed to build UI
    pause
    exit /b 1
)

echo.
echo Starting Blazor UI on port 5216...
cd ../src/JewelryStore.BlazorUI
dotnet run --urls "http://localhost:5216"

pause 