using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryStore.Core.DTOs;
using JewelryStore.Core.Interfaces;
using AutoMapper;
using JewelryStore.Core.Entities;
using Prometheus;

namespace JewelryStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    // Business Metrics
    private static readonly Counter ProductViewsCounter = Metrics.CreateCounter(
        "jewelrystore_product_views_total", "Total product views", new[] { "product_id", "category" });

    private static readonly Counter ProductSearchCounter = Metrics.CreateCounter(
        "jewelrystore_product_searches_total", "Total product searches", new[] { "query_type" });

    public ProductsController(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить все продукты с пагинацией
    /// </summary>
    /// <param name="page">Номер страницы (начиная с 1)</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="category">Фильтр по категории (опционально)</param>
    /// <returns>Список продуктов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null)
    {
        var products = await _productRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(category))
        {
            products = products.Where(p => p.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
            ProductSearchCounter.WithLabels("category_filter").Inc();
        }

        var paginatedProducts = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(paginatedProducts);

        return Ok(productDtos);
    }

    /// <summary>
    /// Получить продукт по ID
    /// </summary>
    /// <param name="id">ID продукта</param>
    /// <returns>Продукт</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null || !product.IsActive)
            return NotFound(new { message = "Продукт не найден" });

        // Отслеживаем просмотр продукта
        ProductViewsCounter.WithLabels(product.Id.ToString(), product.Category).Inc();

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    /// <summary>
    /// Поиск продуктов по названию
    /// </summary>
    /// <param name="query">Поисковый запрос</param>
    /// <returns>Список найденных продуктов</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    public async Task<IActionResult> SearchProducts([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query))
            return BadRequest(new { message = "Поисковый запрос не может быть пустым" });

        // Отслеживаем поисковые запросы
        ProductSearchCounter.WithLabels("text_search").Inc();

        var products = await _productRepository.SearchAsync(query);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return Ok(productDtos);
    }

    /// <summary>
    /// Создать новый продукт (только для администраторов)
    /// </summary>
    /// <param name="createProductDto">Данные нового продукта</param>
    /// <returns>Созданный продукт</returns>
    [HttpPost]
    [Authorize] // В реальности здесь должна быть роль Admin
    [ProducesResponseType(typeof(ProductDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _mapper.Map<Product>(createProductDto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.CreateAsync(product);

        var productDto = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    /// <summary>
    /// Обновить продукт (только для администраторов)
    /// </summary>
    /// <param name="id">ID продукта</param>
    /// <param name="updateProductDto">Данные для обновления</param>
    /// <returns>Обновленный продукт</returns>
    [HttpPut("{id}")]
    [Authorize] // В реальности здесь должна быть роль Admin
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            return NotFound(new { message = "Продукт не найден" });

        _mapper.Map(updateProductDto, existingProduct);
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(existingProduct);

        var productDto = _mapper.Map<ProductDto>(existingProduct);
        return Ok(productDto);
    }

    /// <summary>
    /// Удалить продукт (только для администраторов)
    /// </summary>
    /// <param name="id">ID продукта</param>
    /// <returns>Результат удаления</returns>
    [HttpDelete("{id}")]
    [Authorize] // В реальности здесь должна быть роль Admin
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Продукт не найден" });

        // Мягкое удаление
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        return NoContent();
    }
}