using Microsoft.AspNetCore.Identity;
using RobotCodingPlatform.Web.Features.User.SignIn;

namespace RobotCodingPlatform.Web.Features.User;

public static class UserExtensions
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<SignInService>();
        return services;
    }
}