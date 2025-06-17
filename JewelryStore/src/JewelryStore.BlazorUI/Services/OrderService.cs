using JewelryStore.BlazorUI.Models;
using System.Text.Json;

namespace JewelryStore.BlazorUI.Services;

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAuthService _authService;

    public OrderService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("JewelryStoreAPI");
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    private void SetAuthorizationHeader()
    {
        if (_authService.IsAuthenticated)
        {
            var token = _authService.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    public async Task<OrderResult> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            SetAuthorizationHeader();

            // Дополнительная диагностика токена
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);

                    Console.WriteLine("=== ДИАГНОСТИКА JWT ТОКЕНА ===");
                    Console.WriteLine($"Issuer: {jsonToken.Issuer}");
                    Console.WriteLine($"Audience: {jsonToken.Audiences.FirstOrDefault()}");
                    Console.WriteLine($"Expires: {jsonToken.ValidTo}");
                    Console.WriteLine("Claims:");
                    foreach (var claim in jsonToken.Claims)
                    {
                        Console.WriteLine($"  {claim.Type}: {claim.Value}");
                    }
                    Console.WriteLine("=== КОНЕЦ ДИАГНОСТИКИ ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка чтения токена: {ex.Message}");
                }
            }

            // Преобразуем формат запроса к тому, что ожидает API
            var apiRequest = new ApiCreateOrderDto
            {
                ShippingAddress = $"{request.Address}, {request.City}" +
                    (!string.IsNullOrEmpty(request.PostalCode) ? $", {request.PostalCode}" : ""),
                Notes = $"Имя: {request.FirstName} {request.LastName}\nТелефон: {request.Phone}\nEmail: {request.Email}" +
                    (!string.IsNullOrEmpty(request.Notes) ? $"\nКомментарий: {request.Notes}" : "") +
                    $"\nСпособ оплаты: {request.PaymentMethod}",
                Items = request.Items.Select(item => new ApiCreateOrderItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var json = JsonSerializer.Serialize(apiRequest, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            Console.WriteLine($"Отправляем заказ: {json}");
            Console.WriteLine($"Authorization header: {_httpClient.DefaultRequestHeaders.Authorization}");

            var response = await _httpClient.PostAsync("api/orders", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var apiOrder = JsonSerializer.Deserialize<ApiOrderDto>(responseJson, _jsonOptions);

                if (apiOrder != null)
                {
                    // Генерируем номер заказа для совместимости
                    var orderNumber = $"ORD-{apiOrder.OrderDate:yyyyMMdd}-{apiOrder.Id.ToString().PadLeft(4, '0')}";

                    return new OrderResult
                    {
                        IsSuccess = true,
                        OrderId = apiOrder.Id,
                        OrderNumber = orderNumber,
                        TotalAmount = (decimal)apiOrder.TotalAmount,
                        OrderDate = apiOrder.OrderDate,
                        Status = apiOrder.Status.ToString()
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка создания заказа: {response.StatusCode} - {errorContent}");

                return new OrderResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Ошибка при создании заказа: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке заказа: {ex.Message}");
        }

        return new OrderResult
        {
            IsSuccess = false,
            ErrorMessage = "Произошла ошибка при создании заказа."
        };
    }

    public async Task<List<OrderViewModel>> GetUserOrdersAsync()
    {
        try
        {
            SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("api/orders/my");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiOrders = JsonSerializer.Deserialize<List<ApiOrderDto>>(json, _jsonOptions);

                if (apiOrders != null)
                {
                    return apiOrders.Select(apiOrder => new OrderViewModel
                    {
                        Id = apiOrder.Id,
                        OrderNumber = $"ORD-{apiOrder.OrderDate:yyyyMMdd}-{apiOrder.Id.ToString().PadLeft(4, '0')}",
                        OrderDate = apiOrder.OrderDate,
                        TotalAmount = (decimal)apiOrder.TotalAmount,
                        Status = apiOrder.Status.ToString(),
                        ShippingAddress = apiOrder.ShippingAddress ?? "",
                        Items = apiOrder.OrderItems?.Select(item => new OrderItemViewModel
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = item.Product?.Name ?? "",
                            Quantity = item.Quantity,
                            Price = (decimal)item.UnitPrice
                        }).ToList() ?? new List<OrderItemViewModel>()
                    }).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки заказов: {ex.Message}");
        }

        return new List<OrderViewModel>();
    }

    public async Task<OrderViewModel?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/orders/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiOrder = JsonSerializer.Deserialize<ApiOrderDto>(json, _jsonOptions);

                if (apiOrder != null)
                {
                    return new OrderViewModel
                    {
                        Id = apiOrder.Id,
                        OrderNumber = $"ORD-{apiOrder.OrderDate:yyyyMMdd}-{apiOrder.Id.ToString().PadLeft(4, '0')}",
                        OrderDate = apiOrder.OrderDate,
                        TotalAmount = (decimal)apiOrder.TotalAmount,
                        Status = apiOrder.Status.ToString(),
                        ShippingAddress = apiOrder.ShippingAddress ?? "",
                        Items = apiOrder.OrderItems?.Select(item => new OrderItemViewModel
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = item.Product?.Name ?? "",
                            Quantity = item.Quantity,
                            Price = (decimal)item.UnitPrice
                        }).ToList() ?? new List<OrderItemViewModel>()
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки заказа {orderId}: {ex.Message}");
        }

        return null;
    }
}

// DTOs для API совместимости
public class ApiCreateOrderDto
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<ApiCreateOrderItemDto> Items { get; set; } = new();
}

public class ApiCreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ApiOrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public double TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public ApiOrderStatus Status { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public List<ApiOrderItemDto>? OrderItems { get; set; }
}

public class ApiOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double TotalPrice { get; set; }
    public ApiProductDto? Product { get; set; }
}

public class ApiProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public enum ApiOrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}