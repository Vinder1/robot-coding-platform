namespace RobotCodingPlatform.Web.Features.Auth.SignIn;

public class SignInResponse
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = null!;

    public NewToken NewAccessToken { get; init; }
    public NewToken NewRefreshToken { get; init; }
    public UserDto User { get; init; } = null!;
}