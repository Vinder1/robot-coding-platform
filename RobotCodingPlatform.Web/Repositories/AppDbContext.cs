using Microsoft.EntityFrameworkCore;

namespace RobotCodingPlatform.Web.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}