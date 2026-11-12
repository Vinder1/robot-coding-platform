using Microsoft.Extensions.Options;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth;

public record struct NewToken(string Token, DateTime ExpiresAt);
public record TokenGenerationResult(NewToken NewAccessToken, NewToken NewRefreshToken);

public interface ITokenService
{
    Task<TokenGenerationResult> GenerateTokensAsync(User user, CancellationToken ct);
    Task<TokenGenerationResult> UpdateTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeAllTokensAsync(Guid userId, CancellationToken ct);
}

public class TokenService(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IOptions<JwtSettings> jwtSettings,
    ILogger<TokenService> logger
    ) : ITokenService
{
    public async Task<TokenGenerationResult> GenerateTokensAsync(User user, CancellationToken ct)
    {
        var accessToken = TokenGenerator.GenerateAccessToken(user, jwtSettings.Value);
        var refresh = await CreateRefreshTokenAsync(user, ct);

        return new TokenGenerationResult(new NewToken
        {
            Token = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes)
        }, refresh);
    }

    public async Task<TokenGenerationResult> UpdateTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh or access token is required");
        }

        var hashedToken = TokenGenerator.HashRefreshToken(refreshToken);
        var storedRefreshToken = await refreshTokenRepository.GetByHashedTokenAsync(hashedToken, ct);
        if (storedRefreshToken is not { IsValid: true })
        {
            logger.LogWarning("Refresh-токен {RefreshToken} недействителен", refreshToken);
            
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await userRepository.FindAsync(storedRefreshToken.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found or inactive");
        }
        
        await refreshTokenRepository.DeleteAsync(storedRefreshToken, ct);

        var result = await GenerateTokensAsync(user, ct);
        
        logger.LogInformation("Пользователь {UserId} успешно обновил токены.", user.Id);
        
        return result;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new Exception("Refresh token is required");
        }

        var hashedToken = TokenGenerator.HashRefreshToken(refreshToken);
        var token = await refreshTokenRepository.GetByHashedTokenAsync(hashedToken, ct);

        if (token == null)
        {
            logger.LogWarning("Попытка отзыва несуществующего refresh-токена");
            return;
        }

        logger.LogInformation("Refresh-токен пользователя {UserId} был отозван", token.UserId);
        await refreshTokenRepository.DeleteAsync(token, ct);
    }

    public async Task RevokeAllTokensAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await refreshTokenRepository.GetValidUserTokensAsync(userId, ct);

        logger.LogInformation("Отзывается {TokenCount} refresh-токенов пользователя {UserId}", tokens.Count, userId);

        foreach (var token in tokens)
        {
            await refreshTokenRepository.DeleteAsync(token, ct);
        }
    }

    private async Task<NewToken> CreateRefreshTokenAsync(User user, CancellationToken ct)
    {
        var token = TokenGenerator.GenerateRefreshToken();

        var tokenHash = TokenGenerator.HashRefreshToken(token);
        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays),
        };

        await refreshTokenRepository.StoreAsync(refreshToken, ct);
        return new NewToken { Token=token, ExpiresAt=refreshToken.ExpiresAt };
    }
}
