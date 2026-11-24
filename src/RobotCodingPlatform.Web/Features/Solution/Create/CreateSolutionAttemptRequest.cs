namespace RobotCodingPlatform.Web.Features.Solution.Create;

public class CreateSolutionAttemptRequest
{
    public Guid ChallengeId { get; init; }
    public string Code { get; init; } = null!;
}
