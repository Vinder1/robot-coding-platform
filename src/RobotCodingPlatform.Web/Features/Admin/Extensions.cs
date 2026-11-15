namespace RobotCodingPlatform.Web.Features.Admin;

public static class AdminExtensions
{
    public static void AddAdminServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
    }
}

