using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Tests;

public class ExecutionAndMapValidationTests
{
    [Fact]
    public void Interpreter_InfiniteWhile_StopsAtIterationLimit()
    {
        var code = """
                   while true
                       turn_left
                   """;

        var map = new GameMap(3, 3, goal: (1, 1));
        var robot = new Robot(map);
        var interpreter = new Interpreter(robot, new ExecutionLimits { MaxIterations = 100 });

        var exception = Assert.Throws<ExecutionLimitExceededException>(() =>
            interpreter.Interpret(Parser.Parse(Lexer.Tokenize(code))));

        Assert.Contains("iterations", exception.Message);
    }

    [Fact]
    public void Robot_MoveBeyondStepLimit_StopsBeforeLooping()
    {
        var map = new GameMap(10, 10, goal: (9, 9));
        var robot = new Robot(map, limits: new ExecutionLimits { MaxSteps = 5 });

        Assert.Throws<ExecutionLimitExceededException>(() =>
            robot.ExecuteCommand("move", [int.MaxValue]));
    }

    [Fact]
    public void ScriptExecutor_SyntaxError_IsReportedAsCompilationError()
    {
        var result = ScriptExecutor.Execute("move @", 3, 3, (1, 1), [], 0, 0);

        Assert.False(result.Success);
        Assert.False(result.Compiled);
        Assert.Equal(ScriptExecutionErrorKind.Compilation, result.ErrorKind);
    }

    [Fact]
    public void ScriptExecutor_InfiniteLoop_IsReportedAsLimitExceeded()
    {
        var code = """
                   while true
                       turn_left
                   """;

        var result = ScriptExecutor.Execute(
            code,
            3,
            3,
            (1, 1),
            [],
            0,
            0,
            new ExecutionLimits { MaxIterations = 100 });

        Assert.False(result.Success);
        Assert.True(result.Compiled);
        Assert.Equal(ScriptExecutionErrorKind.LimitExceeded, result.ErrorKind);
    }

    [Fact]
    public void ScriptExecutor_StartOnObstacle_IsReportedAsInvalidMap()
    {
        var result = ScriptExecutor.Execute("move 1", 3, 3, (1, 1), [(0, 0)], 0, 0);

        Assert.False(result.Success);
        Assert.True(result.Compiled);
        Assert.Equal(ScriptExecutionErrorKind.InvalidMap, result.ErrorKind);
    }

    [Fact]
    public void ScriptExecutor_UnreachableGoal_IsReportedAsInvalidMap()
    {
        var obstacles = new List<(int X, int Y)>
        {
            (1, 0),
            (0, 1)
        };

        var result = ScriptExecutor.Execute("move 1", 2, 2, (1, 1), obstacles, 0, 0);

        Assert.False(result.Success);
        Assert.Equal(ScriptExecutionErrorKind.InvalidMap, result.ErrorKind);
        Assert.Contains("unreachable", result.Error);
    }

    [Fact]
    public void ScriptExecutor_OutOfBoundsStart_IsReportedAsInvalidMap()
    {
        var result = ScriptExecutor.Execute("move 1", 3, 3, (1, 1), [], -1, 0);

        Assert.False(result.Success);
        Assert.Equal(ScriptExecutionErrorKind.InvalidMap, result.ErrorKind);
    }

    [Fact]
    public void MapValidation_ValidMap_ReturnsNull()
    {
        var error = MapValidation.Validate(3, 3, (0, 0), (2, 2), [(1, 1)]);

        Assert.Null(error);
    }

    [Fact]
    public void ScriptExecutor_SuccessfulRun_ReturnsIterationCount()
    {
        var result = ScriptExecutor.Execute("move 1", 3, 3, (0, 1), [], 0, 0);

        Assert.True(result.Success);
        Assert.Equal(1, result.Iterations);
    }

    [Fact]
    public void ScriptExecutor_Loop_CountsLoopControlAndBodyIterations()
    {
        var code = """
                   while not wall_forward
                       move 1
                   """;

        var result = ScriptExecutor.Execute(code, 3, 3, (0, 1), [(0, 2)], 0, 0);

        Assert.True(result.Success);
        // 1 итерация на узел while + по 2 итерации на каждое выполнение тела
        // (проверка бюджета цикла и узел move).
        Assert.Equal(3, result.Iterations);
    }

    [Fact]
    public void ScriptExecutor_LimitExceeded_ReturnsIterationCount()
    {
        var code = """
                   while true
                       turn_left
                   """;

        var result = ScriptExecutor.Execute(
            code,
            3,
            3,
            (1, 1),
            [],
            0,
            0,
            new ExecutionLimits { MaxIterations = 100 });

        Assert.Equal(ScriptExecutionErrorKind.LimitExceeded, result.ErrorKind);
        Assert.True(result.Iterations > 100);
    }

    [Fact]
    public void ScriptExecutor_CompilationError_ReturnsZeroIterations()
    {
        var result = ScriptExecutor.Execute("move @", 3, 3, (1, 1), [], 0, 0);

        Assert.Equal(ScriptExecutionErrorKind.Compilation, result.ErrorKind);
        Assert.Equal(0, result.Iterations);
    }
}
