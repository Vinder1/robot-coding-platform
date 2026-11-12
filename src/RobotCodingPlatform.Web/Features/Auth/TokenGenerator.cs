using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth;

public class TokenGenerator
{
    public static List<Claim> ConfigureUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        return claims;
    }

    public static string GenerateAccessToken(User user, JwtSettings settings)
    {
        var claims = ConfigureUserClaims(user);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
    
    public static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public static string HashRefreshToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}