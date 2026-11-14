using RobotCodingPlatform.Web.Features.Profile.Update;

namespace RobotCodingPlatform.Web.Features.Profile;

public static class ProfileExtensions
{
    public static WebApplicationBuilder AddProfileServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUpdateProfileService, UpdateProfileService>();

        return builder;
    }
}