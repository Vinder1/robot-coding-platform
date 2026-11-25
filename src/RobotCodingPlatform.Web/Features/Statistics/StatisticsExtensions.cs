namespace RobotCodingPlatform.Web.Features.Statistics;

public static class StatisticsExtensions
{
    public static WebApplicationBuilder AddStatisticsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        builder.Services.AddScoped<IStatisticsService, StatisticsService>();

        builder.AddCustomOpenSearch();

        return builder;
    }
}
