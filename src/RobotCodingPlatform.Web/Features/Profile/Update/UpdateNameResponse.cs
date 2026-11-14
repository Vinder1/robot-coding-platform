namespace RobotCodingPlatform.Web.Features.Profile.Update;

public class UpdateNameResponse
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = null!;
}