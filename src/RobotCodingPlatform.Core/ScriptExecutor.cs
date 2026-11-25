namespace RobotCodingPlatform.Core;

public enum ScriptExecutionErrorKind
{
    None,
    Compilation,
    InvalidMap,
    Runtime,
    LimitExceeded
}

public record ScriptExecutionResult(
    bool Success,
    string? Error = null,
    ScriptExecutionErrorKind ErrorKind = ScriptExecutionErrorKind.None)
{
    /// <summary>
    /// <c>false</c> только при ошибке лексера/парсера. Ошибки карты и рантайма
    /// означают, что код был успешно разобран.
    /// </summary>
    public bool Compiled => ErrorKind != ScriptExecutionErrorKind.Compilation;
}

/// <summary>
/// Общий запуск программы робота: лексер → парсер → карта → робот → интерпретатор.
/// Используется и синхронной проверкой, и асинхронной попыткой решения.
/// </summary>
public static class ScriptExecutor
{
    public static ScriptExecutionResult Execute(
        string code,
        int width,
        int height,
        (int X, int Y) goal,
        IEnumerable<(int X, int Y)>? obstacles,
        int startX,
        int startY,
        ExecutionLimits? limits = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new ScriptExecutionResult(
                false,
                "Code cannot be empty.",
                ScriptExecutionErrorKind.Compilation);
        }

        List<Node> astNodes;

        try
        {
            var tokens = Lexer.Tokenize(code);
            astNodes = Parser.Parse(tokens);
        }
        catch (SyntaxError ex)
        {
            return new ScriptExecutionResult(
                false,
                ex.Message,
                ScriptExecutionErrorKind.Compilation);
        }
        catch (Exception ex)
        {
            return new ScriptExecutionResult(
                false,
                ex.Message,
                ScriptExecutionErrorKind.Compilation);
        }

        var obstacleList = obstacles?.ToList() ?? [];
        var mapError = MapValidation.Validate(
            width,
            height,
            (startX, startY),
            goal,
            obstacleList);

        if (mapError is not null)
        {
            return new ScriptExecutionResult(
                false,
                mapError,
                ScriptExecutionErrorKind.InvalidMap);
        }

        try
        {
            var map = new GameMap(width, height, goal, obstacleList);
            var robot = new Robot(map, startX, startY, limits);

            var interpreter = new Interpreter(robot, limits);
            interpreter.Interpret(astNodes);

            return new ScriptExecutionResult(
                robot.IsGoalReached,
                null,
                ScriptExecutionErrorKind.None);
        }
        catch (ExecutionLimitExceededException ex)
        {
            return new ScriptExecutionResult(
                false,
                ex.Message,
                ScriptExecutionErrorKind.LimitExceeded);
        }
        catch (Exception ex)
        {
            return new ScriptExecutionResult(
                false,
                ex.Message,
                ScriptExecutionErrorKind.Runtime);
        }
    }
}
