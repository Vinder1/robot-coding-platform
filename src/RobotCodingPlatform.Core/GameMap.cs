namespace RobotCodingPlatform.Core;

public class GameMap(int width, int height, IEnumerable<(int x, int y)>? obstacles = null)
{
    private int Width { get; } = width;
    private int Height { get; } = height;
    private readonly HashSet<(int x, int y)> _obstacles = obstacles != null ? new HashSet<(int, int)>(obstacles) : [];

    private bool IsOutOfBounds(int x, int y) => x < 0 || x >= Width || y < 0 || y >= Height;
    private bool HasObstacle(int x, int y) => _obstacles.Contains((x, y));
    public bool IsPassable(int x, int y) => !IsOutOfBounds(x, y) && !HasObstacle(x, y);
}