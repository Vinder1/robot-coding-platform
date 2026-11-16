namespace RobotCodingPlatform.Web.Features.Code;

public static class Extensions
{
    public static WebApplicationBuilder AddCodeServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddScoped<IRunCodeService, RunCodeService>();
        return builder;
    }
}