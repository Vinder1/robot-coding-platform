using MassTransit;
using RobotCodingPlatform.Core.Contracts;

namespace RobotCodingPlatform.Web.Features.Solution;

public class SolutionAttemptCompletedConsumer(
    ISolutionAttemptRepository repository,
    ILogger<SolutionAttemptCompletedConsumer> logger) : IConsumer<SolutionAttemptCompleted>
{
    public async Task Consume(ConsumeContext<SolutionAttemptCompleted> context)
    {
        var message = context.Message;

        var attempt = await repository.GetTrackedByIdAsync(message.AttemptId, context.CancellationToken);
        if (attempt is null)
        {
            logger.LogWarning(
                "Solution attempt {AttemptId} was not found when completing",
                message.AttemptId);
            return;
        }

        attempt.Status = message.Success
            ? SolutionAttemptStatus.Succeeded
            : SolutionAttemptStatus.Failed;
        attempt.Success = message.Success;
        attempt.Error = message.Error;
        attempt.CompletedAt = message.CompletedAt;

        await repository.SaveChangesAsync(context.CancellationToken);
    }
}
