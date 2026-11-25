using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.Solution;
using RobotCodingPlatform.Web.Features.Statistics.Search;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Statistics;

public interface IStatisticsRepository
{
    Task<StatisticsOverviewDto> GetOverviewAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<IReadOnlyList<ChallengeStatisticsDto>> GetChallengeStatisticsAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<IReadOnlyList<SolutionAttemptStatisticsRowDto>> GetAttemptRowsAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<IReadOnlyList<SolutionAttemptStatisticsDocument>> GetDocumentsAsync(
        int skip,
        int take,
        Guid? challengeId,
        CancellationToken ct = default);
}

public class StatisticsRepository(AppDbContext context) : IStatisticsRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<StatisticsOverviewDto> GetOverviewAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = ApplyFilters(_context.SolutionAttempts.AsNoTracking(), from, to);

        var total = await query.CountAsync(ct);
        var succeeded = await query
            .CountAsync(a => a.Status == SolutionAttemptStatus.Succeeded, ct);
        var failed = await query
            .CountAsync(a => a.Status == SolutionAttemptStatus.Failed, ct);
        var pending = await query
            .CountAsync(a => a.Status == SolutionAttemptStatus.Pending, ct);
        var uniqueUsers = await query.Select(a => a.UserId).Distinct().CountAsync(ct);
        var challenges = await query.Select(a => a.ChallengeId).Distinct().CountAsync(ct);

        DateTime? firstAttemptAt = null;
        DateTime? lastAttemptAt = null;

        if (total > 0)
        {
            firstAttemptAt = await query.MinAsync(a => a.CreatedAt, ct);
            lastAttemptAt = await query.MaxAsync(a => a.CreatedAt, ct);
        }

        return new StatisticsOverviewDto(
            total,
            succeeded,
            failed,
            pending,
            uniqueUsers,
            challenges,
            CalculateSuccessRate(succeeded, total),
            firstAttemptAt,
            lastAttemptAt);
    }

    public async Task<IReadOnlyList<ChallengeStatisticsDto>> GetChallengeStatisticsAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = ApplyFilters(_context.SolutionAttempts.AsNoTracking(), from, to);

        if (challengeId is not null)
            query = query.Where(a => a.ChallengeId == challengeId);

        var rows = await query
            .Select(a => new
            {
                a.ChallengeId,
                ChallengeName = a.Challenge != null ? a.Challenge.Name : string.Empty,
                a.UserId,
                a.Status,
                a.CreatedAt
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(row => new { row.ChallengeId, row.ChallengeName })
            .Select(group =>
            {
                var total = group.Count();
                var succeeded = group.Count(row => row.Status == SolutionAttemptStatus.Succeeded);
                var failed = group.Count(row => row.Status == SolutionAttemptStatus.Failed);
                var pending = group.Count(row => row.Status == SolutionAttemptStatus.Pending);

                return new ChallengeStatisticsDto(
                    group.Key.ChallengeId,
                    group.Key.ChallengeName,
                    total,
                    succeeded,
                    failed,
                    pending,
                    group.Select(row => row.UserId).Distinct().Count(),
                    CalculateSuccessRate(succeeded, total),
                    group.Min(row => row.CreatedAt),
                    group.Max(row => row.CreatedAt));
            })
            .OrderByDescending(row => row.TotalAttempts)
            .ThenBy(row => row.ChallengeName)
            .ToList();
    }

    public async Task<IReadOnlyList<SolutionAttemptStatisticsRowDto>> GetAttemptRowsAsync(
        Guid? challengeId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = ApplyFilters(_context.SolutionAttempts.AsNoTracking(), from, to);

        if (challengeId is not null)
            query = query.Where(a => a.ChallengeId == challengeId);

        var rows = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                ChallengeName = a.Challenge != null ? a.Challenge.Name : string.Empty,
                a.UserId,
                a.Status,
                a.Success,
                a.Error,
                a.CreatedAt,
                a.CompletedAt
            })
            .ToListAsync(ct);

        var users = await GetUsersAsync(rows.Select(row => row.UserId), ct);

        return rows.Select(row =>
        {
            users.TryGetValue(row.UserId, out var user);

            return new SolutionAttemptStatisticsRowDto(
                row.Id,
                row.ChallengeName,
                row.UserId,
                user?.Name ?? "—",
                user?.Email ?? "—",
                row.Status.ToString(),
                row.Success,
                row.Error,
                row.CreatedAt,
                row.CompletedAt);
        }).ToList();
    }

    public async Task<IReadOnlyList<SolutionAttemptStatisticsDocument>> GetDocumentsAsync(
        int skip,
        int take,
        Guid? challengeId,
        CancellationToken ct = default)
    {
        var query = _context.SolutionAttempts.AsNoTracking();

        if (challengeId is not null)
            query = query.Where(a => a.ChallengeId == challengeId);

        var rows = await query
            .OrderBy(a => a.CreatedAt)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 1_000))
            .Select(a => new
            {
                a.Id,
                a.ChallengeId,
                ChallengeName = a.Challenge != null ? a.Challenge.Name : string.Empty,
                a.UserId,
                a.Status,
                a.Success,
                a.Error,
                a.CreatedAt,
                a.CompletedAt
            })
            .ToListAsync(ct);

        var users = await GetUsersAsync(rows.Select(row => row.UserId), ct);

        return rows.Select(row =>
        {
            users.TryGetValue(row.UserId, out var user);

            long? durationMs = row.CompletedAt is { } completedAt
                ? (long)(completedAt - row.CreatedAt).TotalMilliseconds
                : null;

            return new SolutionAttemptStatisticsDocument
            {
                AttemptId = row.Id.ToString(),
                ChallengeId = row.ChallengeId.ToString(),
                ChallengeName = row.ChallengeName,
                UserId = row.UserId.ToString(),
                UserName = user?.Name ?? "—",
                Status = row.Status.ToString(),
                Success = row.Success ?? false,
                Error = row.Error,
                CreatedAt = row.CreatedAt,
                CompletedAt = row.CompletedAt,
                DurationMs = durationMs
            };
        }).ToList();
    }

    private async Task<Dictionary<Guid, UserInfo>> GetUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var users = await _context.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.Name, user.Email })
            .ToListAsync(ct);

        return users.ToDictionary(
            user => user.Id,
            user => new UserInfo(user.Id, user.Name, user.Email));
    }

    private static IQueryable<SolutionAttempt> ApplyFilters(
        IQueryable<SolutionAttempt> query,
        DateTime? from,
        DateTime? to)
    {
        if (from is not null)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(a => a.CreatedAt <= to.Value);

        return query;
    }

    private static double CalculateSuccessRate(int succeeded, int total)
        => total == 0 ? 0 : Math.Round(succeeded * 100.0 / total, 2);

    private sealed record UserInfo(Guid Id, string Name, string Email);
}
