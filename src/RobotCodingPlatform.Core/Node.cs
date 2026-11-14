namespace RobotCodingPlatform.Core;

public abstract record Node(int Line);

public record ActionCommand(string Name, List<int> Arguments, int Line) : Node(Line);

public sealed record IfStatement(Condition Condition, List<Node> Children, List<Node>? ElseChildren, int Line) 
    : Node(Line);

public record WhileStatement(Condition Condition, List<Node> Children, int Line) : Node(Line);