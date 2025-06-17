using JewelryStore.BlazorUI.Models;

namespace JewelryStore.BlazorUI.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    UserViewModel? CurrentUser { get; }
    Task InitializeAsync();
    Task<bool> LoginAsync(LoginModel loginModel);
    Task<bool> RegisterAsync(RegisterModel registerModel);
    Task LogoutAsync();
    string? GetToken();
}