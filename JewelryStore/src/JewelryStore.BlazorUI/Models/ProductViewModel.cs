namespace JewelryStore.BlazorUI.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Material { get; set; } = string.Empty;
    public string Gemstone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Size { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}