namespace JewelryStore.Core.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Material { get; set; } = string.Empty;
    public string? Gemstone { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Size { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Material { get; set; } = string.Empty;
    public string? Gemstone { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Size { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Material { get; set; }
    public string? Gemstone { get; set; }
    public string? Category { get; set; }
    public decimal? Weight { get; set; }
    public string? Size { get; set; }
    public int? StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
}