namespace RobotCodingPlatform.Web.Features.Challenge.Create;

public class CreateChallengeRequest
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }
    public int StartX { get; init; }
    public int StartY { get; init; }
    public int GoalX { get; init; }
    public int GoalY { get; init; }
    public List<CreateChallengeObstacle> Obstacles { get; init; } = [];
}

public class CreateChallengeObstacle
{
    public int X { get; init; }
    public int Y { get; init; }
}
