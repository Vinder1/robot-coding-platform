using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.User;

namespace RobotCodingPlatform.Web.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(p => p.Email);
        });
    }
}