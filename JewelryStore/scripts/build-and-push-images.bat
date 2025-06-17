@echo off
echo ========================================
echo Building and Pushing JewelryStore Images
echo ========================================

cd ..\src

echo.
echo [1/4] Building API image...
docker build -f JewelryStore.API/Dockerfile -t aoishook/jewelrystore_api:latest .
if %errorlevel% neq 0 (
    echo ERROR: Failed to build API image
    pause
    exit /b 1
)

echo.
echo [2/4] Building UI image...
docker build -f JewelryStore.BlazorUI/Dockerfile -t aoishook/jewelrystore_ui:latest .
if %errorlevel% neq 0 (
    echo ERROR: Failed to build UI image
    pause
    exit /b 1
)

echo.
echo [3/4] Pushing API image to Docker Hub...
docker push aoishook/jewelrystore_api:latest
if %errorlevel% neq 0 (
    echo ERROR: Failed to push API image. Make sure you are logged in with 'docker login'
    pause
    exit /b 1
)

echo.
echo [4/4] Pushing UI image to Docker Hub...
docker push aoishook/jewelrystore_ui:latest
if %errorlevel% neq 0 (
    echo ERROR: Failed to push UI image. Make sure you are logged in with 'docker login'
    pause
    exit /b 1
)

echo.
echo ========================================
echo SUCCESS: All images built and pushed!
echo ========================================
echo API: aoishook/jewelrystore_api:latest
echo UI:  aoishook/jewelrystore_ui:latest
echo ========================================

pause 