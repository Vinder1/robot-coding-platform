using FluentValidation;
using MassTransit;
using RobotCodingPlatform.Core.Contracts;
using RobotCodingPlatform.Web.Features.Challenge;

namespace RobotCodingPlatform.Web.Features.Solution.Create;

public enum CreateSolutionAttemptStatus
{
    Created,
    ValidationFailed,
    ChallengeNotFound
}

public record CreateSolutionAttemptResult(
    CreateSolutionAttemptStatus Status,
    SolutionAttemptDto? Attempt,
    IReadOnlyList<string> Errors);

public interface ICreateSolutionAttemptService
{
    Task<CreateSolutionAttemptResult> CreateAsync(
        Guid userId,
        CreateSolutionAttemptRequest request,
        CancellationToken ct = default);
}

public class CreateSolutionAttemptService(
    IValidator<CreateSolutionAttemptRequest> validator,
    IChallengeRepository challengeRepository,
    ISolutionAttemptRepository repository,
    IPublishEndpoint publishEndpoint) : ICreateSolutionAttemptService
{
    public async Task<CreateSolutionAttemptResult> CreateAsync(
        Guid userId,
        CreateSolutionAttemptRequest request,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return new CreateSolutionAttemptResult(
                CreateSolutionAttemptStatus.ValidationFailed,
                null,
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }

        var challenge = await challengeRepository.GetByIdAsync(request.ChallengeId, ct);
        if (challenge is null)
        {
            return new CreateSolutionAttemptResult(
                CreateSolutionAttemptStatus.ChallengeNotFound,
                null,
                ["Испытание не найдено."]);
        }

        var attempt = new SolutionAttempt
        {
            Id = Guid.NewGuid(),
            ChallengeId = challenge.Id,
            UserId = userId,
            Code = request.Code,
            Status = SolutionAttemptStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        repository.Add(attempt);

        // Публикуем команду через EF-outbox: попытка и сообщение сохраняются
        // одной транзакцией при SaveChanges, после чего сообщение уходит воркеру.
        await publishEndpoint.Publish(
            new RunSolutionCommand(
                attempt.Id,
                attempt.Code,
                challenge.Width,
                challenge.Height,
                new MapPoint(challenge.GoalX, challenge.GoalY),
                [.. challenge.Obstacles.Select(o => new MapPoint(o.X, o.Y))],
                challenge.StartX,
                challenge.StartY),
            ct);

        await repository.SaveChangesAsync(ct);

        var dto = new SolutionAttemptDto(
            attempt.Id,
            attempt.ChallengeId,
            challenge.Name,
            attempt.UserId,
            attempt.Code,
            attempt.Status.ToString(),
            attempt.Success,
            attempt.Error,
            attempt.CreatedAt,
            attempt.CompletedAt);

        return new CreateSolutionAttemptResult(
            CreateSolutionAttemptStatus.Created,
            dto,
            []);
    }
}
