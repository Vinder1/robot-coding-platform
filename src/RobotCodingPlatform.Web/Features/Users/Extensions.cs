namespace RobotCodingPlatform.Web.Features.Users;

public static class UserExtensions
{
    public static WebApplicationBuilder AddUserServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        return builder;
    }
}