using JewelryStore.BlazorUI.Models;

namespace JewelryStore.BlazorUI.Services;

public interface IUserService
{
    Task<UserViewModel?> GetCurrentUserAsync();
    Task<bool> UpdateProfileAsync(UpdateProfileModel model);
}