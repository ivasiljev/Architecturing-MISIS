# Лабораторная работа 2 - Кеширование (Redis)

## Описание проекта

JewelryStore реализует паттерн **Cache-Aside** с использованием Redis для кеширования данных продуктов. Это повышает производительность приложения за счет снижения нагрузки на базу данных.

## Архитектура кеширования

### Компоненты системы

1. **Redis** - In-memory хранилище ключ-значение
2. **ICacheService** - Интерфейс для работы с кешем
3. **RedisCacheService** - Реализация кеширования через StackExchange.Redis
4. **ProductRepository** - Репозиторий с интегрированным кешированием

### Схема работы Cache-Aside

```
┌─────────────┐    Cache Miss   ┌──────────────┐    ┌─────────────┐
│ Application │ ─────────────── │ Cache Layer  │    │  Database   │
│             │                 │   (Redis)    │    │ (SQL Server)│
│             │ ← ─ ─ ─ ─ ─ ─ ─ │              │    │             │
└─────────────┘   Cache Hit     └──────────────┘    └─────────────┘
       │                               │                    │
       │         1. Read Request       │                    │
       │ ────────────────────────────→ │                    │
       │                               │                    │
       │         2. Cache Miss         │                    │
       │ ← ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ │                    │
       │                               │                    │
       │         3. Read from DB       │                    │
       │ ─────────────────────────────────────────────────→ │
       │                               │                    │
       │         4. Return Data        │                    │
       │ ← ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│
       │                               │                    │
       │         5. Cache Data         │                    │
       │ ────────────────────────────→ │                    │
```

## Конфигурация Redis

### Docker Compose

```yaml
redis:
  image: redis:7-alpine
  container_name: jewelrystore-redis
  ports:
    - "6379:6379"
  volumes:
    - redis_data:/data
  restart: unless-stopped
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 30s
    timeout: 10s
    retries: 3
```

### Подключение в Program.cs

```csharp
// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString ?? "localhost:6379");
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();
```

## Реализация кеширования

### ICacheService интерфейс

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
}
```

### RedisCacheService реализация

- **Сериализация**: JSON через Newtonsoft.Json
- **TTL**: Настраиваемое время жизни ключей
- **Pattern Matching**: Удаление ключей по шаблону
- **Error Handling**: Graceful degradation при недоступности Redis

### ProductRepository с кешированием

#### Кеширование продуктов

```csharp
public async Task<IEnumerable<Product>> GetAllAsync()
{
    var cached = await _cache.GetAsync<List<Product>>(ALL_PRODUCTS_KEY);
    if (cached != null)
    {
        _logger.LogInformation("Cache HIT for all products");
        return cached;
    }

    _logger.LogInformation("Cache MISS for all products");
    var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
    
    await _cache.SetAsync(ALL_PRODUCTS_KEY, products.ToList(), CacheExpiration);
    return products;
}
```

#### Инвалидация кеша

```csharp
public async Task<Product> UpdateAsync(Product product)
{
    _context.Products.Update(product);
    await _context.SaveChangesAsync();

    // Cache invalidation
    await InvalidateCache();
    await _cache.RemoveAsync($"{CACHE_KEY_PREFIX}{product.Id}");
    
    return product;
}
```

## Настройки кеширования

| Параметр | Значение | Описание |
|----------|----------|----------|
| TTL продуктов | 30 минут | Время жизни кеша продуктов |
| Префикс ключей | `product:` | Префикс для ключей отдельных продуктов |
| Ключ всех продуктов | `products:all` | Ключ для кеша всех продуктов |

## Мониторинг кеша

### Логирование операций

Все операции с кешем логируются:
- **Cache HIT** - данные найдены в кеше
- **Cache MISS** - данные отсутствуют в кеше
- **Cache EVICT** - принудительное удаление из кеша
- **Cache INVALIDATION** - очистка кеша

### Метрики Redis

Redis Exporter предоставляет метрики для Prometheus:
- Количество подключений
- Использование памяти
- Количество операций
- Hit/Miss ratio

## Демонстрация работы с Redis CLI

### Запуск демо-скрипта

```bash
./scripts/redis-lab2-demo.bat
```

### Базовые операции Redis

#### 1. STRING операции
```bash
# Создание строкового значения
SET "student:M8O-213B-21:15" "Ivanov Ivan Ivanovich"

