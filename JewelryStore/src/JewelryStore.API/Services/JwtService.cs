using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JewelryStore.Core.Entities;

namespace JewelryStore.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        var jwtSettings = _configuration.GetSection("JwtSettings");
        _secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = jwtSettings["Issuer"] ?? "JewelryStore";
        _audience = jwtSettings["Audience"] ?? "JewelryStore";
        _expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        // Диагностика настроек JWT
        Console.WriteLine("=== JWT SERVICE CONFIGURATION ===");
        Console.WriteLine($"SecretKey length: {_secretKey.Length}");
        Console.WriteLine($"Issuer: {_issuer}");
        Console.WriteLine($"Audience: {_audience}");
        Console.WriteLine($"ExpiryMinutes: {_expiryMinutes}");
        Console.WriteLine("=== END JWT SERVICE CONFIGURATION ===");
    }

    public string GenerateToken(User user)
    {
        Console.WriteLine($"=== GENERATING JWT TOKEN FOR USER {user.Id} ===");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new[]
        {
            new Claim("nameid", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        Console.WriteLine("Claims being added to token:");
        foreach (var claim in claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        Console.WriteLine($"Generated token: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");
        Console.WriteLine($"=== END GENERATING JWT TOKEN ===");

        return tokenString;
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
        catch
        {
            return null;
        }
    }
}