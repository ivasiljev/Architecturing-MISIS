using JewelryStore.Core.Entities;

namespace JewelryStore.Core.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task UpdateStockAsync(int productId, int newStock);
    Task<bool> IsInStockAsync(int productId, int requiredQuantity);
}