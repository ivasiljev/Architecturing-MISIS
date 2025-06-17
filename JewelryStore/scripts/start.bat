@echo off
chcp 65001 >nul 2>&1
title JewelryStore Infrastructure Setup

echo.
echo ===============================================
echo Starting JewelryStore Infrastructure
echo ===============================================
echo.

REM Check if Docker is running
echo Checking Docker...
docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

REM Start infrastructure
echo Starting infrastructure services...
cd docker
docker-compose up -d
if errorlevel 1 (
    echo ERROR: Failed to start Docker containers.
    pause
    exit /b 1
)

echo Waiting for services to be ready...
sleep 30

REM Go back to root directory
cd ..

REM Check if dotnet is installed
echo Checking .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8 SDK is not installed. Please install it first.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore packages.
    pause
    exit /b 1
)

REM Apply database migrations
echo Applying database migrations...
cd ..\src\JewelryStore.API
dotnet ef database update

echo.
echo ===============================================
echo Infrastructure is ready!
echo ===============================================
echo.
echo Running services:
echo   - SQL Server:     localhost:1433
echo   - Redis:          localhost:6379
echo   - RabbitMQ:       localhost:5672
echo.
echo Monitoring:
echo   - Prometheus:     http://localhost:9090
echo   - Grafana:        http://localhost:3000 (admin/admin123)
echo   - RabbitMQ Mgmt:  http://localhost:15672
echo.
echo Metrics exporters:
echo   - Redis Exporter: localhost:9121
echo   - Node Exporter:  localhost:9100
echo   - SQL Exporter:   localhost:9399
echo   - RabbitMQ:       localhost:9419
echo.
echo To start applications use:
echo   Full start:       start-monitoring.bat
echo   API only:         start-api.bat
echo   UI only:          start-ui.bat
echo   API + UI:         start-app.bat
echo.
echo Check status:       check-services.bat
echo.

pause 