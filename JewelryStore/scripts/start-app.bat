@echo off
chcp 65001 > nul
title JewelryStore Applications

echo.
echo ===============================================
echo Starting JewelryStore Applications
echo ===============================================
echo.

echo Checking Docker services...
docker ps --format "table {{.Names}}\t{{.Status}}" | findstr jewelrystore
if errorlevel 1 (
    echo WARNING: Docker services not running
    echo TIP: First run start.bat
    pause
    exit /b 1
)

echo.
echo Building projects...
dotnet build ../src
if errorlevel 1 (
    echo ERROR: Failed to build projects
    pause
    exit /b 1
)

echo.
echo Starting API on port 5257...
start "JewelryStore API" cmd /k "cd /d %~dp0 && dotnet run --project ../src/JewelryStore.API --urls http://localhost:5257"

echo.
echo Waiting for API to start...
sleep 8

echo Checking API readiness...
:check_api
curl -s http://localhost:5257/health >nul 2>&1
if errorlevel 1 (
    echo API is still loading...
    sleep 2
    goto check_api
)
echo SUCCESS: API is ready!

echo.
echo Starting Blazor UI on port 5216...
start "JewelryStore UI" cmd /k "cd /d %~dp0 && dotnet run --project ../src/JewelryStore.BlazorUI --urls http://localhost:5216"

echo.
echo Waiting for UI to start...
sleep 5

echo.
echo Opening browser...
start http://localhost:5216

echo.
echo ===============================================
echo Applications started!
echo ===============================================
echo.
echo Access to services:
echo   UI:               http://localhost:5216
echo   API:              http://localhost:5257  
echo   Swagger:          http://localhost:5257
echo   Prometheus:       http://localhost:9090
echo   Grafana:          http://localhost:3000 (admin/admin123)
echo   RabbitMQ Management: http://localhost:15672
echo.
echo TIP: To stop, close console windows
echo ===============================================

pause 