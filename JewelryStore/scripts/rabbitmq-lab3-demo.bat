@echo off
title RabbitMQ Lab3 Demo - JewelryStore
echo ===============================================
echo     RabbitMQ LAB 3 DEMO - JewelryStore
echo ===============================================
echo.

echo [INFO] Запуск демонстрации RabbitMQ для лабораторной работы 3...
echo.

echo [STEP 1] Запуск Docker контейнеров...
cd /d "%~dp0\..\docker"
docker-compose up -d rabbitmq
echo.

echo [STEP 2] Ожидание запуска RabbitMQ...
echo Подождите 30 секунд для полной инициализации RabbitMQ...
timeout /t 30 /nobreak >nul
echo.

echo [STEP 3] Проверка статуса RabbitMQ...
docker ps | findstr rabbitmq
echo.

echo [STEP 4] Демонстрация типов обменов в RabbitMQ...
echo.
echo === FANOUT EXCHANGE DEMO ===
echo Отправка сообщения в Fanout Exchange (рассылка всем подписчикам)
curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{},\"routing_key\":\"\",\"payload\":\"Test Fanout Message\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/products.exchange/publish

echo.
echo === DIRECT EXCHANGE DEMO ===
echo Отправка сообщения в Direct Exchange с routing key
curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{},\"routing_key\":\"orders.created\",\"payload\":\"Test Direct Message\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/orders.exchange/publish

echo.
echo === TOPIC EXCHANGE DEMO ===
echo Отправка сообщений в Topic Exchange с различными routing keys
curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{},\"routing_key\":\"notification.email.user\",\"payload\":\"Email notification\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/notifications.exchange/publish

curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{},\"routing_key\":\"notification.sms.user\",\"payload\":\"SMS notification\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/notifications.exchange/publish

echo.
echo === HEADERS EXCHANGE DEMO ===
echo Отправка сообщений в Headers Exchange с метаданными
curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{\"headers\":{\"level\":\"error\"}},\"routing_key\":\"\",\"payload\":\"Error log message\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/logs.exchange/publish

curl -i -u guest:guest -H "content-type:application/json" -X POST ^
-d "{\"properties\":{\"headers\":{\"level\":\"info\"}},\"routing_key\":\"\",\"payload\":\"Info log message\",\"payload_encoding\":\"string\"}" ^
http://localhost:15672/api/exchanges/%%2F/logs.exchange/publish

echo.
echo [STEP 5] Демонстрация завершена!
echo.
echo ===============================================
echo              USEFUL LINKS
echo ===============================================
echo RabbitMQ Management UI: http://localhost:15672
echo Username: guest
echo Password: guest
echo.
echo Exchanges:
echo - orders.exchange (Direct)
echo - products.exchange (Fanout)  
echo - notifications.exchange (Topic)
echo - logs.exchange (Headers)
echo.
echo Queues:
echo - orders.created.queue
echo - products.inventory.queue
echo - products.notifications.queue
echo - notifications.email.queue
echo - notifications.sms.queue
echo - logs.error.queue
echo - logs.info.queue
echo ===============================================
echo.

echo [INFO] Откройте RabbitMQ Management UI для просмотра результатов
start http://localhost:15672

echo.
echo Нажмите любую клавишу для завершения...
pause >nul 