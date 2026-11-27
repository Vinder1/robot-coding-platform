using MassTransit;
using RobotCodingPlatform.Core.Contracts;
using RobotCodingPlatform.Web.Features.Statistics.Search;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Solution;

public class SolutionAttemptCompletedConsumer(
    ISolutionAttemptRepository repository,
    IUserRepository users,
    IStatisticsSearchService search,
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
        attempt.Iterations = message.Iterations;
        attempt.CompletedAt = message.CompletedAt;

        await repository.SaveChangesAsync(context.CancellationToken);

        if (!search.Enabled)
        {
            return;
        }

        try
        {
            var user = await users.FindByIdAsync(attempt.UserId);
            long? durationMs = attempt.CompletedAt is { } completedAt
                ? (long)(completedAt - attempt.CreatedAt).TotalMilliseconds
                : null;

            await search.IndexAsync(
                new SolutionAttemptStatisticsDocument
                {
                    AttemptId = attempt.Id.ToString(),
                    ChallengeId = attempt.ChallengeId.ToString(),
                    ChallengeName = attempt.Challenge?.Name ?? string.Empty,
                    UserId = attempt.UserId.ToString(),
                    UserName = user?.Name ?? "—",
                    Status = attempt.Status.ToString(),
                    Success = attempt.Success ?? false,
                    Error = attempt.Error,
                    Iterations = attempt.Iterations,
                    CreatedAt = attempt.CreatedAt,
                    CompletedAt = attempt.CompletedAt,
                    DurationMs = durationMs
                },
                context.CancellationToken);
        }
        catch (Exception ex)
        {
            // Статистика в OpenSearch — вторичный индекс. PostgreSQL остаётся
            // источником истины, поэтому сбой индексации не должен ломать попытку.
            logger.LogWarning(
                ex,
                "Failed to index completed solution attempt {AttemptId} in OpenSearch",
                attempt.Id);
        }
    }
}
