using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Tests;

public class IntegrationTests
{
    [Fact]
    public void Interpreter_ExecutesConditionalLoop_Successfully()
    {
        var code = """
                   while not wall_forward
                       move 1
                   """;

        var map = new GameMap(3, 3, goal: (0, 1), obstacles: [(0, 2)]);
        var robot = new Robot(map);

        var tokens = Lexer.Tokenize(code);
        var ast = Parser.Parse(tokens);
        var interpreter = new Interpreter(robot);

        interpreter.Interpret(ast);

        Assert.True(robot.IsGoalReached);
    }
    
    [Fact]
    public void Interpreter_IfElseBranching_ExecutesCorrectBranch()
    {
        var code = """
                   if wall_left
                       turn_right
                   else
                       turn_left
                   """;

        var map = new GameMap(3, 3, goal: (1, 0));
        var robot = new Robot(map);

        var tokens = Lexer.Tokenize(code);
        var ast = Parser.Parse(tokens);
        var interpreter = new Interpreter(robot);

        interpreter.Interpret(ast);
        
        robot.ExecuteCommand("move", [1]);
        Assert.True(robot.IsGoalReached);
    }
    
    [Fact]
    public void Interpreter_ComplexLogicalConditions_AndOrNot_WorksCorrectly()
    {
        var code = """
                   while not wall_forward and (wall_left or wall_right)
                       move 1
                   """;

        var map = new GameMap(3, 4, goal: (1, 2), obstacles: [(0, 0), (2, 1)]);
        var robot = new Robot(map, 1);
        
        var tokens = Lexer.Tokenize(code);
        var ast = Parser.Parse(tokens);
        var interpreter = new Interpreter(robot);
        
        interpreter.Interpret(ast);

        Assert.True(robot.IsGoalReached);
    }
    
    [Fact]
    public void Interpreter_SequentialWhileLoopsWithTurn_ExecutesCorrectl()
    {
        var code = """
                   while not wall_forward
                       move 1
                   turn_right
                   while not wall_forward
                       move 1
                   """;

        var map = new GameMap(4, 4, goal: (3, 3));
        var robot = new Robot(map);
        
        var tokens = Lexer.Tokenize(code);
        var ast = Parser.Parse(tokens);
        var interpreter = new Interpreter(robot);
        
        interpreter.Interpret(ast);

        Assert.True(robot.IsGoalReached);
    }
    
    [Fact]
    public void Interpreter_NestedWhileLoops_ExecutesSuccessfully()
    {
        var code = """
                   while not wall_forward
                       while not wall_right
                           turn_right
                       move 1
                   """;

        var map = new GameMap(3, 3, goal: (2, 0));
        var robot = new Robot(map);

        var tokens = Lexer.Tokenize(code);
        var ast = Parser.Parse(tokens);
        var interpreter = new Interpreter(robot);

        interpreter.Interpret(ast);
    
        Assert.True(robot.IsGoalReached);
    }
}