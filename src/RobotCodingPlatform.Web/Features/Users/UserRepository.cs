using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Users;

public interface IUserRepository
{
    public Task CreateAsync(User user);
    public Task<User?> FindActiveAsync(Guid id);
    public Task<User?> FindByEmailAsync(string email);
    public Task<bool> ExistsWithEmailAsync(string email);
    public Task<User?> UpdateNameAsync(Guid id, string name);
    public Task<User?> UpdateRoleAsync(Guid id, string role);
    public Task<User?> SoftDeleteAsync(Guid id);
}

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> FindActiveAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.DeletedAt != null)
        {
            return null;
        }
        return user;
    }

    public Task<bool> ExistsWithEmailAsync(string email)
    {
        return _context.Users
            .AsNoTracking()
            .AnyAsync(p => p.Email == email);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> UpdateNameAsync(Guid id, string name)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user != null)
        {
            user.Name = name;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return user;
        }

        return null;
    }

    public async Task<User?> UpdateRoleAsync(Guid id, string role)
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

    public async Task<User?> SoftDeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user != null)
        {
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return user;
        }

        return null;
    }
}
