using MassTransit;
using Microsoft.EntityFrameworkCore;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Challenge;
using RobotCodingPlatform.Web.Features.Solution;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<SolutionAttempt> SolutionAttempts { get; set; }

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

        modelBuilder.Entity<SolutionAttempt>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Code)
                .IsRequired();

            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(a => a.Error)
                .HasMaxLength(2_000);

            entity.HasOne(a => a.Challenge)
                .WithMany()
                .HasForeignKey(a => a.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.ChallengeId, a.UserId, a.CreatedAt });
            entity.HasIndex(a => new { a.ChallengeId, a.Success, a.Iterations });
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}