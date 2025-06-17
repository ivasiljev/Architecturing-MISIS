@echo off
chcp 65001 > nul
title JewelryStore API

echo.
echo ===============================================
echo Starting JewelryStore API
echo ===============================================
echo.

echo Checking infrastructure...
docker ps --format "table {{.Names}}\t{{.Status}}" | findstr jewelrystore

echo.
echo Building project...
dotnet build ../src/JewelryStore.API
if errorlevel 1 (
    echo ERROR: Failed to build API
    pause
    exit /b 1
)

echo.
echo Starting API on port 5257...
cd ../src/JewelryStore.API
dotnet run --urls "http://localhost:5257"

pause 