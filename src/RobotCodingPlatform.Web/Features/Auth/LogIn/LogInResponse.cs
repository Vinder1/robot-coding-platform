namespace RobotCodingPlatform.Web.Features.Auth.LogIn;

public class LogInResponse
{
    public bool Success { get; set; }
    public string Error { get; set; } = null!;
    public NewToken NewAccessToken { get; init; }
    public NewToken NewRefreshToken { get; init; }
    public UserDto User { get; init; } = null!;
}