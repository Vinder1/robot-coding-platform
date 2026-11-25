using Carter;
using RobotCodingPlatform.Web.Features.Statistics.Search;

namespace RobotCodingPlatform.Web.Features.Statistics;

public class StatisticsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/statistics")
            .WithTags("Statistics")
            .RequireAuthorization("RequireAdminRole");

        group.MapGet("/overview", async (
            DateTime? from,
            DateTime? to,
            IStatisticsService service,
            CancellationToken ct) =>
        {
            var overview = await service.GetOverviewAsync(from, to, ct);
            return Results.Ok(overview);
        })
        .WithSummary("Statistics overview (admin only)");

        group.MapGet("/challenges", async (
            Guid? challengeId,
            DateTime? from,
            DateTime? to,
            IStatisticsService service,
            CancellationToken ct) =>
        {
            var rows = await service.GetChallengesAsync(challengeId, from, to, ct);
            return Results.Ok(rows);
        })
        .WithSummary("Statistics grouped by challenge (admin only)");

        group.MapGet("/export", async (
            Guid? challengeId,
            DateTime? from,
            DateTime? to,
            bool? detail,
            IStatisticsService service,
            CancellationToken ct) =>
        {
            var export = await service.ExportAsync(
                challengeId,
                from,
                to,
                detail ?? false,
                ct);

            return Results.File(
                export.Content,
                "text/csv; charset=utf-8",
                export.FileName);
        })
        .WithSummary("Download statistics as CSV (admin only)");

        group.MapGet("/opensearch/status", async (
            IStatisticsSearchService search,
            CancellationToken ct) =>
        {
            var status = await search.GetStatusAsync(ct);
            return Results.Ok(status);
        })
        .WithSummary("OpenSearch status (admin only)");

        group.MapGet("/opensearch/search", async (
            string? query,
            int? take,
            IStatisticsSearchService search,
            CancellationToken ct) =>
        {
            var items = await search.SearchAsync(
                query,
                Math.Clamp(take ?? 50, 1, 200),
                ct);

            return Results.Ok(new
            {
                Enabled = search.Enabled,
                Items = items
            });
        })
        .WithSummary("Search indexed solution attempts in OpenSearch (admin only)");

        group.MapPost("/opensearch/reindex", async (
            IStatisticsService service,
            CancellationToken ct) =>
        {
            var result = await service.ReindexOpenSearchAsync(ct);

            return result.Enabled
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithSummary("Reindex solution attempts into OpenSearch (admin only)");
    }
}
