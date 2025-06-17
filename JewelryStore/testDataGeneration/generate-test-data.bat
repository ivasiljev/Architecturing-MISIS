@echo off
chcp 65001 > nul
title JewelryStore - Generate Test Data

echo.
echo ===============================================
echo Generating test data for Business Dashboard
echo ===============================================
echo.

echo This script will generate realistic user activity:
echo   - Product views (60%%)
echo   - Category searches (20%%)
echo   - Text searches (15%%)
echo   - Order creation (5%%)
echo.

set /p duration="Enter duration in seconds (default 300): "
if "%duration%"=="" set duration=300

echo.
echo Starting test data generation for %duration% seconds...
echo Press Ctrl+C to stop early
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0generate-test-data.ps1" -Duration %duration%

echo.
echo ===============================================
echo Test data generation completed!
echo ===============================================
echo.
echo Open Grafana to see the results:
echo   URL: http://localhost:3000
echo   Login: admin / Password: admin123
echo.
pause 