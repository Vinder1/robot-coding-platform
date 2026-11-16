namespace RobotCodingPlatform.Web.Features.Code;

public class RunCodeResult
{
    public string Message { get; init; } = null!;
    public bool Compiled { get; init; }
    public bool Success { get; init; }
}