# Чтение значения
GET "student:M8O-213B-21:15"

# Установка TTL
EXPIRE "student:M8O-213B-21:15" 120
TTL "student:M8O-213B-21:15"
```

#### 2. HASH операции
```bash
# Создание хеша
HSET "student:M8O-213B-21:15:info" name "Ivanov Ivan" age "21" email "i.ivanov@misis.edu"

# Чтение всех полей
HGETALL "student:M8O-213B-21:15:info"

# Чтение конкретного поля
HGET "student:M8O-213B-21:15:info" name
```

#### 3. LIST операции
```bash
# Добавление элементов
LPUSH "student:M8O-213B-21:15:timetable" "Architecture"
RPUSH "student:M8O-213B-21:15:timetable" "Database Systems"

# Чтение диапазона
LRANGE "student:M8O-213B-21:15:timetable" 0 -1

# Удаление элемента
LPOP "student:M8O-213B-21:15:timetable"
```

#### 4. SET операции
```bash
# Добавление в множество
SADD "student:M8O-213B-21:15:skills" "Docker" "C#" ".NET"

# Просмотр всех элементов
SMEMBERS "student:M8O-213B-21:15:skills"

# Удаление элемента
SREM "student:M8O-213B-21:15:skills" "Docker"
```

#### 5. ZSET операции
```bash
# Создание сортированного множества
ZADD "student:M8O-213B-21:15:tasks_w_priority" 100 "Lab 1" 150 "Lab 2"

# Просмотр с весами
ZRANGE "student:M8O-213B-21:15:tasks_w_priority" 0 -1 WITHSCORES

# Изменение веса
ZINCRBY "student:M8O-213B-21:15:tasks_w_priority" 50 "Lab 1"
```

## Тестирование кеширования

### 1. Запуск приложения

```bash
cd JewelryStore/docker
docker-compose up -d
```

### 2. Проверка работы кеша

1. **Первый запрос** - Cache MISS, данные загружаются из БД
2. **Повторный запрос** - Cache HIT, данные из кеша
3. **Обновление продукта** - Cache EVICT, кеш очищается
4. **Следующий запрос** - Cache MISS, данные снова из БД

### 3. Мониторинг логов

```bash
# Логи API
docker logs -f jewelrystore-api

# Подключение к Redis CLI
docker exec -it jewelrystore-redis redis-cli

