namespace RobotCodingPlatform.Web.Features.Users;

public class User
{
    public Guid Id { get; init; }
    public string Name { get; set; } = null!;
    public string Email { get; init; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}