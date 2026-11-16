namespace RobotCodingPlatform.Web.Features.Code;

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class RunCodeParams
{
    public int Height { get; init; }
    public int Width { get; init; }
    public Point Goal { get; init; } = null!;
    public Point[] Obstacles { get; init; } = null!;

    public string Code { get; init; } = null!;
}