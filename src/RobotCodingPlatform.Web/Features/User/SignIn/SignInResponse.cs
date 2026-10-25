namespace RobotCodingPlatform.Web.Features.User.SignIn;

public class SignInResponse
{
    public bool Success { get; init; }

    public string Token { get; set; } = null!;

    public string[] Errors { get; init; } = null!;
}