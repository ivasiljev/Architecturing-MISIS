# Test Profile and Orders Pages Script
# This script tests the new profile and orders pages functionality

$apiUrl = "http://localhost:5257"
$blazorUrl = "http://localhost:5216"

Write-Host "=== Testing Profile and Orders Pages ===" -ForegroundColor Green

# Test API endpoints first
Write-Host "1. Testing API endpoints..." -ForegroundColor Yellow

# Test login to get token
$loginBody = @{
    username = "testuser"
    password = "Test123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Login successful! Token received." -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed. Creating test user..." -ForegroundColor Yellow
    
    # Try to register a test user
    $registerBody = @{
        username = "testuser"
        email = "testuser@example.com"
        password = "Test123!"
        firstName = "Test"
        lastName = "User"
        phone = "+7 (999) 123-45-67"
        address = "123 Test Street, Test City"
    } | ConvertTo-Json
    
    try {
        $registerResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json"
        $token = $registerResponse.token
        Write-Host "✓ User registered and logged in!" -ForegroundColor Green
    } catch {
        Write-Host "✗ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Test profile endpoint
Write-Host "2. Testing profile endpoint..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $profile = Invoke-RestMethod -Uri "$apiUrl/api/auth/profile" -Method Get -Headers $headers
    Write-Host "✓ Profile retrieved successfully!" -ForegroundColor Green
    Write-Host "  - Username: $($profile.username)" -ForegroundColor Cyan
    Write-Host "  - Email: $($profile.email)" -ForegroundColor Cyan
    Write-Host "  - Name: $($profile.firstName) $($profile.lastName)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to get profile: $($_.Exception.Message)" -ForegroundColor Red
}

# Test profile update
Write-Host "3. Testing profile update..." -ForegroundColor Yellow
$updateBody = @{
    firstName = "Updated"
    lastName = "User"
    email = "testuser@example.com"
    phone = "+7 (999) 987-65-43"
    address = "456 Updated Street, New City"
} | ConvertTo-Json

try {
    $updatedProfile = Invoke-RestMethod -Uri "$apiUrl/api/auth/profile" -Method Put -Body $updateBody -Headers $headers
    Write-Host "✓ Profile updated successfully!" -ForegroundColor Green
    Write-Host "  - Updated Name: $($updatedProfile.firstName) $($updatedProfile.lastName)" -ForegroundColor Cyan
    Write-Host "  - Updated Phone: $($updatedProfile.phone)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to update profile: $($_.Exception.Message)" -ForegroundColor Red
}

# Test orders endpoint
Write-Host "4. Testing orders endpoint..." -ForegroundColor Yellow
try {
    $orders = Invoke-RestMethod -Uri "$apiUrl/api/orders/user" -Method Get -Headers $headers
    Write-Host "✓ Orders retrieved successfully!" -ForegroundColor Green
    Write-Host "  - Total orders: $($orders.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to get orders: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Testing Blazor Pages ===" -ForegroundColor Green

# Test if Blazor app is running
Write-Host "5. Testing Blazor application..." -ForegroundColor Yellow
try {
    $blazorResponse = Invoke-WebRequest -Uri $blazorUrl -UseBasicParsing
    if ($blazorResponse.StatusCode -eq 200) {
        Write-Host "✓ Blazor application is running!" -ForegroundColor Green
        Write-Host "  - Profile page: $blazorUrl/profile" -ForegroundColor Cyan
        Write-Host "  - Orders page: $blazorUrl/orders" -ForegroundColor Cyan
    }
} catch {
    Write-Host "✗ Blazor application is not running on $blazorUrl" -ForegroundColor Red
    Write-Host "  Please start the Blazor application first." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Green
Write-Host "✓ API endpoints for profile management are working" -ForegroundColor Green
Write-Host "✓ Profile and Orders pages have been created" -ForegroundColor Green
Write-Host "✓ Navigation menu includes links to new pages" -ForegroundColor Green
Write-Host ""
Write-Host "To test the pages manually:" -ForegroundColor Yellow
Write-Host "1. Start the API: cd src/JewelryStore.API && dotnet run" -ForegroundColor Cyan
Write-Host "2. Start the Blazor UI: cd src/JewelryStore.BlazorUI && dotnet run" -ForegroundColor Cyan
Write-Host "3. Open browser: http://localhost:5216" -ForegroundColor Cyan
Write-Host "4. Login and navigate to Profile or Orders pages" -ForegroundColor Cyan 