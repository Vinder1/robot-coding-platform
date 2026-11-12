using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Auth;

public class RefreshToken
{
    public string TokenHash { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsValid => DateTime.UtcNow < ExpiresAt;
}


public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashedTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<ICollection<RefreshToken>> GetValidUserTokensAsync(Guid userId, CancellationToken ct = default);
    Task StoreAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task DeleteAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task DeleteAllAsync(Guid userId, CancellationToken ct = default);
}


public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByHashedTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshToken, ct);
    }

    public async Task<ICollection<RefreshToken>> GetValidUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Where(rt => rt.UserId == userId && DateTime.UtcNow < rt.ExpiresAt)
            .ToListAsync(ct);
    }

    public async Task StoreAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        context.Remove(refreshToken);
        await context.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAllAsync(Guid userId, CancellationToken ct = default)
    {
        await context.RefreshTokens.Where(t => t.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}