# Просмотр ключей кеша
KEYS "product:*"
KEYS "products:*"
```

## Стратегии кеширования

### Cache-Aside (Используется в проекте)

### Другие стратегии

1. **Write-Through** - запись в кеш и БД одновременно
2. **Write-Behind** - отложенная запись в БД
3. **Refresh-Ahead** - упреждающее обновление кеша

## Вопросы к защите

### 1. Что такое кэширование и зачем оно используется?

**Кэширование** - это техника оптимизации производительности, при которой часто используемые данные временно сохраняются в быстром хранилище (кеше) для ускорения последующих обращений.

**Цели кэширования:**
- **Повышение производительности** - снижение времени отклика
- **Снижение нагрузки на БД** - уменьшение количества запросов к медленным источникам данных  
- **Экономия ресурсов** - снижение использования CPU, сети, дисков
- **Улучшение пользовательского опыта** - быстрая загрузка страниц

### 2. Какие стратегии кэширования существуют?

#### **Cache-Aside (Lazy Loading)** - используется в проекте
- Приложение управляет кешем самостоятельно
- При чтении: проверить кеш → если нет, загрузить из БД → сохранить в кеш
- При записи: обновить БД → инвалидировать кеш
- ✅ Простота реализации, устойчивость к сбоям кеша
- ❌ Дублирование логики, возможны race conditions

#### **Write-Through**
- Запись в кеш и БД происходит синхронно
- Гарантирует консистентность, но замедляет запись
- ✅ Данные всегда актуальны
- ❌ Высокая латентность записи

#### **Write-Behind (Write-Back)**
- Запись сначала в кеш, потом асинхронно в БД
- ✅ Быстрая запись, пакетная обработка
- ❌ Риск потери данных, сложность реализации

#### **Refresh-Ahead**
- Упреждающее обновление кеша до истечения TTL
- ✅ Минимизирует cache miss
- ❌ Сложность предсказания использования

### 3. Какие стратегии вытеснения кэша существуют?

#### **LRU (Least Recently Used)**
- Удаляет наименее недавно использованные элементы
- Подходит при временной локальности доступа
- Реализация: двусвязный список + хеш-таблица

#### **LFU (Least Frequently Used)**  
- Удаляет наименее часто используемые элементы
- Учитывает историю использования
- Подходит для стабильных паттернов доступа

#### **FIFO (First In, First Out)**
- Удаляет самые старые элементы
- Простая реализация, но может удалить часто используемые данные

#### **TTL-Based (Time To Live)**
- Удаляет элементы по истечении времени жизни
- Используется в Redis, подходит для временных данных

#### **Random**
- Случайное удаление элементов
- Простая реализация, средняя эффективность

### 4. Что такое и зачем используется Redis (KeyDB)?

**Redis (Remote Dictionary Server)** - это in-memory структура данных, используемая как база данных, кеш и message broker.

**Основные характеристики:**
- **In-memory хранение** - все данные в RAM для максимальной скорости
- **Персистентность** - опциональное сохранение на диск (RDB/AOF)
- **Атомарные операции** - все команды выполняются атомарно
- **Репликация и кластеризация** - для высокой доступности
- **Pub/Sub** - встроенная система сообщений

**Применение Redis:**
- **Кэширование** - основное использование (сессии, данные)
- **Очереди задач** - background jobs, delayed tasks
- **Реал-тайм аналитика** - счетчики, метрики
- **Геолокационные данные** - поиск по координатам
- **Лидерборды** - рейтинги с помощью ZSET

### 5. Какие есть основные типы данных в Redis?

#### **STRING** - простые ключ-значение пары
```bash
SET key "value"
GET key
INCR counter
```
- Использование: кеш объектов, счетчики, флаги

#### **HASH** - словарь полей и значений  
```bash
HSET user:1 name "John" age 30
HGET user:1 name
HGETALL user:1
```
- Использование: объекты с полями, конфигурации

#### **LIST** - упорядоченный список строк
```bash
LPUSH queue "task1"
RPOP queue
LRANGE queue 0 -1
```
- Использование: очереди, стеки, ленты событий

#### **SET** - неупорядоченное множество уникальных строк
```bash
SADD tags "redis" "cache" "nosql"
SISMEMBER tags "redis"
SINTER set1 set2
```
- Использование: теги, уникальные идентификаторы, множественные операции

#### **ZSET (Sorted Set)** - сортированное множество с весами
```bash  
ZADD leaderboard 100 "player1" 200 "player2"
ZRANGE leaderboard 0 -1 WITHSCORES
ZRANK leaderboard "player1"
```
- Использование: рейтинги, приоритетные очереди, временные ряды

#### **Дополнительные типы:**
- **BITMAP** - операции с битами
- **HyperLogLog** - приблизительный подсчет уникальных элементов
- **Streams** - структуры данных типа лога
- **Geospatial** - геолокационные индексы

### 6. Как можно взаимодействовать с Redis?

#### **Клиентские библиотеки**
- **.NET**: StackExchange.Redis, ServiceStack.Redis
- **Java**: Jedis, Lettuce, Redisson
- **Python**: redis-py, aioredis
- **Node.js**: ioredis, node_redis
- **Go**: go-redis, redigo

#### **Инструменты командной строки**
- **redis-cli** - стандартный CLI клиент
- **redis-commander** - веб-интерфейс
- **RedisInsight** - GUI от Redis Labs

#### **Протоколы взаимодействия**
- **RESP (Redis Serialization Protocol)** - основной протокол
- **HTTP REST API** - через Redis modules или прокси
- **GraphQL** - через специальные адаптеры

#### **В нашем проекте:**
```csharp
// Подключение
IConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
IDatabase database = redis.GetDatabase();

// Использование через ICacheService
await _cache.SetAsync("key", value, TimeSpan.FromMinutes(30));
var result = await _cache.GetAsync<MyObject>("key");
```

### 7. LIST операции и их асимптотика

**Команды из демо:**
```bash
# Добавление в начало - O(1)
LPUSH "student:M8O-213B-21:15:timetable" "Architecture and Design Patterns"

