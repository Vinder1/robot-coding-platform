namespace RobotCodingPlatform.Core;

public class SyntaxError(string msg, int line) : Exception($"{msg} (line {line}");