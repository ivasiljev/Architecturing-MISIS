# Система мониторинга JewelryStore

## Обзор

В соответствии с требованиями курсовой работы, система JewelryStore интегрирована с комплексной системой мониторинга на базе Prometheus и Grafana.

## Архитектура мониторинга

### Компоненты мониторинга

1. **Prometheus** (порт 9090)
   - Основная система сбора метрик
   - Собирает данные со всех сервисов каждые 15 секунд
   - Хранит исторические данные

2. **Grafana** (порт 3000)
   - Визуализация метрик
   - Дашборды для мониторинга всех компонентов системы
   - Логин: admin / Пароль: admin123

3. **Экспортеры метрик**:
   - **Redis Exporter** (порт 9121) - мониторинг Redis
   - **Node Exporter** (порт 9100) - системные метрики
   - **SQL Server Exporter** (порт 9399) - мониторинг базы данных
   - **Kafka JMX Exporter** (порт 9308) - мониторинг Kafka

### Мониторинг сервисов приложения

1. **JewelryStore.API**
   - HTTP метрики (количество запросов, время ответа)
   - Статус сервиса (up/down)
   - Пользовательские метрики (счетчик заказов)

2. **JewelryStore.BlazorUI**
   - HTTP метрики веб-интерфейса
   - Статус сервиса
   - Health check endpoint

## Запуск системы мониторинга

### Автоматический запуск
```bash
# Запуск всей системы с мониторингом
./start-monitoring.bat
```

### Ручной запуск
```bash
# 1. Запуск инфраструктуры
docker-compose -f docker/docker-compose.yml up -d

# 2. Запуск API
cd src/JewelryStore.API
dotnet run

# 3. Запуск UI
cd src/JewelryStore.BlazorUI  
dotnet run
```

## Доступ к компонентам мониторинга

- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000
- **Kafka UI**: http://localhost:8080
- **API Health**: http://localhost:5257/health
- **UI Health**: http://localhost:5216/health

## Основные метрики

### API метрики
- `http_requests_total` - общее количество HTTP запросов
- `http_request_duration_seconds` - время выполнения запросов
- `jewelrystore_requests_total` - пользовательский счетчик запросов
- `up` - статус доступности сервиса

### Системные метрики
- `node_cpu_seconds_total` - использование CPU
- `node_memory_MemAvailable_bytes` - доступная память
- `node_filesystem_size_bytes` - использование диска

### Redis метрики
- `redis_memory_used_bytes` - использование памяти Redis
- `redis_connected_clients` - количество подключенных клиентов
- `redis_commands_total` - общее количество команд

### Kafka метрики
- `kafka_server_BrokerTopicMetrics_MessagesInPerSec` - скорость поступления сообщений
- `kafka_server_BrokerTopicMetrics_BytesInPerSec` - скорость поступления данных

### SQL Server метрики
- Подключения к базе данных
- Использование CPU и памяти
- Статистика выполнения запросов

## Дашборды Grafana

### JewelryStore System Overview
Главный дашборд включает:
- График HTTP запросов по времени
- Статус всех сервисов (up/down)
- Использование памяти Redis
- Загрузка CPU системы
- Скорость обработки сообщений Kafka

## Настройка алертов

В Grafana можно настроить алерты для:
- Недоступности сервисов
- Высокой загрузки CPU (>80%)
- Заполнения памяти Redis (>90%)
- Ошибок в HTTP запросах (>5%)

## Соответствие требованиям курсовой работы

### Для оценки 4
✅ Контейнеризированное приложение  
✅ Кеширование (Redis)  
✅ Event-driven архитектура (Kafka)  
✅ Система мониторинга (Prometheus + Grafana)  

### Для оценки 5
✅ Микросервисная архитектура (API + UI + Infrastructure)  
✅ Кеширование в сервисах  
✅ Взаимодействие через брокер сообщений  
✅ Мониторинг всех сервисов  
✅ Дополнительные экспортеры для полного мониторинга  

## Устранение неполадок

### Проблемы с запуском
1. Убедитесь, что Docker запущен
2. Проверьте доступность портов (5257, 5216, 9090, 3000)
3. Запустите `docker-compose logs` для просмотра логов

### Отсутствие метрик
1. Проверьте targets в Prometheus (http://localhost:9090/targets)
2. Убедитесь, что приложения экспонируют /metrics endpoint
3. Проверьте конфигурацию prometheus.yml

### Grafana не показывает данные
1. Проверьте подключение к Prometheus в datasources
2. Убедитесь, что метрики поступают в Prometheus
3. Проверьте корректность queries в дашборде 