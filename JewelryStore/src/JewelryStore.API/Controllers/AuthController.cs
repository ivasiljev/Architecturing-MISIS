using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JewelryStore.API.Services;
using JewelryStore.Core.DTOs;
using JewelryStore.Core.Interfaces;

namespace JewelryStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    /// <param name="loginDto">Данные для входа</param>
    /// <returns>JWT токен и информация о пользователе</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
            return Unauthorized(new { message = "Неверные учетные данные" });

        return Ok(result);
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="registerDto">Данные для регистрации</param>
    /// <returns>JWT токен и информация о пользователе</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userExists = await _authService.UserExistsAsync(registerDto.Username, registerDto.Email);
        if (userExists)
            return Conflict(new { message = "Пользователь с таким именем или email уже существует" });

        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
            return BadRequest(new { message = "Ошибка при регистрации" });

        return Ok(result);
    }

    /// <summary>
    /// Получение профиля текущего пользователя
    /// </summary>
    /// <returns>Информация о профиле пользователя</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("nameid")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Невалидный токен" });

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return Ok(profileDto);
    }

    /// <summary>
    /// Обновление профиля текущего пользователя
    /// </summary>
    /// <param name="updateProfileDto">Данные для обновления профиля</param>
    /// <returns>Результат обновления</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst("nameid")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Невалидный токен" });

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        // Проверяем, не используется ли email другим пользователем
        if (user.Email != updateProfileDto.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(updateProfileDto.Email);
            if (existingUser != null && existingUser.Id != userId)
                return BadRequest(new { message = "Email уже используется другим пользователем" });
        }

        // Обновляем данные пользователя
        user.FirstName = updateProfileDto.FirstName;
        user.LastName = updateProfileDto.LastName;
        user.Email = updateProfileDto.Email;
        user.Phone = updateProfileDto.Phone;
        user.Address = updateProfileDto.Address;

        await _userRepository.UpdateAsync(user);

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return Ok(profileDto);
    }
}