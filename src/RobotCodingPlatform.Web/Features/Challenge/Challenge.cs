namespace RobotCodingPlatform.Web.Features.Challenge;

public class Challenge
{
    public Guid Id { get; init; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int GoalX { get; set; }
    public int GoalY { get; set; }
    public List<ChallengeObstacle> Obstacles { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ChallengeObstacle
{
    public int X { get; set; }
    public int Y { get; set; }
}
