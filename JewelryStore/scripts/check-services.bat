@echo off
chcp 65001 > nul
title JewelryStore Services Check

echo.
echo ===============================================
echo Checking JewelryStore services status
echo ===============================================
echo.

echo == Docker Containers ==
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" --filter "name=jewelrystore"

echo.
echo == Service Tests ==

echo.
echo [Redis] Testing connection...
docker exec jewelrystore-redis redis-cli ping 2>nul
if errorlevel 1 (
    echo   FAILED: Redis not responding
) else (
    echo   SUCCESS: Redis is working
)

echo.
echo [SQL Server] Testing connection...
docker exec jewelrystore-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1 as Test" -W -h -1 >nul 2>&1
if errorlevel 1 (
    echo   WARNING: SQL Server not available
) else (
    echo   SUCCESS: SQL Server is working
)

echo.
echo [Kafka] Testing topics...
docker exec jewelrystore-kafka kafka-topics --bootstrap-server localhost:9092 --list >nul 2>&1
if errorlevel 1 (
    echo   WARNING: Kafka not ready yet
) else (
    echo   SUCCESS: Kafka is working
)

echo.
echo [Prometheus] Testing metrics...
curl -s http://localhost:9090/-/healthy >nul 2>&1
if errorlevel 1 (
    echo   WARNING: Prometheus not available
) else (
    echo   SUCCESS: Prometheus is working
)

echo.
echo [Grafana] Testing availability...
curl -s http://localhost:3000/api/health >nul 2>&1
if errorlevel 1 (
    echo   WARNING: Grafana not available
) else (
    echo   SUCCESS: Grafana is working
)

echo.
echo == Web Interfaces ==
echo Available services in browser:
echo   Grafana:          http://localhost:3000 (admin/admin123)
echo   Prometheus:       http://localhost:9090
echo   Kafka UI:         http://localhost:8080
echo.

echo == Applications ==
echo [API] Checking health endpoint...
curl -s http://localhost:5257/health >nul 2>&1
if errorlevel 1 (
    echo   WARNING: API not running on localhost:5257
    echo   TIP: Run start-api.bat
) else (
    echo   SUCCESS: API working on http://localhost:5257
)

echo.
echo [UI] Checking Blazor UI...
curl -s http://localhost:5216/health >nul 2>&1
if errorlevel 1 (
    echo   WARNING: UI not running on localhost:5216
    echo   TIP: Run start-ui.bat
) else (
    echo   SUCCESS: UI working on http://localhost:5216
)

echo.
echo == Metrics Exporters ==
echo   Redis Exporter:      localhost:9121
echo   Node Exporter:       localhost:9100
echo   SQL Server Exporter: localhost:9399
echo   Kafka JMX Exporter:  localhost:9308
echo.

echo ===============================================
echo Quick commands:
echo   Full start:          start-monitoring.bat
echo   API only:            start-api.bat
echo   UI only:             start-ui.bat
echo   Stop all:            stop-all.bat
echo ===============================================

pause 