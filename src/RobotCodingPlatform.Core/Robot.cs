namespace RobotCodingPlatform.Core;

public class Robot(GameMap map, int x = 0, int y = 0, ExecutionLimits? limits = null) : IRobot
{
    private enum Direction
    {
        North,
        East,
        South,
        West
    }

    private readonly ExecutionLimits _limits = limits ?? ExecutionLimits.Default;

    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;
    public int Steps { get; private set; }
    private Direction _currentDirection = Direction.North;
    
    public bool IsGoalReached => X == map.Goal.x && Y == map.Goal.y;
    
    public void ExecuteCommand(string name, IReadOnlyList<int> arguments)
    {
        switch (name.ToLower())
        {
            case "move":
                var steps = arguments.Count > 0 ? arguments[0] : 1;
                Move(steps);
                break;

            case "turn_left":
                Turn(-1);
                break;

            case "turn_right":
                Turn(1);
                break;
            
            default:
                throw new InvalidOperationException($"Unknown command '{name}'");
        }
    }

    public bool GetSensor(string name)
    {
        return name.ToLower() switch
        {
            "wall_forward" => IsWallAtRelative(0),
            "wall_left" => IsWallAtRelative(-1),
            "wall_right" => IsWallAtRelative(1),
            _ => throw new InvalidOperationException($"Unknown sensor '{name}'")
        };
    }

    private void Move(int steps)
    {
        if (steps < 0)
            throw new InvalidOperationException("Step count cannot be negative.");

        if (steps > _limits.MaxSteps - Steps)
            throw new ExecutionLimitExceededException(
                $"Execution limit exceeded: robot cannot make more than {_limits.MaxSteps} steps.");

        for (var i = 0; i < steps; i++)
        {
            var (dx, dy) = GetDelta(_currentDirection);
            var nextX = X + dx;
            var nextY = Y + dy;

            if (!map.IsPassable(nextX, nextY))
                throw new InvalidOperationException($"Robot crashed into a wall or obstacle at ({nextX}, {nextY})!");

            X = nextX;
            Y = nextY;
            Steps++;
        }
    }

    private void Turn(int delta)
    {
        _currentDirection = (Direction)(((int)_currentDirection + delta + 4) % 4);
    }
    
    private bool IsWallAtRelative(int relativeOffset)
    {
        var targetDirection = (Direction)(((int)_currentDirection + relativeOffset + 4) % 4);
    
        var (dx, dy) = GetDelta(targetDirection);
        var nextX = X + dx;
        var nextY = Y + dy;

        return !map.IsPassable(nextX, nextY);
    }

    private static (int dx, int dy) GetDelta(Direction direction) => direction switch
    {
        Direction.North => (0, 1),
        Direction.East => (1, 0),
        Direction.South => (0, -1),
        Direction.West => (-1, 0),
        _ => (0, 0)
    };
}