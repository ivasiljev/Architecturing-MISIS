using JewelryStore.BlazorUI.Models;
using System.Text.Json;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JewelryStore.BlazorUI.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IJSRuntime _jsRuntime;
    private UserViewModel? _currentUser;
    private string? _jwtToken;
    private const string TOKEN_KEY = "jewelrystore_token";
    private const string TOKEN_EXPIRY_KEY = "jewelrystore_token_expiry";

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("JewelryStoreAPI");
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_jwtToken);
    public UserViewModel? CurrentUser => _currentUser;

    public string? GetToken() => _jwtToken;

    public async Task InitializeAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
            var expiryString = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_EXPIRY_KEY);

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(expiryString))
            {
                if (DateTime.TryParse(expiryString, out var expiry) && expiry > DateTime.UtcNow)
                {
                    // Токен еще действителен
                    _jwtToken = token;
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    // Извлекаем информацию о пользователе из токена
                    _currentUser = ExtractUserFromToken(token);
                }
                else
                {
                    // Токен истек, очищаем
                    await ClearStoredTokenAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации авторизации: {ex.Message}");
            await ClearStoredTokenAsync();
        }
    }

    public async Task<bool> LoginAsync(LoginModel loginModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(loginModel, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseModel>(responseJson, _jsonOptions);

                if (loginResponse?.Token != null)
                {
                    _jwtToken = loginResponse.Token;
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

                    // Сохраняем токен в localStorage с истечением через 1 час
                    var expiry = DateTime.UtcNow.AddHours(1);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, _jwtToken);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_EXPIRY_KEY, expiry.ToString("O"));

                    // Извлекаем информацию о пользователе из токена
                    _currentUser = ExtractUserFromToken(_jwtToken);

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка входа: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> RegisterAsync(RegisterModel registerModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(registerModel, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                // После успешной регистрации можно сразу войти
                var loginModel = new LoginModel
                {
                    Username = registerModel.Username,
                    Password = registerModel.Password
                };
                return await LoginAsync(loginModel);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка регистрации: {ex.Message}");
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _jwtToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;

        await ClearStoredTokenAsync();
    }

    private async Task ClearStoredTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_EXPIRY_KEY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка очистки токена: {ex.Message}");
        }
    }

    private UserViewModel? ExtractUserFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub" || x.Type == "nameid");
            var usernameClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "unique_name" || x.Type == "name");
            var firstNameClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "given_name");
            var lastNameClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "family_name");
            var emailClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "email");

            if (userIdClaim != null && usernameClaim != null)
            {
                return new UserViewModel
                {
                    Id = int.TryParse(userIdClaim.Value, out var id) ? id : 0,
                    Username = usernameClaim.Value,
                    FirstName = firstNameClaim?.Value ?? "",
                    LastName = lastNameClaim?.Value ?? "",
                    Email = emailClaim?.Value ?? ""
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка извлечения пользователя из токена: {ex.Message}");
        }

        return null;
    }
}

public class LoginResponseModel
{
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
}