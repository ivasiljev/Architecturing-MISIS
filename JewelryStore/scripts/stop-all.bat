@echo off
chcp 65001 > nul
title JewelryStore Stop All Services

echo.
echo ===============================================
echo Stopping all JewelryStore services
echo ===============================================
echo.

echo Checking running containers...
docker ps --format "table {{.Names}}\t{{.Status}}" --filter "name=jewelrystore"

echo.
echo Stopping Docker containers...
cd ..\docker
docker-compose down
cd ..

echo.
echo Cleaning unused resources...
docker system prune -f

echo.
echo SUCCESS: All services stopped!
echo.
echo To start services use:
echo   Full start:       start-monitoring.bat
echo   Infrastructure:   start.bat
echo   Applications:     start-app.bat
echo.

pause 