using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.Statistics.Search;

namespace RobotCodingPlatform.Web.Features.Statistics;

public static class OpenSearchExtensions
{
    public static WebApplicationBuilder AddCustomOpenSearch(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OpenSearchOptions>(
            builder.Configuration.GetSection(OpenSearchOptions.SectionName));

        var options = builder.Configuration
            .GetSection(OpenSearchOptions.SectionName)
            .Get<OpenSearchOptions>() ?? new OpenSearchOptions();

        if (!options.Enabled)
        {
            builder.Services.AddSingleton<IStatisticsSearchService, DisabledStatisticsSearchService>();
            return builder;
        }

        var nodeUris = options.NodeUris
            .Where(uri => !string.IsNullOrWhiteSpace(uri))
            .Select(uri => new Uri(uri.Trim()))
            .ToArray();

        if (nodeUris.Length == 0)
        {
            throw new InvalidOperationException(
                "OpenSearch: NodeUris is required when OpenSearch is enabled.");
        }

        var pool = new StaticConnectionPool(nodeUris);

        var settings = new ConnectionSettings(pool)
            .DefaultIndex(options.StatisticsIndex)
            .RequestTimeout(TimeSpan.FromSeconds(Math.Max(options.RequestTimeoutSeconds, 1)))
            .PingTimeout(TimeSpan.FromSeconds(Math.Max(options.ConnectionTimeoutSeconds, 1)))
            .MaximumRetries(2);

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            settings.BasicAuthentication(options.Username, options.Password ?? string.Empty);
        }

        if (options.AllowInvalidCertificate)
        {
            settings.ServerCertificateValidationCallback((_, _, _, _) => true);
        }

        builder.Services.AddSingleton(new OpenSearchClient(settings));
        builder.Services.AddSingleton<IStatisticsSearchService, OpenSearchStatisticsService>();

        return builder;
    }

    public static async Task InitializeOpenSearchAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("OpenSearch");
        var options = app.Services.GetRequiredService<IOptions<OpenSearchOptions>>().Value;

        if (!options.Enabled)
        {
            logger.LogInformation("OpenSearch отключён в конфигурации");
            return;
        }

        var search = app.Services.GetRequiredService<IStatisticsSearchService>();

        if (await search.EnsureIndexAsync())
        {
            logger.LogInformation("OpenSearch готов, индекс статистики: {Index}", options.StatisticsIndex);
        }
        else
        {
            logger.LogWarning(
                "OpenSearch включён, но индекс {Index} не создан. Проверьте доступность и настройки.",
                options.StatisticsIndex);
        }
    }
}
