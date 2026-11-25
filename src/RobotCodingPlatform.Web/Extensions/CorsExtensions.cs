namespace RobotCodingPlatform.Web.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "Frontend";

    public static WebApplicationBuilder AddCustomCors(this WebApplicationBuilder builder)
    {
        var origins = GetAllowedOrigins(builder.Configuration);
        if (origins.Length == 0)
            return builder;

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }

    public static WebApplication UseCustomCors(this WebApplication app)
    {
        if (GetAllowedOrigins(app.Configuration).Length > 0)
            app.UseCors(PolicyName);

        return app;
    }

    private static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        return configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }
}
