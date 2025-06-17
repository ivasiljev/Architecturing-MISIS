using AutoMapper;
using BCrypt.Net;
using JewelryStore.Core.DTOs;
using JewelryStore.Core.Entities;
using JewelryStore.Core.Interfaces;

namespace JewelryStore.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IMapper mapper)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1),
            User = userDto
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await UserExistsAsync(registerDto.Username, registerDto.Email))
            return null;

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Phone = registerDto.Phone,
            Address = registerDto.Address,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.CreateAsync(user);

        var token = _jwtService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1),
            User = userDto
        };
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        return await _userRepository.ExistsAsync(username, email);
    }
}