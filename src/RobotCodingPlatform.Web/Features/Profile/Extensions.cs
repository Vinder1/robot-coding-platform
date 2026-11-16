using RobotCodingPlatform.Web.Features.Profile.SoftDelete;
using RobotCodingPlatform.Web.Features.Profile.Update;
using RobotCodingPlatform.Web.Features.Profile.Get;

namespace RobotCodingPlatform.Web.Features.Profile;

public static class ProfileExtensions
{
    public static WebApplicationBuilder AddProfileServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUpdateProfileService, UpdateProfileService>();
        services.AddScoped<ISoftDeleteService, SoftDeleteService>();

        return builder;
    }
}