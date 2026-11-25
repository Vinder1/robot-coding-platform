using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace RobotCodingPlatform.Web.Extensions;

public sealed class RateLimitingOptions
{
    public int GlobalPermitLimit { get; set; } = 600;
    public int GlobalWindowSeconds { get; set; } = 60;
    public int CodeExecutionPermitLimit { get; set; } = 30;
    public int CodeExecutionWindowSeconds { get; set; } = 60;
    public int AuthPermitLimit { get; set; } = 20;
    public int AuthWindowSeconds { get; set; } = 60;
}

public static class RateLimitingExtensions
{
    public const string CodeExecutionPolicy = "code-execution";
    public const string AuthPolicy = "auth";

    public static WebApplicationBuilder AddCustomRateLimiting(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        builder.Services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Статика и SPA-файлы не ограничиваем.
                if (!context.Request.Path.StartsWithSegments("/api"))
                    return RateLimitPartition.GetNoLimiter("non-api");

                return CreatePartition(
                    context,
                    options.GlobalPermitLimit,
                    options.GlobalWindowSeconds);
            });

            limiter.AddPolicy(CodeExecutionPolicy, context => CreatePartition(
                context,
                options.CodeExecutionPermitLimit,
                options.CodeExecutionWindowSeconds));

            limiter.AddPolicy(AuthPolicy, context => CreatePartition(
                context,
                options.AuthPermitLimit,
                options.AuthWindowSeconds));
        });

        return builder;
    }

    public static WebApplication UseCustomRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        int permitLimit,
        int windowSeconds)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            GetPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(permitLimit, 1),
                Window = TimeSpan.FromSeconds(Math.Max(windowSeconds, 1)),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            return $"user:{userId}";

        var ip = context.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ip) ? "anonymous" : $"ip:{ip}";
    }
}
