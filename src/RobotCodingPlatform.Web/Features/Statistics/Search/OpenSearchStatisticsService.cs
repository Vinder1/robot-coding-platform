using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using RobotCodingPlatform.Web.Extensions;

namespace RobotCodingPlatform.Web.Features.Statistics.Search;

public sealed class OpenSearchStatisticsService(
    OpenSearchClient client,
    IOptions<OpenSearchOptions> options,
    ILogger<OpenSearchStatisticsService> logger) : IStatisticsSearchService
{
    private readonly OpenSearchOptions _options = options.Value;
    private readonly SemaphoreSlim _indexGate = new(1, 1);
    private volatile bool _indexReady;

    public bool Enabled => true;

    public async Task<bool> EnsureIndexAsync(CancellationToken ct = default)
    {
        if (_indexReady)
            return true;

        await _indexGate.WaitAsync(ct);
        try
        {
            if (_indexReady)
                return true;

            var exists = await client.Indices.ExistsAsync(
                _options.StatisticsIndex,
                descriptor => descriptor,
                ct);
            if (exists.Exists)
            {
                _indexReady = true;
                return true;
            }

            var response = await client.Indices.CreateAsync(
                _options.StatisticsIndex,
                descriptor => descriptor
                    .Settings(settings => settings
                        .NumberOfShards(_options.NumberOfShards)
                        .NumberOfReplicas(_options.NumberOfReplicas))
                    .Map<SolutionAttemptStatisticsDocument>(mapping => mapping
                        .Properties(properties => properties
                            .Keyword(keyword => keyword.Name(document => document.AttemptId))
                            .Keyword(keyword => keyword.Name(document => document.ChallengeId))
                            .Text(text => text
                                .Name(document => document.ChallengeName)
                                .Fields(fields => fields
                                    .Keyword(keyword => keyword.Name("keyword"))))
                            .Keyword(keyword => keyword.Name(document => document.UserId))
                            .Text(text => text
                                .Name(document => document.UserName)
                                .Fields(fields => fields
                                    .Keyword(keyword => keyword.Name("keyword"))))
                            .Keyword(keyword => keyword.Name(document => document.Status))
                            .Boolean(boolean => boolean.Name(document => document.Success))
                            .Text(text => text.Name(document => document.Error))
                            .Date(date => date.Name(document => document.CreatedAt))
                            .Date(date => date.Name(document => document.CompletedAt))
                            .Number(number => number
                                .Name(document => document.DurationMs)
                                .Type(NumberType.Long)))),
                ct);

            if (!response.IsValid)
            {
                logger.LogWarning(
                    "Не удалось создать индекс OpenSearch {Index}: {Error}",
                    _options.StatisticsIndex,
                    response.DebugInformation);
                return false;
            }

            logger.LogInformation("Создан индекс OpenSearch {Index}", _options.StatisticsIndex);
            _indexReady = true;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "OpenSearch недоступен при создании индекса {Index}",
                _options.StatisticsIndex);
            return false;
        }
        finally
        {
            _indexGate.Release();
        }
    }

    public async Task<bool> IndexAsync(
        SolutionAttemptStatisticsDocument document,
        CancellationToken ct = default)
    {
        try
        {
            if (!await EnsureIndexAsync(ct))
                return false;

            var response = await client.IndexAsync(
                document,
                descriptor => descriptor
                    .Index(_options.StatisticsIndex)
                    .Id(document.AttemptId),
                ct);

            if (response.IsValid)
                return true;

            logger.LogWarning(
                "Не удалось проиндексировать попытку {AttemptId} в OpenSearch: {Error}",
                document.AttemptId,
                response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Ошибка OpenSearch при индексации попытки {AttemptId}",
                document.AttemptId);
            return false;
        }
    }

    public async Task<int> IndexManyAsync(
        IReadOnlyCollection<SolutionAttemptStatisticsDocument> documents,
        CancellationToken ct = default)
    {
        if (documents.Count == 0)
            return 0;

        try
        {
            if (!await EnsureIndexAsync(ct))
                return 0;

            var response = await client.BulkAsync(
                descriptor => descriptor
                    .Index(_options.StatisticsIndex)
                    .IndexMany(documents, (item, document) => item.Id(document.AttemptId)),
                ct);

            var indexed = response.Items.Count(item => item.Error is null);

            if (!response.IsValid || response.Errors)
            {
                logger.LogWarning(
                    "OpenSearch проиндексировал {Indexed} из {Total} попыток: {Error}",
                    indexed,
                    documents.Count,
                    response.DebugInformation);
            }

            return indexed;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка OpenSearch при массовой индексации попыток");
            return 0;
        }
    }

    public async Task<IReadOnlyList<SolutionAttemptStatisticsDocument>> SearchAsync(
        string? query,
        int take,
        CancellationToken ct = default)
    {
        try
        {
            if (!await EnsureIndexAsync(ct))
                return [];

            var size = Math.Clamp(take, 1, 200);

            var response = await client.SearchAsync<SolutionAttemptStatisticsDocument>(
                descriptor =>
                {
                    descriptor = descriptor
                        .Index(_options.StatisticsIndex)
                        .Size(size)
                        .Sort(sort => sort.Descending(document => document.CreatedAt));

                    return string.IsNullOrWhiteSpace(query)
                        ? descriptor.Query(search => search.MatchAll())
                        : descriptor.Query(search => search.MultiMatch(multiMatch => multiMatch
                            .Query(query)
                            .Fields(fields => fields
                                .Field(document => document.ChallengeName)
                                .Field(document => document.UserName)
                                .Field(document => document.Status))));
                },
                ct);

            if (!response.IsValid)
            {
                logger.LogWarning(
                    "Ошибка поиска в OpenSearch: {Error}",
                    response.DebugInformation);
                return [];
            }

            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка OpenSearch при поиске статистики");
            return [];
        }
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            if (!await EnsureIndexAsync(ct))
                return;

            var response = await client.Indices.RefreshAsync(
                _options.StatisticsIndex,
                descriptor => descriptor,
                ct);

            if (!response.IsValid)
            {
                logger.LogWarning(
                    "Не удалось обновить индекс OpenSearch {Index}: {Error}",
                    _options.StatisticsIndex,
                    response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ошибка OpenSearch при обновлении индекса");
        }
    }

    public async Task<OpenSearchStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var health = await client.Cluster.HealthAsync(
                Indices.All,
                descriptor => descriptor.Timeout($"{Math.Max(_options.ConnectionTimeoutSeconds, 1)}s"),
                ct);

            if (!health.IsValid)
            {
                return new OpenSearchStatusDto(
                    Enabled: true,
                    Healthy: false,
                    ClusterName: null,
                    Status: null,
                    Nodes: null,
                    Message: health.DebugInformation);
            }

            return new OpenSearchStatusDto(
                Enabled: true,
                Healthy: health.Status != HealthStatus.Red,
                ClusterName: health.ClusterName,
                Status: health.Status.ToString(),
                Nodes: health.NumberOfNodes,
                Message: null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenSearch недоступен при проверке статуса");
            return new OpenSearchStatusDto(
                Enabled: true,
                Healthy: false,
                ClusterName: null,
                Status: null,
                Nodes: null,
                Message: ex.Message);
        }
    }
}
