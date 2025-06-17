using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JewelryStore.Core.Entities;
using JewelryStore.Core.Interfaces;
using JewelryStore.Infrastructure.Data;

namespace JewelryStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly JewelryStoreContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductRepository> _logger;
    private const string CACHE_KEY_PREFIX = "product:";
    private const string ALL_PRODUCTS_KEY = "products:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public ProductRepository(JewelryStoreContext context, ICacheService cache, ILogger<ProductRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<List<Product>>(ALL_PRODUCTS_KEY);
        if (cached != null)
        {
            _logger.LogInformation("Cache HIT for all products - serving {Count} products from cache", cached.Count);
            return cached;
        }

        _logger.LogInformation("Cache MISS for all products - fetching from database");
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        await _cache.SetAsync(ALL_PRODUCTS_KEY, products.ToList(), CacheExpiration);
        _logger.LogInformation("Cached {Count} products for {Minutes} minutes", products.Count(), CacheExpiration.TotalMinutes);

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{id}";
        var cached = await _cache.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache HIT for product ID {ProductId} - serving from cache", id);
            return cached;
        }

        _logger.LogInformation("Cache MISS for product ID {ProductId} - fetching from database", id);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            await _cache.SetAsync(cacheKey, product, CacheExpiration);
            _logger.LogInformation("Cached product ID {ProductId} ({ProductName}) for {Minutes} minutes", id, product.Name, CacheExpiration.TotalMinutes);
        }
        else
        {
            _logger.LogWarning("Product ID {ProductId} not found in database", id);
        }

        return product;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _context.Products
            .Where(p => p.IsActive &&
                   (p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.Material.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await InvalidateCache();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cache EVICT after updating product ID {ProductId} ({ProductName})", product.Id, product.Name);
        await InvalidateCache();
        await _cache.RemoveAsync($"{CACHE_KEY_PREFIX}{product.Id}");

        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cache EVICT after deleting product ID {ProductId} ({ProductName})", id, product.Name);
            await InvalidateCache();
            await _cache.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent product ID {ProductId}", id);
        }
    }

    public async Task UpdateStockAsync(int productId, int newStock)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity = newStock;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"{CACHE_KEY_PREFIX}{productId}");
            await InvalidateCache();
        }
    }

    public async Task<bool> IsInStockAsync(int productId, int requiredQuantity)
    {
        var product = await GetByIdAsync(productId);
        return product != null && product.StockQuantity >= requiredQuantity;
    }

    private async Task InvalidateCache()
    {
        _logger.LogInformation("Cache INVALIDATION - clearing all product caches");
        await _cache.RemoveAsync(ALL_PRODUCTS_KEY);
        await _cache.RemoveByPatternAsync($"{CACHE_KEY_PREFIX}*");
    }
}