namespace RobotCodingPlatform.Web.Features.Code;

public static class RunCodeErrorKind
{
    public const string Compilation = "Compilation";
    public const string InvalidMap = "InvalidMap";
    public const string Runtime = "Runtime";
    public const string LimitExceeded = "LimitExceeded";
    public const string Timeout = "Timeout";
    public const string WorkerError = "WorkerError";
}

public class RunCodeResult
{
    public string Message { get; init; } = "";
    public bool Compiled { get; init; }
    public bool Success { get; init; }
    public string? ErrorKind { get; init; }

    public static RunCodeResult InvalidMap(string message) => new()
    {
        Compiled = true,
        Success = false,
        ErrorKind = RunCodeErrorKind.InvalidMap,
        Message = message
    };

    public static RunCodeResult CompilationError(string message) => new()
    {
        Compiled = false,
        Success = false,
        ErrorKind = RunCodeErrorKind.Compilation,
        Message = message
    };

    public static RunCodeResult Timeout(string message) => new()
    {
        Compiled = false,
        Success = false,
        ErrorKind = RunCodeErrorKind.Timeout,
        Message = message
    };

    public static RunCodeResult WorkerError(string message) => new()
    {
        Compiled = false,
        Success = false,
        ErrorKind = RunCodeErrorKind.WorkerError,
        Message = message
    };
}
