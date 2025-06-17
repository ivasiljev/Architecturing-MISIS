using JewelryStore.BlazorUI.Models;

namespace JewelryStore.BlazorUI.Services;

public interface ICartService
{
    event Action? OnCartChanged;
    Cart Cart { get; }
    Task AddToCartAsync(ProductViewModel product, int quantity = 1);
    Task RemoveFromCartAsync(int productId);
    Task UpdateQuantityAsync(int productId, int quantity);
    Task ClearCartAsync();
    Task<OrderResult> CheckoutAsync(CheckoutModel checkoutModel);
}