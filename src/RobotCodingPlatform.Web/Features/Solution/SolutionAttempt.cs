namespace RobotCodingPlatform.Web.Features.Solution;

public enum SolutionAttemptStatus
{
    Pending,
    Succeeded,
    Failed
}

public class SolutionAttempt
{
    public Guid Id { get; init; }
    public Guid ChallengeId { get; set; }
    public Challenge.Challenge? Challenge { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = null!;
    public SolutionAttemptStatus Status { get; set; } = SolutionAttemptStatus.Pending;
    public bool? Success { get; set; }
    public string? Error { get; set; }
    /// Used as score, the less, the better
    public int? Iterations { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
