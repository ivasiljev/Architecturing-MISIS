using JewelryStore.BlazorUI.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace JewelryStore.BlazorUI.Services;

public class CartService : ICartService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Cart _cart = new();
    private readonly IOrderService _orderService;
    private const string CART_KEY = "jewelrystore_cart";

    public event Action? OnCartChanged;
    public Cart Cart => _cart;

    public CartService(IJSRuntime jsRuntime, IOrderService orderService)
    {
        _jsRuntime = jsRuntime;
        _orderService = orderService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Инициализация будет происходить в MainLayout
    }

    public async Task InitializeAsync()
    {
        try
        {
            var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CART_KEY);

            if (!string.IsNullOrEmpty(cartJson))
            {
                var savedCart = JsonSerializer.Deserialize<Cart>(cartJson, _jsonOptions);
                if (savedCart?.Items != null)
                {
                    _cart.Items.Clear();
                    _cart.Items.AddRange(savedCart.Items);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки корзины: {ex.Message}");
        }
    }

    public async Task AddToCartAsync(ProductViewModel product, int quantity = 1)
    {
        try
        {
            var existingItem = _cart.Items.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name ?? "",
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = GetProductImage(product),
                    Material = product.Material ?? "",
                    Gemstone = product.Gemstone,
                    Category = product.Category ?? ""
                });
            }

            await SaveCartAsync();
            OnCartChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка добавления в корзину: {ex.Message}");
        }
    }

    public async Task RemoveFromCartAsync(int productId)
    {
        try
        {
            var item = _cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _cart.Items.Remove(item);
                await SaveCartAsync();
                OnCartChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления из корзины: {ex.Message}");
        }
    }

    public async Task UpdateQuantityAsync(int productId, int quantity)
    {
        try
        {
            var item = _cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    await RemoveFromCartAsync(productId);
                }
                else
                {
                    item.Quantity = quantity;
                    await SaveCartAsync();
                    OnCartChanged?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обновления количества: {ex.Message}");
        }
    }

    public async Task ClearCartAsync()
    {
        try
        {
            _cart.Items.Clear();
            await SaveCartAsync();
            OnCartChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка очистки корзины: {ex.Message}");
        }
    }

    public async Task<OrderResult> CheckoutAsync(CheckoutModel checkoutModel)
    {
        try
        {
            if (_cart.IsEmpty)
            {
                return new OrderResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Корзина пуста"
                };
            }

            // Создаем запрос для API
            var createOrderRequest = new CreateOrderRequest
            {
                FirstName = checkoutModel.FirstName,
                LastName = checkoutModel.LastName,
                Email = checkoutModel.Email,
                Phone = checkoutModel.Phone,
                Address = checkoutModel.Address,
                City = checkoutModel.City,
                PostalCode = checkoutModel.PostalCode,
                PaymentMethod = checkoutModel.PaymentMethod,
                Notes = checkoutModel.Notes,
                Items = _cart.Items.Select(item => new OrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            // Отправляем заказ через API
            var result = await _orderService.CreateOrderAsync(createOrderRequest);

            if (result.IsSuccess)
            {
                // Очищаем корзину после успешного создания заказа
                await ClearCartAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка оплаты: {ex.Message}");
            return new OrderResult
            {
                IsSuccess = false,
                ErrorMessage = "Произошла ошибка при обработке платежа. Попробуйте позже."
            };
        }
    }

    private async Task SaveCartAsync()
    {
        try
        {
            var cartJson = JsonSerializer.Serialize(_cart, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CART_KEY, cartJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения корзины: {ex.Message}");
        }
    }

    private string GetProductImage(ProductViewModel product)
    {
        // Используем ту же логику генерации изображений, что и в каталоге
        var seed = Math.Abs(product.Name?.GetHashCode() ?? product.Id.GetHashCode()) % 1000;
        return $"https://picsum.photos/300/200?random={seed}";
    }
}