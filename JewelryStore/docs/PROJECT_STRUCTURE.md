# 📁 Структура проекта JewelryStore

## 🗂️ Обзор организации файлов

Проект организован по принципу разделения ответственности с четкой структурой папок:

```
JewelryStore/
├── 📂 src/                          # Исходный код приложения
├── 📂 docker/                       # Docker конфигурации
├── 📂 scripts/                      # Скрипты запуска и управления
├── 📂 docs/                         # Документация проекта
├── 📂 tests/                        # Тестовые данные и скрипты
├── 📂 tools/                        # Вспомогательные утилиты
├── 📄 README.md                     # Краткое описание проекта
└── 📄 JewelryStore.sln             # Solution файл
```

## 📂 Детальное описание папок

### `src/` - Исходный код
```
src/
├── JewelryStore.Core/               # Доменные сущности и интерфейсы
│   ├── Entities/                    # Сущности (Product, Order, User)
│   ├── Interfaces/                  # Интерфейсы репозиториев
│   ├── DTOs/                        # Data Transfer Objects
│   └── Events/                      # Доменные события
├── JewelryStore.Infrastructure/     # Реализация инфраструктуры
│   ├── Data/                        # DbContext и конфигурации
│   ├── Repositories/                # Реализация репозиториев
│   ├── Caching/                     # Redis кэширование
│   └── Events/                      # Kafka события
├── JewelryStore.API/                # Web API сервис
│   ├── Controllers/                 # REST API контроллеры
│   ├── Services/                    # Бизнес-сервисы
│   └── Program.cs                   # Точка входа API
└── JewelryStore.BlazorUI/           # Blazor пользовательский интерфейс
    ├── Components/                  # Blazor компоненты
    ├── Pages/                       # Страницы приложения
    ├── Services/                    # UI сервисы
    └── Program.cs                   # Точка входа UI
```

### `docker/` - Контейнеризация
```
docker/
├── docker-compose.yml               # Основная конфигурация сервисов
└── monitoring/                      # Конфигурации мониторинга
    ├── prometheus.yml               # Настройки Prometheus
    └── grafana/                     # Настройки Grafana
        ├── datasources/             # Источники данных
        ├── dashboards/              # JSON дашборды
        └── alerting/                # Правила алертов
```

### `scripts/` - Автоматизация
```
scripts/
├── start-monitoring.bat             # 🌟 Полный запуск с мониторингом
├── start.bat                        # Запуск инфраструктуры
├── start-app.bat                    # Запуск приложений
├── start-api.bat                    # Запуск только API
├── start-ui.bat                     # Запуск только UI
├── stop-all.bat                     # Остановка всех сервисов
├── check-services.bat               # Диагностика системы
├── generate-test-data.bat           # Генерация тестовых данных
├── generate-test-data.ps1           # PowerShell скрипт генерации
├── test-profile-pages.ps1           # Тестирование страниц профиля
├── test-order-creation.ps1          # Тестирование создания заказов
└── start.sh                         # Linux/Mac версия запуска
```

### `docs/` - Документация
```
docs/
├── README.md                        # Подробная документация проекта
├── BUSINESS_DASHBOARD_GUIDE.md      # Руководство по бизнес-дашборду
├── SCRIPTS_GUIDE.md                 # Описание всех скриптов
├── MONITORING.md                    # Настройка мониторинга
├── DOCKER_UPDATE_GUIDE.md           # Работа с Docker контейнерами
├── PROFILE_PAGES_GUIDE.md           # Руководство по страницам профиля
└── PROJECT_STRUCTURE.md             # Этот файл
```

### `tests/` - Тестирование
```
tests/
└── test-data.sql                    # SQL скрипт с тестовыми данными
```

### `tools/` - Утилиты
```
tools/
└── webhook-simulator.py             # Python симулятор webhook'ов
```

## 🎯 Принципы организации

### 1. **Разделение по назначению**
- **Код** - в `src/`
- **Инфраструктура** - в `docker/`
- **Автоматизация** - в `scripts/`
- **Документация** - в `docs/`

### 2. **Логическая группировка**
- Все скрипты запуска в одном месте
- Вся документация собрана вместе
- Четкое разделение слоев в коде

### 3. **Удобство использования**
- Главный README в корне для быстрого старта
- Подробная документация в `docs/`
- Все команды запуска в `scripts/`

## 🚀 Быстрая навигация

### Хочу запустить проект:
```bash
scripts/start-monitoring.bat
```

### Хочу изучить код:
```
src/JewelryStore.API/Controllers/     # REST API
src/JewelryStore.BlazorUI/Pages/      # UI страницы
src/JewelryStore.Core/Entities/       # Доменные модели
```

### Хочу настроить мониторинг:
```
docker/monitoring/grafana/dashboards/ # Дашборды
docs/MONITORING.md                     # Документация
```

### Хочу понять архитектуру:
```
docs/README.md                         # Подробное описание
docs/BUSINESS_DASHBOARD_GUIDE.md       # Бизнес-метрики
```

## 📋 Преимущества новой структуры

✅ **Четкая организация** - каждый тип файлов в своей папке  
✅ **Легкая навигация** - интуитивно понятные названия папок  
✅ **Масштабируемость** - легко добавлять новые компоненты  
✅ **Профессиональный вид** - соответствует стандартам индустрии  
✅ **Удобство разработки** - быстрый поиск нужных файлов  

---

*Эта структура обеспечивает удобство разработки, поддержки и масштабирования проекта.* 