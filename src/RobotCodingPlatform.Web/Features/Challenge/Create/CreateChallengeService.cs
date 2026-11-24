using FluentValidation;

namespace RobotCodingPlatform.Web.Features.Challenge.Create;

public interface ICreateChallengeService
{
    Task<CreateChallengeResult> CreateAsync(CreateChallengeRequest request, CancellationToken ct = default);
}

public record CreateChallengeResult(
    bool Success,
    ChallengeDto? Challenge,
    IReadOnlyList<string> Errors);

public class CreateChallengeService(
    IValidator<CreateChallengeRequest> validator,
    IChallengeRepository repository) : ICreateChallengeService
{
    public async Task<CreateChallengeResult> CreateAsync(
        CreateChallengeRequest request,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return new CreateChallengeResult(
                false,
                null,
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }

        var challenge = new Challenge
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Width = request.Width,
            Height = request.Height,
            StartX = request.StartX,
            StartY = request.StartY,
            GoalX = request.GoalX,
            GoalY = request.GoalY,
            Obstacles = [.. (request.Obstacles ?? []).Select(o => new ChallengeObstacle { X = o.X, Y = o.Y })],
            CreatedAt = DateTime.UtcNow
        };

        await repository.CreateAsync(challenge, ct);

        return new CreateChallengeResult(true, challenge.ToDto(), []);
    }
}
