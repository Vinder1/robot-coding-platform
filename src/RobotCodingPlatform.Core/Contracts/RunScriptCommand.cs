namespace RobotCodingPlatform.Core.Contracts;

public record MapPoint(int X, int Y);

public record RunScriptCommand(
    string Code,
    int Width,
    int Height,
    MapPoint Goal,
    List<MapPoint> Obstacles,
    int StartX,
    int StartY
); // наверное стоит добавить отдельный проект с контрактами чтобы сюда их не пихать
