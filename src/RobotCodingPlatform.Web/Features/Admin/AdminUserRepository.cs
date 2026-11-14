using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Admin;

public interface IAdminUserRepository
{
    Task<User?> FindByIdAsync(Guid id);
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
