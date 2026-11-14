using Microsoft.AspNetCore.Identity;
using RobotCodingPlatform.Web.Features.Auth.LogIn;
using RobotCodingPlatform.Web.Features.Auth.Logout;
using RobotCodingPlatform.Web.Features.Auth.SignIn;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth;

public static class AuthExtensions
{
    public static WebApplicationBuilder AddAuthServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddScoped<ILogInService, LogInService>();
        services.AddScoped<ILogoutService, LogoutService>();
        services.AddScoped<ITokenService, TokenService>();

        services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        return builder;
    }
}