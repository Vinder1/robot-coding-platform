using RobotCodingPlatform.Web.Features.Statistics.Search;

namespace RobotCodingPlatform.Web.Features.Statistics;

public interface IStatisticsService
{
    Task<StatisticsOverviewDto> GetOverviewAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<IReadOnlyList<ChallengeStatisticsDto>> GetChallengesAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<StatisticsExportResult> ExportAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        bool detail,
        CancellationToken ct = default);

    Task<StatisticsReindexResult> ReindexOpenSearchAsync(CancellationToken ct = default);
}

public class StatisticsService(
    IStatisticsRepository repository,
    IStatisticsSearchService search) : IStatisticsService
{
    private const int ReindexPageSize = 500;

    public Task<StatisticsOverviewDto> GetOverviewAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
        => repository.GetOverviewAsync(from, to, ct);

    public Task<IReadOnlyList<ChallengeStatisticsDto>> GetChallengesAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
        => repository.GetChallengeStatisticsAsync(challengeId, from, to, ct);

    public async Task<StatisticsExportResult> ExportAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        bool detail,
        CancellationToken ct = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        if (detail)
        {
            var rows = await repository.GetAttemptRowsAsync(challengeId, from, to, ct);

            return new StatisticsExportResult(
                StatisticsCsvBuilder.BuildDetail(rows),
                $"statistics-detail-{timestamp}.csv");
        }

        var summary = await repository.GetChallengeStatisticsAsync(challengeId, from, to, ct);

        return new StatisticsExportResult(
            StatisticsCsvBuilder.BuildSummary(summary),
            $"statistics-summary-{timestamp}.csv");
    }

    public async Task<StatisticsReindexResult> ReindexOpenSearchAsync(CancellationToken ct = default)
    {
        if (!search.Enabled)
            return new StatisticsReindexResult(false, 0, "OpenSearch отключён в конфигурации.");

        var indexed = 0;
        var total = 0;
        var skip = 0;

        while (true)
        {
            var documents = await repository.GetDocumentsAsync(skip, ReindexPageSize, null, ct);
            if (documents.Count == 0)
                break;

            total += documents.Count;
            var indexedThisPage = await search.IndexManyAsync(documents, ct);
            indexed += indexedThisPage;

            // Если кластер недоступен, не перебираем оставшиеся страницы впустую.
            if (indexedThisPage == 0)
                break;

            if (documents.Count < ReindexPageSize)
                break;

            skip += ReindexPageSize;
        }

        var message = total > 0 && indexed == 0
            ? "OpenSearch недоступен: ни одна попытка не была проиндексирована."
            : null;

        if (indexed > 0)
        {
            await search.RefreshAsync(ct);
        }

        return new StatisticsReindexResult(true, indexed, message);
    }
}
