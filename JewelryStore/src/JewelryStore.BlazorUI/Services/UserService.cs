using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JewelryStore.BlazorUI.Models;

namespace JewelryStore.BlazorUI.Services;

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<UserViewModel?> GetCurrentUserAsync()
    {
        try
        {
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
                return null;

            var httpClient = _httpClientFactory.CreateClient("JewelryStoreAPI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync("api/auth/profile");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserViewModel>(content, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current user: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileModel model)
    {
        try
        {
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
                return false;

            var httpClient = _httpClientFactory.CreateClient("JewelryStoreAPI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync("api/auth/profile", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating profile: {ex.Message}");
            return false;
        }
    }
}