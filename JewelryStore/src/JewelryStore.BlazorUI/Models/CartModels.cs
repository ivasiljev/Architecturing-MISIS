namespace JewelryStore.BlazorUI.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Material { get; set; } = string.Empty;
    public string? Gemstone { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal TotalPrice => Price * Quantity;
}

public class Cart
{
    public List<CartItem> Items { get; set; } = new();
    public int TotalItems => Items.Sum(x => x.Quantity);
    public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
    public bool IsEmpty => !Items.Any();
}

public class CheckoutModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "card"; // card, cash
    public string? CardNumber { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardCvc { get; set; }
    public string? Notes { get; set; }
}

public class OrderResult
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}