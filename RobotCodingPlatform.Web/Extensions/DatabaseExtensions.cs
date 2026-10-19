using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Extensions;

public static class DatabaseExtensions
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    }

    public static void Migrate(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied");
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Database migration failed, application cannot start");
            throw;
        }
    }
}