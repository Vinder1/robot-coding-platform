using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.Users;

public interface IUserRepository
{
    public Task CreateAsync(User user);
    public Task<User?> FindAsync(Guid id);
    public Task<User?> FindByEmailAsync(string email);
    public Task<bool> ExistsWithEmailAsync(string email);
}

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsWithEmailAsync(string email)
    {
        return _context.Users
            .AsNoTracking()
            .AnyAsync(p => p.Email == email);
    }

    public async Task<User?> FindAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}