# Добавление в конец - O(1)  
RPUSH "student:M8O-213B-21:15:timetable" "Database Systems"

# Получение диапазона - O(S+N), где S - start offset, N - количество элементов
LRANGE "student:M8O-213B-21:15:timetable" 0 -1  # Все элементы
LRANGE "student:M8O-213B-21:15:timetable" 0 0   # Первый элемент

# Удаление из начала - O(1)
LPOP "student:M8O-213B-21:15:timetable"
```

**Асимптотическая сложность LIST операций:**
- **LPUSH/RPUSH/LPOP/RPOP**: O(1) - константное время
- **LRANGE**: O(S+N) - зависит от позиции start и количества элементов
- **LINDEX**: O(N) - поиск по индексу в середине списка
- **LINSERT**: O(N) - вставка в произвольную позицию
- **LREM**: O(N+M) - удаление M элементов из списка длиной N

### 8. STRING операции и их асимптотика

**Команды из демо:**
```bash
# Установка значения - O(1)
SET "student:M8O-213B-21:15" "Ivanov Ivan Ivanovich"

# Получение значения - O(1)
GET "student:M8O-213B-21:15"

# Замена значения - O(1)
SET "student:M8O-213B-21:15" "Petrov Ivan Sergeevich"
```

**Асимптотическая сложность STRING операций:**
- **GET/SET**: O(1) - константное время
- **MGET/MSET**: O(N) - где N - количество ключей
- **INCR/DECR**: O(1) - атомарное изменение
- **APPEND**: O(1) - добавление к строке
- **GETRANGE/SETRANGE**: O(1) - работа с подстроками

### 9. HASH операции и их асимптотика

**Команды из демо:**
```bash
# Установка полей - O(1) на каждое поле
HSET "student:M8O-213B-21:15:info" name "Ivanov Ivan" age "21" email "i.ivanov@misis.edu"

# Получение всех полей - O(N), где N - количество полей
HGETALL "student:M8O-213B-21:15:info"

# Получение конкретного поля - O(1)
HGET "student:M8O-213B-21:15:info" name

# Изменение поля - O(1)
HSET "student:M8O-213B-21:15:info" name "Ivan Petrov"
```

**Асимптотическая сложность HASH операций:**
- **HGET/HSET/HDEL**: O(1) - работа с одним полем
- **HMGET/HMSET**: O(N) - где N - количество полей
- **HGETALL**: O(N) - получение всех полей
- **HKEYS/HVALS**: O(N) - получение всех ключей/значений
- **HEXISTS**: O(1) - проверка существования поля

### 10. SET операции и их асимптотика

**Команды из демо:**
```bash
# Добавление элементов - O(1) на каждый элемент
SADD "student:M8O-213B-21:15:skills" "Docker" "C#" ".NET"

# Получение всех элементов - O(N)
SMEMBERS "student:M8O-213B-21:15:skills"

# Попытка добавить дубликат - O(1), но элемент не добавляется
SADD "student:M8O-213B-21:15:skills" "Docker"

# Добавление нового элемента - O(1)
SADD "student:M8O-213B-21:15:skills" "Kubernetes"

# Удаление элемента - O(1)
SREM "student:M8O-213B-21:15:skills" "Kubernetes"
```

**Асимптотическая сложность SET операций:**
- **SADD/SREM/SISMEMBER**: O(1) - основные операции
- **SMEMBERS**: O(N) - получение всех элементов
- **SCARD**: O(1) - подсчет элементов
- **SINTER/SUNION/SDIFF**: O(N*M) - операции между множествами
- **SPOP/SRANDMEMBER**: O(1) - случайный элемент

### 11. ZSET операции и их асимптотика

**Команды из демо:**
```bash
# Добавление элементов с весами - O(log(N)) на каждый элемент
ZADD "student:M8O-213B-21:15:tasks_w_priority" 100 "Lab 1" 150 "Lab 2" 200 "Exam"

# Получение всех элементов с весами - O(log(N)+M)
ZRANGE "student:M8O-213B-21:15:tasks_w_priority" 0 -1 WITHSCORES

