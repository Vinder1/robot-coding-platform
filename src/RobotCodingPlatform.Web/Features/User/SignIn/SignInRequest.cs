using Destructurama.Attributed;

namespace RobotCodingPlatform.Web.Features.User.SignIn;

public class SignInRequest
{
    public string Name { get; set; } = null!;

    [LogMasked(ShowFirst = 3, ShowLast = 2)]
    public string Email { get; set; } = null!;

    [LogMasked(ShowFirst = 3, ShowLast = 2)]
    public string Password { get; init; } = null!;
}