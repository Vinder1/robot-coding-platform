using MassTransit;
using RobotCodingPlatform.Core;
using RobotCodingPlatform.Core.Contracts;

namespace RobotExecutorWorker;

public class RunScriptConsumer(ILogger<RunScriptConsumer> logger) : IConsumer<RunScriptCommand>
{
    public async Task Consume(ConsumeContext<RunScriptCommand> context)
    {
        var message = context.Message;

        try
        {
            var tokens = Lexer.Tokenize(message.Code);
            var astNodes = Parser.Parse(tokens);

            var map = new GameMap(message.Width, message.Height, message.Goal, message.Obstacles);
            var robot = new Robot(map, message.StartX, message.StartY);

            var interpreter = new Interpreter(robot);
            interpreter.Interpret(astNodes);

            await context.RespondAsync(new RunScriptResult(robot.IsGoalReached));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Parsing error");
            await context.RespondAsync(new RunScriptResult(false, ex.Message));
        }
    }
}