using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Features.User;

public interface IUserRepository
{
    public Task CreateAsync(User user);
    public Task<bool> ExistsWithEmail(string email);
}

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsWithEmail(string email)
    {
        return _context.Users
            .AsNoTracking()
            .AnyAsync(p => p.Email == email);
    }
}