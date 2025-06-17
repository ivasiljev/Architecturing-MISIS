using JewelryStore.BlazorUI.Models;
using System.Text.Json;

namespace JewelryStore.BlazorUI.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("JewelryStoreAPI");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public async Task<List<ProductViewModel>> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null)
    {
        try
        {
            var url = $"api/products?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(category))
            {
                url += $"&category={Uri.EscapeDataString(category)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, _jsonOptions);

                return products ?? new List<ProductViewModel>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products: {ex.Message}");
        }

        return new List<ProductViewModel>();
    }

    public async Task<ProductViewModel?> GetProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(json, _jsonOptions);

                return product;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting product: {ex.Message}");
        }

        return null;
    }

    public async Task<List<ProductViewModel>> SearchProductsAsync(string query)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/search?query={Uri.EscapeDataString(query)}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, _jsonOptions);

                return products ?? new List<ProductViewModel>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching products: {ex.Message}");
        }

        return new List<ProductViewModel>();
    }

    public async Task<List<ProductViewModel>> GetAllProductsAsync()
    {
        try
        {
            // Получаем все товары (большое значение pageSize)
            var response = await _httpClient.GetAsync("api/products?page=1&pageSize=1000");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, _jsonOptions);

                return products ?? new List<ProductViewModel>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all products: {ex.Message}");
        }

        return new List<ProductViewModel>();
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id)
    {
        // Используем уже существующий метод GetProductAsync
        return await GetProductAsync(id);
    }
}