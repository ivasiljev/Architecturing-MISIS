# Генерация тестовых данных для демонстрации бизнес-дашборда
# Этот скрипт создает активность на сайте для наполнения метрик

param(
    [int]$Duration = 300,  # Длительность в секундах (по умолчанию 5 минут)
    [string]$ApiUrl = "http://localhost:5257",
    [string]$UiUrl = "http://localhost:5216"
)

Write-Host "🎯 Генерация тестовых данных для бизнес-дашборда" -ForegroundColor Green
Write-Host "Длительность: $Duration секунд" -ForegroundColor Yellow
Write-Host "API URL: $ApiUrl" -ForegroundColor Yellow
Write-Host "UI URL: $UiUrl" -ForegroundColor Yellow
Write-Host ""

# Проверяем доступность сервисов
try {
    $apiHealth = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -TimeoutSec 5
    Write-Host "✅ API доступен" -ForegroundColor Green
} catch {
    Write-Host "❌ API недоступен: $ApiUrl" -ForegroundColor Red
    exit 1
}

try {
    $uiHealth = Invoke-RestMethod -Uri "$UiUrl/health" -Method Get -TimeoutSec 5
    Write-Host "✅ UI доступен" -ForegroundColor Green
} catch {
    Write-Host "❌ UI недоступен: $UiUrl" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Получаем список продуктов
try {
    $products = Invoke-RestMethod -Uri "$ApiUrl/api/products?pageSize=50" -Method Get
    Write-Host "📦 Загружено $($products.Count) продуктов" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Не удалось загрузить продукты" -ForegroundColor Red
    exit 1
}

if ($products.Count -eq 0) {
    Write-Host "❌ Нет продуктов для тестирования" -ForegroundColor Red
    exit 1
}

# Регистрируем тестового пользователя
$testUser = @{
    email = "testuser@jewelry.com"
    password = "Password123!"
    firstName = "Test"
    lastName = "User"
}

try {
    $registerResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/register" -Method Post -Body ($testUser | ConvertTo-Json) -ContentType "application/json"
    Write-Host "👤 Тестовый пользователь зарегистрирован" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Пользователь уже существует или ошибка регистрации" -ForegroundColor Yellow
}

# Логинимся
try {
    $loginData = @{
        email = $testUser.email
        password = $testUser.password
    }
    $loginResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" -Method Post -Body ($loginData | ConvertTo-Json) -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "🔑 Получен токен авторизации" -ForegroundColor Green
} catch {
    Write-Host "❌ Ошибка авторизации" -ForegroundColor Red
    exit 1
}

# Заголовки для авторизованных запросов
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host ""
Write-Host "🚀 Начинаем генерацию активности..." -ForegroundColor Green
Write-Host ""

$startTime = Get-Date
$endTime = $startTime.AddSeconds($Duration)
$requestCount = 0
$orderCount = 0
$viewCount = 0

# Категории для поиска
$categories = @("Кольца", "Серьги", "Ожерелья", "Браслеты", "Броши")
$searchQueries = @("золото", "серебро", "бриллиант", "изумруд", "рубин", "сапфир")

while ((Get-Date) -lt $endTime) {
    $remainingTime = ($endTime - (Get-Date)).TotalSeconds
    
    # Обновляем прогресс каждые 10 секунд
    if ($requestCount % 10 -eq 0) {
        Write-Host "⏱️ Осталось: $([math]::Round($remainingTime)) сек | Запросы: $requestCount | Просмотры: $viewCount | Заказы: $orderCount" -ForegroundColor Cyan
    }
    
    try {
        # 50% - просмотр продуктов
        if ((Get-Random -Maximum 100) -lt 50) {
            $randomProduct = $products | Get-Random
            $response = Invoke-RestMethod -Uri "$ApiUrl/api/products/$($randomProduct.id)" -Method Get -TimeoutSec 3
            $viewCount++
        }
        # 20% - поиск по категориям
        elseif ((Get-Random -Maximum 100) -lt 70) {
            $randomCategory = $categories | Get-Random
            $response = Invoke-RestMethod -Uri "$ApiUrl/api/products?category=$randomCategory" -Method Get -TimeoutSec 3
        }
        # 15% - текстовый поиск
        elseif ((Get-Random -Maximum 100) -lt 85) {
            $randomQuery = $searchQueries | Get-Random
            $response = Invoke-RestMethod -Uri "$ApiUrl/api/products/search?query=$randomQuery" -Method Get -TimeoutSec 3
        }
        # 15% - создание заказа
        else {
            $randomProducts = $products | Get-Random -Count (Get-Random -Minimum 1 -Maximum 4)
            $orderItems = @()
            
            foreach ($product in $randomProducts) {
                $orderItems += @{
                    productId = $product.id
                    quantity = Get-Random -Minimum 1 -Maximum 3
                }
            }
            
            $orderData = @{
                shippingAddress = "Тестовый адрес доставки, г. Москва"
                notes = "Тестовый заказ для демонстрации"
                items = $orderItems
            }
            
            try {
                $orderResponse = Invoke-RestMethod -Uri "$ApiUrl/api/orders" -Method Post -Body ($orderData | ConvertTo-Json -Depth 3) -Headers $headers -TimeoutSec 5
                $orderCount++
                Write-Host "🛒 Создан заказ #$($orderResponse.id) на сумму $($orderResponse.totalAmount)₽" -ForegroundColor Green
                
                # 10% шанс отменить заказ
                if ((Get-Random -Maximum 100) -lt 10) {
                    Start-Sleep -Seconds 2
                    $cancelResponse = Invoke-RestMethod -Uri "$ApiUrl/api/orders/$($orderResponse.id)/cancel" -Method Post -Headers $headers -TimeoutSec 5
                    Write-Host "❌ Заказ #$($orderResponse.id) отменен" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️ Ошибка создания заказа: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
        
        $requestCount++
        
        # Случайная задержка между запросами (0.5-2 секунды)
        Start-Sleep -Milliseconds (Get-Random -Minimum 500 -Maximum 2000)
        
    } catch {
        Write-Host "⚠️ Ошибка запроса: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "✅ Генерация данных завершена!" -ForegroundColor Green
Write-Host "📊 Статистика:" -ForegroundColor Cyan
Write-Host "   • Всего запросов: $requestCount" -ForegroundColor White
Write-Host "   • Просмотры товаров: $viewCount" -ForegroundColor White
Write-Host "   • Создано заказов: $orderCount" -ForegroundColor White
Write-Host "   • Длительность: $Duration секунд" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Теперь можно открыть бизнес-дашборд в Grafana:" -ForegroundColor Green
Write-Host "   http://localhost:3000" -ForegroundColor Yellow
Write-Host "   Логин: admin / Пароль: admin123" -ForegroundColor Yellow