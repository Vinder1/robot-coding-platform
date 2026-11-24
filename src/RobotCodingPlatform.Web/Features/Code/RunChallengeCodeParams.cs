namespace RobotCodingPlatform.Web.Features.Code;

public class RunChallengeCodeParams
{
    public Guid ChallengeId { get; init; }
    public string Code { get; init; } = null!;
}
