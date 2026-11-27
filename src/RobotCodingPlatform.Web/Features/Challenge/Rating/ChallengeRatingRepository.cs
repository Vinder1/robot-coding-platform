using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Challenge.Rating;

public interface IChallengeRatingRepository
{
    /// <summary>
    /// Возвращает рейтинг испытания: по одной лучшей успешной попытке на пользователя.
    /// Оценка — количество итераций интерпретатора, чем меньше, тем лучше.
    /// </summary>
    Task<ChallengeRatingDto?> GetAsync(
        Guid challengeId,
        int limit,
        CancellationToken ct = default);
}

public class ChallengeRatingRepository(AppDbContext context) : IChallengeRatingRepository
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 100;

    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<ChallengeRatingDto?> GetAsync(
        Guid challengeId,
        int limit,
        CancellationToken ct = default)
    {
        var challenge = await _context.Challenges
            .AsNoTracking()
            .Where(c => c.Id == challengeId)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(ct);

        if (challenge is null)
        {
            return null;
        }

        // Загружаем только успешные попытки с посчитанной оценкой.
        // Группировка по пользователю выполняется в памяти: на одно испытание
        // приходится ограниченное число попыток, а так проще корректно
        // разрешить ничьи по времени завершения.
        var attempts = await _context.SolutionAttempts
            .AsNoTracking()
            .Where(a => a.ChallengeId == challengeId && a.Success == true && a.Iterations != null)
            .Select(a => new RatingAttempt(
                a.UserId,
                a.Iterations!.Value,
                a.CompletedAt,
                a.CreatedAt))
            .ToListAsync(ct);

        var bestPerUser = attempts
            .GroupBy(a => a.UserId)
            .Select(group => group
                .OrderBy(a => a.Iterations)
                .ThenBy(a => a.CompletedAt ?? a.CreatedAt)
                .First())
            .OrderBy(a => a.Iterations)
            .ThenBy(a => a.CompletedAt ?? a.CreatedAt)
            .ToList();

        var top = bestPerUser
            .Take(Math.Clamp(limit <= 0 ? DefaultLimit : limit, 1, MaxLimit))
            .ToList();

        var userIds = top.Select(a => a.UserId).ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.Name })
            .ToListAsync(ct);

        var names = users.ToDictionary(user => user.Id, user => user.Name);

        var entries = top
            .Select((attempt, index) => new ChallengeRatingEntryDto(
                Place: index + 1,
                UserId: attempt.UserId,
                UserName: names.GetValueOrDefault(attempt.UserId, "—"),
                Iterations: attempt.Iterations,
                CompletedAt: attempt.CompletedAt))
            .ToList();

        return new ChallengeRatingDto(
            challenge.Id,
            challenge.Name,
            bestPerUser.Count,
            entries);
    }

    private sealed record RatingAttempt(
        Guid UserId,
        int Iterations,
        DateTime? CompletedAt,
        DateTime CreatedAt);
}
