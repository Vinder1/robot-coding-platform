using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Admin;

public interface IAdminUserRepository
{
    Task<User?> FindByIdAsync(Guid id);
    Task<(IReadOnlyList<User> Items, int Total)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        bool includeDeleted,
        CancellationToken ct = default);
    Task<User?> RestoreAsync(Guid id);
    Task<User?> ChangeRoleAsync(Guid id, string role);
}


public class AdminUserRepository(AppDbContext context) : IAdminUserRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<User?> FindByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        return user;
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        bool includeDeleted,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!includeDeleted)
        {
            query = query.Where(u => u.DeletedAt == null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<User?> RestoreAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user != null && user.DeletedAt != null)
        {
            user.DeletedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        return null;
    }

    public async Task<User?> ChangeRoleAsync(Guid id, string role)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user != null)
        {
            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return user;
        }

        return null;
    }
}
