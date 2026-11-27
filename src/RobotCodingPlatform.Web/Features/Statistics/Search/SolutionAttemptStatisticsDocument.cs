namespace RobotCodingPlatform.Web.Features.Statistics.Search;

public sealed class SolutionAttemptStatisticsDocument
{
    public string AttemptId { get; set; } = "";
    public string ChallengeId { get; set; } = "";
    public string ChallengeName { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Status { get; set; } = "";
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int? Iterations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
}

public record OpenSearchStatusDto(
    bool Enabled,
    bool Healthy,
    string? ClusterName,
    string? Status,
    int? Nodes,
    string? Message);
