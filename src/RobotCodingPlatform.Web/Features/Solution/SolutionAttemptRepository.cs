using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Solution;

public interface ISolutionAttemptRepository
{
    void Add(SolutionAttempt attempt);

    Task SaveChangesAsync(CancellationToken ct = default);

    Task<SolutionAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<SolutionAttempt?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<SolutionAttempt>> GetForUserAsync(
        Guid? challengeId,
        Guid userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<SolutionAttempt>> GetForChallengeAsync(
        Guid challengeId,
        CancellationToken ct = default);
}

public class SolutionAttemptRepository(AppDbContext context) : ISolutionAttemptRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Add(SolutionAttempt attempt)
    {
        _context.SolutionAttempts.Add(attempt);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public Task<SolutionAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.SolutionAttempts
            .AsNoTracking()
            .Include(a => a.Challenge)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public Task<SolutionAttempt?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.SolutionAttempts
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<SolutionAttempt>> GetForUserAsync(
        Guid? challengeId,
        Guid userId,
        CancellationToken ct = default)
    {
        var query = _context.SolutionAttempts
            .AsNoTracking()
            .Include(a => a.Challenge)
            .Where(a => a.UserId == userId);

        if (challengeId is not null)
        {
            query = query.Where(a => a.ChallengeId == challengeId);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SolutionAttempt>> GetForChallengeAsync(
        Guid challengeId,
        CancellationToken ct = default)
    {
        return await _context.SolutionAttempts
            .AsNoTracking()
            .Include(a => a.Challenge)
            .Where(a => a.ChallengeId == challengeId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
