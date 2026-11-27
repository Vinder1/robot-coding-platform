namespace RobotCodingPlatform.Web.Features.Statistics;

public record StatisticsOverviewDto(
    int TotalAttempts,
    int Succeeded,
    int Failed,
    int Pending,
    int UniqueUsers,
    int Challenges,
    double SuccessRate,
    DateTime? FirstAttemptAt,
    DateTime? LastAttemptAt);

public record ChallengeStatisticsDto(
    Guid ChallengeId,
    string ChallengeName,
    int TotalAttempts,
    int Succeeded,
    int Failed,
    int Pending,
    int UniqueUsers,
    double SuccessRate,
    DateTime? FirstAttemptAt,
    DateTime? LastAttemptAt);

public record SolutionAttemptStatisticsRowDto(
    Guid AttemptId,
    string ChallengeName,
    Guid UserId,
    string UserName,
    string UserEmail,
    string Status,
    bool? Success,
    string? Error,
    int? Iterations,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record StatisticsExportResult(byte[] Content, string FileName);

public record StatisticsReindexResult(bool Enabled, int Indexed, string? Message);
