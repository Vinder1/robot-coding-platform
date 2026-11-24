namespace RobotCodingPlatform.Web.Features.Challenge;

public record ChallengeObstacleDto(int X, int Y);

public record ChallengeDto(
    Guid Id,
    string Name,
    string Description,
    int Width,
    int Height,
    int StartX,
    int StartY,
    int GoalX,
    int GoalY,
    IReadOnlyList<ChallengeObstacleDto> Obstacles,
    DateTime CreatedAt);

public record ChallengeSummaryDto(
    Guid Id,
    string Name,
    int Width,
    int Height,
    int StartX,
    int StartY,
    int GoalX,
    int GoalY,
    int ObstacleCount,
    DateTime CreatedAt);

public static class ChallengeMappings
{
    public static ChallengeDto ToDto(this Challenge challenge) => new(
        challenge.Id,
        challenge.Name,
        challenge.Description,
        challenge.Width,
        challenge.Height,
        challenge.StartX,
        challenge.StartY,
        challenge.GoalX,
        challenge.GoalY,
        [.. challenge.Obstacles.Select(o => new ChallengeObstacleDto(o.X, o.Y))],
        challenge.CreatedAt);

    public static ChallengeSummaryDto ToSummaryDto(this Challenge challenge) => new(
        challenge.Id,
        challenge.Name,
        challenge.Width,
        challenge.Height,
        challenge.StartX,
        challenge.StartY,
        challenge.GoalX,
        challenge.GoalY,
        challenge.Obstacles.Count,
        challenge.CreatedAt);
}
