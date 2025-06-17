using System.ComponentModel.DataAnnotations;

namespace JewelryStore.BlazorUI.Models;

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class UpdateProfileModel
{
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Некорректный формат телефона")]
    [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "Адрес не должен превышать 500 символов")]
    public string? Address { get; set; }
}