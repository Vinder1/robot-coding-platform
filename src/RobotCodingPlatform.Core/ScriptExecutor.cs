namespace RobotCodingPlatform.Core;

public record ScriptExecutionResult(bool Success, string? Error);

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
        int startY)
    {
        try
        {
            var tokens = Lexer.Tokenize(code);
            var astNodes = Parser.Parse(tokens);

            var map = new GameMap(width, height, goal, obstacles);
            var robot = new Robot(map, startX, startY);

            var interpreter = new Interpreter(robot);
            interpreter.Interpret(astNodes);

            return new ScriptExecutionResult(robot.IsGoalReached, null);
        }
        catch (Exception ex)
        {
            return new ScriptExecutionResult(false, ex.Message);
        }
    }
}
