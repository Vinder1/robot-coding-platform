namespace RobotCodingPlatform.Core.Contracts;

public record SolutionAttemptCompleted(
    Guid AttemptId,
    bool Success,
    string? Error,
    DateTime CompletedAt
);
