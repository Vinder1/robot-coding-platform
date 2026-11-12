using Microsoft.AspNetCore.Identity;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth.LogIn;

public interface ILogInService
{
    Task<LogInResponse> LoginAsync(LogInRequest request, CancellationToken ct = default);
}

public class LogInService(
    ITokenService tokenService,
    IUserRepository userRepository, 
    IPasswordHasher<User> passwordHasher,
    ILogger<LogInService> logger) : ILogInService
{
    public async Task<LogInResponse> LoginAsync(LogInRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Неудачная попытка входа. Пользователь не найден.");
            
            return new LogInResponse
            {
                Success = false,
                Error = "Invalid login or password"
            };
        }

        var pvResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (pvResult != PasswordVerificationResult.Success)
        {
            logger.LogWarning("Неудачная попытка входа для пользователя {UserId}. Неверный пароль.", user.Id);
            
            return new LogInResponse
            {
                Success = false,
                Error = "Invalid login or password"
            };
        }
        
        var result = await tokenService.GenerateTokensAsync(user, ct);
        
        logger.LogInformation("Пользователь {UserId} успешно аутентифицирован.", user.Id);
        
        return new LogInResponse
        {
            Success = true,
            NewAccessToken = result.NewAccessToken,
            NewRefreshToken = result.NewRefreshToken,
            User = new UserDto(user.Name)
        };
    }
}