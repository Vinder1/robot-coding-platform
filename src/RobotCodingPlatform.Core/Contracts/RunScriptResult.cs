namespace RobotCodingPlatform.Core.Contracts;

public record RunScriptResult(
    bool Success,
    string? Error = null,
    bool Compiled = true,
    string? ErrorKind = null);