# Получение элемента с наибольшим весом - O(log(N))
ZRANGE "student:M8O-213B-21:15:tasks_w_priority" -1 -1 WITHSCORES

# Добавление элемента с весом 0 - O(log(N))
ZADD "student:M8O-213B-21:15:tasks_w_priority" 0 "Review"

# Изменение веса - O(log(N))
ZINCRBY "student:M8O-213B-21:15:tasks_w_priority" 100 "Review"

# Удаление элемента - O(log(N))
ZREM "student:M8O-213B-21:15:tasks_w_priority" "Review"
```

**Асимптотическая сложность ZSET операций:**
- **ZADD/ZREM/ZSCORE**: O(log(N)) - основные операции
- **ZRANGE/ZREVRANGE**: O(log(N)+M) - где M - количество возвращаемых элементов
- **ZRANK/ZREVRANK**: O(log(N)) - получение позиции элемента
- **ZCOUNT**: O(log(N)) - подсчет элементов в диапазоне весов
- **ZUNIONSTORE/ZINTERSTORE**: O(N)+O(M*log(M)) - операции между множествами

### 12. Как реализуется работа с Redis в вашем приложении?

#### **Архитектура взаимодействия:**

```csharp
// 1. Подключение к Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString ?? "localhost:6379");
});
```

#### **Абстракция через ICacheService:**

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
}
```

#### **Реализация RedisCacheService:**

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IServer _server;

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? JsonConvert.DeserializeObject<T>(value!) : null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonConvert.SerializeObject(value);
        await _database.StringSetAsync(key, json, expiration);
    }
}
```

#### **Особенности реализации:**
- **JSON сериализация** - для сложных объектов
- **Graceful degradation** - обработка ошибок без падения приложения
- **Singleton connection** - один IConnectionMultiplexer на приложение
- **Dependency Injection** - через интерфейс ICacheService

### 13. Как реализуется стратегия cache-aside в вашем приложении?

#### **Паттерн Cache-Aside в ProductRepository:**

```csharp
public async Task<IEnumerable<Product>> GetAllAsync()
{
    // 1. Проверка наличия в кеше
    var cached = await _cache.GetAsync<List<Product>>(ALL_PRODUCTS_KEY);
    if (cached != null)
    {
        _logger.LogInformation("Cache HIT for all products");
        return cached; // Возврат из кеша
    }

    // 2. Cache Miss - загрузка из БД
    _logger.LogInformation("Cache MISS for all products");
    var products = await _context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .ToListAsync();

    // 3. Сохранение в кеш
    await _cache.SetAsync(ALL_PRODUCTS_KEY, products.ToList(), CacheExpiration);
    return products;
}
```

#### **Инвалидация кеша при изменениях:**

```csharp
public async Task<Product> UpdateAsync(Product product)
{
    // 1. Обновление в БД
    product.UpdatedAt = DateTime.UtcNow;
    _context.Products.Update(product);
    await _context.SaveChangesAsync();

    // 2. Инвалидация кеша
    _logger.LogInformation("Cache EVICT after updating product");
    await InvalidateCache();
    await _cache.RemoveAsync($"{CACHE_KEY_PREFIX}{product.Id}");

    return product;
}
```

#### **Когда применение Cache-Aside обосновано:**

**✅ Подходящие сценарии:**
- **Read-heavy приложения** - много чтений, мало записей
- **Толерантность к устаревшим данным** - eventual consistency приемлема
- **Предсказуемые паттерны доступа** - одни и те же данные запрашиваются часто
- **Дорогие вычисления** - сложные запросы к БД или внешним API

**❌ Неподходящие сценарии:**
- **Write-heavy системы** - частые обновления делают кеш неэффективным
- **Строгая консистентность** - требуется всегда актуальная информация
- **Уникальные запросы** - каждый запрос уникален, кеш не помогает
- **Малые объемы данных** - накладные расходы превышают выгоду

#### **Преимущества нашей реализации:**
- **Устойчивость к сбоям** - приложение работает даже при недоступности Redis
- **Гибкое управление TTL** - разное время жизни для разных типов данных
- **Наблюдаемость** - подробное логирование всех операций кеширования
- **Простота тестирования** - через интерфейс ICacheService