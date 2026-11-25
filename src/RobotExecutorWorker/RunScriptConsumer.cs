using MassTransit;
using RobotCodingPlatform.Core;
using RobotCodingPlatform.Core.Contracts;

namespace RobotExecutorWorker;

public class RunScriptConsumer(
    ILogger<RunScriptConsumer> logger,
    ExecutionLimits limits) : IConsumer<RunScriptCommand>
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
            message.StartY,
            limits);

        if (result.Error is not null)
        {
            logger.LogWarning(
                "Script execution finished with {Kind}: {Error}",
                result.ErrorKind,
                result.Error);
        }

        await context.RespondAsync(new RunScriptResult(
            result.Success,
            result.Error,
            result.Compiled,
            result.ErrorKind.ToString()));
    }
}
