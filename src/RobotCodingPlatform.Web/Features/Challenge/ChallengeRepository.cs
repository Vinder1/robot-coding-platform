using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Challenge;

public interface IChallengeRepository
{
    Task<Challenge> CreateAsync(Challenge challenge, CancellationToken ct = default);
    Task<IReadOnlyList<Challenge>> GetAllAsync(CancellationToken ct = default);
    Task<Challenge?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public class ChallengeRepository(AppDbContext context) : IChallengeRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Challenge> CreateAsync(Challenge challenge, CancellationToken ct = default)
    {
        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync(ct);
        return challenge;
    }

    public async Task<IReadOnlyList<Challenge>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Challenges
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<Challenge?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.Challenges
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
