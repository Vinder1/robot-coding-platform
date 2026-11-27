using MassTransit;
using RobotCodingPlatform.Core;
using RobotCodingPlatform.Core.Contracts;

namespace RobotExecutorWorker;

public class RunSolutionConsumer(
    ILogger<RunSolutionConsumer> logger,
    ExecutionLimits limits) : IConsumer<RunSolutionCommand>
{
    public async Task Consume(ConsumeContext<RunSolutionCommand> context)
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
                "Solution attempt {AttemptId} finished with {Kind}: {Error}",
                message.AttemptId,
                result.ErrorKind,
                result.Error);
        }

        await context.Publish(new SolutionAttemptCompleted(
            message.AttemptId,
            result.Success,
            result.Error,
            DateTime.UtcNow,
            result.Iterations));
    }
}
