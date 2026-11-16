using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Web.Features.Code;

public interface IRunCodeService
{
    public Task<RunCodeResult> Run(RunCodeParams parameters);
}

public class RunCodeService : IRunCodeService
{
    public Task<RunCodeResult> Run(RunCodeParams parameters)
    {
        try
        {
            var map = new GameMap(parameters.Width, parameters.Height,
                goal: (parameters.Goal.X, parameters.Goal.Y),
                obstacles: parameters.Obstacles.Select(p => (p.X, p.Y)));
            var robot = new Robot(map);

            var code = parameters.Code.Replace("\t", "    ");

            var tokens = Lexer.Tokenize(parameters.Code);
            var ast = Parser.Parse(tokens);
            var interpreter = new Interpreter(robot);

            interpreter.Interpret(ast);

            return Task.FromResult(new RunCodeResult() { Success = robot.IsGoalReached, Compiled = true });
        }
        catch
        {
            return Task.FromResult(new RunCodeResult() { Compiled = false, Success = false });
        }
    }
}