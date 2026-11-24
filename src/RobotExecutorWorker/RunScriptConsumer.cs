using MassTransit;
using RobotCodingPlatform.Core;
using RobotCodingPlatform.Core.Contracts;

namespace RobotExecutorWorker;

public class RunScriptConsumer(ILogger<RunScriptConsumer> logger) : IConsumer<RunScriptCommand>
{
    public async Task Consume(ConsumeContext<RunScriptCommand> context)
    {
        var message = context.Message;

        var result = ScriptExecutor.Execute(
            message.Code,
            message.Width,
            message.Height,
            (message.Goal.X, message.Goal.Y),
            (message.Obstacles ?? []).Select(o => (o.X, o.Y)),
            message.StartX,
            message.StartY);

        if (result.Error is not null)
        {
            logger.LogError("Parsing error: {Error}", result.Error);
        }

        await context.RespondAsync(new RunScriptResult(result.Success, result.Error));
    }
}
