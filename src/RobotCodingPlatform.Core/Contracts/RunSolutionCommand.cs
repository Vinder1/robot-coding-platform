namespace RobotCodingPlatform.Core.Contracts;

public record RunSolutionCommand(
    Guid AttemptId,
    string Code,
    int Width,
    int Height,
    MapPoint Goal,
    List<MapPoint> Obstacles,
    int StartX,
    int StartY
);
