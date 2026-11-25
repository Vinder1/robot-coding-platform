namespace RobotCodingPlatform.Web.Features.Statistics.Search;

public interface IStatisticsSearchService
{
    bool Enabled { get; }

    Task<bool> EnsureIndexAsync(CancellationToken ct = default);

    Task<bool> IndexAsync(SolutionAttemptStatisticsDocument document, CancellationToken ct = default);

    Task<int> IndexManyAsync(
        IReadOnlyCollection<SolutionAttemptStatisticsDocument> documents,
        CancellationToken ct = default);

    Task RefreshAsync(CancellationToken ct = default);

    Task<IReadOnlyList<SolutionAttemptStatisticsDocument>> SearchAsync(
        string? query,
        int take,
        CancellationToken ct = default);

    Task<OpenSearchStatusDto> GetStatusAsync(CancellationToken ct = default);
}

public sealed class DisabledStatisticsSearchService : IStatisticsSearchService
{
    public bool Enabled => false;

    public Task<bool> EnsureIndexAsync(CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> IndexAsync(
        SolutionAttemptStatisticsDocument document,
        CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<int> IndexManyAsync(
        IReadOnlyCollection<SolutionAttemptStatisticsDocument> documents,
        CancellationToken ct = default)
        => Task.FromResult(0);

    public Task RefreshAsync(CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<SolutionAttemptStatisticsDocument>> SearchAsync(
        string? query,
        int take,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SolutionAttemptStatisticsDocument>>([]);

    public Task<OpenSearchStatusDto> GetStatusAsync(CancellationToken ct = default)
        => Task.FromResult(new OpenSearchStatusDto(
            Enabled: false,
            Healthy: false,
            ClusterName: null,
            Status: null,
            Nodes: null,
            Message: "OpenSearch отключён в конфигурации."));
}
