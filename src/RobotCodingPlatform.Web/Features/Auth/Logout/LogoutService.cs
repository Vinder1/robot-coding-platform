namespace RobotCodingPlatform.Web.Features.Auth.Logout;

public interface ILogoutService
{
    Task LogoutAsync(string refreshToken, Guid userId, CancellationToken ct = default);
}

public class LogoutService(
    ITokenService tokenService,
    ILogger<LogoutService> logger
) : ILogoutService
{
    public async Task LogoutAsync(string refreshToken, Guid userId, CancellationToken ct = default)
    {
        await tokenService.RevokeRefreshTokenAsync(refreshToken, ct);
        logger.LogInformation("Пользователь {UserId} успешно вышел.", userId);
    }
}