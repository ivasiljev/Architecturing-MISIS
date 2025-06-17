@echo off
chcp 65001 > nul
title JewelryStore - Full Start with Monitoring

echo.
echo =====================================================
echo Starting JewelryStore with Monitoring (Prometheus + Grafana)
echo =====================================================

cd ..

echo Stopping any existing containers and processes...
docker-compose -f docker/docker-compose.yml down

echo Killing any existing .NET processes on ports 5257 and 5216...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5257 ^| findstr LISTENING') do taskkill /f /pid %%a >nul 2>&1
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5216 ^| findstr LISTENING') do taskkill /f /pid %%a >nul 2>&1

echo Starting infrastructure services...
docker-compose -f docker/docker-compose.yml up -d sqlserver redis

echo Waiting for infrastructure to start...
ping 127.0.0.1 -n 21 > nul

echo Starting RabbitMQ...
docker-compose -f docker/docker-compose.yml up -d rabbitmq

echo Waiting for RabbitMQ to start...
ping 127.0.0.1 -n 31 > nul

echo Starting monitoring services...
docker-compose -f docker/docker-compose.yml up -d prometheus grafana redis-exporter node-exporter sql-exporter rabbitmq-exporter

echo Waiting for monitoring services to start...
ping 127.0.0.1 -n 21 > nul

echo Building and starting API...
cd src/JewelryStore.API
dotnet build
if errorlevel 1 (
    echo ERROR: Failed to build API
    cd ../..
    pause
    exit /b 1
)
start /B dotnet run --urls "http://localhost:5257"
cd ../..

echo Waiting for API to start...
ping 127.0.0.1 -n 11 > nul

echo Building and starting Blazor UI...
cd src/JewelryStore.BlazorUI
dotnet build
if errorlevel 1 (
    echo ERROR: Failed to build UI
    cd ../..
    pause
    exit /b 1
)
start /B dotnet run --urls "http://localhost:5216"
cd ../..

echo.
echo =====================================================
echo All services started!
echo =====================================================
echo API: http://localhost:5257
echo UI: http://localhost:5216
echo Prometheus: http://localhost:9090
echo Grafana: http://localhost:3000 (admin/admin123)
echo RabbitMQ Management: http://localhost:15672
echo =====================================================
echo Press any key to show service status...
pause >nul

echo.
echo Checking service status...
docker ps --filter "name=jewelrystore" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo Checking API health...
ping 127.0.0.1 -n 6 > nul
curl -s http://localhost:5257/health >nul 2>&1
if errorlevel 1 (
    echo WARNING: API health check failed
) else (
    echo SUCCESS: API is responding
)

echo.
echo Checking UI health...
curl -s http://localhost:5216/health >nul 2>&1
if errorlevel 1 (
    echo WARNING: UI health check failed
) else (
    echo SUCCESS: UI is responding
)

echo.
echo To stop all services, run: scripts/stop-all.bat
pause 