# Test Order Creation Script
# This script tests if the Notes column issue has been resolved

$apiUrl = "http://localhost:5257"
$username = "newuser"
$password = "Test123!"

Write-Host "=== Testing Order Creation After Database Fix ===" -ForegroundColor Green

# Step 1: Login and get JWT token
Write-Host "1. Logging in as user: $username" -ForegroundColor Yellow
$loginBody = @{
    username = $username
    password = $password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Login successful! Token received." -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Get available products
Write-Host "2. Getting available products..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $products = Invoke-RestMethod -Uri "$apiUrl/api/products" -Method Get -Headers $headers
    Write-Host "✓ Found $($products.Count) products" -ForegroundColor Green
    
    if ($products.Count -eq 0) {
        Write-Host "✗ No products available to order!" -ForegroundColor Red
        exit 1
    }
    
    $firstProduct = $products[0]
    Write-Host "  - Using product: $($firstProduct.name) (ID: $($firstProduct.id))" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to get products: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Create an order (this should now work with the Notes column)
Write-Host "3. Creating test order..." -ForegroundColor Yellow
$orderBody = @{
    items = @(
        @{
            productId = $firstProduct.id
            quantity = 1
        }
    )
    shippingAddress = "123 Test Street, Test City, TC 12345"
    notes = "Test order to verify Notes column fix"
} | ConvertTo-Json

try {
    $orderResponse = Invoke-RestMethod -Uri "$apiUrl/api/orders" -Method Post -Body $orderBody -Headers $headers
    Write-Host "✓ Order created successfully!" -ForegroundColor Green
    Write-Host "  - Order ID: $($orderResponse.id)" -ForegroundColor Cyan
    Write-Host "  - Total Amount: $$($orderResponse.totalAmount)" -ForegroundColor Cyan
    Write-Host "  - Notes: $($orderResponse.notes)" -ForegroundColor Cyan
    Write-Host "  - Status: $($orderResponse.status)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Order creation failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
    exit 1
}

Write-Host "=== All Tests Passed! Database schema issue is resolved. ===" -ForegroundColor Green 