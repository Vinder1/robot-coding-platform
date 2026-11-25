namespace RobotCodingPlatform.Core;

/// <summary>
/// Проверка карты перед запуском программы: границы, препятствия и достижимость цели.
/// </summary>
public static class MapValidation
{
    public const int MaxDimension = 1_000;
    public const int MaxObstacles = 10_000;

    private static readonly (int Dx, int Dy)[] Directions = [(0, 1), (1, 0), (0, -1), (-1, 0)];

    /// <summary>
    /// Возвращает текст ошибки или <c>null</c>, если карта корректна.
    /// </summary>
    public static string? Validate(
        int width,
        int height,
        (int X, int Y) start,
        (int X, int Y) goal,
        IEnumerable<(int X, int Y)>? obstacles)
    {
        if (width <= 0 || height <= 0)
            return "Map dimensions must be positive.";

        if (width > MaxDimension || height > MaxDimension)
            return $"Map dimensions must not exceed {MaxDimension}.";

        var obstacleSet = new HashSet<(int X, int Y)>();

        if (obstacles is not null)
        {
            foreach (var obstacle in obstacles)
            {
                if (!InBounds(width, height, obstacle))
                    return $"Obstacle ({obstacle.X}, {obstacle.Y}) is outside the map.";

                obstacleSet.Add(obstacle);

                if (obstacleSet.Count > MaxObstacles)
                    return $"Too many obstacles (limit {MaxObstacles}).";
            }
        }

        if (!InBounds(width, height, start))
            return $"Start point ({start.X}, {start.Y}) is outside the map.";

        if (!InBounds(width, height, goal))
            return $"Goal point ({goal.X}, {goal.Y}) is outside the map.";

        if (obstacleSet.Contains(start))
            return "Start point cannot be an obstacle.";

        if (obstacleSet.Contains(goal))
            return "Goal point cannot be an obstacle.";

        if (!IsReachable(width, height, start, goal, obstacleSet))
            return "Goal is unreachable from the start point.";

        return null;
    }

    /// <summary>
    /// Проверяет, что от стартовой точки до цели можно добраться по свободным клеткам.
    /// </summary>
    public static bool IsReachable(
        int width,
        int height,
        (int X, int Y) start,
        (int X, int Y) goal,
        IReadOnlySet<(int X, int Y)> obstacles)
    {
        if (width <= 0 || height <= 0)
            return false;

        if (!InBounds(width, height, start) || !InBounds(width, height, goal))
            return false;

        if (start == goal)
            return true;

        var visited = new bool[width, height];
        var queue = new Queue<(int X, int Y)>();

        visited[start.X, start.Y] = true;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            foreach (var (dx, dy) in Directions)
            {
                var nextX = x + dx;
                var nextY = y + dy;

                if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                    continue;

                if (visited[nextX, nextY] || obstacles.Contains((nextX, nextY)))
                    continue;

                if (nextX == goal.X && nextY == goal.Y)
                    return true;

                visited[nextX, nextY] = true;
                queue.Enqueue((nextX, nextY));
            }
        }

        return false;
    }

    private static bool InBounds(int width, int height, (int X, int Y) point)
        => point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height;
}
