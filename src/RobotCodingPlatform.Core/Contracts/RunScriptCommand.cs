namespace RobotCodingPlatform.Core.Contracts;

public record RunScriptCommand(
    string Code,
    int Width,
    int Height,
    (int x, int y) Goal,
    List<(int x, int y)> Obstacles,
    int StartX,
    int StartY
); // наверное стоит добавить отдельный проект с контрактами чтобы сюда их не пихать