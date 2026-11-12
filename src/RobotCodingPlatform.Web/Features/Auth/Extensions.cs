using Microsoft.AspNetCore.Identity;
using RobotCodingPlatform.Web.Features.Auth.LogIn;
using RobotCodingPlatform.Web.Features.Auth.Logout;
using RobotCodingPlatform.Web.Features.Auth.SignIn;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddScoped<ILogInService, LogInService>();
        services.AddScoped<ILogoutService, LogoutService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        return services;
    }
}