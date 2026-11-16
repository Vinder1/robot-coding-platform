namespace RobotCodingPlatform.Web.Features.RunScript;

public record RunScriptRequest(
    string Code,
    int Width,
    int Height,
    int GoalX,
    int GoalY,
    List<ObstacleDto>? Obstacles,
    int StartX,
    int StartY
);

public record ObstacleDto(int X, int Y);
