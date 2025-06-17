using JewelryStore.BlazorUI.Models;

namespace JewelryStore.BlazorUI.Services;

public interface IProductService
{
    Task<List<ProductViewModel>> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null);
    Task<ProductViewModel?> GetProductAsync(int id);
    Task<List<ProductViewModel>> SearchProductsAsync(string query);
    Task<List<ProductViewModel>> GetAllProductsAsync();
    Task<ProductViewModel?> GetProductByIdAsync(int id);
}