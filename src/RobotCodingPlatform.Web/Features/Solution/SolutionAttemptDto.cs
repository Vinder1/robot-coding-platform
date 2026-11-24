namespace RobotCodingPlatform.Web.Features.Solution;

public record SolutionAttemptSummaryDto(
    Guid Id,
    Guid ChallengeId,
    string ChallengeName,
    Guid UserId,
    string Status,
    bool? Success,
    string? Error,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record SolutionAttemptDto(
    Guid Id,
    Guid ChallengeId,
    string ChallengeName,
    Guid UserId,
    string Code,
    string Status,
    bool? Success,
    string? Error,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public static class SolutionAttemptMappings
{
    public static SolutionAttemptSummaryDto ToSummaryDto(this SolutionAttempt attempt) => new(
        attempt.Id,
        attempt.ChallengeId,
        attempt.Challenge?.Name ?? string.Empty,
        attempt.UserId,
        attempt.Status.ToString(),
        attempt.Success,
        attempt.Error,
        attempt.CreatedAt,
        attempt.CompletedAt);

    public static SolutionAttemptDto ToDto(this SolutionAttempt attempt) => new(
        attempt.Id,
        attempt.ChallengeId,
        attempt.Challenge?.Name ?? string.Empty,
        attempt.UserId,
        attempt.Code,
        attempt.Status.ToString(),
        attempt.Success,
        attempt.Error,
        attempt.CreatedAt,
        attempt.CompletedAt);
}
