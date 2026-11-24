using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Challenge;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Challenge> Challenges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(p => p.Email);
            entity.Property(p => p.Name)
                .HasMaxLength(50);
            entity.Property(p => p.Email)
                .HasMaxLength(254);
            entity.Property(p => p.Role)
                .HasMaxLength(10);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.TokenHash);
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Description)
                .HasMaxLength(10_000)
                .IsRequired();

            entity.OwnsMany(c => c.Obstacles, obstacle =>
            {
                obstacle.ToTable("ChallengeObstacles");
                obstacle.WithOwner().HasForeignKey("ChallengeId");
                obstacle.Property<int>("Id");
                obstacle.HasKey("Id");
            });
        });
    }
}