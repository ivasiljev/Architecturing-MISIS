using JewelryStore.Core.DTOs;

namespace JewelryStore.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<bool> UserExistsAsync(string username, string email);
}